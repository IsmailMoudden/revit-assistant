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
        private const string BaseUrl   = "https://autodesk-revit-backend.up.railway.app";
        private const int    TimeoutMs = 30_000;

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
                throw new Exception("Request timed out after 30 seconds. Check that the backend is running.");
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse errResp)
            {
                using (StreamReader r = new StreamReader(errResp.GetResponseStream(), Encoding.UTF8))
                {
                    string body = r.ReadToEnd();
                    if (errResp.StatusCode == HttpStatusCode.UnprocessableEntity) // 422
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
