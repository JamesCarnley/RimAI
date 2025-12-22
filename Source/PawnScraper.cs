using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using System.Reflection;

namespace RimAI
{
    public static class PawnScraper
    {
        public static ColonistDTO Scrape(Pawn pawn)
        {
            var dto = new ColonistDTO();
            dto.Name = pawn.Name?.ToStringShort ?? pawn.LabelShort;
            dto.Job = pawn.CurJobDef?.label ?? "Idle";

            // Basic Properties
            dto.Race = pawn.def.label;
            
            // Inspect String
            try { dto.InspectString = pawn.GetInspectString(); } catch {}

            // Safe Reflection for Pawn Properties
            ScrapeProperties(pawn, dto.RawProperties);
            
            // Safe Reflection for Def Properties
            ScrapeProperties(pawn.def, dto.DefProperties);

            // Stat Bases
            if (pawn.def.statBases != null)
            {
                foreach (var stat in pawn.def.statBases)
                {
                    try { dto.StatBases[stat.stat.label] = stat.value.ToString(); } catch {}
                }
            }

            // Gear & Inventory
            if (pawn.equipment != null)
            {
                foreach (var eq in pawn.equipment.AllEquipmentListForReading)
                {
                    string quality = "";
                    if (eq.TryGetQuality(out var qc)) quality = $" ({qc.ToString()})";
                    dto.Equipment.Add($"{eq.LabelCap}{quality}");
                }
            }
            if (pawn.apparel != null)
            {
                foreach (var ap in pawn.apparel.WornApparel)
                {
                    string quality = "";
                    if (ap.TryGetQuality(out var qc)) quality = $" ({qc.ToString()})";
                    string hp = ap.HitPoints < ap.MaxHitPoints ? $" {ap.HitPoints}/{ap.MaxHitPoints}" : "";
                    dto.Apparel.Add($"{ap.LabelCap}{quality}{hp}");
                }
            }

            // Skills & Traits (Humanlike/Mech)
            if (pawn.story != null && pawn.story.traits != null)
            {
                foreach (var trait in pawn.story.traits.allTraits) dto.Traits.Add(trait.Label);
            }
            if (pawn.skills != null)
            {
                foreach (var skill in pawn.skills.skills)
                {
                    if (!skill.TotallyDisabled && skill.Level > 0) dto.Skills.Add($"{skill.def.label} ({skill.Level})");
                }
            }

            // Health
            if (pawn.health != null && pawn.health.hediffSet != null)
            {
                foreach (var h in pawn.health.hediffSet.hediffs)
                {
                    if (h.Visible) dto.Health.Add(h.LabelCap);
                }
            }

            // Needs (just a few key ones)
            if (pawn.needs != null)
            {
                if (pawn.needs.mood != null) dto.Mood = pawn.needs.mood.CurLevel.ToString("P0");
                if (pawn.needs.food != null) dto.RawProperties["FoodLevel"] = pawn.needs.food.CurLevel.ToString("P0");
                if (pawn.needs.rest != null) dto.RawProperties["RestLevel"] = pawn.needs.rest.CurLevel.ToString("P0");
            }

            // DLCs
            if (ModsConfig.BiotechActive && pawn.genes != null) dto.Xenotype = pawn.genes.XenotypeLabelCap;
            if (ModsConfig.RoyaltyActive && pawn.royalty != null)
            {
                var t = pawn.royalty.MostSeniorTitle;
                if (t != null) dto.Title = t.Label;
            }
            if (ModsConfig.IdeologyActive && pawn.Ideo != null) dto.Ideology = pawn.Ideo.name;

            // Abilities
            if (pawn.abilities != null)
            {
                foreach (var ab in pawn.abilities.abilities) dto.Abilities.Add(ab.def.LabelCap);
            }

            // Relations (Overseer/Master)
            if (pawn.relations != null)
            {
                var overseer = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Overseer);
                if (overseer != null) dto.RawProperties["Overseer"] = overseer.Name.ToStringShort;
                
                var master = pawn.playerSettings?.Master;
                if (master != null) dto.RawProperties["Master"] = master.Name.ToStringShort;
            }

            return dto;
        }

        private static void ScrapeProperties(object obj, Dictionary<string, string> target)
        {
            if (obj == null) return;
            try
            {
                var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    if (prop.CanRead && (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string) || prop.PropertyType.IsEnum))
                    {
                        try 
                        { 
                            var val = prop.GetValue(obj);
                            if (val != null) target[prop.Name] = val.ToString();
                        } catch {}
                    }
                }
            }
            catch {}
        }
    }
}
