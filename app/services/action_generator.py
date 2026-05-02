import json
from pydantic import TypeAdapter, ValidationError

from app.core.llm import call_llm
from app.prompts.bim_prompt import SYSTEM_PROMPT
from app.schemas.actions import BIMAction
from app.schemas.request import GenerateActionRequest, GenerateActionResponse

_action_adapter = TypeAdapter(BIMAction)


def _strip_markdown_fences(text: str) -> str:
    """Strip ```json ... ``` fences that some models add despite json_object mode."""
    text = text.strip()
    if text.startswith("```"):
        # drop first line (```json or ```)
        text = text.split("\n", 1)[-1]
        # drop closing fence — find last occurrence so embedded ``` don't break it
        if "```" in text:
            text = text[:text.rfind("```")]
    return text.strip()


def generate_bim_action(request: GenerateActionRequest) -> GenerateActionResponse:
    raw = call_llm(SYSTEM_PROMPT, request.instruction)
    clean = _strip_markdown_fences(raw)

    try:
        parsed = json.loads(clean)
    except json.JSONDecodeError as e:
        raise ValueError(f"LLM returned invalid JSON: {e}\nRaw output: {raw}")

    try:
        action = _action_adapter.validate_python(parsed)
    except ValidationError as e:
        raise ValueError(f"LLM output failed schema validation:\n{e}\nRaw output: {raw}")

    return GenerateActionResponse(
        instruction=request.instruction,
        action=action,
        raw_llm_output=raw,
    )
