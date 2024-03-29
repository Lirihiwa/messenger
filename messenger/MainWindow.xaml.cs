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

    public class DataBase
    {
        private static string StringPath = @"C:\Users\Lirihiwa\Desktop\Данные от БД.txt";

        private static string Host = "localhost";
        private static string User = "postgres";
        private static string DBname = "messenger";
        private static string Password = GetPassword(StringPath);
        private static string Port = "5432";
        
        public static string ConnString =
                String.Format(
                    "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                    Host,
                    User,
                    DBname,
                    Port,
                    Password);

        public static string GetPassword(string path)
        {
            string pass;

            using(StreamReader sr = new StreamReader(path))
            {
                pass =  sr.ReadLine();
            }

            return pass;
        }

        public static bool CreateAccount(string name, int age, string email, string password)
        {
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                bool userIsNotExists = true;

                using (var command = new NpgsqlCommand($"SELECT * FROM users WHERE name = '{name}'", conn))
                {
                    var reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        userIsNotExists = false;
                    }
                    reader.Close();

                    command.ExecuteNonQuery();
                }

                bool userIsNotExistsAndPasswordNotNull = userIsNotExists & password != "";

                if (userIsNotExistsAndPasswordNotNull)
                {
                    using (var command = new NpgsqlCommand("INSERT INTO users(name, age, email, password) VALUES (@p1, @p2, @p3, @p4)", conn))
                    {
                        command.Parameters.AddWithValue("p1", name);
                        command.Parameters.AddWithValue("p2", age);
                        command.Parameters.AddWithValue("p3", email);
                        command.Parameters.AddWithValue("p4", password);

                        command.ExecuteNonQuery();
                    }
                } 

                conn.Dispose();
                conn.Close();
                return userIsNotExistsAndPasswordNotNull;
            }
        }

        public static bool TryEnter(string name, string password)
        {
            bool canEnter = false;

            using(var conn = new NpgsqlConnection( ConnString))
            {
                conn.Open();

                using(var command = new NpgsqlCommand("SELECT * FROM users WHERE name LIKE @p1 and password LIKE @p2;", conn))
                {
                    command.Parameters.AddWithValue("p1", name);
                    command.Parameters.AddWithValue("p2", password);

                    var reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        canEnter = true;
                    }
                }


                conn.Dispose();
                conn.Close();
            }

            return canEnter;
        }

        public static string GetAllowedChats()
        {
            string chats="";


            using var conn = new NpgsqlConnection(ConnString);
            conn.Open();

            using var command = new NpgsqlCommand("SELECT * FROM chats", conn);

            using NpgsqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                chats += reader.GetString(0) + "\n";
            }


            return chats;
        }
    }
}