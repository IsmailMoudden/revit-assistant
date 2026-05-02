from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    openrouter_api_key: str
    openrouter_model: str = "openai/gpt-4o"
    openrouter_base_url: str = "https://openrouter.ai/api/v1"
    allowed_origins: str = "*"  # override in Railway with your front URL

    class Config:
        env_file = ".env"


settings = Settings()
