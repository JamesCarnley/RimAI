using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;

namespace RimAI
{
    public class BillProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            // Iterate all buildings that can have bills
            // Using listerBuildings.allBuildingsColonist is efficient
            var billGivers = map.listerBuildings.allBuildingsColonist.OfType<IBillGiver>();

            foreach (var giver in billGivers)
            {
                var building = giver as Building;
                if (building == null) continue;

                foreach (var b in giver.BillStack.Bills)
                {
                    if (!(b is Bill_Production bill)) continue;

                    string label = bill.recipe.LabelCap;
                    string status = "";
                    
                    // Bill repeat mode (Do X times, Forever, etc.)
                   if (bill.repeatMode == BillRepeatModeDefOf.RepeatCount)
                    {
                        // bill.recipe.WorkAmountTotal(null) is not quite right for "Left", simplifying to just "Do X"
                         status = $"Do {bill.repeatCount}";
                    }
                    else if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
                    {
                         status = $"Do until you have {bill.targetCount}";
                         if (bill.includeEquipped) status += " (incl. equipped)";
                         if (bill.includeTainted) status += " (incl. tainted)";
                    }
                    else if (bill.repeatMode == BillRepeatModeDefOf.Forever)
                    {
                        status = "Do forever";
                    }

                    if (bill.suspended) status += " [Suspended]";

                    dto.ActiveBills.Add($"{building.LabelCap}: {label} - {status}");
                }
            }
        }
    }
}
