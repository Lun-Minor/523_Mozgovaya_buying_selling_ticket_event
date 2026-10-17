using _523_Mozgovaya.OrganizatorPages;
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
    /// Логика взаимодействия для ClientTicketPage.xaml
    /// </summary>
    public partial class ClientTicketPage : Page
    {

        private Event currentEvent;
        private Session selectedSession;
        private TicketCategory selectedCategory;
        private Lgot selectedLgot;

        public ClientTicketPage(Event ev)
        {
            InitializeComponent();
            currentEvent = ev;
            LoadEventInfo();
            LoadSessions();
            LoadLgots();
        }


        private void LoadEventInfo()
        {
            tbEventName.Text = currentEvent.name;
        }


        private void LoadSessions()
        {
            var sessions = Core.entities.Session.Include(s => s.Location).Where(s => s.id_event == currentEvent.id_event).OrderBy(s => s.time_start).ToList();
            cbSession.ItemsSource = sessions;
            cbSession.SelectedIndex = 0;
        }


        private void LoadLgots()
        {
            var lgots = Core.entities.Lgot.ToList();
            cbLgot.ItemsSource = lgots;
            cbLgot.SelectedIndex = 0;
        }


        private void cbSession_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSession = cbSession.SelectedItem as Session;
            if (selectedSession == null) return;

            var categories = Core.entities.TicketCategory.Where(c => c.id_session == selectedSession.id_session).ToList();

            lvCategories.ItemsSource = categories;

            if (categories.Count > 0) lvCategories.SelectedIndex = 0;
            else selectedCategory = null;
            UpdateTotal();
        }


        private void lvCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedCategory = lvCategories.SelectedItem as TicketCategory;
            UpdateTotal();
        }


        private void cbQuantity_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTotal();
        }


        private void cbLgot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedLgot = cbLgot.SelectedItem as Lgot;
            UpdateTotal();
        }


        private void UpdateTotal()
        {

            if (tbTotal == null || cbQuantity == null)
                return;

            if (selectedCategory == null || cbQuantity.SelectedItem == null)
            {
                tbTotal.Text = "0 ₽";
                return;
            }

            int quantity = GetSelectedQuantity();
            double price = selectedCategory.price;
            double discount = selectedLgot?.skidka ?? 0;

            double total = price * quantity * (1 - discount);
            tbTotal.Text = $"{total:F2} ₽";
        }


        private int GetSelectedQuantity()
        {
            if (cbQuantity.SelectedItem is ComboBoxItem item)
            {
                return int.Parse(item.Content.ToString());
            }
            return 1;
        }


        private void btnBuy_Click(object sender, RoutedEventArgs e)
        {

            int quantity = GetSelectedQuantity();
            double price = selectedCategory.price;
            double discount = selectedLgot?.skidka ?? 0;
            double total = price * quantity * (1 - discount);



            var order = new Order
            {
                id_user = CurrentUser.IdUser,
                order_date = DateTime.Now,
                total_summ = (float)total,
                amount_ticket = quantity
            };
            Core.entities.Order.Add(order);
            Core.entities.SaveChanges();


            for (int i = 1; i <= quantity; i++)
            {
                var ticket = new Ticket
                {
                    id_ticket_category = selectedCategory.id_ticket_category,
                    id_lgot = selectedLgot?.id_lgot,
                    id_order = order.id_order,
                    seat_number = $"M{order.id_order}-{i}",
                    status = 1
                };
                Core.entities.Ticket.Add(ticket);
            }
            Core.entities.SaveChanges();


            var category = Core.entities.TicketCategory.FirstOrDefault(c => c.id_ticket_category == selectedCategory.id_ticket_category);

            if (category != null)
            {
                category.quantity -= quantity;
                if (category.quantity < 0)
                    category.quantity = 0;

                Core.entities.SaveChanges();
            }


            var categories = Core.entities.TicketCategory.Where(c => c.id_session == selectedSession.id_session).ToList();

            lvCategories.ItemsSource = categories;
            lvCategories.SelectedItem = categories.FirstOrDefault(c => c.id_ticket_category == selectedCategory.id_ticket_category);


            MessageBox.Show(
                $"Заказ №{order.id_order} успешно оформлен!\n\n" +
                $"Мероприятие: {currentEvent.name}\n" +
                $"Сессия: {selectedSession.time_start:dd.MM.yyyy HH:mm}\n" +
                $"Категория: {selectedCategory.name}\n" +
                $"Количество: {quantity}\n" +
                $"Итого: {total:F2} ₽",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
           
            NavigationService?.Navigate(new ClientPage());


        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {

            NavigationService?.Navigate(new ClientEventPage(currentEvent));
        }

    }
}