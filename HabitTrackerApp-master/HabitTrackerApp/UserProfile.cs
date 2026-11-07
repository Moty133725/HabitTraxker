using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HabitTrackerApp
{
    /// <summary>
    /// Класс, представляющий профиль пользователя и его данные.
    /// Реализует интерфейс INotifyPropertyChanged для обновления привязок.
    /// </summary>
    public class UserProfile : INotifyPropertyChanged
    {
        private string _firstName;
        private string _lastName;
        private string _password;
        private DateTime _birthDate;
        private string _education;
        private ObservableCollection<string> _hobbies;
        private bool _receiveNotifications;
        private bool _showPublicStats;
        private bool _autoSave;
        private string _activityLevel;
        private string _avatarPath;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string FirstName
        {
            get => _firstName;
            set { _firstName = value; OnPropertyChanged(); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public DateTime BirthDate
        {
            get => _birthDate;
            set
            {
                if (value <= DateTime.Now)
                {
                    _birthDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Education
        {
            get => _education;
            set { _education = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Hobbies
        {
            get => _hobbies;
            set { _hobbies = value; OnPropertyChanged(); }
        }

        public bool ReceiveNotifications
        {
            get => _receiveNotifications;
            set { _receiveNotifications = value; OnPropertyChanged(); }
        }

        public bool ShowPublicStats
        {
            get => _showPublicStats;
            set { _showPublicStats = value; OnPropertyChanged(); }
        }

        public bool AutoSave
        {
            get => _autoSave;
            set { _autoSave = value; OnPropertyChanged(); }
        }

        public string ActivityLevel
        {
            get => _activityLevel;
            set { _activityLevel = value; OnPropertyChanged(); }
        }

        public string AvatarPath
        {
            get => _avatarPath;
            set { _avatarPath = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Конструктор по умолчанию. Инициализирует коллекции и значения по умолчанию.
        /// </summary>
        public UserProfile()
        {
            BirthDate = DateTime.Now.AddYears(-20);
            Hobbies = new ObservableCollection<string>();
            ActivityLevel = "Средний";
            Education = "Среднее";
            ReceiveNotifications = true;
            AutoSave = false;
        }

        /// <summary>
        /// Метод для сброса всех полей к значениям по умолчанию.
        /// </summary>
        public void Reset()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Password = string.Empty;
            BirthDate = DateTime.Now.AddYears(-20);
            Education = "Среднее";
            Hobbies = new ObservableCollection<string>();
            ReceiveNotifications = false;
            ShowPublicStats = false;
            AutoSave = false;
            ActivityLevel = "Средний";
            AvatarPath = null;
        }

        /// <summary>
        /// Валидация обязательных полей.
        /// </summary>
        /// <returns>Сообщение об ошибке или null, если всё в порядке.</returns>
        public string Validate()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
                return "Имя обязательно для заполнения.";
            if (string.IsNullOrWhiteSpace(LastName))
                return "Фамилия обязательна для заполнения.";
            if (BirthDate > DateTime.Now)
                return "Дата рождения не может быть в будущем.";
            return null;
        }
    }
}