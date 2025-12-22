using System.Collections.Generic;
using RimWorld;
using Verse;

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
        }
    }
}
