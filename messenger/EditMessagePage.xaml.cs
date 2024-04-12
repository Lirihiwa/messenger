using System;
using System.Collections.Generic;
using System.Linq;
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

        private void Look_Button(object sender, RoutedEventArgs e)
        {
            string? username = InfoClass.GlobalUser;
            string? chatName = InfoClass.GlobalChatName;

            messagesViewer.Content = DBControl.GetAllMessageFromUser();
        }
    }
}
