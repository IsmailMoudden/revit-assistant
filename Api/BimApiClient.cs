using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using BimAiAssistant.Models;
using Newtonsoft.Json;

namespace BimAiAssistant.Api
{
    public static class BimApiClient
    {
        private const string BaseUrl   = "https://autodesk-revit-backend.up.railway.app";
        private const int    TimeoutMs = 30_000;

        /// <summary>
        /// First call: send instruction + optional Revit context.
        /// </summary>
        public static ActionResponse GenerateAction(string instruction, RevitContext context = null)
        {
            var payload = new { instruction, context };
            return Post(payload);
        }

        /// <summary>
        /// Follow-up call: send original instruction + user answers to clarification questions.
        /// </summary>
        public static ActionResponse SendAnswers(string instruction, Dictionary<string, object> answers)
        {
            var payload = new { instruction, answers };
            return Post(payload);
        }

        // ── shared HTTP POST ──────────────────────────────────────────────────

        private static ActionResponse Post(object payload)
        {
            string url       = $"{BaseUrl}/api/v1/generate-action";
            byte[] bodyBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method        = "POST";
            req.ContentType   = "application/json";
            req.ContentLength = bodyBytes.Length;
            req.Timeout       = TimeoutMs;

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
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (StreamReader r = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                {
                    responseBody = r.ReadToEnd();

                    if (resp.StatusCode != HttpStatusCode.OK)
                        throw new Exception($"Backend returned HTTP {(int)resp.StatusCode}:\n{responseBody}");
                }
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.Timeout)
            {
                throw new Exception("Request timed out after 30 seconds. Check that the backend is running.");
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse errResp)
            {
                using (StreamReader r = new StreamReader(errResp.GetResponseStream(), Encoding.UTF8))
                    throw new Exception($"Backend returned HTTP {(int)errResp.StatusCode}:\n{r.ReadToEnd()}");
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
