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
            listingStandard.Label("Gemini API Key (Get one from aistudio.google.com):");
            settings.apiKey = listingStandard.TextEntry(settings.apiKey);
            
            if (listingStandard.ButtonText("Test Connection (Not Implemented)"))
            {
                // TODO: Add simple ping test
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
