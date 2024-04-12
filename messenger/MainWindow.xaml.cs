using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.IO;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.ComponentModel;

namespace messenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainFrame.Content = new AuthPage();
        }
    }
}