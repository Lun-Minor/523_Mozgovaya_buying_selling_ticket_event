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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private int failedAttempts = 0;
      //  private User currentUser;

        public LoginPage()
        {
            InitializeComponent();
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new MainPage());
        }

        private void bt_reg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegistrationPage());
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
        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(log_tb.Text) ||
            string.IsNullOrEmpty(password_box.Password))
            {
                MessageBox.Show("Введите логин или пароль");
                return;
            }

            string hashedPassword = GetHash(password_box.Password);

            using (var db = new Entities2())
            {
                var user = db.User.AsNoTracking().FirstOrDefault(u => u.login == log_tb.Text && u.password == hashedPassword);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!");
                    failedAttempts++;
                    if (failedAttempts >= 3)
                    {
                        if (captchaPanel.Visibility != Visibility.Visible)
                        {
                            CaptchaSwitch();
                        }
                        CaptchaChange();
                    }
                    return;
                }

                else
                {
                    MessageBox.Show("Пользователь успешно найден!");

                    switch (user.role.ToString())
                    {
                        case "3":
                            NavigationService?.Navigate(new UserPage());
                            break;
                        case "1":
                            NavigationService?.Navigate(new AdminPage());
                            break;
                            case "2": NavigationService?.Navigate(new OrganizatorPage()); break;

                    }
                }
            }

        }

        public void CaptchaSwitch()
        {
            switch (captchaPanel.Visibility)
            {
                case Visibility.Visible:
                    log_tb.Clear();
                    password_box.Clear();

                    captchaPanel.Visibility = Visibility.Hidden;

                    log_tb.Visibility = Visibility.Visible;
                    password_box.Visibility = Visibility.Visible;
                    ButtonEnter.Visibility = Visibility.Visible;
                    bt_reg.Visibility = Visibility.Visible;
                    return;

                case Visibility.Hidden:
                    captchaPanel.Visibility = Visibility.Visible;

                    log_tb.Visibility = Visibility.Hidden;
                    password_box.Visibility = Visibility.Hidden;
                    ButtonEnter.Visibility = Visibility.Hidden;
                    bt_reg.Visibility = Visibility.Hidden;
                    return;
            }
        }
        public void CaptchaChange()

        {

            String allowchar = " ";
            allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            allowchar += "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z";
            allowchar += "1,2,3,4,5,6,7,8,9,0";
            char[] a = { ',' };
            String[] ar = allowchar.Split(a);
            String pwd = "";
            string temp = "";
            Random r = new Random();

            for (int i = 0; i < 6; i++)
            {
                temp = ar[(r.Next(0, ar.Length))];
                pwd += temp;
            }
            captcha.Text = pwd;
        }
        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text != captcha.Text)
            {
                MessageBox.Show("Неверно введена капча", "Ошибка");
                CaptchaChange();

            }
            else
            {
                MessageBox.Show("Капча введена успешно, можете продолжить авторизацию", "Успех");
                CaptchaSwitch();
                failedAttempts = 0;
            }
        }
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
            e.Command == ApplicationCommands.Cut ||
            e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void txtHintLogin_MouseLeftButtonUp(object sender,
MouseButtonEventArgs e)
        {
            log_tb.Focus();
        }

        private void txtHintPass_MouseLeftButtonUp(object sender,
        MouseButtonEventArgs e)
        {
            password_box.Focus();
        }
    }



        
}


