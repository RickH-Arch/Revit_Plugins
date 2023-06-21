using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Revit_Plugin_Rick.UI
{
    /// <summary>
    /// Interaction logic for SearchCommandWindow.xaml
    /// </summary>
    public partial class SearchCommandWindow : Window
    {
        CommandFinder finder;

        public SearchCommandWindow()
        {
            InitializeComponent();
            search_input.Focus();
            finder = CommandFinder.Instance;

            //binding search_input
            Binding binding = new Binding();
            binding.Source = finder;
            binding.Path = new PropertyPath("Search_input");
            this.search_input.SetBinding(TextBox.TextProperty, binding);

            //binding listbox
            this.command_list.ItemsSource = finder.BindingCmdName;

        }


        private void exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void search_input_TextChanged(object sender, TextChangedEventArgs e)
        {
            finder.Search_input = this.search_input.Text;
            finder.RefreshFiltedCmdName();
        }

        private void search_input_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Down)
            {
                command_list.Focus();
                command_list.SelectedIndex = 0;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SearchBoxToRibbon.textBox_search.Value = finder.Search_input;
        }

        private void command_list_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                var selectedItem = command_list.SelectedItem;
                if(selectedItem != null)
                {
                    string cmdName = selectedItem.ToString();
                    this.Close();
                    finder.PostCommandByName(cmdName);
                }
            }
        }

        private void ListBoxIte_MouseDoubleClick(object sender,MouseButtonEventArgs e)
        {
            var selectedItem = command_list.SelectedItem;
            if (selectedItem != null)
            {
                string cmdName = selectedItem.ToString();
                this.Close();
                finder.PostCommandByName(cmdName);
            }
        }
    }
}
