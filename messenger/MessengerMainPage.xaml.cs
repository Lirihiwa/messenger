using Microsoft.Win32;
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
    /// Логика взаимодействия для MessengerMainPage.xaml
    /// </summary>
    public partial class MessengerMainPage : Page
    {
        public MessengerMainPage()
        {
            InitializeComponent();

            Acc.Content = User.GlobalUser;
        }

        private void ShowChats_Click(object sender, RoutedEventArgs e)
        {
            AllowedChats_ScrollViewer.Content = DBControl.GetAllowedChats();

            showChatsButton.Content = "Обновить";

            //Это просто ненужный код, но потом он может пригодиться)
            //OpenFileDialog op = new OpenFileDialog();
            //op.Title = "Select a picture";
            //op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
            //  "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
            //  "Portable Network Graphic (*.png)|*.png";
            //if (op.ShowDialog() == true)
            //{
            //    PICTURE.Source = new BitmapImage(new Uri(op.FileName));
            //}
        }

        private void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            string name = chatName.Text.Trim();
            string password = chatPass.Text.Trim();

            if(name == "")
            {
                errorContent.Foreground = Brushes.Red;
                errorContent.Content = "Введите название чата!";
            }
            else if(name.Length > 12)
            {
                errorContent.Foreground = Brushes.Red;
                errorContent.Content = "Название чата слишком длинное(max 12)!";
            }
            else if(password == "")
            {
                errorContent.Foreground = Brushes.Red;
                errorContent.Content = "Введите пароль!";
            }
            else if(password.Length > 16)
            {
                errorContent.Foreground = Brushes.Red;
                errorContent.Content = "Пароль слишком длинный(max 16)!";
            }
            else if(DBControl.CreateChat(name, password))
            {
                errorContent.Foreground = Brushes.Green;
                errorContent.Content = "Чат успешно создан!";
            }
            else
            {
                errorContent.Foreground = Brushes.Red;
                errorContent.Content = "Чат уже существует!";
            }
        }
    }
}
