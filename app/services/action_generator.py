import json
from pydantic import TypeAdapter, ValidationError

from app.core.llm import call_llm
from app.prompts.bim_prompt import SYSTEM_PROMPT
from app.schemas.actions import BIMAction, CreateGridAction, CreateColumnAction, CreateBeamAction
from app.services.eurocode import select_column_section, select_beam_section, best_available_column, best_available_beam
from app.schemas.request import GenerateActionRequest, GenerateActionResponse, Question, ErrorDetail
from app.services.grid_expander import expand_grid

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
        ctx = request.bim_context
        auto_fix_hint = {}
        if ctx.loaded_column_families:
            auto_fix_hint["use_column_section"] = ctx.loaded_column_families[0]
        if ctx.loaded_beam_families:
            auto_fix_hint["use_beam_section"] = ctx.loaded_beam_families[0]
        if ctx.levels:
            auto_fix_hint["available_levels"] = ctx.levels
        hint_str = f" Auto-fix hint: {json.dumps(auto_fix_hint)}" if auto_fix_hint else ""
        parts.append(f"Results from last execution: {json.dumps(results)}.{hint_str}")

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

    # Support both "type":"clarification" and "status":"needs_clarification" — normalise here
    if parsed.get("type") == "clarification":
        parsed["status"] = "needs_clarification"

    status = parsed.get("status")
    if status not in ("ok", "needs_clarification", "error"):
        raise ValueError(f"LLM response missing valid 'status' field.\nRaw output: {raw}")

    if status == "needs_clarification":
        raw_questions = parsed.get("questions", [])
        if not raw_questions:
            raise ValueError(f"LLM returned needs_clarification but 'questions' is empty.\nRaw: {raw}")
        try:
            questions = [Question.model_validate(q) for q in raw_questions]
        except ValidationError as e:
            raise ValueError(f"Invalid question schema:\n{e}")
        return GenerateActionResponse(
            status="needs_clarification",
            questions=questions,
            raw_llm_output=raw,
        )

    if status == "error":
        try:
            error = ErrorDetail.model_validate(parsed.get("error", {}))
        except ValidationError as e:
            raise ValueError(f"Invalid error schema:\n{e}")
        return GenerateActionResponse(
            status="error",
            error=error,
            raw_llm_output=raw,
        )

    # status == "ok"
    actions: list[BIMAction] = []
    warnings: list[str] = []

    for i, item in enumerate(parsed.get("actions", [])):
        try:
            action = _action_adapter.validate_python(item)
        except ValidationError as e:
            raise ValueError(f"Action #{i} failed schema validation:\n{e}\nItem: {item}")

        # Resolve null sections — LLM sometimes returns null, never send null to plugin
        if isinstance(action, CreateColumnAction) and action.section is None:
            loaded = request.bim_context.loaded_column_families
            ideal = select_column_section(action.height)
            resolved, w = best_available_column(ideal, loaded)
            action = action.model_copy(update={"section": resolved})
            if w:
                warnings.append(w)

        elif isinstance(action, CreateBeamAction) and action.section is None:
            loaded = request.bim_context.loaded_beam_families
            span = abs(action.end.x - action.start.x) or abs(action.end.y - action.start.y) or 5.0
            ideal = select_beam_section(span)
            resolved, w = best_available_beam(ideal, loaded)
            action = action.model_copy(update={"section": resolved})
            if w:
                warnings.append(w)

        if isinstance(action, CreateGridAction):
            expanded, grid_warnings = expand_grid(
                origin_x=action.origin.x,
                origin_y=action.origin.y,
                bays_x=action.bays_x,
                bays_y=action.bays_y,
                spacing_x=action.spacing_x,
                spacing_y=action.spacing_y,
                floors=action.floors,
                floor_height=action.floor_height,
                base_level=action.base_level,
                column_section=action.column_section,
                beam_section_x=action.beam_section_x,
                beam_section_y=action.beam_section_y,
                loaded_column_families=request.bim_context.loaded_column_families or None,
                loaded_beam_families=request.bim_context.loaded_beam_families or None,
            )
            actions.extend(expanded)
            warnings.extend(grid_warnings)
        else:
            actions.append(action)

    return GenerateActionResponse(
        status="ok",
        actions=actions,
        warnings=warnings,
        raw_llm_output=raw,
    )
