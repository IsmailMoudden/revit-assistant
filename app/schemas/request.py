from typing import Literal, Any
from pydantic import BaseModel, Field, model_validator
from app.schemas.actions import BIMAction


class ConversationMessage(BaseModel):
    role: Literal["user", "assistant"]
    content: str


class GenerateActionRequest(BaseModel):
    instruction: str
    selected_level: str = "Level 1"
    answers: dict[str, Any] | None = None
    history: list[ConversationMessage] = []  # client owns and sends this each time


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
