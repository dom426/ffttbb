using ffTTbb.Models;
using System.Windows;
using System.Windows.Controls;

namespace ffTTbb
{
    public partial class MainWindow : Window
    {         
        MainWindowVM _vm;

        public MainWindow()
        {
            InitializeComponent();
            
            _vm = new MainWindowVM();
            DataContext = _vm;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            _vm.DoSearch();
        }
        
        private void CardWrapper_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _vm.SelectCard((sender as Border).DataContext as CardInfo);
        }
    }
}
