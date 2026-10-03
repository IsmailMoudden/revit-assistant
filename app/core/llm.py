from openai import OpenAI
from app.core.config import settings

def call_llm(system_prompt: str, messages: list[dict]) -> str:
    if not settings.llm_model.strip():
        raise ValueError("Set LLM_MODEL to the model identifier offered by your provider.")
    options = {"response_format": {"type": "json_object"}} if settings.llm_json_mode else {}
    if settings.llm_send_temperature:
        options["temperature"] = settings.llm_temperature
    with OpenAI(
        api_key=settings.llm_api_key.get_secret_value() or "local",
        base_url=settings.llm_base_url,
        timeout=settings.llm_timeout_seconds,
        max_retries=0,
    ) as client:
        response = client.chat.completions.create(
            model=settings.llm_model,
            messages=[{"role": "system", "content": system_prompt}, *messages],
            **options,
        )
    content = response.choices[0].message.content
    if not content:
        raise ValueError("The provider returned an empty response.")
    return content
