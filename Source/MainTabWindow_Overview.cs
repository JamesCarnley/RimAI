using RimWorld;
using UnityEngine;
using Verse;
using System.Collections;
using Newtonsoft.Json;

namespace RimAI
{
    public class MainTabWindow_Overview : MainTabWindow
    {
        private bool isLoading = false;
        private string aiResult = "";
        private Vector2 scrollPosition = Vector2.zero;

        public override Vector2 RequestedTabSize => new Vector2(600f, 700f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0, 0, inRect.width, 35), "Colony Overview");
            Text.Font = GameFont.Small;

            float y = 45f;

            Rect btnRect = new Rect(0, y, 200, 40);
            if (Widgets.ButtonText(btnRect, "Analyze Colony"))
            {
                if (!isLoading)
                {
                    GenerateOverview();
                }
            }

            y += 50f;

            // Debug Section Removed

            
            Rect contentRect = new Rect(0, y, inRect.width, inRect.height - y);
            
            if (isLoading)
            {
                Widgets.Label(contentRect, "Contacting AI Satellite (via UnityWebRequest)...");
            }
            else if (!aiResult.NullOrEmpty())
            {
                float height = Text.CalcHeight(aiResult, contentRect.width - 16f);
                Rect viewRect = new Rect(0, 0, contentRect.width - 16f, height);
                
                Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);
                Widgets.Label(viewRect, aiResult);
                Widgets.EndScrollView();
            }
            else
            {
                Widgets.Label(contentRect, "Press 'Analyze Colony' to generate a report via Google Gemini.");
            }
        }
        
        public void GenerateOverview()
        {
            isLoading = true;
            aiResult = "";
            
            string apiKey = RimAIMod.settings.apiKey;
            
            try
            {
                // 1. Scrape Data
                var data = DataScraper.Scrape();

                // 2. Serialize Data (Safe string for Coroutine)
                string jsonState = JsonConvert.SerializeObject(data);
                
                // 3. Construct Payload (Main Thread)
                string prompt = $"Analyze this RimWorld colony state and provide a succinct, narrative overview (2 paragraphs max) and 3 bullet point recommendations. State:\n{jsonState}";
                var payload = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
                string finalJson = JsonConvert.SerializeObject(payload);
                Log.Message($"[RimAI] Payload constructed. Size: {finalJson.Length} chars");

                // 4. Start Request via HttpClient
                APIClient.SendWithHttpClient(apiKey, finalJson,
                    onSuccess: (result) => 
                    {
                        aiResult = result;
                        isLoading = false;
                        Log.Message("[RimAI] Success:\n" + result);
                    },
                    onError: (error) =>
                    {
                        aiResult = error;
                        isLoading = false;
                        Log.Error("[RimAI] Error: " + error);
                    }
                );
            }
            catch (System.Exception ex)
            {
                Log.Error($"[RimAI] Generation Error: {ex}");
                aiResult = $"Error: {ex.Message}";
                isLoading = false;
            }
        }
    }
}
