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
            
            // REMOVED: DefProperties (Too verbose/token heavy)
            // ScrapeProperties(pawn.def, dto.DefProperties);

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
                    if (!skill.TotallyDisabled)
                    {
                        string passion = "";
                        if (skill.passion == Passion.Minor) passion = " (Minor)";
                        else if (skill.passion == Passion.Major) passion = " (Major)";
                        
                        if (skill.Level > 0 || skill.passion != Passion.None)
                        {
                            dto.Skills.Add($"{skill.def.label} {skill.Level}{passion}");
                        }
                    }
                }
            }
            
            // Social
            if (pawn.relations != null)
            {
                 foreach(var rel in pawn.relations.DirectRelations)
                 {
                     string opinion = "";
                     try { opinion = $" (Opinion: {pawn.relations.OpinionOf(rel.otherPawn)})"; } catch {}
                     dto.Social.Add($"{rel.def.LabelCap}: {rel.otherPawn.Name.ToStringShort ?? rel.otherPawn.LabelShort}{opinion}");
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
                
                // Thoughts
                if (pawn.needs.mood != null && pawn.needs.mood.thoughts != null)
                {
                    List<Thought> thoughts = new List<Thought>();
                    pawn.needs.mood.thoughts.GetAllMoodThoughts(thoughts);
                    foreach(var t in thoughts)
                    {
                        dto.Thoughts.Add($"{t.LabelCap} ({t.MoodOffset():F0})");
                    }
                }
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

            if (pawn.relations != null)
            {
                var overseer = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Overseer);
                if (overseer != null) dto.RawProperties["Overseer"] = overseer.Name.ToStringShort;
                
                var master = pawn.playerSettings?.Master;
                if (master != null) dto.RawProperties["Master"] = master.Name.ToStringShort;
            }

            // Management (Work, Schedule, Assignments)
            // Only for colonists
            if (pawn.IsColonist)
            {
                var m = new WorkScheduleDTO();
                
                // Work Priorities
                if (pawn.workSettings != null)
                {
                    // Iterate all work types
                    foreach (var w in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                    {
                        if (pawn.workSettings.WorkIsActive(w))
                        {
                            m.WorkPriorities[w.labelShort] = pawn.workSettings.GetPriority(w);
                        }
                    }
                }

                // Schedule
                if (pawn.timetable != null)
                {
                    // Create a compact string of 24h assignments
                    // U = Undefined / Anything, W = Work, R = Joy, S = Sleep, M = Meditation
                    var chars = new char[24];
                    for (int i = 0; i < 24; i++)
                    {
                        var type = pawn.timetable.GetAssignment(i);
                        if (type == TimeAssignmentDefOf.Anything) chars[i] = 'U';
                        else if (type == TimeAssignmentDefOf.Work) chars[i] = 'W';
                        else if (type == TimeAssignmentDefOf.Joy) chars[i] = 'R';
                        else if (type == TimeAssignmentDefOf.Sleep) chars[i] = 'S';
                        else if (type == TimeAssignmentDefOf.Meditate) chars[i] = 'M';
                        else chars[i] = '?';
                    }
                    m.Schedule = new string(chars);
                }

                // Assignments
                if (pawn.outfits != null) m.CurrentAssignment_Outfit = pawn.outfits.CurrentApparelPolicy?.label;
                if (pawn.foodRestriction != null) m.CurrentAssignment_Food = pawn.foodRestriction.CurrentFoodPolicy?.label;
                if (pawn.drugs != null) m.CurrentAssignment_Drugs = pawn.drugs.CurrentPolicy?.label;
                // m.CurrentAssignment_Reading // 1.5 only

                if (pawn.playerSettings != null)
                {
                    // Use reflection to find the area restriction property as it changes names between versions
                    var psType = pawn.playerSettings.GetType();
                    var areaProp = psType.GetProperty("AreaRestriction") 
                                ?? psType.GetProperty("AreaRestrictionInPawnCurrentMap") 
                                ?? psType.GetProperty("EffectiveAreaRestriction");
                    
                    if (areaProp != null)
                    {
                        var area = areaProp.GetValue(pawn.playerSettings) as Area;
                        m.Area = area?.Label;
                    }

                    // Also scrape other settings
                    ScrapeProperties(pawn.playerSettings, dto.RawProperties);
                }

                dto.Management = m;
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
                    // Filter out verbose descriptions
                    if (prop.Name.Contains("Description")) continue;

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
