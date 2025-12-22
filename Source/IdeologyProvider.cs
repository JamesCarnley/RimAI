using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;

namespace RimAI
{
    public class IdeologyProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            if (!ModsConfig.IdeologyActive) return;

            if (!ModsConfig.IdeologyActive) return;

            Ideo ideo = null;
            if (Faction.OfPlayer.ideos != null)
            {
                ideo = Faction.OfPlayer.ideos.PrimaryIdeo;
            }
            
            if (ideo == null)
            {
                // Fallback: Find ideo with most colonists
                 ideo = Find.IdeoManager.IdeosListForReading
                    .OrderByDescending(i => map.mapPawns.FreeColonists.Count(p => p.Ideo == i))
                    .FirstOrDefault();
            }
            
            if (ideo != null)
            {
                var iDto = new IdeologyDTO
                {
                    Name = ideo.name
                };

                // Memes
                foreach (var meme in ideo.memes)
                {
                    iDto.Memes.Add(meme.LabelCap);
                }

                // Precepts (Roles, Rituals, etc are precepts)
                // We want to separate interesting precepts.
                // Or just dump them all?
                
                // Roles
                foreach (var precept in ideo.PreceptsListForReading)
                {
                     if (precept is Precept_Role role)
                     {
                         string assignee = role.ChosenPawnSingle()?.Name.ToStringShort ?? "Unassigned";
                         iDto.Roles.Add($"{role.LabelCap}: {assignee}");
                     }
                     else
                     {
                         // Optional: Filter common/boring precepts?
                         // Let's just grab High impact ones or all distinct labels.
                         if (precept.def.visible)
                         {
                             iDto.Precepts.Add(precept.LabelCap);
                         }
                     }
                }
                
                // Deduplicate precepts
                iDto.Precepts = iDto.Precepts.Distinct().ToList();

                dto.Ideology = iDto;
            }
        }
    }
}
