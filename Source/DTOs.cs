using System.Collections.Generic;

namespace RimAI
{
    public class ColonyStateDTO
    {
        public string Date;
        public string Weather;
        public int Population;
        public float Wealth;
        public List<ColonistDTO> Colonists = new List<ColonistDTO>();
        public Dictionary<string, string> Resources = new Dictionary<string, string>();
        public List<string> RecentEvents = new List<string>();
        public List<string> ActiveThreats = new List<string>();
    }

    public class ColonistDTO
    {
        public string Name;
        public string Mood; // "Happy", "Broken", etc.
        public List<string> Thoughts = new List<string>();
        public List<string> Health = new List<string>(); // "Flu (Major)", "Bleeding"
        public string Job;
    }
}
