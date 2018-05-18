using System;
using System.Collections.Generic;
using System.Text;

namespace PathTracker_Backend
{
    public class CharacterProgress
    {
        public List<Item> EquippedItems = new List<Item>();
        
        public string Name { get; set; }

        public string League { get; set; }

        public int AscendencyClass { get; set; }

        public Dictionary<int, double> LevelProgress = new Dictionary<int, double>();

        public long ExperienceProgress { get; set; }

        public long TotalExperienceNonPenalized { get; set; }

    }
}
