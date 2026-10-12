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
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        private void reg_b_Click(object sender, RoutedEventArgs e)
        {
            //временно
            NavigationService?.Navigate(new RegistrationPage());
        }

        private void back_b_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate (new RegistrationPage());
        }
    }
}
