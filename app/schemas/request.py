from typing import Literal, Any
from pydantic import BaseModel, ConfigDict, Field, model_validator, field_validator
from app.schemas.actions import BIMAction


class ConversationMessage(BaseModel):
    role: Literal["user", "assistant"]
    content: str


# ── BIM Context — snapshot of the Revit model sent by the plugin ───────────────

class ExistingElement(BaseModel):
    model_config = ConfigDict(coerce_numbers_to_str=True)

    id: str
    type: str
    position: dict[str, float] | None = None
    start: dict[str, float] | None = None
    end: dict[str, float] | None = None
    level: str | None = None

    @field_validator("id", mode="before")
    @classmethod
    def coerce_id_to_str(cls, v: Any) -> str:
        return str(v)


class BIMContext(BaseModel):
    existing_elements: list[ExistingElement] = []
    levels: list[str] = []
    selected_element_ids: list[str] = []
    loaded_column_families: list[str] = []
    loaded_beam_families: list[str] = []
    loaded_wall_types: list[str] = []

    @field_validator(
        "existing_elements", "levels",
        "loaded_column_families", "loaded_beam_families", "loaded_wall_types",
        mode="before",
    )
    @classmethod
    def null_to_empty_list(cls, v: Any) -> Any:
        return v if v is not None else []

    @field_validator("selected_element_ids", mode="before")
    @classmethod
    def coerce_element_ids(cls, v: Any) -> list:
        if v is None:
            return []
        return [str(item) for item in v]


# ── Execution feedback — what happened after the plugin ran the last actions ───

class ExecutionResult(BaseModel):
    model_config = ConfigDict(coerce_numbers_to_str=True)

    action: str
    status: Literal["success", "error"]
    revit_id: str | None = None
    reason: str | None = None
    original_params: dict | None = None

    @field_validator("revit_id", mode="before")
    @classmethod
    def coerce_revit_id(cls, v: Any) -> str | None:
        return str(v) if v is not None else None


# ── Request ────────────────────────────────────────────────────────────────────

class GenerateActionRequest(BaseModel):
    model_config = ConfigDict(coerce_numbers_to_str=True)

    instruction: str
    selected_level: str = "Level 1"
    answers: dict[str, Any] | None = None
    history: list[ConversationMessage] = []
    bim_context: BIMContext = BIMContext()
    execution_results: list[ExecutionResult] = []

    @field_validator("history", "execution_results", mode="before")
    @classmethod
    def null_to_empty_list(cls, v: Any) -> Any:
        return v if v is not None else []

    @field_validator("selected_level", mode="before")
    @classmethod
    def coerce_level_to_str(cls, v: Any) -> str:
        return str(v) if v is not None else "Level 1"


class Question(BaseModel):
    id: str
    question: str
    type: str           # "text" | "number" | "choice"
    default: Any        # ALWAYS present — plugin shows "Use default" button with this value


class ErrorDetail(BaseModel):
    message: str        # human-readable explanation of what went wrong
    cause: str | None = None    # raw Revit error if available
    fix: str | None = None      # step-by-step instructions to resolve manually


class GenerateActionResponse(BaseModel):
    status: Literal["ok", "needs_clarification", "error"]
    actions: list[BIMAction] | None = None
    questions: list[Question] | None = None
    error: ErrorDetail | None = None
    warnings: list[str] = []    # engineering notes (Eurocode, spans, etc.) — show in plugin UI
    raw_llm_output: str = Field(
        description="Raw LLM string. For debugging/logging only. Never use in plugin logic."
    )

    @model_validator(mode="after")
    def check_exclusive(self) -> "GenerateActionResponse":
        if self.status == "ok" and self.actions is None:
            raise ValueError("status is 'ok' but 'actions' is missing")
        if self.status == "needs_clarification" and not self.questions:
            raise ValueError("status is 'needs_clarification' but 'questions' is missing or empty")
        if self.status == "error" and not self.error:
            raise ValueError("status is 'error' but 'error' field is missing")
        return self
