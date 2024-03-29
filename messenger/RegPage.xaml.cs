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
    /// Логика взаимодействия для RegPage.xaml
    /// </summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }

        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            error_Label.Content = "";

            try
            {
                string name = name_TextBox.Text.Trim();
                int age = Convert.ToInt32(age_TextBox.Text.Replace(" ", ""));
                string? email = email_TextBox.Text.Replace(" ", "");
                string password = password_PasswordBox.Password.ToString().Replace(" ","");

                if (name.Trim() == "")
                {
                    error_Label.Foreground = Brushes.Red;
                    error_Label.Content = "Введите логин.";
                }
                else if (email == "")
                {
                    error_Label.Foreground = Brushes.Red;
                    error_Label.Content = "Введите email.";
                }
                else if (!email.EndsWith("@gmail.com"))
                {
                    error_Label.Foreground = Brushes.Red;
                    error_Label.Content = "Введите корректный  email.";
                }
                else if (DataBase.CreateAccount(name, age, email, password))
                {
                    error_Label.Foreground = Brushes.Green;
                    error_Label.Content = "Аккаунт успешно создан.";
                }
                else if(password != "")
                {
                    error_Label.Foreground = Brushes.Red;
                    error_Label.Content = "Такое имя уже существует.";
                }
                else { error_Label.Content = "Пароль отсутствует."; }
            }
            catch
            {
                if(name_TextBox.Text.Length > 12) 
                {
                    error_Label.Foreground = Brushes.Red;
                    error_Label.Content = "Длина логина больше 12 символов.";
                }
                else
                {
                    error_Label.Foreground = Brushes.Red;
                    error_Label.Content = "Возраст не число.";
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AuthPage());
        }   
    }
}
