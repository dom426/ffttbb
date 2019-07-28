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
            Loaded += MainWindow_Loaded;
            
            _vm = new MainWindowVM();
            DataContext = _vm;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SortByBox.SelectedIndex = 0;
            CollectionSearchBox.SelectedIndex = 0;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            _vm.DoSearch();
        }
        
        private void CardWrapper_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var card = (sender as Grid).DataContext as CardInfoDisplay;
            if (card != null)
            {
                if (e.ClickCount == 2) // double-click
                {
                    _vm.ToggleCollectedCard(card);
                }
                else
                {
                    _vm.SelectCard(card);
                }
            }
        }

        private void SearchPatchItem_Click(object sender, RoutedEventArgs e)
        {
            var card = (sender as MenuItem).DataContext as CardInfoDisplay;
            if (card != null)
            {
                _vm.ResetSearch();
                _vm.PatchSearch = card.Info.Patch;
                _vm.DoSearch();
            }
        }

        private void SearchNPCItem_Click(object sender, RoutedEventArgs e)
        {
            var txt = (sender as TextBlock);
            if (txt != null)
            {
                _vm.ResetSearch();
                _vm.NPCNameSearch = txt.Text;
                _vm.DoSearch();
            }
        }

        private void CardSearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                _vm.DoSearch();

                var cbo = sender as ComboBox;
                var txtbox = (TextBox)cbo.Template.FindName("PART_EditableTextBox", cbo);
                txtbox.SelectionStart = txtbox.Text.Length;
            }
        }

        private void NPCSearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                _vm.DoSearch();

                var cbo = sender as ComboBox;
                var txtbox = (TextBox)cbo.Template.FindName("PART_EditableTextBox", cbo);
                txtbox.SelectionStart = txtbox.Text.Length;
            }
        }

        private void PatchSearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                _vm.DoSearch();

                var cbo = sender as ComboBox;
                var txtbox = (TextBox)cbo.Template.FindName("PART_EditableTextBox", cbo);
                txtbox.SelectionStart = txtbox.Text.Length;
            }
        }

        private void SortByBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.DoSortBy((EnumCardField)(sender as ComboBox).SelectedIndex);
        }

        private void CollectionSearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                _vm.DoSearch();

                var cbo = sender as ComboBox;
                var txtbox = (TextBox)cbo.Template.FindName("PART_EditableTextBox", cbo);
                txtbox.SelectionStart = txtbox.Text.Length;
            }
        }

        private void MinDifficultySearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                _vm.DoSearch();
        }

        private void MaxDifficultySearchBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space)
                (sender as TextBox).Text = (sender as TextBox).Text.Trim();

            if (e.Key == System.Windows.Input.Key.Enter)            
                _vm.DoSearch();            
        }

        private void MinDifficultySearchBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new System.Text.RegularExpressions.Regex("[0-9]+");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void MaxDifficultySearchBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var regex = new System.Text.RegularExpressions.Regex("[0-9]+");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void MinDifficultySearchBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space)
                e.Handled = true;
        }

        private void MaxDifficultySearchBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space)
                e.Handled = true;
        }

        private void CollectionSearchBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _vm.DoSearch();
        }
    }
}
