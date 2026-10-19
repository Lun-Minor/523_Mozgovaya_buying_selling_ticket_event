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

namespace _523_Mozgovaya.ClientPages
{
    /// <summary>
    /// Логика взаимодействия для RequestPage.xaml
    /// </summary>
    public partial class RequestPage : Page
    {
        public RequestPage()
        {
            InitializeComponent();
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
           

            if (string.IsNullOrWhiteSpace(tbOrgName.Text) || string.IsNullOrWhiteSpace(tbEmail.Text))
            {
                MessageBox.Show("Заполните обязательные поля!");
                return;
            }
            var existingRequest = Core.entities.OrganizerRequest.FirstOrDefault(r => r.id_user == CurrentUser.IdUser);

            if (existingRequest != null)
            {
                MessageBox.Show("Вы уже отправили заявку. Дождитесь ответа администратора.");
                return;
            }


                var request = new OrganizerRequest
                {
                    id_user = CurrentUser.IdUser,
                    org_name = tbOrgName.Text,
                    inn = tbINN.Text,
                    contact_email = tbEmail.Text,
                    request_date = DateTime.Now
                };

                Core.entities.OrganizerRequest.Add(request);
                Core.entities.SaveChanges();

                MessageBox.Show("Заявка успешно отправлена! Ожидайте решения.");
                NavigationService?.Navigate(new ProfileClientPage());

        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProfileClientPage());
        }
    }
}