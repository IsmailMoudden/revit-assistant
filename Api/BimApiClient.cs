using System;
using System.Net.Http;
using System.Text;
using BimAiAssistant.Models;
using Newtonsoft.Json;

namespace BimAiAssistant.Api
{
    public static class BimApiClient
    {
        private const string BaseUrl = "https://autodesk-revit-backend.up.railway.app";

        // 30-second timeout — prevents indefinite UI freeze
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public static ActionResponse GenerateAction(string instruction)
        {
            string url = $"{BaseUrl}/api/v1/generate-action";

            var body    = JsonConvert.SerializeObject(new { instruction });
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = _http.PostAsync(url, content).GetAwaiter().GetResult();
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                throw new Exception("Request timed out after 30 seconds. Check that the backend is running.");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Cannot reach backend at {BaseUrl}. Details: {ex.Message}");
            }

            string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Backend returned HTTP {(int)response.StatusCode}:\n{responseBody}");

            ActionResponse result;
            try
            {
                result = JsonConvert.DeserializeObject<ActionResponse>(responseBody);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Could not parse backend response:\n{ex.Message}\n\nRaw:\n{responseBody}");
            }

            return result;
        }
    }
}
