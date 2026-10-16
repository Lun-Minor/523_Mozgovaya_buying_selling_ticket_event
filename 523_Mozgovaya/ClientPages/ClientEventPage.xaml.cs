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
    /// Логика взаимодействия для ClientEventPage.xaml
    /// </summary>
    public partial class ClientEventPage : Page
    {
        private Event currentEvent;

        public ClientEventPage(Event ev)
        {
            InitializeComponent();
            currentEvent = ev;
            LoadEventData();
            LoadReviews();
            CheckFavorite();
        }


        private void LoadEventData()
        {
            var fullEvent = Core.entities.Event.Include(e => e.Genre).Include(e => e.Category).FirstOrDefault(e => e.id_event == currentEvent.id_event);


            this.DataContext = fullEvent;
            tb_genre.Text = "Жанры: " + string.Join(", ", fullEvent.Genre.Select(g => g.name));
            tb_category.Text = "Категория: " + fullEvent.Category.name;
        }

        private void LoadReviews()
        {
            var reviews = Core.entities.Review.Include(r => r.User).Where(r => r.id_event == currentEvent.id_event).OrderByDescending(r => r.created_at).Select(r => new
            {
                name = r.User.surname + " " + r.User.name,
                comment = r.comment,
                rating = r.rating,
                created_at = r.created_at
            }).ToList();

            list_review.ItemsSource = reviews;
        }


        private void CheckFavorite()
        {
            if (!CurrentUser.IsLoggedIn)
                return;

            var user = Core.entities.User.Include(u => u.Event).FirstOrDefault(u => u.id_user == CurrentUser.IdUser);

            if (user == null) return;

            if (user.Event.Any(evt => evt.id_event == currentEvent.id_event))
            {
                loved.Content = "убрать из избранного";
            }
            else
            {
                loved.Content = "добавить в избранное";
            }
        }


        private void btnSubmitReview_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentUser.IsLoggedIn)
            {
                MessageBox.Show("Чтобы оставить отзыв, войдите в аккаунт");
                return;
            }
            if (string.IsNullOrWhiteSpace(tb_review.Text))
            {
                MessageBox.Show("Введите текст отзыва");
                return;
            }

            int rating = 10;
            if (cbRating.SelectedItem is ComboBoxItem item)
            {
                rating = int.Parse(item.Content.ToString());
            }


            var existingReview = Core.entities.Review.FirstOrDefault(r => r.id_event == currentEvent.id_event && r.id_user_rev == CurrentUser.IdUser);

            if (existingReview != null)
            {
                MessageBox.Show("Вы уже оставили отзыв на это мероприятие");
                return;
            }


            var review = new Review
            {
                id_event = currentEvent.id_event,
                id_user_rev = CurrentUser.IdUser,
                rating = rating,
                comment = tb_review.Text,
                created_at = DateTime.Now
            };

            Core.entities.Review.Add(review);
            Core.entities.SaveChanges();

            UpdateEventRating();

            tb_review.Text = "";
            LoadReviews();
            MessageBox.Show("Спасибо за отзыв!", "Успех");
        }


        private void loved_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentUser.IsLoggedIn)
            {
                MessageBox.Show("Войдите в аккаунт!");

                NavigationService?.Navigate(new LoginPage());
            }

            var user = Core.entities.User.Include(u => u.Event).FirstOrDefault(u => u.id_user == CurrentUser.IdUser);

            var ev = Core.entities.Event.FirstOrDefault(x => x.id_event == currentEvent.id_event);

            if (user == null || ev == null) return;

            if (user.Event.Any(x => x.id_event == currentEvent.id_event))
            {
                user.Event.Remove(ev);
                Core.entities.SaveChanges();
                loved.Content = "добавить в избранное";
                MessageBox.Show("Удалено из избранного");
            }
            else
            {
                user.Event.Add(ev);
                Core.entities.SaveChanges();
                loved.Content = "убрать из избранного";
                MessageBox.Show("Добавлено в избранное!");
            }
        }

        private void btnBuyTickets_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentUser.IsLoggedIn)
            {
                MessageBox.Show("Для покупки билетов нужно войти в аккаунт!");
                NavigationService?.Navigate(new LoginPage());
                return;
            }
            NavigationService?.Navigate(new TicketPage(currentEvent));
        }


        private void UpdateEventRating()
        {
            var reviews = Core.entities.Review.Where(r => r.id_event == currentEvent.id_event).ToList();

            if (reviews.Count == 0)
                return;
            double avgRating = reviews.Average(r => (double)r.rating);

            var ev = Core.entities.Event.FirstOrDefault(e => e.id_event == currentEvent.id_event);

            if (ev != null)
            {
                ev.rating = (float)Math.Round(avgRating, 1);
                Core.entities.SaveChanges();
                this.DataContext = null;
                this.DataContext = ev;
                currentEvent.rating = ev.rating;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ClientPage());
        }
    }
}