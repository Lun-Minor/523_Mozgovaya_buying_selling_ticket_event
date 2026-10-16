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
    /// Логика взаимодействия для AdminLovedPage.xaml
    /// </summary>
    public partial class AdminLovedPage : Page
    {
        public AdminLovedPage()
        {
            InitializeComponent();
            LoadFavorites();
        }

        private void LoadFavorites()
        {

            var user = Core.entities.User.Include(u => u.Event.Select(e => e.Category)).Include(u => u.Event.Select(e => e.Genre))
                .FirstOrDefault(u => u.id_user == CurrentUser.IdUser);

            var favorites = user.Event.OrderByDescending(e => e.rating).ToList();

            if (favorites.Count == 0)
            {
                icFavorites.Visibility = Visibility.Collapsed;
                tbEmptyFavorites.Visibility = Visibility.Visible;
                return;
            }


            var view = favorites.Select(e => new
            {
                IdEvent = e.id_event,
                Name = e.name,
                CoverImage = e.cover_image,
                Rating = e.rating ?? 0,
                AgeLimit = e.age_limit,
                CategoryName = e.Category.name,
                GenresText = "Жанры: " + string.Join(", ", e.Genre.Select(g => g.name))

            }).ToList();

            icFavorites.ItemsSource = view;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int eventId)
            {
                var ev = Core.entities.Event.FirstOrDefault(x => x.id_event == eventId);
                NavigationService?.Navigate(new EventPage(ev));

            }
        }


        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is int eventId))
                return;


            var result = MessageBox.Show(
                "Удалить мероприятие из избранного?", "Подтверждение", MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;
            var user = Core.entities.User.Include(u => u.Event).FirstOrDefault(u => u.id_user == CurrentUser.IdUser);
            var favorite = user.Event.FirstOrDefault(evt => evt.id_event == eventId);
            if (favorite != null)
            {
                user.Event.Remove(favorite);
                Core.entities.SaveChanges();
                MessageBox.Show("Удалено из избранного");
                LoadFavorites();
            }


        }


        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminProfilePage());
        }
    }
}