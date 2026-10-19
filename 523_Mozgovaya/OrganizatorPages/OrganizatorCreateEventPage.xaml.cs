using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Validation;
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
using System.Xml.Linq;

namespace _523_Mozgovaya.OrganizatorPages
{
    /// <summary>
    /// Логика взаимодействия для OrganizatorCreateEventPage.xaml
    /// </summary>

    public class SessionDraft
    {
        public string City { get; set; }
        public string Street { get; set; }
        public string House { get; set; }
        public string Hall { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public ObservableCollection<TicketDraft> Tickets { get; set; } = new ObservableCollection<TicketDraft>();
    }

    public class TicketDraft
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string Quantity { get; set; }
    }

    public partial class OrganizatorCreateEventPage : Page
    {
     

        private List<GenreFilter> genreFilters;
        private ObservableCollection<SessionDraft> sessions;

        public OrganizatorCreateEventPage()
        {
            InitializeComponent();
            LoadCategories();
            LoadGenres();

            sessions = new ObservableCollection<SessionDraft>();
            icSessions.ItemsSource = sessions;
        }

        private void LoadCategories()
        {
            cbCategory.ItemsSource = Core.entities.Category.ToList();
        }

        private void LoadGenres()
        {
            genreFilters = Core.entities.Genre.Select(g => new GenreFilter
            {
                id_genre = g.id_genre,
                name = g.name,
                IsSelected = false
            }).ToList();

            cbGenres.ItemsSource = genreFilters;
        }

        private void cbGenres_DropDownClosed(object sender, EventArgs e)
        {
            int count = genreFilters.Count(g => g.IsSelected);
            cbGenres.Text = count == 0 ? "Выберите жанры" : $"Выбрано: {count}";
        }

     

        private void btnAddSession_Click(object sender, RoutedEventArgs e)
        {
            var newSession = new SessionDraft();
            newSession.Tickets.Add(new TicketDraft { Name = "Стандарт" });
            sessions.Add(newSession);
        }

        private void btnAddTicket_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is SessionDraft session)
            {
                session.Tickets.Add(new TicketDraft());
            }
        }

        private void btnRemoveTicket_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is TicketDraft ticket)
            {
                foreach (var session in sessions)
                {
                    if (session.Tickets.Contains(ticket))
                    {
                        session.Tickets.Remove(ticket);
                        break;
                    }
                }
            }
        }

       

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
          
            if (string.IsNullOrWhiteSpace(tbName.Text) || cbCategory.SelectedItem == null)
            {
                MessageBox.Show("Заполните название и выберите категорию мероприятия");
                return;
            }

            if (sessions.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну сессию для мероприятия");
                return;
            }

    
            string imageUrl = tbImagePath.Text.Trim();

           
            var organizer = Core.entities.Organizer.FirstOrDefault(o => o.id_user == CurrentUser.IdUser);
            
            var newEvent = new Event{
                id_organizer = organizer.id_organizator,
                id_category = (cbCategory.SelectedItem as Category).id_category,
                name = tbName.Text,
                description = tbDescription.Text,
                age_limit = int.TryParse(tbAgeLimit.Text, out int age) ? age : 0,
                cover_image = imageUrl,
                rating = 0,
                status = 0 
                };

                
                var selectedIds = genreFilters.Where(g => g.IsSelected).Select(g => g.id_genre).ToList();
                foreach (var id in selectedIds)
                {
                    var genreEntity = Core.entities.Genre.FirstOrDefault(x => x.id_genre == id);
                    if (genreEntity != null) newEvent.Genre.Add(genreEntity);
                }
                Core.entities.Event.Add(newEvent);

                
                foreach (var sessionDraft in sessions)
                {
                
                    if (string.IsNullOrWhiteSpace(sessionDraft.City) || string.IsNullOrWhiteSpace(sessionDraft.Street) ||
                        !DateTime.TryParse(sessionDraft.StartTime, out DateTime start) || !DateTime.TryParse(sessionDraft.EndTime, out DateTime end))
                    {
                        MessageBox.Show($"Проверьте заполнение города, улицы и дат в сессии.");
                        return;
                    }

                    
                    var newLocation = new Location
                    {
                        city = sessionDraft.City,
                        street = sessionDraft.Street,
                        house = sessionDraft.House,
                        hall = sessionDraft.Hall
                    };
                    Core.entities.Location.Add(newLocation);

                    
                    var newSession = new Session
                    {
                        time_start = start,
                        time_end = end,
                        Event = newEvent,
                        Location = newLocation
                    };
                    Core.entities.Session.Add(newSession);

                    
                    foreach (var ticketDraft in sessionDraft.Tickets)
                    {
                        if (string.IsNullOrWhiteSpace(ticketDraft.Name) || !decimal.TryParse(ticketDraft.Price, out decimal price) || !int.TryParse(ticketDraft.Quantity, out int qty))
                        {
                            MessageBox.Show($"Проверьте данные билетов в сессии. Убедитесь, что цена и количество — числа.");
                            return;
                        }

                        var newTicketCategory = new TicketCategory
                        {
                            name = ticketDraft.Name,
                            price = (float)price,
                            quantity = qty,
                            Session = newSession
                        };
                        Core.entities.TicketCategory.Add(newTicketCategory);
                    }
                }

               
                Core.entities.SaveChanges();
                MessageBox.Show("Мероприятие отправлено на модерацию!");
                NavigationService?.Navigate (new OrganizatorPage());
            
           
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new OrganizatorPage());
        }
    }
}