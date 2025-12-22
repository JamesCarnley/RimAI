using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;

namespace RimAI
{
    public class WildlifeProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            if (dto.WildAnimals == null) dto.WildAnimals = new List<string>();

            // Scrape wild animals (Fauna)
            var wildAnimals = map.mapPawns.AllPawns
                .Where(p => p.Faction == null && p.RaceProps.Animal && !p.Dead && !p.Downed && !p.Position.Fogged(map))
                .ToList();

            if (wildAnimals.Any())
            {
                var grouped = wildAnimals.GroupBy(p => p.KindLabel).Select(g => $"{g.Count()}x {g.Key}");
                dto.WildAnimals.AddRange(grouped);
            }
        }
    }
}
