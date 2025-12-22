using Verse;
using RimWorld;
using LudeonTK;
using System;

namespace RimAI
{
    public static class RimAIDebugTools
    {
        // Using named arguments to avoid positional mismatch errors
        [DebugAction("RimAI", "Open Overview Tab", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TriggerOverview()
        {
            MainButtonDef def = DefDatabase<MainButtonDef>.GetNamed("RimAI_Overview");
            if (def != null)
            {
                Find.MainTabsRoot.SetCurrentTab(def);
                Log.Message("[RimAI] Opened Overview Tab.");
            }
            else
            {
                Log.Error("[RimAI] MainButtonDef 'RimAI_Overview' not found.");
            }
        }

        [DebugAction("RimAI", "Check UI Status", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void CheckStatus()
        {
             MainButtonDef def = DefDatabase<MainButtonDef>.GetNamed("RimAI_Overview");
             if (def != null)
             {
                 Log.Message($"[RimAI] MainButtonDef found. Label: {def.label}");
             }
             else
             {
                 Log.Error("[RimAI] MainButtonDef MISSING.");
             }
        }
    }
}
