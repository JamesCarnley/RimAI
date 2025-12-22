using System;
using System.Text;
using System.Collections;
using UnityEngine.Networking;
using Verse;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RimAI
{
    public class APIClient
    {
        // No INIT needed for UnityWebRequest usually, keeps things simple
        public static void Init() { }

        // Debug methods removed for release


        // HttpClient Pattern to bypass Unity Networking completely
        public static Action<string> PendingSuccessCallback;
        public static Action<string> PendingErrorCallback;
        
        // Results populated by background thread
        public static volatile string PendingResult;
        public static volatile string PendingErrorText;

        public static void SendWithHttpClient(string apiKey, string payload, Action<string> onSuccess, Action<string> onError)
        {
            Log.Message("[RimAI] Starting HttpClient Task...");

            PendingSuccessCallback = onSuccess;
            PendingErrorCallback = onError;
            PendingResult = null;
            PendingErrorText = null;

            Task.Run(() => 
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                         string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent?key={apiKey}";
                         
                         var content = new StringContent(payload, Encoding.UTF8, "application/json");
                         
                         // Post Synchronously
                         HttpResponseMessage response = client.PostAsync(url, content).GetAwaiter().GetResult();
                         
                         string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                         if (response.IsSuccessStatusCode)
                         {
                             // Parse using JObject (Statically typed)
                             JObject json = JObject.Parse(responseBody);
                             string text = json["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                             
                             if (text == null) throw new Exception("Invalid JSON structure");
                             
                             PendingResult = text;
                         }
                         else
                         {
                             PendingErrorText = $"HTTP {response.StatusCode}: {responseBody}";
                         }
                    }
                }
                catch (Exception ex)
                {
                    PendingErrorText = "Exception: " + ex.Message;
                }
            });
        }

    }
}
