using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Windows;

namespace VaultPass
{
    public static class Database
    {
        private static string connectionString = "Data Source=vaultpass.db;Version=3;";

        // Подключение к базе данных
        public static void Connect()
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    CreateTable(connection);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}");
            }
        }

        // Создание таблицы
        private static void CreateTable(SQLiteConnection connection)
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

        // Добавление записи
        public static void InsertData(string id, string login, string password)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO passwords (id, login, password) VALUES (@id, @login, @password)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Чтение всех данных
        public static DataTable ReadData()
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT id, login, password FROM passwords";
                    using (var adapter = new SQLiteDataAdapter(query, connection))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении: {ex.Message}");
            }
            return dataTable;
        }

        public static void UpdateData(string id, string newLogin, string newPassword)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE passwords SET login = @login, password = @password WHERE id = @id";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@login", newLogin);
                        command.Parameters.AddWithValue("@password", newPassword);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}");
            }
        }

        // Удаление записи
        public static void DeleteData(string value)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM passwords WHERE id = @value OR login = @value OR password = @value";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@value", value);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }

        public static void BackupDatabase(string destinationPath)
        {
            try
            {
                // Путь к текущей базе данных
                string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vaultpass.db");

                // Если файл существует - копируем
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

        // Восстановление из бэкапа
        public static void RestoreDatabase(string sourcePath)
        {
            try
            {
                // Путь к текущей базе данных
                string destinationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vaultpass.db");

                // Закрываем соединение перед копированием
                Close();

                // Если файл существует - заменяем
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

        // Закрывать отдельно теперь не нужно, так как using сам закрывает соединение
        public static void Close()
        {
            // Метод оставлен для совместимости, но ничего не делает
        }
    }
}