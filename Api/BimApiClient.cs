using System;
using System.IO;
using System.Net;
using System.Text;
using BimAiAssistant.Models;
using Newtonsoft.Json;

namespace BimAiAssistant.Api
{
    // Uses HttpWebRequest (always available in net48 — no extra assembly reference needed)
    public static class BimApiClient
    {
        private const string BaseUrl    = "https://autodesk-revit-backend.up.railway.app";
        private const int    TimeoutMs  = 30_000;

        public static ActionResponse GenerateAction(string instruction)
        {
            string url  = $"{BaseUrl}/api/v1/generate-action";
            string body = JsonConvert.SerializeObject(new { instruction });
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

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
                using (Stream s = resp.GetResponseStream())
                using (StreamReader r = new StreamReader(s, Encoding.UTF8))
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
                using (Stream s = errResp.GetResponseStream())
                using (StreamReader r = new StreamReader(s, Encoding.UTF8))
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
