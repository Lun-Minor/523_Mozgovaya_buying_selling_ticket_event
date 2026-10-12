using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            LoadData();
        }
        private void LoadData()
        {
            // Создаем экземпляр вашего контекста базы данных
            using (var db = new Entities2())
            {
                // Извлекаем данные из таблицы (например, Users) и переводим в список
                var usersList = db.User.ToList();

                // Привязываем список к DataGrid
                MyDataGrid.ItemsSource = usersList;
            }
        }


        private void btn_login_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }
    }
}
