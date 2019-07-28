using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ffTTbb.Models
{
    public class CardInfoDisplay : INotifyPropertyChanged
    {
        bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        bool _isCollected;
        public bool IsCollected
        {
            get
            {
                return _isCollected;
            }
            set
            {
                _isCollected = value;
                OnPropertyChanged(nameof(IsCollected));
            }
        }

        bool _isDisplayed;
        public bool IsDisplayed
        {
            get
            {
                return _isDisplayed;
            }
            set
            {
                _isDisplayed = value;
                OnPropertyChanged(nameof(IsDisplayed));
            }
        }

        BitmapImage _smallCardImage;
        public BitmapImage SmallCardImage
        {
            get
            {
                return _smallCardImage;
            }
            set
            {
                _smallCardImage = value;
                OnPropertyChanged(nameof(SmallCardImage));
                OnPropertyChanged(nameof(CardImage));
            }
        }

        BitmapImage _cardImage;
        public BitmapImage CardImage
        {
            get
            {
                if (_cardImage == null)
                    return _smallCardImage;
                return _cardImage;
            }
            set
            {
                _cardImage = value;
                OnPropertyChanged(nameof(CardImage));
            }
        }

        BitmapImage _rarityImage;
        public BitmapImage RarityImage
        {
            get
            {
                return _rarityImage;
            }
            set
            {
                _rarityImage = value;
                OnPropertyChanged(nameof(RarityImage));
            }
        }

        public CardInfo Info { get; set; }
        public int Difficulty { get; set; }

        public CardInfoDisplay(CardInfo cardInfo, int difficulty)
        {
            Info = cardInfo;
            Difficulty = difficulty;
            LoadImages();
        }

        public async void LoadImages()
        {
            if (Info == null)
                return;

            var imageName = Info.ImageUrl.Replace("/images/cards/", "").Replace(".png", "");
            CardImage = await LoadImage(imageName.Replace("small_", ""), "card" + Info.Order);
            SmallCardImage = await LoadImage(imageName);

            if (CardImage == null)
                Console.WriteLine("Could not find: " + imageName + ".png");
            //RarityImage = await LoadImage("rarity" + Info.Rarity);
        }

        async Task<BitmapImage> LoadImage(string imageName, string altName = "")
        {
            BitmapImage bmp = null;

            var imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
            if (!Directory.Exists(imageDirectory))
                Directory.CreateDirectory(imageDirectory);

            var imageFilePath = Path.Combine(imageDirectory, imageName + ".png");
            if (!File.Exists(imageFilePath))
            {
                var imageUrl = "https://arrtripletriad.com/images/cards/" + imageName + ".png";
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        var imageData = await client.DownloadDataTaskAsync(imageUrl);
                        if (imageData != null)
                            File.WriteAllBytes(imageFilePath, imageData);
                    }
                    catch
                    {
                    }
                }
            }

            if (!File.Exists(imageFilePath))
            {
                if (!string.IsNullOrEmpty(altName))
                    bmp = await LoadImage(altName);
            }
            else
            {
                try
                {
                    bmp = new BitmapImage(new Uri(imageFilePath));
                }
                catch
                {
                }
            }
            return bmp;
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
