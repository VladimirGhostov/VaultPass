using System;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.IO;

namespace VaultPass
{
    public static class Database
    {
        private static string? connectionString;
        private static SQLiteConnection? connection;

        public static void Connect()
        {
            try
            {
                // Используем папку Documents (там точно есть права на запись)
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string appFolder = Path.Combine(documentsPath, "VaultPass");

                // Создаём папку, если её нет
                Directory.CreateDirectory(appFolder);

                // Путь к базе данных
                string dbPath = Path.Combine(appFolder, "vaultpass.db");

                // Формируем строку подключения
                connectionString = $"Data Source={dbPath};Version=3;";

                connection = new SQLiteConnection(connectionString);
                connection.Open();
                CreateTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}");
            }
        }

        private static void CreateTable()
        {
            string query = @"CREATE TABLE IF NOT EXISTS passwords (
                            id TEXT PRIMARY KEY,
                            login TEXT NOT NULL,
                            password TEXT NOT NULL)";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static void Close()
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        public static void InsertData(string id, string login, string password)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO passwords (id, login, password) VALUES (@id, @login, @password)";
                using (var command = new SQLiteCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static DataTable ReadData()
        {
            DataTable dataTable = new DataTable();
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT id, login, password FROM passwords";
                using (var adapter = new SQLiteDataAdapter(query, conn))
                {
                    adapter.Fill(dataTable);
                }
            }
            return dataTable;
        }

        public static void DeleteData(string value)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM passwords WHERE id = @value OR login = @value OR password = @value";
                using (var command = new SQLiteCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@value", value);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void UpdateData(string id, string newLogin, string newPassword)
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE passwords SET login = @login, password = @password WHERE id = @id";
                using (var command = new SQLiteCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@login", newLogin);
                    command.Parameters.AddWithValue("@password", newPassword);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void BackupDatabase(string destinationPath)
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string appFolder = Path.Combine(documentsPath, "VaultPass");
                string sourcePath = Path.Combine(appFolder, "vaultpass.db");

                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destinationPath, true);
                }
                else
                {
                    throw new FileNotFoundException("Файл базы данных не найден");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании бэкапа: {ex.Message}");
            }
        }

        public static void RestoreDatabase(string sourcePath)
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string appFolder = Path.Combine(documentsPath, "VaultPass");
                string destinationPath = Path.Combine(appFolder, "vaultpass.db");

                Close();

                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destinationPath, true);
                }
                else
                {
                    throw new FileNotFoundException("Файл бэкапа не найден");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при восстановлении: {ex.Message}");
            }
        }
    }
}