using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HabitTrackerApp
{
    /// <summary>
    /// Класс для сериализации/десериализации всех данных приложения.
    /// </summary>
    public class AppData
    {
        public UserProfileData UserProfile { get; set; }
        public List<HabitData> Habits { get; set; }
        public Dictionary<DateTime, DayStatsData> DailyStats { get; set; }

        public AppData()
        {
            UserProfile = new UserProfileData();
            Habits = new List<HabitData>();
            DailyStats = new Dictionary<DateTime, DayStatsData>();
        }
    }

    /// <summary>
    /// Данные профиля пользователя для сериализации.
    /// </summary>
    public class UserProfileData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public DateTime BirthDate { get; set; }
        public string Education { get; set; }
        public List<string> Hobbies { get; set; }
        public bool ReceiveNotifications { get; set; }
        public bool ShowPublicStats { get; set; }
        public bool AutoSave { get; set; }
        public string ActivityLevel { get; set; }
        public string AvatarPath { get; set; }
    }

    /// <summary>
    /// Данные привычки для сериализации.
    /// </summary>
    public class HabitData
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public bool IsCompleted { get; set; }
    }

    /// <summary>
    /// Данные статистики дня для сериализации.
    /// </summary>
    public class DayStatsData
    {
        public DateTime Date { get; set; }
        public int TotalHabits { get; set; }
        public int CompletedHabits { get; set; }
        public int Productivity { get; set; }
        public int Satisfaction { get; set; }
    }
}