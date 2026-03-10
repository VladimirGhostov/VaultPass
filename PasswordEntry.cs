using System.ComponentModel;

namespace VaultPass
{
    public class PasswordEntry : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _login = string.Empty;
        private string _password = string.Empty;

        public string Id
        {
            get => _id;
            set
            {
                _id = value ?? string.Empty;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string Login
        {
            get => _login;
            set
            {
                _login = value ?? string.Empty;
                OnPropertyChanged(nameof(Login));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value ?? string.Empty;
                OnPropertyChanged(nameof(Password));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}