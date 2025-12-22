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
            dto.ModVersion = "1.1.0 (Dev)";
            dto.Date = GenDate.DateFullStringAt(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(map.Tile));
            dto.Weather = map.weatherManager.curWeather.label;
            dto.Wealth = map.wealthWatcher.WealthTotal;
            if (Find.Storyteller != null)
            {
                if (Find.Storyteller.def != null) dto.Storyteller = Find.Storyteller.def.label;
                
                // Difficulty
                if (Find.Storyteller.difficultyDef != null) dto.Difficulty = Find.Storyteller.difficultyDef.label;
                else if (Find.Storyteller.difficulty != null) dto.Difficulty = "Custom/Unknown";
                
                // Fallback for nulls
                if (dto.Storyteller == null) dto.Storyteller = "Unknown";
                if (dto.Difficulty == null) dto.Difficulty = "Unknown";
            }
            
            dto.Population = map.mapPawns.FreeColonistsCount;
        }
    }

    public class ResourceProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            var counter = map.resourceCounter;
            dto.Resources["Food"] = counter.TotalHumanEdibleNutrition.ToString("F1") + " Nutrition";

            foreach (var def in counter.AllCountedAmounts.Keys)
            {
                int count = counter.GetCount(def);
                if (count > 0)
                {
                    dto.Resources[def.LabelCap] = count.ToString();
                }
            }
        }
    }

    public class ColonistProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            foreach (var pawn in map.mapPawns.FreeColonists)
            {
                dto.Colonists.Add(PawnScraper.Scrape(pawn));
            }
        }
    }

    public class AnimalProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
             var animals = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer)
                .Where(p => p.RaceProps.Animal);

            foreach (var pawn in animals)
            {
                dto.Animals.Add(PawnScraper.Scrape(pawn));
            }
        }
    }

    public class MechanoidProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
             var mechs = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer)
                .Where(p => p.RaceProps.IsMechanoid);

            foreach (var pawn in mechs)
            {
                dto.Mechanoids.Add(PawnScraper.Scrape(pawn));
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
            // We iterate PowerComp objects to determine production/consumption
            
            float stored = 0f;
            float producing = 0f;
            float consuming = 0f;

            foreach (var net in map.powerNetManager.AllNetsListForReading)
            {
                stored += net.CurrentStoredEnergy();
                foreach (var comp in net.powerComps)
                {
                     if (comp.PowerOutput > 0) producing += comp.PowerOutput;
                     else if (comp.PowerOutput < 0) consuming -= comp.PowerOutput; // Make positive
                }
            }
            
            dto.Power = new PowerDTO
            {
                Stored = stored,
                Producing = producing, 
                Consuming = consuming,
                GridStatus = (producing > consuming) ? "Gaining" : (producing < consuming ? "Draining" : "Stable")
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
            var enemies = map.mapPawns.AllPawns.Where(p => p.HostileTo(Faction.OfPlayer) && !p.Downed && !p.Dead && !p.Position.Fogged(map));
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

    public class RoomProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            if (map.regionGrid == null) return;

            List<Room> allRooms = null;
            try
            {
                // Access allRooms via reflection as it might be internal/protected in some versions
                var field = typeof(RegionGrid).GetField("allRooms", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                {
                    allRooms = field.GetValue(map.regionGrid) as List<Room>;
                }
            }
            catch
            {
                // Fallback or silence
            }

            if (allRooms == null) return;

            foreach (var room in allRooms)
            {
                if (room.PsychologicallyOutdoors || room.IsHuge || room.CellCount < 6 || room.Role == RoomRoleDefOf.None) continue;
                
                // Check if room is hidden/fogged
                if (room.Cells.FirstOrDefault().Fogged(map)) continue;

                var rDto = new RoomDTO
                {
                    Role = room.Role.label,
                    Impressiveness = room.GetStat(RoomStatDefOf.Impressiveness),
                    Wealth = room.GetStat(RoomStatDefOf.Wealth),
                    CellCount = room.CellCount
                };

                // Notable buildings
                // We'll iterate the contained things. This can be heavy, so we limit it.
                // A better heuristic is checking the contained buildings that define the room role or are high value.
                
                var buildings = room.ContainedAndAdjacentThings.Where(t => t.def.building != null && (t.def.building.isSittable || (t.def.recipes != null && t.def.recipes.Count > 0) || t.def.building.bed_maxBodySize > 0));
                
                // Just group by count
                var grouped = buildings.GroupBy(b => b.LabelCap).Select(g => $"{g.Count()}x {g.Key}");
                rDto.NotableBuildings.AddRange(grouped);

                dto.Rooms.Add(rDto);
            }
            
            // Sort by impressiveness to prioritize important rooms
            dto.Rooms.Sort((a, b) => b.Impressiveness.CompareTo(a.Impressiveness));
        }
    }
}
