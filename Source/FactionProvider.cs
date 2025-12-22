using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;

namespace RimAI
{
    public class FactionProvider : IContextProvider
    {
        public void Populate(ColonyStateDTO dto, Map map)
        {
            if (dto.Factions == null) dto.Factions = new List<FactionDTO>();

            foreach (var faction in Find.FactionManager.AllFactionsVisible)
            {
                if (faction.IsPlayer || faction.Hidden) continue;

                var fDto = new FactionDTO
                {
                    Name = faction.Name,
                    Type = faction.def.LabelCap,
                    Goodwill = faction.GoodwillWith(Faction.OfPlayer),
                    RelationKind = faction.RelationKindWith(Faction.OfPlayer).GetLabel()
                };

                dto.Factions.Add(fDto);
            }
        }
    }
}
