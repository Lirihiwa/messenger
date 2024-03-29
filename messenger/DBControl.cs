using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace messenger
{
    class DBControl
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

            using (StreamReader sr = new StreamReader(path))
            {
                pass = sr.ReadLine();
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

                return userIsNotExistsAndPasswordNotNull;
            }
        }

        public static bool TryEnter(string name, string password)
        {
            bool canEnter = false;

            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var command = new NpgsqlCommand("SELECT * FROM users WHERE name LIKE @p1 and password LIKE @p2;", conn))
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
            }

            return canEnter;
        }

        public static string GetAllowedChats()
        {
            string chats = "";

            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                int chatsCount = 0;

                using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM chats", conn))
                {
                    var reader = command.ExecuteReader();

                    reader.Read();
                    chatsCount = reader.GetInt32(0);  
                    
                    reader.Close();
                }

                using (var command = new NpgsqlCommand("SELECT * FROM chats", conn))
                {
                    using NpgsqlDataReader reader = command.ExecuteReader();

                    int i = 1;
                    while (reader.Read())
                    {
                        string newStroke = "";

                        if(i != chatsCount)
                        {
                            newStroke = "\n";
                        }


                        if(reader.GetString(1) == "0")
                        {
                            chats += reader.GetString(0) + " (общий)" + newStroke;
                        }
                        else
                        {
                            chats += reader.GetString(0) + " (приватный)" + newStroke;
                        }

                        i++;
                    }

                    reader.Close();
                }

            conn.Dispose();
            }
            
            
            return chats;
        }

        public static bool CreateChat(string name, string password)
        {
            bool wasCreate = false;

            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                bool canCreate = true;
                using (var command = new NpgsqlCommand($"SELECT * FROM chats WHERE chat_name = '{name}'", conn))
                {
                    var reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        canCreate = false;
                    }

                    reader.Close();
                }

                if(canCreate)
                {
                    using (var command = new NpgsqlCommand($"INSERT INTO chats(chat_name, chat_password) VALUES('{name}', '{password}')", conn))
                    {
                        command.ExecuteNonQuery();
                    }

                    wasCreate = true;
                }               
            }

            return wasCreate;
        }
    }
}
