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

namespace _523_Mozgovaya.ClientPages
{
    /// <summary>
    /// Логика взаимодействия для ProfileClientPage.xaml
    /// </summary>
    public partial class ProfileClientPage : Page
    {
        public ProfileClientPage()
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
            
                NavigationService?.Navigate(new ClientPage());
        }

        private void bt_loved_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LovedClientPage());
        }

        private void bt_zavki_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RequestPage());
        }

        private void bt_change_password_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePasswordClientPage());
        }
    }
}