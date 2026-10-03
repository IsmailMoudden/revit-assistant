from pydantic import AliasChoices, Field, SecretStr
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")
    llm_api_key: SecretStr = Field(default=SecretStr(""), validation_alias=AliasChoices("LLM_API_KEY", "OPENROUTER_API_KEY"))
    llm_model: str = Field(default="", validation_alias=AliasChoices("LLM_MODEL", "OPENROUTER_MODEL"))
    llm_base_url: str = Field(default="http://localhost:11434/v1", validation_alias=AliasChoices("LLM_BASE_URL", "OPENROUTER_BASE_URL"))
    llm_timeout_seconds: float = Field(default=120, gt=0, le=600)
    llm_json_mode: bool = True
    llm_send_temperature: bool = True
    llm_temperature: float = Field(default=0, ge=0, le=2)
    backend_api_key: SecretStr = SecretStr("")
    allowed_origins: str = "http://localhost,http://127.0.0.1"


settings = Settings()
