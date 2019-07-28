using System.Collections.Generic;

namespace ffTTbb.Models
{
    public class CardInfo
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public int Rarity { get; set; }
        public string ImageUrl { get; set; }
        public string Patch { get; set; }
        public List<string> NPCs { get; set; }
        public List<string> Locations { get; set; }
    }
}
