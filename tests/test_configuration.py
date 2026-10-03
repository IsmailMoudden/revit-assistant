import unittest
from unittest.mock import MagicMock, patch

from fastapi.testclient import TestClient
from app.core.config import Settings, settings
from app.core.llm import call_llm
from app.main import app


class ConfigurationTests(unittest.TestCase):
    def test_generic_and_legacy_settings(self):
        for prefix in ("LLM", "OPENROUTER"):
            with patch.dict("os.environ", {}, clear=True):
                config = Settings(_env_file=None, **{
                    f"{prefix}_API_KEY": "test-secret",
                    f"{prefix}_MODEL": "custom-model",
                    f"{prefix}_BASE_URL": "http://localhost:9999/v1",
                })
                self.assertEqual(config.llm_model, "custom-model")
                self.assertEqual(config.llm_api_key.get_secret_value(), "test-secret")
                self.assertNotIn("test-secret", repr(config))

    def test_provider_selection_and_json_mode(self):
        for json_mode in (True, False):
            client = MagicMock()
            client.chat.completions.create.return_value.choices[0].message.content = '{}'
            with patch("app.core.llm.OpenAI") as factory, \
                 patch.object(settings, "llm_model", "my-model"), \
                 patch.object(settings, "llm_json_mode", json_mode), \
                 patch.object(settings, "llm_send_temperature", json_mode):
                factory.return_value.__enter__.return_value = client
                self.assertEqual(call_llm("prompt", []), '{}')
                args = client.chat.completions.create.call_args.kwargs
                self.assertEqual(args["model"], "my-model")
                self.assertEqual("response_format" in args, json_mode)
                self.assertEqual("temperature" in args, json_mode)

    def test_backend_token_and_health(self):
        from pydantic import SecretStr
        with patch.object(settings, "backend_api_key", SecretStr("private-token")), \
             patch("app.api.routes.generate_bim_action", side_effect=ValueError("private output")) as generate:
            with TestClient(app) as client:
                self.assertEqual(client.get("/health").status_code, 200)
                self.assertEqual(client.post("/api/v1/generate-action", json={"instruction": "wall"}).status_code, 401)
                generate.assert_not_called()
                response = client.post("/api/v1/generate-action", json={"instruction": "wall"}, headers={"Authorization": "Bearer private-token"})
                self.assertEqual(response.status_code, 422)
                self.assertNotIn("private output", response.text)


if __name__ == "__main__":
    unittest.main()
