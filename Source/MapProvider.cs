using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;

namespace RimAI
{
    public class MapProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            dto.Map = new MapDTO
            {
                Biome = map.Biome.LabelCap,
                Season = GenLocalDate.Season(map).LabelCap(),
                Temperature = map.mapTemperature.OutdoorTemp,
                Size = $"{map.Size.x}x{map.Size.z}",
                Pollution = ModsConfig.BiotechActive ? map.pollutionGrid.TotalPollutionPercent : 0f
            };

            // Roof/Mountain Analysis
            int thickRoof = 0;
            int mountain = 0;
            foreach (var cell in map.AllCells)
            {
                if (map.roofGrid.RoofAt(cell)?.isThickRoof ?? false) thickRoof++;
                if (cell.GetTerrain(map).affordances.Contains(TerrainAffordanceDefOf.Heavy)) mountain++;
            }
            dto.Map.RoofInfo = $"Thick Roof: {thickRoof} ({thickRoof * 100f / map.AllCells.Count():F1}%)";
        }
    }
}
