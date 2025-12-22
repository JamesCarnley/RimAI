using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;

namespace RimAI
{
    public static class DataScraper
    {
        public static ColonyStateDTO Scrape()
        {
            var dto = new ColonyStateDTO();
            var map = Find.CurrentMap;

            // Date and Weather
            dto.Date = GenDate.DateFullStringAt(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(map.Tile));
            dto.Weather = map.weatherManager.curWeather.label;
            
            // Wealth
            dto.Wealth = map.wealthWatcher.WealthTotal;
            
            // Colonists
            foreach (var pawn in map.mapPawns.FreeColonists)
            {
                var pDto = new ColonistDTO();
                pDto.Name = pawn.Name.ToStringShort;
                pDto.Job = pawn.CurJobDef?.label ?? "Idle";
                
                // Mood
                if (pawn.needs != null && pawn.needs.mood != null)
                {
                    pDto.Mood = pawn.needs.mood.CurLevel.ToString("P0");
                    foreach (var memory in pawn.needs.mood.thoughts.memories.Memories)
                    {
                        // Filter for significant thoughts only to save tokens? 
                        // For now, take recent or strong ones. 
                        if (memory.MoodOffset() != 0)
                        {
                            pDto.Thoughts.Add($"{memory.LabelCap}: {memory.MoodOffset()}");
                        }
                    }
                }

                // Health
                if (pawn.health != null && pawn.health.hediffSet != null)
                {
                    foreach (var h in pawn.health.hediffSet.hediffs)
                    {
                        if (h.Visible && (h.def.isBad || h.def.makesSickThought))
                        {
                            pDto.Health.Add(h.LabelCap);
                        }
                    }
                }

                dto.Colonists.Add(pDto);
            }
            dto.Population = dto.Colonists.Count;

            // Resources
            // Simple grouping
            var counter = map.resourceCounter;
            dto.Resources["Food"] = counter.TotalHumanEdibleNutrition.ToString("F0") + " Nutrition";
            dto.Resources["Medicine"] = (counter.GetCount(ThingDefOf.MedicineHerbal) + counter.GetCount(ThingDefOf.MedicineIndustrial)).ToString();
            dto.Resources["Silver"] = counter.GetCount(ThingDefOf.Silver).ToString();
            dto.Resources["Components"] = (counter.GetCount(ThingDefOf.ComponentIndustrial) + counter.GetCount(ThingDefOf.ComponentSpacer)).ToString();

            // Events (Letters)
            // Accessing the archive is tricky as it stores IArchivable. 
            // We'll look at the LetterStack's recent history if possible, or just the Archive.
            // Find.Archive.Archivables is a list.
            var recentLetters = Find.Archive.ArchivablesListForReading
                .OrderByDescending(x => x.CreatedTicksGame)
                .Where(x => x is Letter)
                .Cast<Letter>()
                .Take(10);
            
            foreach (var letter in recentLetters)
            {
                dto.RecentEvents.Add(letter.Label);
            }

            // Threats
            var enemies = map.mapPawns.AllPawns.Where(p => p.HostileTo(Faction.OfPlayer) && !p.Downed && !p.Dead);
            if (enemies.Any())
            {
                var groups = enemies.GroupBy(p => p.KindLabel).Select(g => $"{g.Count()}x {g.Key}");
                dto.ActiveThreats.AddRange(groups);
            }

            return dto;
        }
    }
}
