using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

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

        public static string ConnString = String.Format
            (
                    "Server={0};Username={1};Database={2};Port={3};Password={4};SSLMode=Prefer",
                    Host,
                    User,
                    DBname,
                    Port,
                    Password
            );

        /// <summary>
        /// Получение пароля из файла
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPassword(string path)
        {
            string pass;

            using (StreamReader sr = new StreamReader(path))
            {
#pragma warning disable CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.
                pass = sr.ReadLine();
#pragma warning restore CS8600 // Преобразование литерала, допускающего значение NULL или возможного значения NULL в тип, не допускающий значение NULL.
            }

#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            return pass;
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }

        /// <summary>
        /// Проверяет сущестует ли аккаунт, если не существует, то создает новый
        /// </summary>
        /// <param name="name"></param>
        /// <param name="age"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool CreateAccount(string name, int age, string email, string password, string color)
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
                    using (var command = new NpgsqlCommand("INSERT INTO users(name, age, email, password, color) VALUES (@p1, @p2, @p3, @p4, @p5)", conn))
                    {
                        command.Parameters.AddWithValue("p1", name);
                        command.Parameters.AddWithValue("p2", age);
                        command.Parameters.AddWithValue("p3", email);
                        command.Parameters.AddWithValue("p4", password);
                        command.Parameters.AddWithValue("p5", color);

                        command.ExecuteNonQuery();
                    }
                }

                conn.Dispose();

                return userIsNotExistsAndPasswordNotNull;
            }
        }

        /// <summary>
        /// Проверяет существует ли пользователь с указанным именем и паролем
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Собирает названия всех созданных чатов и их приватность
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Создает новый чат если имя не занято
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Создает таблицу чата в БД если имя не занято
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool OpenChat(string name, string password)
        {
            bool isCreated = false;

            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                string createChatCommand = $"CREATE TABLE public.\"{name}\" (message_id SERIAL PRIMARY KEY, sender character varying(12), message character varying(180), color character varying(12));";
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

        /// <summary>
        /// Собирает все сообщения указанного пользователя
        /// </summary>
        /// <param name="username"></param>
        /// <param name="chatName"></param>
        /// <returns></returns>
        public static string GetAllMessageFromUser()
        {
            string messages = "";
            int messagesCount = 0;

            using(var conn =  new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var command = new NpgsqlCommand($"SELECT COUNT(*) FROM \"{InfoClass.GlobalChatName}\"", conn))
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

                var command = new NpgsqlCommand($"SELECT * FROM \"{InfoClass.GlobalChatName}\" WHERE sender LIKE '{InfoClass.GlobalUser}' ORDER BY \"message_id\" ASC", conn);

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

        /// <summary>
        /// Записывает сообщение в таблицу чата
        /// </summary>
        /// <param name="username"></param>
        /// <param name="chatName"></param>
        /// <param name="message"></param>
        public static void SendMessage(string message)
        {
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var command = new NpgsqlCommand($"INSERT INTO \"{InfoClass.GlobalChatName}\" (sender, message, color) VALUES('{InfoClass.GlobalUser}', '{message}', (SELECT color FROM users WHERE name LIKE '{InfoClass.GlobalUser}'))", conn))
                {
                    command.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        /// <summary>
        /// Собирает все сообщения в формате(отправитель, сообщение, цвет отправителя) в список
        /// </summary>
        /// <param name="chatName"></param>
        /// <returns></returns>
        public static List<string> GetAllMassagesFromChat()
        {
            List<string> parameters = new List<string>();

            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                var command = new NpgsqlCommand($"SELECT * FROM \"{InfoClass.GlobalChatName}\" ORDER BY \"message_id\" ASC", conn);

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    parameters.Add(reader.GetString(1));
                    parameters.Add(reader.GetString(2));
                    parameters.Add(reader.GetString(3));
                }

                reader.Close();
                conn.Close();
            }

            return parameters;
        }

        /// <summary>
        /// Считывает с БД количество сообщений на текущий момент
        /// </summary>
        public static int GetCurrentMessagesCount()
        {
            int count;
            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using(var command = new NpgsqlCommand($"SELECT COUNT(*) FROM \"{InfoClass.GlobalChatName}\"", conn))
                {
                    var reader = command.ExecuteReader();

                    reader.Read();
                    count = reader.GetInt32(0);
                }

                conn.Close();                
            }
            return count;
        }

        /// <summary>
        /// Проверяет чат на наличие новых сообщений
        /// </summary>
        /// <param name="chatName"></param>
        /// <param name="lastMessagesCount"></param>
        /// <returns></returns>
        public static bool NewMessageHasAppeared()
        {
            bool HasAppeared = false;
            int messagesCount = GetCurrentMessagesCount();

            if(messagesCount != InfoClass.GlobalLastMessagesCount)
            {
                HasAppeared = true;                
            }

            return HasAppeared;
        }

        /// <summary>
        /// Определяет сколько новых сообщений повилось в чате
        /// </summary>
        /// <returns></returns>
        public static int NewMessagesCount()
        {
            int messagesCount = GetCurrentMessagesCount();

            int newMessages = messagesCount - InfoClass.GlobalLastMessagesCount;

            InfoClass.GlobalLastMessagesCount = messagesCount;

            return newMessages;
        }

        /// <summary>
        /// Поиск сообщений содержащих подстроку 'findThisSubstring'.
        /// При 'methodState' = (true - Искать по текущему пользователю, false - Искать среди всех пользователей)
        /// </summary>
        /// <param name="findThisSubstring"></param>
        /// <param name="methodState"></param>
        /// <returns></returns>
        public static List<string> FindMessagesFromUserOrAll(string findThisSubstring, bool methodState)
        {
            if (findThisSubstring == null) findThisSubstring = "";

            List<string> messagesFromUser = new List<string>();
            string commandString = $"SELECT * FROM \"{InfoClass.GlobalChatName}\" WHERE \"message\" " +
                $"LIKE CONCAT('%', '{findThisSubstring}', '%') ORDER BY \"message_id\" ASC";

            if (methodState)
            {
                commandString = $"SELECT * FROM \"{InfoClass.GlobalChatName}\" WHERE \"message\" " +
                    $"LIKE CONCAT('%', '{findThisSubstring}', '%') AND \"sender\" LIKE '{InfoClass.GlobalUser}' ORDER BY \"message_id\" ASC";
            }

            using(var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var command = new NpgsqlCommand(commandString, conn))
                {
                    var reader = command.ExecuteReader();

                    while(reader.Read())
                    {
#pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
                        messagesFromUser.Add(Convert.ToString(reader.GetValue(0)));
#pragma warning restore CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
#pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
                        messagesFromUser.Add(Convert.ToString(reader.GetValue(2)));
#pragma warning restore CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
                    }

                    reader.Close();
                }

                conn.Close();
            }

            return messagesFromUser;
        }

        /// <summary>
        /// Поиск сообщений от пользователя 'username' содержащих подстроку 'findThisSubstring'
        /// </summary>
        /// <param name="findThisSubstring"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static List<string> FindMessagesFromUserOrAll(string findThisSubstring, string username)
        {
            if (findThisSubstring == null) findThisSubstring = "";
            if (username == null) username = "";

            List<string> messagesFromUser = new List<string>();
            string commandString = $"SELECT * FROM \"{InfoClass.GlobalChatName}\" WHERE \"message\" " +
                $"LIKE CONCAT('%', '{findThisSubstring}', '%') AND \"sender\" LIKE '{username}' ORDER BY \"message_id\" ASC";

            using (var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using (var command = new NpgsqlCommand(commandString, conn))
                {
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
#pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
                        messagesFromUser.Add(Convert.ToString(reader.GetValue(0)));
#pragma warning restore CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
#pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
                        messagesFromUser.Add(Convert.ToString(reader.GetValue(2)));
#pragma warning restore CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
                    }

                    reader.Close();
                }

                conn.Close();
            }

            return messagesFromUser;
        }

        /// <summary>
        /// Возвращает сообщение текущего пользователя по ID
        /// </summary>
        /// <param name="username"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static string GetMessageFromUsernameAndId(uint messageId)
        {
            string message = "";
            string comm = $"SELECT \"message\" FROM \"{InfoClass.GlobalChatName}\" WHERE \"message_id\" = {messageId}" +
                $" AND \"sender\" LIKE '{InfoClass.GlobalUser}'";

            using(var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using(var command = new NpgsqlCommand(comm, conn))
                {
                    var reader = command.ExecuteReader();

                    reader.Read();
                    message = reader.GetString(0);

                    reader.Close();
                }

                conn.Close();
            }

            return message;
        }

        /// <summary>
        /// Обновляет сообщение в БД по ID
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="message"></param>
        public static void UpdateMessage(uint messageId, string message)
        {
            string comm = $"UPDATE \"{InfoClass.GlobalChatName}\" " +
                $"SET \"message\" = '{message}' WHERE \"message_id\" = {messageId}";

            using( var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using( var command = new NpgsqlCommand(comm, conn))
                {
                    command.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        /// <summary>
        /// Удаляет сообщение из текущего чата по ID
        /// </summary>
        /// <param name="messageId"></param>
        public static void DeleteMessage(uint messageId)
        {
            string comm = $"DELETE FROM \"{InfoClass.GlobalChatName}\" WHERE \"message_id\" = {messageId}";

            using( var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using(var command = new NpgsqlCommand(comm, conn))
                {
                    command.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        /// <summary>
        /// Возвращает цвет пользователя. 'state' = (true - текущий пользователь, false - указанный)
        /// </summary>
        /// <param name="state"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public static string GetUserColor(bool state, string username)
        {
            string color;

            string user = username;
            if(state)
                user = InfoClass.GlobalUser;

            string comm = $"SELECT \"color\" FROM \"users\" WHERE \"name\" LIKE '{user}'";
 
            using( var conn = new NpgsqlConnection(ConnString))
            {
                conn.Open();

                using(var command = new NpgsqlCommand(comm, conn))
                {
                    var reader = command.ExecuteReader();

                    reader.Read();
                    color = reader.GetString(0);
                    reader.Close();
                }

                conn.Close();
            }

            return color;
        }
    }
}