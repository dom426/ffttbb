using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace ffTTbb.Models
{
    public class CardInfo: INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public int Rarity { get; set; }
        public string ImageUrl { get; set; }
        public string Patch { get; set; }
        public List<string> NPCs { get; set; }
        public List<string> Locations { get; set; }
        public BitmapImage CardImage { get; set; }
        public BitmapImage RarityImage { get; set; }

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

        public void LoadImages()
        {
            var imageName = ImageUrl.Replace("/images/cards/", "").Replace("small_", "").Replace(".png", "");
            CardImage = LoadImage(imageName, "card" + Order);

            RarityImage = LoadImage("rarity" + Rarity);
        }

        BitmapImage LoadImage(string imageName, string altName = "")
        {
            BitmapImage bmp = null;

            var imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
            if (!Directory.Exists(imageDirectory))
                Directory.CreateDirectory(imageDirectory);

            var imageFilePath = Path.Combine(imageDirectory, imageName + ".png");
            if (!File.Exists(imageFilePath))
            {
                Console.WriteLine("Could not find: " + imageName + ".png");
                return null;

                var imageUrl = "https://arrtripletriad.com/images/cards/" + imageName + ".png";
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        client.DownloadFile(imageUrl, imageFilePath);
                    }
                    catch
                    {
                    }
                }
            }

            if (!File.Exists(imageFilePath))
            {
                if (!string.IsNullOrEmpty(altName))
                    bmp = LoadImage(altName);
                else if (!altName.StartsWith("small_"))
                    bmp = LoadImage("small_" + altName);
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
