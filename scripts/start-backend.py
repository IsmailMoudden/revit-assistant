"""Bootstrap a local backend and guide first-time provider configuration."""
import argparse
import getpass
import os
from pathlib import Path
import shutil
import subprocess
import sys
import venv


ROOT = Path(__file__).resolve().parent.parent


def main():
    parser = argparse.ArgumentParser(description="Set up and start Revit Assistant's local backend.")
    parser.add_argument("--ready", action="store_true", help=argparse.SUPPRESS)
    parser.add_argument("--setup-only", action="store_true", help="Configure the backend without starting it")
    args = parser.parse_args()
    if sys.version_info < (3, 11):
        raise SystemExit("Install Python 3.11 or newer, then run this launcher again.")
    os.chdir(ROOT)
    if not args.ready:
        environment = ROOT / '.venv'
        python = environment / ('Scripts/python.exe' if os.name == 'nt' else 'bin/python')
        if not python.exists():
            print('Preparing your Python environment...', flush=True)
            venv.create(environment, with_pip=True)
        import hashlib
        fingerprint = hashlib.sha256((ROOT / 'requirements.txt').read_bytes()).hexdigest()
        marker = environment / '.requirements-sha256'
        if not marker.exists() or marker.read_text().strip() != fingerprint:
            subprocess.run([str(python), '-m', 'pip', 'install', '-r', 'requirements.txt'], check=True)
            marker.write_text(fingerprint)
        return subprocess.run([str(python), str(Path(__file__).resolve()), '--ready',
                               *(['--setup-only'] if args.setup_only else [])]).returncode

    from dotenv import set_key
    sys.path.insert(0, str(ROOT))
    from app.core.config import Settings

    configuration = ROOT / '.env'
    if not configuration.exists():
        shutil.copyfile(ROOT / '.env.example', configuration)
        if os.name != 'nt':
            configuration.chmod(0o600)
    settings = Settings()
    if not settings.llm_model.strip():
        print('\nChoose your model provider:')
        print('1. Local Ollama (install and download a model separately)')
        print('2. OpenRouter (your own API key)')
        print('3. Another OpenAI-compatible endpoint')
        provider = input('Provider [1]: ').strip() or '1'
        if provider not in ('1', '2', '3'):
            raise SystemExit('Choose 1, 2 or 3 and restart the launcher.')
        url = {'1': 'http://localhost:11434/v1', '2': 'https://openrouter.ai/api/v1'}.get(provider)
        if url is None:
            url = input('Endpoint base URL: ').strip()
        if not url or not url.startswith(('http://', 'https://')):
            raise SystemExit('Enter an http:// or https:// endpoint URL.')
        model = input('Exact model name: ').strip()
        if not model:
            raise SystemExit('A model name is required. Restart to finish configuration.')
        key = '' if provider == '1' else getpass.getpass('Provider API key (hidden; optional for local endpoints): ')
        if provider == '2' and not key:
            raise SystemExit('OpenRouter requires your API key.')
        for name, value in {'LLM_BASE_URL': url, 'LLM_MODEL': model, 'LLM_API_KEY': key}.items():
            set_key(str(configuration), name, value)
        print('Configuration saved locally in .env.')
    if args.setup_only:
        return 0
    print('\nBackend: http://127.0.0.1:8000\nKeep this window open while using Revit. Ctrl+C stops the backend.', flush=True)
    return subprocess.run([sys.executable, '-m', 'uvicorn', 'app.main:app',
                           '--host', '127.0.0.1', '--port', '8000']).returncode


if __name__ == '__main__':
    try:
        sys.exit(main())
    except KeyboardInterrupt:
        print('\nStopped.')
        sys.exit(0)
    except subprocess.CalledProcessError as error:
        sys.exit(error.returncode)
