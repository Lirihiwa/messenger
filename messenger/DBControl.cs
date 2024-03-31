using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

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

                conn.Close();
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

        public static bool OpenChat(string name, string password)
        {
            bool isCreated = false;

            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                string createChatCommand = $"CREATE TABLE public.\"{name}\" (message_id SERIAL PRIMARY KEY, sender character varying(12), message character varying(180));";
                string findChat = $"SELECT * FROM pg_catalog.pg_tables WHERE tablename LIKE '{name}'";
                string findChatInChatsTable = $"SELECT * FROM chats WHERE chat_name LIKE '{name}'";
                
                using(var command =  new NpgsqlCommand(findChatInChatsTable , conn))
                {
                    var reader = command.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        return false;
                    }

                    reader.Read();

                    if(reader.GetString(1) != password)
                    {
                        return false;
                    }

                    reader.Close();
                    command.ExecuteNonQuery();
                }

                using(var command = new NpgsqlCommand(findChat , conn))
                {
                    var reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        isCreated = true;
                    }

                    reader.Close();
                    command.ExecuteNonQuery();
                }

                if (!isCreated)
                {
                    using(var command = new NpgsqlCommand(createChatCommand, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }

            InfoClass.GlobalChatName = name;

            return true;
        }

        public static string GetAllMessageFromUser(string username, string chatName)
        {
            string messages = "";
            int messagesCount = 0;

            using(var conn =  new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var command = new NpgsqlCommand($"SELECT COUNT(*) FROM \"{chatName}\"", conn))
                {
                    var reader = command.ExecuteReader();

                    reader.Read();
                    messagesCount = reader.GetInt32(0);

                    reader.Close();
                }

                conn.Close();
            }

            using(var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                var command = new NpgsqlCommand($"SELECT * FROM \"{chatName}\" WHERE sender LIKE '{username}'", conn);

                var reader = command.ExecuteReader();

                string endOfMessage = "\n\n";

                int i = 1;
                while (reader.Read())
                {
                    if(i == messagesCount)
                    {
                        endOfMessage = "";
                    }
                    messages += reader.GetString(1) + "\n" + reader.GetString(2) + endOfMessage;

                    i++;
                }

                reader.Close();
                conn.Close();
            }

            return messages;
        }

        public static void SendMessage(string username, string chatName, string message)
        {
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var command = new NpgsqlCommand($"INSERT INTO \"{chatName}\" (sender, message) VALUES('{username}', '{message}')", conn))
                {
                    command.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        public static string UpdateChat(string chatName)
        {
            string updatedChat = "";
            int messagesCount = 0;

            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var command = new NpgsqlCommand($"SELECT COUNT(*) FROM \"{chatName}\"", conn))
                {
                    var reader = command.ExecuteReader();

                    reader.Read();
                    messagesCount = reader.GetInt32(0);

                    reader.Close();
                }

                conn.Close();
            }

            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                var command = new NpgsqlCommand($"SELECT * FROM \"{chatName}\"", conn);

                var reader = command.ExecuteReader();

                int i = 1;
                while (reader.Read())
                {
                    string endOfMessage = "\n\n";

                    if (i == messagesCount)
                    {
                        endOfMessage = "";
                    }
                    updatedChat += "(-> " + reader.GetString(1) + " <-)" + "{\n" + reader.GetString(2) + "\n}" + endOfMessage;

                    i++;
                }

                reader.Close();
                conn.Close();
            }

            return updatedChat;
        }
    }
}
