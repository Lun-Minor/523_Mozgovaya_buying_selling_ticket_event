using _523_Mozgovaya.ClientPages;
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
using System.Data.Entity;

namespace _523_Mozgovaya.AdminPages
{
    /// <summary>
    /// Логика взаимодействия для AdminProfilePage.xaml
    /// </summary>
    public partial class AdminProfilePage : Page
    {
        public AdminProfilePage()
        {
            InitializeComponent();
            LoadUserData();
            LoadOrders();
        }


        private void LoadUserData()
        {
            var user = Core.entities.User.Include(u => u.Role).FirstOrDefault(u => u.id_user == CurrentUser.IdUser);
            tbFullName.Text = $"{user.surname} {user.name} {user.last_name}".Trim();

        }


        private void LoadOrders()
        {

            var orders = Core.entities.Order.Include(o => o.Ticket.Select(t => t.TicketCategory.Session.Event)).Where(o => o.id_user == CurrentUser.IdUser)
                .OrderByDescending(o => o.order_date).ToList();


            var orderView = orders.Select(o =>
            {
                var firstTicket = o.Ticket.FirstOrDefault();
                var session = firstTicket?.TicketCategory?.Session;
                var ev = session?.Event;

                return new
                {
                    OrderId = o.id_order,
                    EventName = ev.name,
                    SessionTime = session.time_start.ToString("dd.MM.yyyy HH:mm"),
                    OrderDate = o.order_date.ToString("dd.MM.yyyy"),
                    TicketsCount = o.amount_ticket,
                    TotalSum = $"{o.total_summ:F2} ₽"
                };
            }).ToList();

            if (orderView.Count == 0)
            {
                lvOrders.Visibility = Visibility.Collapsed;
                tbEmptyOrders.Visibility = Visibility.Visible;
            }
            else
            {
                lvOrders.ItemsSource = orderView;


                tbOrdersCount.Text = orderView.Count.ToString();
                tbTotalSpent.Text = $"{orders.Sum(o => o.total_summ):F2} ₽";
            }
        }


        private void btnBack_Click(object sender, RoutedEventArgs e)
        {

            NavigationService?.Navigate(new AdminPage());
        }

        private void bt_loved_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminLovedPage());
        }

        private void bt_zavki_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminRequestPage());
        }

        private void bt_change_password_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePasswordAdminPage());
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из аккаунта?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                CurrentUser.IdUser = 0;
                CurrentUser.Login = null;
                CurrentUser.FullName = null;
                CurrentUser.IdRole = 0;
                NavigationService?.Navigate(new LoginPage());
            }
        }

        private void bt_charts_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new DiagrammPage());
        }
    }
}