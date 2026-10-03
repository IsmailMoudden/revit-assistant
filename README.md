# Revit Assistant

**Your model. Your provider. Your workspace.**

An open-source Revit add-in and Python service for turning natural-language
instructions into structured BIM operations. Run your model locally or connect
an OpenAI-compatible provider with your own credentials.

[Getting started](#quick-start) | [Configuration](docs/configuration.md) |
[Architecture](docs/architecture.md) | [Contributing](CONTRIBUTING.md)

## What you can build

- Walls, doors, windows, structural columns and beams.
- Parametric frames expanded deterministically into individual elements.
- Instructions grounded in project levels, loaded families and selected IDs.
- Clarification questions and execution feedback with per-action rollback.

Example instructions:

> Create a 5 meter wall from (0, 0) to (5, 0).
>
> Create a structural frame with 3 bays in X and 2 bays in Y, spaced 5 meters apart.

**Status: experimental.** Review generated operations in a disposable model first.
The current add-in targets **Revit 2024 on Windows**. Other releases are not
supported by this build. Revit and third-party model services are licensed separately.

## Quick Start

Requires Python 3.11+ and an OpenAI-compatible model endpoint.

```sh
python -m venv .venv
# macOS / Linux
source .venv/bin/activate
# Windows PowerShell: .venv\Scripts\Activate.ps1
pip install -r requirements.txt
cp .env.example .env
# Windows PowerShell: Copy-Item .env.example .env
```

Edit `.env`: set `LLM_MODEL` to your model identifier, and configure the URL and
key for your provider. For local Ollama, install and download a model yourself,
then use its installed name. See the [provider examples](docs/configuration.md).

```sh
uvicorn app.main:app --host 127.0.0.1 --port 8000
```

Open <http://127.0.0.1:8000/docs> for the interactive API and
<http://127.0.0.1:8000/health> for service health.

Alternatively, run `docker compose up --build` after configuring `.env`.
For a model running on the host, use
`LLM_BASE_URL=http://host.docker.internal:11434/v1` and make sure the model server
accepts connections from Docker. The backend port is exposed only on loopback.

### Build And Install The Add-in

On Windows with the .NET SDK and Revit 2024:

```powershell
dotnet build "Front-Revit AI/BimAiAssistant/BimAiAssistant.csproj" -c Release
```

Copy `BimAiAssistant.dll` and `Newtonsoft.Json.dll` from the build output into
`Front-Revit AI/BimAiAssistant/Installer/`, then run its `install.bat`.
The Windows CI also packages these files as a downloadable installer artifact.
Start the backend before opening Revit. The add-in connects to
`http://127.0.0.1:8000` by default. Use `BIM_BACKEND_URL` to choose another server.

## Design

```mermaid
flowchart LR
    R[Revit add-in] -->|Instruction and project context| B[FastAPI service]
    B --> P[Local or hosted model]
    P --> V[Schema validation]
    V --> G[Deterministic frame expansion]
    G --> R
    R -->|Execution feedback| B
```

The model proposes data, not executable source code. Revit changes go through
explicit handlers and transactions. No account or database is required.
Provider credentials stay in the backend environment; they are not part of requests.

## Current Limits

- Move and delete schemas exist, but their Revit handlers are not implemented.
- Wall thickness and door/window dimensions are not applied by the current handlers.
- Multi-floor placement and family/type resolution need further validation.
- Existing geometry is not yet included in project context.
- Automatic retries may repeat successful operations; inspect the resulting model.
- Section suggestions are heuristics, not verified Eurocode structural calculations.
- Requests currently block the Revit command while waiting for the model.

## Development

```sh
python -m unittest discover -s tests -v
```

Contributions that improve geometry, type resolution, provider compatibility and
installation are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE). Free to use, modify and integrate, including commercial projects.
Hosted inference can incur provider charges; local inference requires your own hardware.
This project is independent and is not affiliated with Autodesk.
