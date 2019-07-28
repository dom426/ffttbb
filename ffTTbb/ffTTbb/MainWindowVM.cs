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
        CardInfoDisplay _selectedCard;
        public CardInfoDisplay SelectedCard
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

        string _collectedSearch;
        public string CollectedSearch
        {
            get
            {
                return _collectedSearch;
            }
            set
            {
                _collectedSearch = value;
                OnPropertyChanged(nameof(CollectedSearch));
            }
        }

        string _minDifficultySearch;
        public string MinDifficultySearch
        {
            get
            {
                return _minDifficultySearch;
            }
            set
            {
                _minDifficultySearch = value;
                OnPropertyChanged(nameof(MinDifficultySearch));
            }
        }

        string _maxDifficultySearch;
        public string MaxDifficultySearch
        {
            get
            {
                return _maxDifficultySearch;
            }
            set
            {
                _maxDifficultySearch = value;
                OnPropertyChanged(nameof(MaxDifficultySearch));
            }
        }

        ObservableCollection<CardInfoDisplay> _cards;
        public ObservableCollection<CardInfoDisplay> Cards
        {
            get
            {
                return _cards;
            }
            set
            {
                _cards = value;
                OnPropertyChanged(nameof(Cards));
            }
        }

        ObservableCollection<string> _cardNames;
        public ObservableCollection<string> CardNames
        {
            get
            {
                return _cardNames;
            }
            set
            {
                _cardNames = value;
                OnPropertyChanged(nameof(CardNames));
            }
        }

        ObservableCollection<string> _npcNames;
        public ObservableCollection<string> NPCNames
        {
            get
            {
                return _npcNames;
            }
            set
            {
                _npcNames = value;
                OnPropertyChanged(nameof(NPCNames));
            }
        }

        ObservableCollection<string> _patches;
        public ObservableCollection<string> Patches
        {
            get
            {
                return _patches;
            }
            set
            {
                _patches = value;
                OnPropertyChanged(nameof(Patches));
            }
        }

        public List<string> _collectedOptions;
        public List<string> CollectedOptions
        {
            get
            {
                return _collectedOptions;
            }
            set
            {
                _collectedOptions = value;
                OnPropertyChanged(nameof(CollectedOptions));
            }
        }

        public MainWindowVM()
        {
            Cards = new ObservableCollection<CardInfoDisplay>();
            CardNames = new ObservableCollection<string>();
            Patches = new ObservableCollection<string>();
            NPCNames = new ObservableCollection<string>();
            CollectedOptions = new List<string>(new string[] { "No Preference", "Collected", "Uncollected" });

            var npcs = DownloadNpcs(); // using to cross-reference some data into card data (like WinAmount/Difficulty)

            var collectedCards = LoadCollectedCards();
            var cardInfos = DownloadCards();            
            foreach (var cardInfo in cardInfos)
            {
                var difficulty = int.MaxValue;
                foreach (var npcName in cardInfo.NPCs)
                {
                    var npc = npcs.FirstOrDefault(n => n.NpcName == npcName);
                    if (npc != null && npc.WinAmount < difficulty)
                        difficulty = npc.WinAmount;
                }

                var card = new CardInfoDisplay(cardInfo, difficulty == int.MaxValue ? 0 : difficulty);
                card.IsDisplayed = true;
                card.IsCollected = collectedCards.Contains(card.Info.Order.ToString());
                Cards.Add(card);
            }

            DoSearch();

            if (Cards.Count > 0)
                SelectCard(Cards[0]);
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

        public void SelectCard(CardInfoDisplay selectedCard)
        {
            foreach (var card in _cards)
            {
                card.IsSelected = false;
            }

            SelectedCard = selectedCard;
            SelectedCard.IsSelected = true;
        }

        public List<string> LoadCollectedCards()
        {
            var collectedCards = new List<string>();

            var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "collectedCards.log");
            try
            {
                if (File.Exists(logFilePath))
                {
                    var collectedCardsStr = File.ReadAllText(logFilePath);
                    if (collectedCardsStr != null)
                        collectedCards = collectedCardsStr.Split(',').ToList();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("There was an issue while attempting to load your card collection from disk: " + ex.Message, "Failed to Load Collection!");
            }

            return collectedCards;
        }

        public void SaveCollectedCards(List<string> collectedCards)
        {
            var collectedCardStr = "";
            foreach (var collectedCard in collectedCards)
            {
                if (!string.IsNullOrEmpty(collectedCardStr))
                    collectedCardStr += ",";
                collectedCardStr += collectedCard;
            }

            try
            {
                var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "collectedCards.log");
                File.WriteAllText(logFilePath, collectedCardStr);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("There was an issue while attempting to save your card collection: " + ex.Message, "Failed to Save Collection!");
                return;
            }
        }

        public void ToggleCollectedCard(CardInfoDisplay selectedCard)
        {
            var collectedCards = LoadCollectedCards();

            if (!selectedCard.IsCollected && !collectedCards.Contains(selectedCard.Info.Order.ToString()))
                collectedCards.Add(selectedCard.Info.Order.ToString());
            else if (selectedCard.IsCollected && collectedCards.Contains(selectedCard.Info.Order.ToString()))
                collectedCards.Remove(selectedCard.Info.Order.ToString());

            SaveCollectedCards(collectedCards);

            selectedCard.IsCollected = !selectedCard.IsCollected;
        }

        public void ResetSearch(bool doSearch = false)
        {
            CardNameSearch = "";
            NPCNameSearch = "";
            PatchSearch = "";

            if (doSearch)
                DoSearch();
        }

        public void DoSearch()
        {
            foreach (var card in Cards)
            {
                card.IsDisplayed = true;
            }
            
            foreach (var card in Cards)
            {
                var isNameMatch = string.IsNullOrEmpty(CardNameSearch) ? true : card.Info.Name.ToLower().Contains(CardNameSearch.ToLower());
                var isPatchMatch = string.IsNullOrEmpty(PatchSearch) ? true : card.Info.Patch.Contains(PatchSearch);
                var isNpcMatch = string.IsNullOrEmpty(NPCNameSearch) ? true : card.Info.NPCs.FirstOrDefault(n => n.ToLower().Contains(NPCNameSearch.ToLower())) != null;
                var isMinDifficultyMatch = string.IsNullOrEmpty(MinDifficultySearch) ? true : card.Difficulty >= int.Parse(MinDifficultySearch);
                var isMaxDifficultyMatch = string.IsNullOrEmpty(MaxDifficultySearch) ? true : card.Difficulty <= int.Parse(MaxDifficultySearch);
                var isCollectionMatch = string.IsNullOrEmpty(CollectedSearch) || CollectedSearch == "No Preference" ? true : (CollectedSearch == "Collected" && card.IsCollected) || (CollectedSearch == "Uncollected" && !card.IsCollected);

                card.IsDisplayed = isNameMatch && isPatchMatch && isNpcMatch && isMinDifficultyMatch && isMaxDifficultyMatch && isCollectionMatch;
            }

            // store fields for resetting later
            var cardName = CardNameSearch;
            var patch = PatchSearch;
            var npcName = NPCNameSearch;

            CardNames.Clear();
            Patches.Clear();
            NPCNames.Clear();

            var displayedCards = Cards.Where(c => c.IsDisplayed);
            foreach (var displayedCard in displayedCards)
            {
                if (!CardNames.Contains(displayedCard.Info.Name))
                    CardNames.Add(displayedCard.Info.Name);

                if (!Patches.Contains(displayedCard.Info.Patch))
                    Patches.Add(displayedCard.Info.Patch);

                foreach (var npc in displayedCard.Info.NPCs)
                {
                    if (!NPCNames.Contains(npc))
                        NPCNames.Add(npc);
                }
            }

            // set fields back after the search
            CardNameSearch = cardName;
            PatchSearch = patch;
            NPCNameSearch = npcName;
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
                        var location = "";
                        if (locationCell.HasClass("aNpc"))
                        {
                            var npcCell = locationCell.ChildNodes.FirstOrDefault(n => n.Name == "a");
                            var npcName = npcCell.InnerHtml;
                            npcs.Add(npcName);

                            location = locationCell.LastChild.InnerHtml;
                            if (location.StartsWith(", "))
                                location = location.Substring(2);
                        }
                        else
                        {
                            location = locationCell.InnerHtml;
                        }

                        if (!string.IsNullOrEmpty(location))
                        {
                            location = RemoveUnwantedTags(WebUtility.HtmlDecode(location));
                            locations.Add(location);
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

        string RemoveUnwantedTags(string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            var document = new HtmlDocument();
            document.LoadHtml(data);

            var acceptableTags = new String[] { "strong", "em", "u" };

            var nodes = new Queue<HtmlNode>(document.DocumentNode.SelectNodes("./*|./text()"));
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;

                if (!acceptableTags.Contains(node.Name) && node.Name != "#text")
                {
                    var childNodes = node.SelectNodes("./*|./text()");

                    if (childNodes != null)
                    {
                        foreach (var child in childNodes)
                        {
                            nodes.Enqueue(child);
                            parentNode.InsertBefore(child, node);
                        }
                    }

                    parentNode.RemoveChild(node);

                }
            }

            return document.DocumentNode.InnerHtml;
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

        public void DoSortBy(EnumCardField sortByField)
        {
            var sortedCards = new List<CardInfoDisplay>();
            switch (sortByField)
            {
                case EnumCardField.Order:
                    sortedCards = Cards.OrderBy(c => c.Info.Order).ToList();
                    break;
                case EnumCardField.Name:
                    sortedCards = Cards.OrderBy(c => c.Info.Name).ToList();
                    break;
                case EnumCardField.Difficulty:
                    sortedCards = Cards.OrderBy(c => c.Difficulty).ToList();
                    break;
                case EnumCardField.Patch:
                    sortedCards = Cards.OrderBy(c => c.Info.Patch).ToList();
                    break;
            }
            Cards = new ObservableCollection<CardInfoDisplay>(sortedCards);
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

    public enum EnumCardField
    {
        Order,
        Name,
        Difficulty,
        Patch
    }
}
