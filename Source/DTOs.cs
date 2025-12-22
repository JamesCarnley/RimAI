using System.Collections.Generic;

namespace RimAI
{
    public class ColonyStateDTO
    {
        public string Date;
        public string Weather;
        public int Population;
        public float Wealth;
        public PowerDTO Power;
        public ResearchDTO Research;
        
        public List<ColonistDTO> Colonists = new List<ColonistDTO>();
        public Dictionary<string, string> Resources = new Dictionary<string, string>();
        public List<string> RecentEvents = new List<string>();
        public List<string> ActiveThreats = new List<string>();
        public List<QuestDTO> ActiveQuests = new List<QuestDTO>();
    }

    public class ColonistDTO
    {
        public string Name;
        public string Job;
        public string Mood; // "Happy", "Broken", etc.
        public List<string> Skills = new List<string>();
        public List<string> Traits = new List<string>();
        public List<string> Thoughts = new List<string>();
        public List<string> Health = new List<string>();
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
}
