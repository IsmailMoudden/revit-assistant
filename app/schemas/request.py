from typing import Literal, Any
from pydantic import BaseModel, Field, model_validator
from app.schemas.actions import BIMAction


class ConversationMessage(BaseModel):
    role: Literal["user", "assistant"]
    content: str


# ── BIM Context — snapshot of the Revit model sent by the plugin ───────────────

class ExistingElement(BaseModel):
    id: str                          # Revit element ID, used to reference in instructions
    type: str                        # action type: "create_column", "create_wall", etc.
    position: dict[str, float] | None = None   # for point elements (columns, doors)
    start: dict[str, float] | None = None      # for linear elements (walls, beams)
    end: dict[str, float] | None = None
    level: str | None = None


class BIMContext(BaseModel):
    existing_elements: list[ExistingElement] = []
    levels: list[str] = []
    selected_element_ids: list[str] = []       # IDs currently selected in Revit


# ── Execution feedback — what happened after the plugin ran the last actions ───

class ExecutionResult(BaseModel):
    action: str                      # action type that was executed
    status: Literal["success", "error"]
    revit_id: str | None = None      # Revit element ID if created successfully
    reason: str | None = None        # error message if status == "error"


# ── Request ────────────────────────────────────────────────────────────────────

class GenerateActionRequest(BaseModel):
    instruction: str
    selected_level: str = "Level 1"
    answers: dict[str, Any] | None = None
    history: list[ConversationMessage] = []
    bim_context: BIMContext = BIMContext()             # optional — empty = blind mode
    execution_results: list[ExecutionResult] = []     # optional — feedback from last run


class Question(BaseModel):
    id: str
    question: str
    default: Any
    type: Literal["number", "text", "choice"]


class GenerateActionResponse(BaseModel):
    status: Literal["ok", "needs_clarification"]
    actions: list[BIMAction] | None = None      # present when status == "ok"
    questions: list[Question] | None = None     # present when status == "needs_clarification"
    raw_llm_output: str = Field(
        description="Raw LLM string. For debugging/logging only. Never use in plugin logic."
    )

    @model_validator(mode="after")
    def check_exclusive(self) -> "GenerateActionResponse":
        if self.status == "ok" and not self.actions:
            raise ValueError("status is 'ok' but 'actions' is missing or empty")
        if self.status == "needs_clarification" and not self.questions:
            raise ValueError("status is 'needs_clarification' but 'questions' is missing or empty")
        return self
