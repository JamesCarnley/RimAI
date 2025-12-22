using System.Collections.Generic;

namespace RimAI
{
    public class ColonyStateDTO
    {
        public string Date;
        public string ModVersion;
        public string Weather;
        public MapDTO Map;
        public int Population;
        public float Wealth;
        public PowerDTO Power;
        public ResearchDTO Research;
        
        public List<ColonistDTO> Colonists = new List<ColonistDTO>();
        public Dictionary<string, string> Resources = new Dictionary<string, string>();
        public List<string> RecentEvents = new List<string>();
        public List<string> ActiveThreats = new List<string>();
        public List<QuestDTO> ActiveQuests = new List<QuestDTO>();
        public List<RoomDTO> Rooms = new List<RoomDTO>();
        public List<ColonistDTO> Animals = new List<ColonistDTO>();
        public List<ColonistDTO> Mechanoids = new List<ColonistDTO>();
    }

    public class MapDTO
    {
        public string Biome;
        public string Season;
        public float Temperature; // Celsius
        public string Size;
        public float Pollution; // 0-1
    }

    public class ColonistDTO
    {
        public string Name;
        public string Job;
        public string Mood; // "Happy", "Broken", etc.
        public string Race;
        public string Xenotype;
        public string Title; // Royalty title
        public string Ideology; // Ideoligion name
        
        public string InspectString;
        public Dictionary<string, string> RawProperties = new Dictionary<string, string>();
        public Dictionary<string, string> DefProperties = new Dictionary<string, string>();
        public Dictionary<string, string> StatBases = new Dictionary<string, string>();

        public List<string> Skills = new List<string>();
        public List<string> Traits = new List<string>();
        public List<string> Thoughts = new List<string>();
        public List<string> Health = new List<string>();
        public List<string> Abilities = new List<string>(); // Psycasts / Gene abilities
        public List<string> Equipment = new List<string>(); // Weapons
        public List<string> Apparel = new List<string>(); // Armor / Clothing
    }

    public class PowerDTO
    {
        public string GridStatus; // "Stable", "Draining", "Offline"
        public float Stored;
        public float Producing;
        public float Consuming;
    }

    public class ResearchDTO
    {
        public string CurrentProject;
        public float ProgressPercent;
    }

    public class QuestDTO
    {
        public string Label;
        public string State; // "Offer", "Active"
        public string DaysLeft;
    }

    public class RoomDTO
    {
        public string Role;
        public float Impressiveness;
        public float Wealth;
        public int CellCount;
        public List<string> NotableBuildings = new List<string>();
    }
}
