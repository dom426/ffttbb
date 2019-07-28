using System.Collections.Generic;

namespace ffTTbb.Models
{
    public class NpcInfo
    {
        public string NpcName { get; set; }
        public List<CardInfo> Cards { get; set; }
        public List<string> Rules { get; set; }
        public string Location { get; set; }
        public int WinAmount { get; set; }
        public string Patch { get; set; }

        public bool HasCard(string cardName)
        {
            if (Cards == null)
                return false;

            foreach (var card in Cards)
            {
                if (card.Name.ToLower() == cardName.ToLower())
                    return true;
            }

            return false;
        }
    }
}
