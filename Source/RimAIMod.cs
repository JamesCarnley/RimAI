using UnityEngine;
using Verse;

namespace RimAI
{
    public class RimAISettings : ModSettings
    {
        public string apiKey = "";

        public override void ExposeData()
        {
            Scribe_Values.Look(ref apiKey, "apiKey", "");
            base.ExposeData();
        }
    }

    public class RimAIMod : Mod
    {
        public static RimAISettings settings;
        private string connectionStatus = "";
        private string fullError;
        private UnityEngine.Color statusColor = UnityEngine.Color.white;

        public RimAIMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<RimAISettings>();
            APIClient.Init();
            RimAILifecycle.EnsureCreated();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Gemini API Key:");
            
            if (listingStandard.ButtonText("Get Free API Key (aistudio.google.com)"))
            {
                Application.OpenURL("https://aistudio.google.com/app/apikey");
            }
            listingStandard.Label("The Gemini API has a generous free tier for personal use.");
            listingStandard.Gap(6f);
            settings.apiKey = listingStandard.TextEntry(settings.apiKey);
            
            if (listingStandard.ButtonText("Test Connection"))
            {
                if (string.IsNullOrEmpty(settings.apiKey))
                {
                    connectionStatus = "Error: API Key is empty.";
                    statusColor = Color.red;
                    fullError = null;
                }
                else
                {
                    connectionStatus = "Testing connection...";
                    statusColor = Color.yellow;
                    fullError = null;
                    
                    var payload = new { 
                        contents = new[] { 
                            new { 
                                role = "user", 
                                parts = new[] { new { text = "Hello" } } 
                            } 
                        } 
                    };
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                    
                    APIClient.SendWithHttpClient(settings.apiKey, json,
                        onSuccess: (result) => {
                            connectionStatus = "Success: API Connected!";
                            statusColor = Color.green;
                            fullError = null;
                        },
                        onError: (error) => {
                            connectionStatus = "Connection Failed. Click for details.";
                            statusColor = Color.red;
                            fullError = error;
                        }
                    );
                }
            }
            
            if (!string.IsNullOrEmpty(connectionStatus))
            {
                GUI.color = statusColor;
                listingStandard.Label(connectionStatus);
                GUI.color = Color.white;
                
                if (!string.IsNullOrEmpty(fullError))
                {
                    if (listingStandard.ButtonText("View Error Details"))
                    {
                        Find.WindowStack.Add(new Dialog_DebugInfo(fullError));
                    }
                }
            }
            
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "RimAI";
        }
    }
}
