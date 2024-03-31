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
    /// Логика взаимодействия для ChatPage.xaml
    /// </summary>
    public partial class ChatPage : Page
    {
        public ChatPage()
        {
            InitializeComponent();

            chatName.Content = "Чат: " + InfoClass.GlobalChatName;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            string chatName = InfoClass.GlobalChatName;
            ChatViewer.Content = DBControl.UpdateChat(chatName);
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string username = InfoClass.GlobalUser;
            string chatName = InfoClass.GlobalChatName;
            string message = messageText.Text.Trim();

            DBControl.SendMessage(username, chatName, message);

            messageText.Text = "";
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MessengerMainPage());
        }
    }
}
