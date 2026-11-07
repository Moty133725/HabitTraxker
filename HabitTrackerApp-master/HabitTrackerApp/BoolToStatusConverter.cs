using System;
using System.Globalization;
using System.Windows.Data;

namespace HabitTrackerApp
{
    /// <summary>
    /// Конвертер bool в текст статуса привычки.
    /// </summary>
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? "✅ Выполнено" : "❌ Не выполнено";
            return "❓ Неизвестно";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}