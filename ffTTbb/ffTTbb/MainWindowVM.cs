using ffTTbb.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ffTTbb
{
    public class MainWindowVM: INotifyPropertyChanged
    {
        CardInfo _selectedCard;
        public CardInfo SelectedCard
        {
            get
            {
                return _selectedCard;
            }
            set
            {
                _selectedCard = value;
                OnPropertyChanged(nameof(SelectedCard));
            }
        }

        string _cardNameSearch;
        public string CardNameSearch
        {
            get
            {
                return _cardNameSearch;
            }
            set
            {
                _cardNameSearch = value;
                OnPropertyChanged(nameof(CardNameSearch));
            }
        }

        string _npcNameSearch;
        public string NPCNameSearch
        {
            get
            {
                return _npcNameSearch;
            }
            set
            {
                _npcNameSearch = value;
                OnPropertyChanged(nameof(NPCNameSearch));
            }
        }

        string _patchSearch;
        public string PatchSearch
        {
            get
            {
                return _patchSearch;
            }
            set
            {
                _patchSearch = value;
                OnPropertyChanged(nameof(PatchSearch));
            }
        }

        List<CardInfo> _cards;
        ObservableCollection<CardInfo> _displayedCards;
        public ObservableCollection<CardInfo> DisplayedCards
        {
            get
            {
                return _displayedCards;
            }
            set
            {
                _displayedCards = value;
                OnPropertyChanged(nameof(DisplayedCards));
            }
        }

        public MainWindowVM()
        {
            _cards = DownloadCards();            
            DisplayedCards = new ObservableCollection<CardInfo>();
            foreach (var card in _cards)
            {
                card.LoadImages();
                DisplayedCards.Add(card);
            }

            if (_cards.Count > 0)
                SelectCard(_cards[0]);

            var npcs = DownloadNpcs();
        }

        void LoadCards()
        {
            var npcs = LoadFromFile<NpcInfo>("npcs.json");
            var cards = LoadFromFile<CardInfo>("cards.json");

            //_cards = new List<CardInfo>();
            //foreach (var card in cards)
            //{
            //    card.LoadImages();
            //    card.NPCs = npcs.Where(n => n.HasCard(card.Name)).ToList();
            //    _cards.Add(card);
            //}
        }

        public void SelectCard(CardInfo selectedCard)
        {
            foreach (var card in _cards)
            {
                card.IsSelected = false;
            }

            SelectedCard = selectedCard;
            SelectedCard.IsSelected = true;
        }

        public void DoSearch()
        {
            var displayedCards = _cards;

            if (!string.IsNullOrEmpty(CardNameSearch))
                displayedCards = displayedCards.Where(c => c.Name.ToLower().Contains(CardNameSearch.ToLower())).ToList();

            if (!string.IsNullOrEmpty(NPCNameSearch))
                displayedCards = displayedCards.Where(c => c.NPCs.FirstOrDefault(n => n.ToLower().Contains(NPCNameSearch.ToLower())) != null).ToList();

            if (!string.IsNullOrEmpty(PatchSearch))
                displayedCards = displayedCards.Where(c => c.Patch.Contains(PatchSearch)).ToList();

            DisplayedCards.Clear();
            foreach (var displayedCard in displayedCards)
            {
                DisplayedCards.Add(displayedCard);
            }
        }

        public List<CardInfo> DownloadCards()
        {
            var cards = new List<CardInfo>();

            var cardsHtml = DownloadWebpage("https://arrtripletriad.com/en/cards");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(cardsHtml);

            var cardsTable = doc.GetElementbyId("shortCardsListTable").ChildNodes["tbody"];
            foreach (var row in cardsTable.ChildNodes)
            {
                if (row.Name != "tr")
                    continue;

                var cells = row.ChildNodes.Where(n => n.Name == "td").ToList();

                // get Order value
                var orderStr = cells[0].InnerHtml;
                var order = int.Parse(orderStr);

                // get Image Url value
                var imageUrl = "";
                var imageUrlCell = cells[1].ChildNodes.FirstOrDefault(n => n.Name == "img");
                if (imageUrlCell != null)
                    imageUrl = imageUrlCell.Attributes["src"].Value;

                // get Rarity Image Url value
                var rarityImageUrl = "";
                var rarityImageUrlCell = cells[2].ChildNodes.FirstOrDefault(n => n.Name == "img");
                if (rarityImageUrlCell != null)
                    rarityImageUrl = rarityImageUrlCell.Attributes["src"].Value;
                var rarityStr = rarityImageUrl.Substring(rarityImageUrl.IndexOf(".png") - 1, 1);
                var rarity = int.Parse(rarityStr);

                // get Name value
                var name = "";
                var nameCell = cells[3].ChildNodes.FirstOrDefault(n => n.Name == "a");
                if (nameCell != null)
                    name = WebUtility.HtmlDecode(nameCell.InnerHtml);

                // get Locations (and NPCs)
                var npcs = new List<string>();
                var locations = new List<string>();
                var locationList = cells[4].ChildNodes.FirstOrDefault(n => n.Name == "ul");
                if (locationList != null)
                {
                    var locationCells = locationList.ChildNodes.Where(n => n.Name == "li").ToList();
                    foreach (var locationCell in locationCells)
                    {
                        if (locationCell.HasClass("aNpc"))
                        {
                            var npcCell = locationCell.ChildNodes.FirstOrDefault(n => n.Name == "a");
                            var npcName = npcCell.InnerHtml;
                            npcs.Add(npcName);

                            var location = locationCell.LastChild.InnerHtml;
                            if (location.StartsWith(", "))
                                location = location.Substring(2);
                            locations.Add(location);
                        }
                        else
                        {
                            locations.Add(locationCell.InnerHtml);
                        }
                    }
                }

                // get Patch value
                var patch = cells[5].InnerHtml;

                var card = new CardInfo()
                {
                    Order = order,
                    Patch = patch,
                    Rarity = rarity,
                    Name = name,
                    NPCs = npcs,
                    Locations = locations,
                    ImageUrl = imageUrl,
                };
                cards.Add(card);
            }

            return cards;
        }

        public List<NpcInfo> DownloadNpcs()
        {
            var npcs = new List<NpcInfo>();

            var cardsHtml = DownloadWebpage("https://arrtripletriad.com/en/npcs");

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(cardsHtml);

            var cardsTable = doc.GetElementbyId("npcTable").ChildNodes["tbody"];
            foreach (var row in cardsTable.ChildNodes)
            {
                if (row.Name != "tr")
                    continue;

                var cells = row.ChildNodes.Where(n => n.Name == "td").ToList();
                
                // get Name value
                var name = "";
                var nameCell = cells[0].ChildNodes.FirstOrDefault(n => n.Name == "a");
                if (nameCell != null)
                    name = nameCell.InnerHtml;

                // get Location
                var location = cells[2].InnerHtml;

                // get WinAmount
                var winAmount = int.Parse(cells[1].InnerHtml);

                // get Rules
                var rules = cells[4].InnerHtml.Split(',').ToList();

                // get Patch value
                var patch = cells[5].InnerHtml;

                var npc = new NpcInfo()
                {
                    NpcName = name,
                    Location = location,
                    Patch = patch,
                    Rules = rules,
                    WinAmount = winAmount
                };
                npcs.Add(npc);
            }

            return npcs;
        }

        string DownloadWebpage(string urlAddress)
        {
            string result = "";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (Stream receiveStream = response.GetResponseStream())
                {
                    if (response.CharacterSet == null)
                    {
                        using (var readStream = new StreamReader(receiveStream))
                        {
                            result = readStream.ReadToEnd();
                        }
                    }
                    else
                    {
                        using (var readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet)))
                        {
                            result = readStream.ReadToEnd();
                        }
                    }
                }
            }

            return result;
        }

        List<T> LoadFromFile<T>(string fileName)
        {
            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            var rawData = File.ReadAllText(Path.Combine(dataDirectory, fileName));
            var data = JsonConvert.DeserializeObject<List<T>>(rawData);
            return data;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
