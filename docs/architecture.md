# Architecture

## Service

`app/main.py` initializes FastAPI. `app/api/routes.py` exposes generation under
`/api/v1/generate-action`, authenticates an optional bearer token and translates
provider failures into HTTP errors. `/health` checks service liveness, not model availability.

`app/core/config.py` loads configuration. `app/core/llm.py` uses an
OpenAI-compatible Chat Completions client with bounded timeout and no automatic
SDK retries. Provider keys never belong in repository files.

`app/services/action_generator.py` builds messages from client history, context,
answers and execution results. It parses and validates JSON with the discriminated
action union in `app/schemas/actions.py`. Grid commands are expanded by
`grid_expander.py`; section suggestions in `eurocode.py` are heuristic defaults.

## Contract

Requests contain `instruction`, `selected_level`, optional `answers`, `history`,
`bim_context` and `execution_results`. Responses have one of three statuses:

| Status | Payload |
| --- | --- |
| `ok` | `actions`, optional warnings |
| `needs_clarification` | Questions with IDs and defaults |
| `error` | Message, optional cause and proposed fix |

Coordinates and dimensions use meters. Element IDs cross the boundary as strings.
The API schema is available at `/openapi.json`. `raw_llm_output` is diagnostic
data and may contain project details; avoid publishing real responses in issues.

## Add-in

`RunAiCommand` gathers context and runs the clarification and feedback workflows.
`BimApiClient` reads the backend address and optional access token from environment
variables. `ActionDispatcher` executes supported actions in subtransactions within
a parent Revit transaction. Individual action failures roll back independently.

The backend is stateless; conversation history lives in the add-in process.
Revit 2024 is the current compilation target. The Windows CI builds that target;
it does not execute Revit or establish compatibility with other versions.

## Contribution Priorities

1. Stable action IDs and retries restricted to failed actions.
2. Family symbol IDs, level elevations and explicit coordinate conventions.
3. Plan preview and approval before model changes.
4. Move/delete handlers with focused integration coverage.
5. Separate builds and validation for additional Revit versions.
