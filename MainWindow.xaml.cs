using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace VaultPass
{
    public partial class MainWindow : Window
    {
        private System.Collections.ObjectModel.ObservableCollection<PasswordEntry>? _allEntries;

        public MainWindow()
        {
            InitializeComponent();

            // Загружаем кастомный курсор
            var cursorStream = Application.GetResourceStream(
                new Uri("pack://application:,,,/Cursors/cursor.cur", UriKind.RelativeOrAbsolute)).Stream;
            this.Cursor = new System.Windows.Input.Cursor(cursorStream);

            Database.Connect();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                DataTable dataTable = Database.ReadData();

                // Создаём список всех записей
                var allEntries = new System.Collections.ObjectModel.ObservableCollection<PasswordEntry>();

                foreach (DataRow row in dataTable.Rows)
                {
                    // Добавляем проверку на null с оператором ??
                    string id = row["id"]?.ToString() ?? string.Empty;
                    string login = row["login"]?.ToString() ?? string.Empty;
                    string password = row["password"]?.ToString() ?? string.Empty;

                    allEntries.Add(new PasswordEntry
                    {
                        Id = id,
                        Login = login,
                        Password = password
                    });
                }

                // Сохраняем все записи для поиска
                _allEntries = allEntries;

                // Применяем текущий фильтр поиска
                ApplySearchFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void ApplySearchFilter()
        {
            try
            {
                if (_allEntries == null) return;

                string searchText = TxtSearch.Text.ToLower();

                // Фильтруем записи
                var filteredEntries = new System.Collections.ObjectModel.ObservableCollection<PasswordEntry>();

                foreach (var entry in _allEntries)
                {
                    if (string.IsNullOrWhiteSpace(searchText) ||
                        entry.Id.ToLower().Contains(searchText) ||
                        entry.Login.ToLower().Contains(searchText) ||
                        entry.Password.ToLower().Contains(searchText))
                    {
                        filteredEntries.Add(entry);
                    }
                }

                // Привязываем отфильтрованный список
                DataGridPasswords.ItemsSource = filteredEntries;

                // Обновляем статус
                TxtStatus.Text = $"Найдено записей: {filteredEntries.Count} из {_allEntries.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}");
            }
        }

        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Диалог сохранения файла
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Сохранить бэкап базы данных",
                    Filter = "База данных SQLite (*.db)|*.db|Все файлы (*.*)|*.*",
                    DefaultExt = ".db",
                    FileName = $"vaultpass_backup_{DateTime.Now:yyyy-MM-dd_HH-mm}.db"
                };

                if (dialog.ShowDialog() == true)
                {
                    Database.BackupDatabase(dialog.FileName);
                    TxtStatus.Text = $"✅ Бэкап сохранён: {Path.GetFileName(dialog.FileName)}";

                    // Милое сообщение
                    MessageBox.Show($"❤️ Бэкап успешно сохранён!\n\nФайл: {dialog.FileName}",
                                   "Ура!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка при создании бэкапа:\n{ex.Message}",
                               "Ой!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Диалог выбора файла
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Выберите файл бэкапа",
                    Filter = "База данных SQLite (*.db)|*.db|Все файлы (*.*)|*.*",
                    DefaultExt = ".db"
                };

                if (dialog.ShowDialog() == true)
                {
                    // Спрашиваем подтверждение
                    var result = MessageBox.Show(
                        "⚠️ Восстановление заменит текущую базу данных!\n\n" +
                        "Все несохранённые данные будут потеряны.\n\n" +
                        "Вы уверены?",
                        "Подтверждение восстановления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        Database.RestoreDatabase(dialog.FileName);

                        // Перезагружаем данные
                        Database.Connect(); // Переподключаемся
                        LoadData();

                        TxtStatus.Text = $"✅ Восстановлено из: {Path.GetFileName(dialog.FileName)}";

                        MessageBox.Show($"❤️ База данных успешно восстановлена!",
                                       "Всё получилось!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"💔 Ошибка при восстановлении:\n{ex.Message}",
                               "Ой!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplySearchFilter();
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Clear();
            ApplySearchFilter();
        }

        private void DataGridPasswords_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                // Получаем элемент, на который реально кликнули
                var originalSource = e.OriginalSource as FrameworkElement;

                // Поднимаемся по дереву элементов, чтобы найти ячейку
                var cell = originalSource?.Parent as DataGridCell;
                while (cell == null && originalSource != null)
                {
                    originalSource = originalSource.Parent as FrameworkElement;
                    cell = originalSource as DataGridCell;
                }

                // Если нашли ячейку - копируем её содержимое
                if (cell != null)
                {
                    // Получаем колонку и строку
                    var column = cell.Column;
                    var row = cell.DataContext as PasswordEntry;

                    if (row != null)
                    {
                        string copiedText = "";

                        // Определяем, какая это колонка по заголовку
                        if (column.Header.ToString() == "ID")
                            copiedText = row.Id;
                        else if (column.Header.ToString() == "Логин")
                            copiedText = row.Login;
                        else if (column.Header.ToString() == "Пароль")
                            copiedText = row.Password;

                        if (!string.IsNullOrEmpty(copiedText))
                        {
                            Clipboard.SetText(copiedText);
                            TxtStatus.Text = $"✅ Скопировано: {copiedText}";

                            // Сброс статуса через 2 секунды
                            var timer = new System.Windows.Threading.DispatcherTimer();
                            timer.Interval = TimeSpan.FromSeconds(2);
                            timer.Tick += (s, args) =>
                            {
                                timer.Stop();
                                TxtStatus.Text = "❤️ Готов к работе";
                            };
                            timer.Start();
                            return;
                        }
                    }
                }

                // Если кликнули не на ячейку (на фон строки) - копируем всю строку
                if (DataGridPasswords.SelectedItem is PasswordEntry entry)
                {
                    string rowText = $"{entry.Id}\t{entry.Login}\t{entry.Password}";
                    Clipboard.SetText(rowText);
                    TxtStatus.Text = $"✅ Скопирована строка: {entry.Id}";

                    var timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(2);
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop();
                        TxtStatus.Text = "❤️ Готов к работе";
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при копировании: {ex.Message}");
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, выбрана ли строка
                if (DataGridPasswords.SelectedItem == null)
                {
                    TxtStatus.Text = "❌ Выберите запись для редактирования";
                    return;
                }

                // Получаем выбранную запись
                var selectedEntry = DataGridPasswords.SelectedItem as PasswordEntry;
                if (selectedEntry == null) return;

                // Создаём и показываем окно редактирования
                var editWindow = new EditWindow(selectedEntry)
                {
                    Owner = this // Текущее окно будет родительским
                };

                // Если пользователь нажал "Сохранить"
                if (editWindow.ShowDialog() == true)
                {
                    // Обновляем данные в базе
                    Database.UpdateData(selectedEntry.Id, selectedEntry.Login, selectedEntry.Password);

                    // Перезагружаем данные
                    LoadData();

                    TxtStatus.Text = $"✅ Запись с ID {selectedEntry.Id} обновлена";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}");
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            TxtStatus.Text = "✅ Список обновлён";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Database.Close();
            base.OnClosing(e);
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TxtId.Clear();
            TxtLogin.Clear();
            TxtPassword.Clear();
            TxtStatus.Text = "✅ Поля очищены";
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataGridPasswords.SelectedCells.Count == 0)
                {
                    TxtStatus.Text = "❌ Выберите ячейку для удаления";
                    return;
                }

                var cell = DataGridPasswords.SelectedCells[0];
                var cellContent = cell.Column.GetCellContent(cell.Item);

                if (cellContent is TextBlock textBlock)
                {
                    string valueToDelete = textBlock.Text;

                    if (string.IsNullOrWhiteSpace(valueToDelete))
                    {
                        TxtStatus.Text = "❌ Нельзя удалить пустое значение";
                        return;
                    }

                    var result = MessageBox.Show($"Удалить запись, содержащую '{valueToDelete}'?",
                                                 "Подтверждение",
                                                 MessageBoxButton.YesNo,
                                                 MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        Database.DeleteData(valueToDelete);
                        LoadData();
                        TxtStatus.Text = $"✅ Запись '{valueToDelete}' удалена";
                    }
                }
                else
                {
                    TxtStatus.Text = "❌ Не удалось получить данные ячейки";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            string password = GeneratePassword(10);
            TxtPassword.Text = password;
            Clipboard.SetText(password);
            TxtStatus.Text = "✅ Пароль сгенерирован и скопирован в буфер";
        }

        private string GeneratePassword(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, что поля не пустые
            if (string.IsNullOrWhiteSpace(TxtId.Text) ||
                string.IsNullOrWhiteSpace(TxtLogin.Text) ||
                string.IsNullOrWhiteSpace(TxtPassword.Text))
            {
                MessageBox.Show("Заполните все поля!", "Внимание",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Добавляем в базу данных
                Database.InsertData(TxtId.Text, TxtLogin.Text, TxtPassword.Text);

                // Очищаем поля
                TxtId.Clear();
                TxtLogin.Clear();
                TxtPassword.Clear();

                // Обновляем таблицу
                LoadData();
                TxtStatus.Text = "✅ Запись добавлена!";
            }
            catch (Exception ex)
            {
                // Проверяем, это ошибка уникальности?
                if (ex.Message.Contains("UNIQUE constraint failed") ||
                    ex.Message.Contains("constraint failed"))
                {
                    // Специальное милое сообщение для повтора ID
                    var result = MessageBox.Show(
                        $"💔 ID '{TxtId.Text}' уже существует!\n\nХотите ввести другой ID?",
                        "Ой, совпадение!",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Просто очищаем поле ID для повторного ввода
                        TxtId.Clear();
                        TxtId.Focus(); // Ставим курсор в поле ID
                        TxtStatus.Text = "💡 Введите другой ID";
                    }
                    else
                    {
                        // Очищаем все поля если передумали
                        TxtId.Clear();
                        TxtLogin.Clear();
                        TxtPassword.Clear();
                        TxtStatus.Text = "❤️ Попробуйте снова";
                    }
                }
                else
                {
                    // Другие ошибки
                    MessageBox.Show($"Ошибка: {ex.Message}", "Что-то пошло не так",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}