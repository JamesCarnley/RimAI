using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RimAI
{
    public interface IContextProvider
    {
        void Populate(ColonyStateDTO dto, Map map);
    }

    public class GeneralStateProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            dto.Date = GenDate.DateFullStringAt(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(map.Tile));
            dto.Weather = map.weatherManager.curWeather.label;
            dto.Wealth = map.wealthWatcher.WealthTotal;
            dto.Population = map.mapPawns.FreeColonistsCount;
        }
    }

    public class ResourceProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            var counter = map.resourceCounter;
            dto.Resources["Food"] = counter.TotalHumanEdibleNutrition.ToString("F0") + " Nutrition";
            dto.Resources["Medicine"] = (counter.GetCount(ThingDefOf.MedicineHerbal) + counter.GetCount(ThingDefOf.MedicineIndustrial)).ToString();
            dto.Resources["Silver"] = counter.GetCount(ThingDefOf.Silver).ToString();
            dto.Resources["Components"] = (counter.GetCount(ThingDefOf.ComponentIndustrial) + counter.GetCount(ThingDefOf.ComponentSpacer)).ToString();
            dto.Resources["Steel"] = counter.GetCount(ThingDefOf.Steel).ToString();
            dto.Resources["Wood"] = counter.GetCount(ThingDefOf.WoodLog).ToString();
        }
    }

    public class ColonistProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            foreach (var pawn in map.mapPawns.FreeColonists)
            {
                var pDto = new ColonistDTO();
                pDto.Name = pawn.Name.ToStringShort;
                pDto.Job = pawn.CurJobDef?.label ?? "Idle";

                // Traits
                if (pawn.story != null && pawn.story.traits != null)
                {
                    foreach (var trait in pawn.story.traits.allTraits)
                    {
                        pDto.Traits.Add(trait.Label);
                    }
                }

                // Skills (Notable ones > 8)
                if (pawn.skills != null)
                {
                    foreach (var skill in pawn.skills.skills)
                    {
                        if (!skill.TotallyDisabled && skill.Level > 8)
                        {
                            pDto.Skills.Add($"{skill.def.label} ({skill.Level})");
                        }
                    }
                }

                // Mood
                if (pawn.needs != null && pawn.needs.mood != null)
                {
                    pDto.Mood = pawn.needs.mood.CurLevel.ToString("P0");
                    foreach (var memory in pawn.needs.mood.thoughts.memories.Memories)
                    {
                        if (memory.MoodOffset() != 0) // Capture everything significant
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
        }
    }

    public class ResearchProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            dto.Research = new ResearchDTO();
            var proj = Find.ResearchManager.GetProject();
            if (proj != null)
            {
                dto.Research.CurrentProject = proj.label;
                dto.Research.ProgressPercent = Find.ResearchManager.GetProgress(proj) / proj.baseCost;
            }
            else
            {
                dto.Research.CurrentProject = "None";
            }
        }
    }

    public class PowerProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            // Simple power net analysis
            // We just look at the first connected battery or generator we find to assume the main grid state
            // This is a simplification; colonies can have multiple grids.
            // We will sum up all batteries and production on the map for a rough estimate.
            
            float stored = 0f;
            float producing = 0f;
            // float consuming = 0f;

            // This is computationally expensive if we iterate everything.
            // Using PowerNetManager is better.
            
            foreach (var net in map.powerNetManager.AllNetsListForReading)
            {
                stored += net.CurrentStoredEnergy();
                producing += net.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick; // Raw watts roughly
            }
            
            // Simplified: Just report stored and total gain rate
            dto.Power = new PowerDTO
            {
                Stored = stored,
                Producing = producing, // This is actually Gain Rate (Producing - Consuming)
                GridStatus = producing > 0 ? "Gaining" : (producing < 0 ? "Draining" : "Stable")
            };
        }
    }

    public class EventProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            // Letters
            var recentLetters = Find.Archive.ArchivablesListForReading
                .OrderByDescending(x => x.CreatedTicksGame)
                .Where(x => x is Letter)
                .Cast<Letter>()
                .Take(10);
            
            foreach (var letter in recentLetters)
            {
                dto.RecentEvents.Add(letter.Label);
            }

            // Quests
            var quests = Find.QuestManager.QuestsListForReading
                .Where(q => q.State == QuestState.Ongoing || q.State == QuestState.NotYetAccepted);
            
            foreach (var q in quests)
            {
                // string days = (q.ticksUntilAcceptanceExpiry > 0) ? (q.ticksUntilAcceptanceExpiry / 60000f).ToString("F1") + "d" : "N/A";
                dto.ActiveQuests.Add(new QuestDTO
                {
                    Label = q.name,
                    State = q.State.ToString(),
                    DaysLeft = "Unknown" // q.ticksUntilAcceptanceExpiry is not accessible or correct field name
                });
            }
        }
    }

    public class ThreatProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            var enemies = map.mapPawns.AllPawns.Where(p => p.HostileTo(Faction.OfPlayer) && !p.Downed && !p.Dead);
            if (enemies.Any())
            {
                var groups = enemies.GroupBy(p => p.KindLabel).Select(g => $"{g.Count()}x {g.Key}");
                dto.ActiveThreats.AddRange(groups);
            }
            
            // Game conditions (e.g. fallout)
            foreach(var cond in map.gameConditionManager.ActiveConditions)
            {
                 dto.ActiveThreats.Add($"Condition: {cond.LabelCap}");
            }
        }
    }
}
