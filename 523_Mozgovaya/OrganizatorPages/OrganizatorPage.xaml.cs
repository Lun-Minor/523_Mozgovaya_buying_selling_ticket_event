
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

namespace _523_Mozgovaya.OrganizatorPages
{
    /// <summary>
    /// Логика взаимодействия для OrganizatorPage.xaml
    /// </summary>
    public partial class OrganizatorPage : Page
    {
        private List<GenreFilter> genreFilters;
        public OrganizatorPage()
        {
            InitializeComponent();
            genreFilters = Core.entities.Genre.Select(g => new GenreFilter
            {
                id_genre = g.id_genre,
                name = g.name,
                IsSelected = false
            }).ToList();

            cbGenres.ItemsSource = genreFilters;
            UpdateEvents();
        }



        private void UpdateEvents()
        {
            if (!IsInitialized) return;

            var events = Core.entities.Event.Include(e => e.Genre).Where(e => e.status == 1).ToList();

            // Поиск
            if (!string.IsNullOrWhiteSpace(tb_search.Text))
            {
                events = events.Where(x => x.name != null && x.name.ToLower().Contains(tb_search.Text.ToLower())).ToList();
            }

            // Сортировка 
            switch (combo_box.SelectedIndex)
            {
                case 0: break;
                case 1: events = events.OrderBy(x => x.name).ToList(); break;
                case 2: events = events.OrderByDescending(x => x.rating).ToList(); break;
            }

            // Фильтр по жанрам
            var selectedIds = genreFilters.Where(g => g.IsSelected).Select(g => g.id_genre).ToList();
            if (selectedIds.Count > 0)
            {
                events = events.Where(e => e.Genre.Any(g => selectedIds.Contains(g.id_genre))).ToList();
            }

            list_events.ItemsSource = events;


        }


        private void btn_profile_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new OrganizatorProfilePage());
        }


       



        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            UpdateEvents();
        }
        private void cbGenres_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbGenres.SelectedIndex = -1;
            cbGenres.Text = "Фильтры";
        }
        private void cbGenres_DropDownClosed(object sender, EventArgs e)
        {
            cbGenres.Text = "Фильтры";
            UpdateEvents();
        }


        private void tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEvents();
        }
        private void GenreCheckBox_Changed(object sender, RoutedEventArgs e)
        {

            UpdateEvents();
        }

        private void list_events_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected_event = list_events.SelectedItem as Event;
            if (selected_event != null)
            {
               // NavigationService?.Navigate(new EventPage());
            }
        }
    }


}










