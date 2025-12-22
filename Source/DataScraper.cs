using System.Collections.Generic;
using RimWorld;
using Verse;
using System.Linq;

namespace RimAI
{
    public static class DataScraper
    {
        private static List<IContextProvider> providers = new List<IContextProvider>();

        static DataScraper()
        {
            // Register default providers
            RegisterProvider(new GeneralStateProvider());
            RegisterProvider(new ColonistProvider());
            RegisterProvider(new ResourceProvider());
            RegisterProvider(new ResearchProvider());
            RegisterProvider(new PowerProvider());
            RegisterProvider(new EventProvider());
            RegisterProvider(new ThreatProvider());
            RegisterProvider(new RoomProvider());
            RegisterProvider(new AnimalProvider());
            RegisterProvider(new MechanoidProvider());
            RegisterProvider(new MapProvider());
            providers.Add(new ZoneProvider());
            providers.Add(new FactionProvider());
            providers.Add(new WildlifeProvider());
            providers.Add(new IdeologyProvider());
            providers.Add(new BillProvider());
        }

        public static void RegisterProvider(IContextProvider provider)
        {
            if (!providers.Contains(provider))
            {
                providers.Add(provider);
            }
        }

        public static ColonyStateDTO Scrape()
        {
            var dto = new ColonyStateDTO();
            var map = Find.CurrentMap;

            if (map == null) return dto;

            foreach (var provider in providers)
            {
                try
                {
                    provider.Populate(dto, map);
                }
                catch (System.Exception ex)
                {
                    Log.Error($"[RimAI] Provider {provider.GetType().Name} failed: {ex}");
                }
            }

            return dto;
        }
    }
}
