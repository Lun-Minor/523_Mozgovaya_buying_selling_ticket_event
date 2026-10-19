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

namespace _523_Mozgovaya.ClientPages
{
    /// <summary>
    /// Логика взаимодействия для ChangePasswordClientPage.xaml
    /// </summary>
    public partial class ChangePasswordClientPage : Page
    {
        public ChangePasswordClientPage()
        {
            InitializeComponent();
            tb_login.Text = CurrentUser.Login;
        }
        private void reg_b_Click(object sender, RoutedEventArgs e)
        {
            
            if (string.IsNullOrEmpty(cur_pas_box.Password) 
                || string.IsNullOrEmpty(pas_box.Password)
                || string.IsNullOrEmpty(pas_box2.Password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            
            if (pas_box.Password != pas_box2.Password)
            {
                MessageBox.Show("Новые пароли не совпадают!");
                return;
            }
 
            if (pas_box.Password.Length < 6)
            {
                MessageBox.Show("Новый пароль слишком короткий, должно быть минимум 6 символов!");
                return;
            }

            bool en = true;
            bool number = false;

            for (int i = 0; i < pas_box.Password.Length; i++)
            {
                if (pas_box.Password[i] >= '0' && pas_box.Password[i] <= '9')
                    number = true;
                else if (!((pas_box.Password[i] >= 'A' && pas_box.Password[i] <= 'Z')
                        || (pas_box.Password[i] >= 'a' && pas_box.Password[i] <= 'z')))
                    en = false;
            }

            if (!en)
            {
                MessageBox.Show("Используйте только английскую раскладку!");
                return;
            }
            if (!number)
            {
                MessageBox.Show("Добавьте хотя бы одну цифру!");
                return;
            }

            
             using (var db = new Entities()){
                var user = db.User.FirstOrDefault(u => u.id_user == CurrentUser.IdUser);

                

                
                string hashedCurrentPassword = GetHash(cur_pas_box.Password);
                if (user.password != hashedCurrentPassword)
                {
                    MessageBox.Show("Текущий пароль введен неверно!");
                    return;
                }
                user.password = GetHash(pas_box.Password);
                db.SaveChanges();
                MessageBox.Show("Пароль успешно изменен!", "Успех");
                NavigationService?.Navigate(new ProfileClientPage());
             }
        }

        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void back_b_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ProfileClientPage()); 
        }
    }
}
