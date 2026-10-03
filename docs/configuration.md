# Configuration

Settings are read from the backend process environment and `.env` in the working
directory. Environment values override the file. Restart the backend after changes.

| Variable | Purpose | Default |
| --- | --- | --- |
| `LLM_BASE_URL` | OpenAI-compatible Chat Completions endpoint base | `http://localhost:11434/v1` |
| `LLM_MODEL` | Exact provider or installed model identifier | Required for generation |
| `LLM_API_KEY` | Your provider credential | Empty for local services |
| `LLM_TIMEOUT_SECONDS` | Provider timeout | `120` |
| `LLM_JSON_MODE` | Request JSON object output | `true` |
| `BACKEND_API_KEY` | Optional bearer token protecting generation | Empty |
| `ALLOWED_ORIGINS` | Comma-separated browser origins | Localhost |

Legacy `OPENROUTER_API_KEY`, `OPENROUTER_MODEL` and `OPENROUTER_BASE_URL` remain
accepted. Remove legacy values when migrating so configuration is unambiguous.

## Local Model

```dotenv
LLM_BASE_URL=http://localhost:11434/v1
LLM_MODEL=your-installed-model
LLM_API_KEY=
```

Ollama setup: <https://docs.ollama.com/>. Use a model capable of reliable JSON
generation. Latency and output quality depend on the model and hardware.

## OpenRouter

```dotenv
LLM_BASE_URL=https://openrouter.ai/api/v1
LLM_MODEL=your-provider/model-id
LLM_API_KEY=your-own-key
```

## Another Compatible Provider

```dotenv
LLM_BASE_URL=https://your-provider.example/v1
LLM_MODEL=your-model-id
LLM_API_KEY=your-own-key
```

The endpoint must implement Chat Completions. If it rejects `response_format`,
set `LLM_JSON_MODE=false`; responses must still contain valid action JSON.
Provider support is not universal: test your chosen model with representative commands.

## Revit Connection

Set user environment variables in Windows PowerShell, then restart Revit:

```powershell
[Environment]::SetEnvironmentVariable("BIM_BACKEND_URL", "http://127.0.0.1:8000", "User")
# Only for a backend protected by BACKEND_API_KEY:
[Environment]::SetEnvironmentVariable("BIM_BACKEND_API_KEY", "your-backend-token", "User")
```

`BIM_BACKEND_API_KEY` must match the backend's `BACKEND_API_KEY`. It is a backend
access token, not the model provider key. Use HTTPS for remote connections.

Keep the service bound to loopback for personal local use. Before sharing a server,
set a backend token and configure deployment-level request limits. CORS is a browser
policy and does not authenticate desktop clients. Context and instructions are sent
to the selected provider; choose an endpoint suitable for your project's data.
