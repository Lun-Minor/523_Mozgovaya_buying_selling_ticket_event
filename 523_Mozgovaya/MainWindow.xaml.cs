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

namespace _523_Mozgovaya
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            mainframe.Navigated += Mainframe_Navigated;
            mainframe.Navigate(new MainPage());
        }

        
        private void Mainframe_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is Page page)
            {
                this.Title = page.Title;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите закрыть окно?", "Message",
                MessageBoxButton.YesNo) == MessageBoxResult.No)
                e.Cancel = true;
            else
                e.Cancel = false;
        }

      

        private void cbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            
            var uri = new Uri(cbTheme.SelectedIndex == 0 ? "/style/Dictionary.xaml" : "/style/OwnDictionary.xaml", UriKind.Relative);
            
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }
    }
}
