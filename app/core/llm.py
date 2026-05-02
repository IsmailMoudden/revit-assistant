from openai import OpenAI
from app.core.config import settings

# OpenRouter is OpenAI-compatible — we just override base_url and api_key
client = OpenAI(
    api_key=settings.openrouter_api_key,
    base_url=settings.openrouter_base_url,
)


def call_llm(system_prompt: str, messages: list[dict]) -> str:
    response = client.chat.completions.create(
        model=settings.openrouter_model,
        messages=[{"role": "system", "content": system_prompt}, *messages],
        response_format={"type": "json_object"},
        temperature=0,
    )
    return response.choices[0].message.content
