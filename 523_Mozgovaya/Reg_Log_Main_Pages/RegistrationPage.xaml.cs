using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace _523_Mozgovaya
{
    /// <summary>
    /// Логика взаимодействия для RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Page
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        private void reg_b_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_login.Text) ||
                string.IsNullOrEmpty(tb_name.Text) 
                || string.IsNullOrEmpty(pas_box.Password) 
                || string.IsNullOrEmpty(pas_box2.Password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }


            Entities db = new Entities();
            {
                var user = db.User.AsNoTracking().FirstOrDefault(u => u.login == tb_login.Text);
                if (user != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!");
                    return;
                }
            }

            if (pas_box.Password.Length >= 6)
            {
                bool en = true;
                bool number = false;

                for (int i = 0; i < pas_box.Password.Length; i++)
                {
                    if (pas_box.Password[i] >= '0' && pas_box.Password[i] <= '9') number =
                    true;
                    else if (!((pas_box.Password[i] >= 'A' && pas_box.Password[i] <=
                    'Z') || (pas_box.Password[i] >= 'a' && pas_box.Password[i] <= 'z'))) en = false;
                }

                if (!en)
                {
                    MessageBox.Show("Используйте только английскую расскладку!");
                }
                else if (!number) { MessageBox.Show("Добавьте хотя бы одну цифру!"); }
                    
                if (en && number)
                {
                    if (pas_box.Password != pas_box2.Password)
                    {
                        MessageBox.Show("Пароли не совпадают!"); return;
                    }
                }
            }
            else {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!"); return;
            }

                User userObject = new User
                {
                    name = tb_name.Text,
                    last_name = string.IsNullOrWhiteSpace(tb_lastname.Text) ? null : tb_lastname.Text,
                    surname = tb_surname.Text,
                    login = tb_login.Text,
                    password = GetHash(pas_box.Password),
                    id_role = 3
                };
                db.User.Add(userObject);
                db.SaveChanges();
                MessageBox.Show("Пользователь успешно зарегистрирован!");
               
            NavigationService?.Navigate(new LoginPage());
        }
        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return
                string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x =>
                x.ToString("X2")));
            }
        }
        private void back_b_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate (new LoginPage());
        }

        
    }
}
