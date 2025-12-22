using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;

namespace RimAI
{
    public class ZoneProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            // We need a list to hold zones in the DTO? 
            // Wait, I forgot to add a list of zones to ColonyStateDTO! 
            // I added ZoneDTO class but didn't add the list to ColonyStateDTO.
            // I should assume I will add it or add it now.
            // Let's add it to DTOs.cs first or assuming I will.
            // Actually, I can't write this file until I update ColonyStateDTO.
            // But I can write this and then update DTOs.cs again.
            // The compiler would fail if I don't.
            // Let's just write this assuming the field exists, then go fix DTOs.cs.
            
            // Actually, better to maintain correctness.
            // I will implement the Logic here.
            
            if (dto.Zones == null) dto.Zones = new List<ZoneDTO>();

            foreach (var zone in map.zoneManager.AllZones)
            {
                var zDto = new ZoneDTO
                {
                    Label = zone.label,
                    CellCount = zone.Cells.Count
                };

                if (zone is Zone_Growing growZone)
                {
                    zDto.Type = "Growing";
                    zDto.Settings.Add($"Plant: {growZone.GetPlantDefToGrow().LabelCap}");
                    if (!growZone.allowSow) zDto.Settings.Add("Sowing Disabled");
                    
                    // Crop Growth Analysis
                    // Iterate cells to find plants
                    int plantCount = 0;
                    float totalGrowth = 0f;
                    int harvestable = 0;
                    
                    foreach (var cell in growZone.Cells)
                    {
                        var plant = map.thingGrid.ThingAt<Plant>(cell);
                        if (plant != null && plant.def == growZone.GetPlantDefToGrow())
                        {
                            plantCount++;
                            totalGrowth += plant.Growth;
                            if (plant.HarvestableNow) harvestable++;
                        }
                    }
                    
                    if (plantCount > 0)
                    {
                        zDto.Settings.Add($"Growth: {(totalGrowth / plantCount):P0} Avg ({plantCount} plants)");
                        if (harvestable > 0) zDto.Settings.Add($"Ready to Harvest: {harvestable}");
                    }
                    
                    // Soil Analysis
                    var soilStats = growZone.Cells.Select(c => map.fertilityGrid.FertilityAt(c))
                                                  .GroupBy(f => f)
                                                  .OrderByDescending(g => g.Count())
                                                  .FirstOrDefault();
                                                  
                    if (soilStats != null)
                    {
                         // Map fertility to text roughly
                         float fert = soilStats.Key;
                         string label = "Soil";
                         if (fert > 1.0f) label = "Rich Soil";
                         else if (fert < 1.0f && fert > 0f) label = "Poor Soil";
                         else if (fert == 0f) label = "Barren";
                         
                         zDto.SoilInfo = $"{label} ({fert:P0})";
                    }
                }
                else if (zone is Zone_Stockpile stockpile)
                {
                    zDto.Type = "Stockpile";
                    zDto.Settings.Add($"Priority: {stockpile.settings.Priority.Label()}");
                    // Maybe list a summary of allowed items? Too detailed.
                    // Just basic priority for now.
                }
                else
                {
                    zDto.Type = zone.GetType().Name;
                }

                dto.Zones.Add(zDto);
            }
        }
    }
}
