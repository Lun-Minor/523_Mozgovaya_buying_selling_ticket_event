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
    /// Логика взаимодействия для AdminRequestPage.xaml
    /// </summary>
    public partial class AdminRequestPage : Page
    {
        public AdminRequestPage()
        {
            InitializeComponent();
            LoadOrgRequests();
            LoadEventRequests();
        }

        private void cbRequestFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (spOrgRequests == null || spEventRequests == null) return;

            switch (cbRequestFilter.SelectedIndex)
            {
                case 0: // Все заявки
                    spOrgRequests.Visibility = Visibility.Visible;
                    spEventRequests.Visibility = Visibility.Visible;
                    break;
                case 1: //организаторы
                    spOrgRequests.Visibility = Visibility.Visible;
                    spEventRequests.Visibility = Visibility.Collapsed;
                    break;
                case 2: //мероприятия
                    spOrgRequests.Visibility = Visibility.Collapsed;
                    spEventRequests.Visibility = Visibility.Visible;
                    break;
            }
        }
        private void LoadOrgRequests()
        {
            var requests = Core.entities.OrganizerRequest.Include(r => r.User).ToList();

            if (requests.Count == 0)
            {
                tbEmptyOrg.Visibility = Visibility.Visible;
                icOrgRequests.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbEmptyOrg.Visibility = Visibility.Collapsed;
                icOrgRequests.Visibility = Visibility.Visible;

                icOrgRequests.ItemsSource = requests.Select(r => new {
                    IdRequest = r.id_request,
                    UserName = $"{r.User.surname} {r.User.name} ({r.User.login})",
                    OrgName = r.org_name,
                    Email = r.contact_email ?? "—"
                }).ToList();
            }
        }


        private void LoadEventRequests()
        {
            var events = Core.entities.Event.Include(e => e.Organizer).Where(e => e.status == 0).ToList();

            if (events.Count == 0)
            {
                tbEmptyEvent.Visibility = Visibility.Visible;
                icEventRequests.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbEmptyEvent.Visibility = Visibility.Collapsed;
                icEventRequests.Visibility = Visibility.Visible;

                icEventRequests.ItemsSource = events.Select(e => new {
                    IdEvent = e.id_event,
                    EventName = e.name,
                    CoverImage = e.cover_image,
                    OrganizerName = e.Organizer.org_name
                }).ToList();
            }
        }

        
        private void btnAcceptOrg_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int reqId)) return;

            var req = Core.entities.OrganizerRequest.Include(r => r.User).FirstOrDefault(r => r.id_request == reqId);
            if (req == null) return;

            req.User.id_role = 2;
            Core.entities.Organizer.Add(new Organizer
            {
                id_user = req.User.id_user,
                org_name = req.org_name,
                inn = req.inn,
                contact_email = req.contact_email
            });
            Core.entities.OrganizerRequest.Remove(req);
            Core.entities.SaveChanges();

            MessageBox.Show("Пользователь стал организатором!");
            LoadOrgRequests();
        }

        private void btnRejectOrg_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int reqId)) return;
            var req = Core.entities.OrganizerRequest.Find(reqId);
            if (req != null)
            {
                Core.entities.OrganizerRequest.Remove(req);
                Core.entities.SaveChanges();
                LoadOrgRequests();
            }
        }

        private void btnAcceptEvent_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int eventId)) return;
            var ev = Core.entities.Event.Find(eventId);
            if (ev != null)
            {
                ev.status = 1;
                Core.entities.SaveChanges();
                MessageBox.Show("Мероприятие одобрено!");
                LoadEventRequests();
            }
        }

        private void btnRejectEvent_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int eventId)) return;
            var ev = Core.entities.Event.Find(eventId);
            if (ev != null)
            {
                ev.status = 2;
                Core.entities.SaveChanges();
                MessageBox.Show("Мероприятие отклонено.");
                LoadEventRequests();
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminProfilePage());
        }
    }
}