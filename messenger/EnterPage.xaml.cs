using Npgsql.Replication.PgOutput.Messages;
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

namespace messenger
{
    /// <summary>
    /// Логика взаимодействия для EnterPage.xaml
    /// </summary>

    

    public partial class EnterPage : Page
    {
        public EnterPage()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AuthPage());
        }

        public void Enter_Click(object sender, RoutedEventArgs e)
        {
            string name = name_TextBox.Text.Trim();
            string password = password_PasswordBox.Password.Trim();

            if(DBControl.TryEnter(name, password))
            {
                User.GlobalUser = name;
                NavigationService.Navigate(new MessengerMainPage());
            }
            else
            {
                error_Label.Foreground = Brushes.Red;
                if(name_TextBox.Text == "")
                {
                    error_Label.Content = "Введите логин.";
                }
                else if (password_PasswordBox.Password == "")
                {
                    error_Label.Content = "Введите пароль.";
                }
                else
                {
                    error_Label.Content = "Аккаунт не найден";
                }  
            }
        }
    }
}
