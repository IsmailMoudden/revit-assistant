import contextlib
import io
import os
from pathlib import Path
import runpy
import shutil
import sys
import tempfile
import unittest
from unittest.mock import patch

from dotenv import dotenv_values


ROOT = Path(__file__).resolve().parent.parent


class LauncherTests(unittest.TestCase):
    def test_first_launch_and_reuse(self):
        launcher = runpy.run_path(str(ROOT / 'scripts/start-backend.py'))
        main = launcher['main']
        for provider in ('1', '2'):
            with self.subTest(provider=provider), tempfile.TemporaryDirectory() as folder:
                destination = Path(folder)
                shutil.copyfile(ROOT / '.env.example', destination / '.env.example')
                previous = Path.cwd()
                main.__globals__['ROOT'] = destination
                output = io.StringIO()
                try:
                    with patch.dict(os.environ, {}, clear=True), \
                         patch.object(sys, 'argv', ['start-backend.py', '--ready', '--setup-only']), \
                         patch('builtins.input', side_effect=[provider, 'test-model']), \
                         patch('getpass.getpass', return_value='private-test-key'), \
                         contextlib.redirect_stdout(output):
                        self.assertEqual(main(), 0)
                    configuration = dotenv_values(destination / '.env')
                    self.assertEqual(configuration['LLM_MODEL'], 'test-model')
                    self.assertEqual(configuration['LLM_API_KEY'], '' if provider == '1' else 'private-test-key')
                    self.assertNotIn('private-test-key', output.getvalue())
                    with patch.dict(os.environ, {}, clear=True), \
                         patch.object(sys, 'argv', ['start-backend.py', '--ready', '--setup-only']), \
                         patch('builtins.input', side_effect=AssertionError('Configuration was not reused')):
                        self.assertEqual(main(), 0)
                finally:
                    os.chdir(previous)
