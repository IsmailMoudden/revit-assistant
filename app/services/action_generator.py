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
        text = text.split("\n", 1)[-1]
        if "```" in text:
            text = text[:text.rfind("```")]
    return text.strip()


def _build_user_message(request: GenerateActionRequest) -> str:
    context = json.dumps({"selected_level": request.selected_level})
    return f"Context: {context}\n\nInstruction: {request.instruction}"


def generate_bim_action(request: GenerateActionRequest) -> GenerateActionResponse:
    raw = call_llm(SYSTEM_PROMPT, _build_user_message(request))
    clean = _strip_markdown_fences(raw)

    try:
        parsed = json.loads(clean)
    except json.JSONDecodeError as e:
        raise ValueError(f"LLM returned invalid JSON: {e}\nRaw output: {raw}")

    if "actions" not in parsed:
        raise ValueError(f"LLM response missing 'actions' key.\nRaw output: {raw}")

    actions = []
    for i, item in enumerate(parsed["actions"]):
        try:
            actions.append(_action_adapter.validate_python(item))
        except ValidationError as e:
            raise ValueError(f"Action #{i} failed schema validation:\n{e}\nItem: {item}")

    return GenerateActionResponse(
        instruction=request.instruction,
        actions=actions,
        raw_llm_output=raw,
    )
