using System;
using System.Threading;
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
using System.Windows.Threading;
using System.CodeDom;
using System.Reflection;
using System.ComponentModel;

namespace messenger
{
    /// <summary>
    /// Логика взаимодействия для ChatPage.xaml
    /// </summary>
    public partial class ChatPage : Page
    {
        BackgroundWorker chatUpdater;

        public ChatPage()
        {
            InitializeComponent();

            usernameLabel.Foreground = SelectedColor(DBControl.GetUserColor(true, ""));
            usernameLabel.Content = InfoClass.GlobalUser;

            ChatViewer.Visibility = Visibility.Visible;
            MessagesViewer.Visibility = Visibility.Hidden;

            InfoClass.GlobalLastMessagesCount = DBControl.GetCurrentMessagesCount();
            List<string> messages = DBControl.GetAllMassagesFromChat();            

            chatName.Content = "Чат: " + InfoClass.GlobalChatName;
            LoadChat(messages);
            ChatViewer.ScrollToBottom();

            chatUpdater = new BackgroundWorker();
            chatUpdater.DoWork += (obj, ea) => UpdateChat();
            chatUpdater.RunWorkerAsync();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditDelete();

            editWindow.Content = new EditMessagePage();

            editWindow.Show();
        }

        private void ScrollDown_Click(object sender, RoutedEventArgs e)
        { 
            ChatViewer.ScrollToBottom();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string message = messageText.Text.Trim();
            if(message.Length > 50 )
            {
                messageText.Text = "Сообщение слишком длинное( " + message.Length + "/50)";

                return;
            }
            DBControl.SendMessage(message);

            ChatViewer.ScrollToBottom();

            messageText.Text = "";
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MessengerMainPage());
        }

        private void LoadChat(List<string> messages)
        {
            string color;
            string sender;
            string message;

            while (messages.Count > 0)
            {
                sender = messages[0];
                message = messages[1];
                color = messages[2];

                string authorIsUser = "";
                if (InfoClass.GlobalUser == sender)
                {
                    authorIsUser = InfoClass.GlobalDelta;
                }

                ChatViewer.Content += authorIsUser + sender + ": " + message;
                
                if (messages.Count != 3)
                {
                    ChatViewer.Content += "\n\n";
                }
                
                for (int i = 0; i < 3; i++) 
                { 
                    messages.RemoveAt(0); 
                }
            }
        }

        private void UpdateChat()
        {
            while(true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.1));

                bool isChatScrollToTheEnd = false;
                ChatViewer.Dispatcher.Invoke(delegate
                {
                    if (ChatViewer.VerticalOffset == ChatViewer.ScrollableHeight)
                        isChatScrollToTheEnd = true;
                });

                if (isChatScrollToTheEnd)
                {
                    NewMessageAlert.Dispatcher.Invoke(delegate 
                    {
                        NewMessageAlert.Visibility = Visibility.Hidden;
                    });
                }

                if (DBControl.NewMessageHasAppeared())
                {
                    List<string> messages = DBControl.GetAllMassagesFromChat();

                    int newMesagesCount = DBControl.NewMessagesCount();                    
                    
                    for(int i = newMesagesCount; i > 0; i--)
                    {
                        string sender = messages[messages.Count - (i * 3)];
                        string message = messages[messages.Count - (i * 3 - 1)];
                        string color = messages[messages.Count - (i * 3 - 2)];

                        string authorIsUser = "";
                        if(InfoClass.GlobalUser ==  sender)
                        {
                            authorIsUser = InfoClass.GlobalDelta;
                        }

                        ChatViewer.Dispatcher.Invoke(delegate 
                        { 
                            ChatViewer.Content += "\n\n" + authorIsUser + sender + ": " + message;
                        });

                        ChatViewer.Dispatcher.Invoke(delegate
                        {
                            if(ChatViewer.VerticalOffset ==  ChatViewer.ScrollableHeight) 
                                isChatScrollToTheEnd = true;
                        });


                        Visibility isHidden = Visibility.Visible;
                        ChatViewer.Dispatcher.Invoke(delegate
                        {
                            isHidden = ChatViewer.Visibility;
                        });

                        NewMessageAlert.Dispatcher.Invoke(delegate
                        {
                            if (!isChatScrollToTheEnd | (isHidden == Visibility.Hidden))
                            {
                                NewMessageAlert.Visibility = Visibility.Visible;
                            }                          
                        });
                    }
                }
            }            
        }

        private SolidColorBrush SelectedColor(string color)
        {
            if (color == "White")
            {
                return Brushes.White;
            }
            else if (color == "Gold")
            {
                return Brushes.Gold;
            }
            else if (color == "HotPink")
            {
                return Brushes.HotPink;
            }
            else if (color == "Coral")
            {
                return Brushes.Coral;
            }

            return Brushes.White;
        }

        private void FindMessage_Click(object sender, RoutedEventArgs e)
        {
            MessagesViewer.Content = "";

            ReturnChat_Button.Visibility = Visibility.Visible;
            ChatViewer.Visibility = Visibility.Hidden;
            MessagesViewer.Visibility = Visibility.Visible;

            string name = nameText.Text;
            string keyWord = keyWordText.Text;

            List<string> messages = DBControl.FindMessagesFromUserOrAll(keyWord, name);

            if (messages.Count == 0)
            {
                MessagesViewer.Content = "Ничего не найдено";
                return;
            }

            MessagesViewer.Content = "id -> message\n\n";

            while (messages.Count > 0)
            {
                if (messages.Count == 2)
                {
                    MessagesViewer.Content += messages[0] + " -> " + messages[1];
                }
                else
                {
                    MessagesViewer.Content += messages[0] + " -> " + messages[1] + "\n\n";
                }

                messages.RemoveAt(0);
                messages.RemoveAt(0);
            }
        }

        private void ReturnChat_Click(object sender, RoutedEventArgs e)
        {
            ChatViewer.Visibility = Visibility.Visible;
            ChatViewer.ScrollToBottom();

            MessagesViewer.Visibility = Visibility.Hidden;
            MessagesViewer.Content = "";

            ReturnChat_Button.Visibility = Visibility.Hidden;
        }
    }
}