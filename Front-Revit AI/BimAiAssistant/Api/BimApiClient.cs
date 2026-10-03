using System;
using System.IO;
using System.Net;
using System.Text;
using BimAiAssistant.Models;
using Newtonsoft.Json;

namespace BimAiAssistant.Api
{
    public static class BimApiClient
    {
        private static string BaseUrl => (Environment.GetEnvironmentVariable("BIM_BACKEND_URL") ?? "http://127.0.0.1:8000").TrimEnd('/');
        private const int    TimeoutMs = 180_000;

        /// <summary>
        /// Single entry point for all calls.
        /// The caller builds the BimRequest (with or without answers/history).
        /// </summary>
        public static ActionResponse Post(BimRequest request)
        {
            string url       = $"{BaseUrl}/api/v1/generate-action";
            byte[] bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method        = "POST";
            req.ContentType   = "application/json";
            req.ContentLength = bodyBytes.Length;
            req.Timeout       = TimeoutMs;
            req.ReadWriteTimeout = TimeoutMs;
            string token = Environment.GetEnvironmentVariable("BIM_BACKEND_API_KEY");
            if (!string.IsNullOrWhiteSpace(token))
                req.Headers[HttpRequestHeader.Authorization] = "Bearer " + token;

            try
            {
                using (Stream s = req.GetRequestStream())
                    s.Write(bodyBytes, 0, bodyBytes.Length);
            }
            catch (WebException ex)
            {
                throw new Exception($"Cannot reach backend at {BaseUrl}.\n{ex.Message}");
            }

            string responseBody;
            HttpStatusCode statusCode;
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (StreamReader r = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                {
                    statusCode   = resp.StatusCode;
                    responseBody = r.ReadToEnd();
                }
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.Timeout)
            {
                throw new Exception("Request timed out after 180 seconds. Check the backend and model provider.");
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse errResp)
            {
                using (StreamReader r = new StreamReader(errResp.GetResponseStream(), Encoding.UTF8))
                {
                    string body = r.ReadToEnd();
                    if ((int)errResp.StatusCode == 422)
                        throw new Exception($"Invalid instruction (HTTP 422):\n{body}");
                    throw new Exception($"Backend returned HTTP {(int)errResp.StatusCode}:\n{body}");
                }
            }

            try
            {
                return JsonConvert.DeserializeObject<ActionResponse>(responseBody);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Could not parse backend response:\n{ex.Message}\n\nRaw:\n{responseBody}");
            }
        }
    }
}
