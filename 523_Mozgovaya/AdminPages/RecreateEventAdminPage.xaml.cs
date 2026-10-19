using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Data.Entity;

namespace _523_Mozgovaya.AdminPages
{
    /// <summary>
    /// Логика взаимодействия для RecreateEventAdminPage.xaml
    /// </summary>
    public class AdminSessionDraft
    {
        public int? IdSession { get; set; } 
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

    public partial class RecreateEventAdminPage : Page
    {
        private Event currentEvent;
        private List<GenreFilter> genreFilters;
        private ObservableCollection<AdminSessionDraft> sessions;

        
        public RecreateEventAdminPage(Event ev)
        {
            InitializeComponent();
            currentEvent = ev;

            LoadCategories();
            LoadGenres();

            sessions = new ObservableCollection<AdminSessionDraft>();
            icSessions.ItemsSource = sessions;

            LoadEventData(); 
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

        private void LoadEventData()
        {
            
            var ev = Core.entities.Event.Include(e => e.Genre).Include(e => e.Session.Select(s => s.Location)).Include(e => e.Session.Select(s => s.TicketCategory)).FirstOrDefault(e => e.id_event == currentEvent.id_event);

            tbName.Text = ev.name;
            tbDescription.Text = ev.description;
            tbAgeLimit.Text = ev.age_limit.ToString();
            tbImagePath.Text = ev.cover_image;

            cbCategory.SelectedItem = cbCategory.Items.Cast<Category>().FirstOrDefault(c => c.id_category == ev.id_category);

            
            foreach (var g in ev.Genre)
            {
                var filter = genreFilters.FirstOrDefault(f => f.id_genre == g.id_genre);
                if (filter != null) filter.IsSelected = true;
            }
            UpdateGenreText();

            
            foreach (var s in ev.Session)
            {
                var draft = new AdminSessionDraft
                {
                    IdSession = s.id_session,
                    City = s.Location?.city,
                    Street = s.Location?.street,
                    House = s.Location?.house,
                    Hall = s.Location?.hall,
                    StartTime = s.time_start.ToString("dd.MM.yyyy HH:mm"),
                    EndTime = s.time_end.ToString("dd.MM.yyyy HH:mm")
                };

            
                foreach (var tc in s.TicketCategory)
                {
                    draft.Tickets.Add(new TicketDraft
                    {
                        Name = tc.name,
                        Price = tc.price.ToString(),
                        Quantity = tc.quantity.ToString()
                    });
                }

                sessions.Add(draft);
            }
        }

        private void UpdateGenreText()
        {
            int count = genreFilters.Count(g => g.IsSelected);
            cbGenres.Text = count == 0 ? "Выберите жанры" : $"Выбрано: {count}";
        }

        private void cbGenres_DropDownClosed(object sender, EventArgs e)
        {
            UpdateGenreText();
        }

        

        private void btnAddSession_Click(object sender, RoutedEventArgs e)
        {
            var newSession = new AdminSessionDraft();
            newSession.Tickets.Add(new TicketDraft { Name = "Стандарт" });
            sessions.Add(newSession);
        }

        private void btnAddTicket_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is AdminSessionDraft session)
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

       
        private void btnRemoveSession_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is AdminSessionDraft session)
            {
                sessions.Remove(session);
            }
        }


        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text) || cbCategory.SelectedItem == null)
            {
                MessageBox.Show("Заполните название и категорию!");
                return;
            }

            if (sessions.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну сессию!");
                return;
            }

            

            var ev = Core.entities.Event.Include(x => x.Genre).Include(x => x.Session.Select(s => s.TicketCategory)).FirstOrDefault(x => x.id_event == currentEvent.id_event);
            ev.name = tbName.Text;
            ev.description = tbDescription.Text;
            ev.age_limit = int.TryParse(tbAgeLimit.Text, out int age) ? age : 0;
            ev.cover_image = tbImagePath.Text;
            ev.id_category = (cbCategory.SelectedItem as Category).id_category;

            ev.Genre.Clear();
            var selectedIds = genreFilters.Where(g => g.IsSelected).Select(g => g.id_genre).ToList();
            foreach (var id in selectedIds)
            {
                var genreEntity = Core.entities.Genre.Find(id);
                if (genreEntity != null) ev.Genre.Add(genreEntity);
            }

            foreach (var oldSession in ev.Session.ToList())
            {
                Core.entities.TicketCategory.RemoveRange(oldSession.TicketCategory);
                Core.entities.Session.Remove(oldSession);
            }
            var oldLocationIds = ev.Session.Select(s => s.id_location).ToList();
            var oldLocations = Core.entities.Location.Where(l => oldLocationIds.Contains(l.id_location)).ToList();
            Core.entities.Location.RemoveRange(oldLocations);

            foreach (var sessionDraft in sessions)
            {
                if (string.IsNullOrWhiteSpace(sessionDraft.City) || !DateTime.TryParse(sessionDraft.StartTime, out DateTime start) || !DateTime.TryParse(sessionDraft.EndTime, out DateTime end))
                {
                    MessageBox.Show("Проверьте заполнение городов и дат во всех сессиях.");
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
                    Event = ev,
                    Location = newLocation
                };
                Core.entities.Session.Add(newSession);

                foreach (var ticketDraft in sessionDraft.Tickets)
                {
                    if (string.IsNullOrWhiteSpace(ticketDraft.Name) || !decimal.TryParse(ticketDraft.Price, out decimal price) || !int.TryParse(ticketDraft.Quantity, out int qty))
                    {
                        MessageBox.Show("Проверьте данные билетов. Цена и количество должны быть числами.");
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
            MessageBox.Show("Изменения успешно сохранены!");
            NavigationService?.Navigate(new AdminPage()); 
            
            
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminPage());
        }
    }
}