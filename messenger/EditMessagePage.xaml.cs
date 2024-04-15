using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Channels;
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
    /// Логика взаимодействия для EditMessagePage.xaml
    /// </summary>
    public partial class EditMessagePage : Page
    {
        public EditMessagePage()
        {
            InitializeComponent();
        }

        private void FindMessagesByText_Click(object sender, RoutedEventArgs e)
        {
            string findIt = FindIt.Text;
            List<string> messages = DBControl.FindMessagesFromUserOrAll(findIt, true);

            if (messages.Count == 0)
            {
                messagesViewer.Content = "Ничего не найдено";
                return;
            }

            messagesViewer.Content = "id -> message\n\n";

            while (messages.Count > 0)
            {
                if (messages.Count == 2)
                {
                    messagesViewer.Content += messages[0] + " -> " + messages[1];
                }
                else
                {
                    messagesViewer.Content += messages[0] + " -> " + messages[1] + "\n\n";
                }

                messages.RemoveAt(0);
                messages.RemoveAt(0);
            }
        }

        private void FindForEdit_Click(object sender, RoutedEventArgs e)
        {     
            string strId = messageId.Text;
            uint Id;

            if(!uint.TryParse(strId.Trim(), out Id))
            {
                editMessage.Text = "Неверный ID";
            }

            try
            {
                editMessage.Text = DBControl.GetMessageFromUsernameAndId(Id);
            }
            catch
            {
                editMessage.Text = "Под этим ID нет ващего сообщения";
                return;
            }
            
            findForEdit_Button.Visibility = Visibility.Hidden;
            edit_Button.Visibility = Visibility.Visible;
            delete_Button.Visibility = Visibility.Visible;

            editMessage.IsEnabled = true;
            messageId.IsEnabled = false;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            string message = editMessage.Text;
            uint Id = uint.Parse(messageId.Text.Trim());

            if (message == "")
            {
                editMessage.Text = "Нельзя менять на пустоту";
                returnProperties();

                return;
            }      

            try
            {
                DBControl.UpdateMessage(Id, message);
            }
            catch
            {
                editMessage.Text = "Произошла неизвестная ошибка";
                returnProperties();

                return;
            }
            
            editMessage.Text = "Успешно";
            returnProperties();
        }
        
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if(delete_Button.Content == "Подтвердить удаление")
            {
                uint Id = uint.Parse(messageId.Text.Trim());
                DBControl.DeleteMessage(Id);

                delete_Button.Foreground = Brushes.Black;
                delete_Button.Content = "Удалить";

                editMessage.Text = "Успешно";
                returnProperties();
            }
            else
            {
                delete_Button.Foreground = Brushes.Red;
                delete_Button.Content = "Подтвердить удаление";
            }
        }

        private void returnProperties()
        {
            messageId.IsEnabled = true;
            editMessage.IsEnabled = false;

            edit_Button.Visibility = Visibility.Hidden;
            delete_Button.Visibility = Visibility.Hidden;
            findForEdit_Button.Visibility = Visibility.Visible;
        }       
    }
}
