using System.Windows;

namespace VaultPass
{
    public partial class EditWindow : Window
    {
        private PasswordEntry _entry;

        public EditWindow(PasswordEntry entry)
        {
            InitializeComponent();

            // Сохраняем ссылку на запись
            _entry = entry;

            // Заполняем поля
            EditId.Text = entry.Id;
            EditLogin.Text = entry.Login;
            EditPassword.Text = entry.Password;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, что поля не пустые
            if (string.IsNullOrWhiteSpace(EditLogin.Text) ||
                string.IsNullOrWhiteSpace(EditPassword.Text))
            {
                MessageBox.Show("Логин и пароль не могут быть пустыми!",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Обновляем данные в объекте
            _entry.Login = EditLogin.Text;
            _entry.Password = EditPassword.Text;

            // Помечаем, что нужно обновить в базе
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}