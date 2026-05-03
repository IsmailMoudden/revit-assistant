import json
from pydantic import TypeAdapter, ValidationError

from app.core.llm import call_llm
from app.prompts.bim_prompt import SYSTEM_PROMPT
from app.schemas.actions import BIMAction
from app.schemas.request import GenerateActionRequest, GenerateActionResponse, Question

_action_adapter = TypeAdapter(BIMAction)


def _strip_markdown_fences(text: str) -> str:
    """Strip ```json ... ``` fences that some models add despite json_object mode."""
    text = text.strip()
    if text.startswith("```"):
        text = text.split("\n", 1)[-1]
        if "```" in text:
            text = text[:text.rfind("```")]
    return text.strip()


def _build_messages(request: GenerateActionRequest) -> list[dict]:
    messages = [{"role": m.role, "content": m.content} for m in request.history]

    parts = [f"Context: {json.dumps({'selected_level': request.selected_level})}"]

    ctx = request.bim_context
    if ctx.existing_elements or ctx.levels or ctx.selected_element_ids:
        parts.append(f"BIM model state: {ctx.model_dump_json()}")

    if request.execution_results:
        results = [r.model_dump() for r in request.execution_results]
        parts.append(f"Results from last execution: {json.dumps(results)}")

    if request.answers:
        parts.append(f"Answers to previous questions: {json.dumps(request.answers)}")

    parts.append(f"Instruction: {request.instruction}")
    messages.append({"role": "user", "content": "\n\n".join(parts)})
    return messages


def generate_bim_action(request: GenerateActionRequest) -> GenerateActionResponse:
    raw = call_llm(SYSTEM_PROMPT, _build_messages(request))
    clean = _strip_markdown_fences(raw)

    try:
        parsed = json.loads(clean)
    except json.JSONDecodeError as e:
        raise ValueError(f"LLM returned invalid JSON: {e}\nRaw output: {raw}")

    status = parsed.get("status")
    if status not in ("ok", "needs_clarification"):
        raise ValueError(f"LLM response missing valid 'status' field.\nRaw output: {raw}")

    if status == "needs_clarification":
        try:
            questions = [Question.model_validate(q) for q in parsed.get("questions", [])]
        except ValidationError as e:
            raise ValueError(f"Invalid question schema:\n{e}")
        return GenerateActionResponse(
            status="needs_clarification",
            questions=questions,
            raw_llm_output=raw,
        )

    # status == "ok"
    actions = []
    for i, item in enumerate(parsed.get("actions", [])):
        try:
            actions.append(_action_adapter.validate_python(item))
        except ValidationError as e:
            raise ValueError(f"Action #{i} failed schema validation:\n{e}\nItem: {item}")

    return GenerateActionResponse(
        status="ok",
        actions=actions,
        raw_llm_output=raw,
    )
