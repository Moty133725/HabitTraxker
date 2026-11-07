using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace HabitTrackerApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string DATA_FILE = "habit_data.json";

        private UserProfile _userProfile;
        private ObservableCollection<Habit> _habits;
        private Dictionary<DateTime, DayStats> _dailyStats;

        /// <summary>
        /// Конструктор главного окна. Инициализирует компоненты и данные по умолчанию.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _userProfile = new UserProfile();
            _habits = new ObservableCollection<Habit>();
            _dailyStats = new Dictionary<DateTime, DayStats>();

            this.DataContext = _userProfile;
            this.Loaded += MainWindow_Loaded;

            _userProfile.PropertyChanged += (s, e) =>
            {
                if (_userProfile.AutoSave)
                {
                    SaveUserData();
                    if (LastSaveText != null)
                        LastSaveText.Text = $"Автосохранение: {DateTime.Now:HH:mm:ss}";
                }
            };

            // Подписка на изменение коллекции привычек
            _habits.CollectionChanged += (s, e) => UpdateAllStatistics();
        }

        /// <summary>
        /// Обработчик загрузки окна. Выполняет инициализацию после загрузки всех элементов.
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeHobbiesList();

            if (HabitsDataGrid != null)
                HabitsDataGrid.ItemsSource = _habits;

            InitializeStats();

            if (DateTimeText != null)
                DateTimeText.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            if (MainCalendar != null)
                MainCalendar.SelectedDate = DateTime.Today;

            if (HabitDatePicker != null)
                HabitDatePicker.SelectedDate = DateTime.Today;

            if (DayProgressBar != null && ProgressTextBlock != null &&
                ProductivitySlider != null && SatisfactionSlider != null)
            {
                double productivity = ProductivitySlider.Value;
                double satisfaction = SatisfactionSlider.Value;
                double average = (productivity + satisfaction) / 2;
                DayProgressBar.Value = average;
                ProgressTextBlock.Text = $"{average:F0}%";
            }

            if (StatusBarText != null)
                StatusBarText.Text = "Готов";

            // Загружаем данные или демо-версию
            if (File.Exists(DATA_FILE))
            {
                LoadUserData();
            }
            else
            {
                LoadDemoData();
            }

            UpdateAllStatistics();
        }

        /// <summary>
        /// Загрузка демонстрационных данных для показа статистики.
        /// </summary>
        private void LoadDemoData()
        {
            var today = DateTime.Today;

            // Добавляем привычки за последние 7 дней
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);

                var habits = new[]
                {
                    new Habit { Name = "Утренняя зарядка", Date = date, Time = new TimeSpan(7, 30, 0), IsCompleted = i != 0 },
                    new Habit { Name = "Чтение книги", Date = date, Time = new TimeSpan(20, 0, 0), IsCompleted = i <= 5 },
                    new Habit { Name = "Прогулка", Date = date, Time = new TimeSpan(18, 0, 0), IsCompleted = i <= 4 },
                    new Habit { Name = "Медитация", Date = date, Time = new TimeSpan(21, 0, 0), IsCompleted = i <= 3 }
                };

                foreach (var habit in habits)
                {
                    _habits.Add(habit);
                }

                // Статистика за день
                if (i > 0) // Для прошлых дней
                {
                    _dailyStats[date] = new DayStats
                    {
                        Date = date,
                        Productivity = 50 + (6 - i) * 8,
                        Satisfaction = 55 + (6 - i) * 7
                    };
                }
            }

            // Статистика за сегодня
            _dailyStats[today] = new DayStats
            {
                Date = today,
                Productivity = 60,
                Satisfaction = 65
            };

            if (HabitDatePicker != null)
                HabitDatePicker.SelectedDate = today;
        }

        /// <summary>
        /// Инициализация списка хобби в ListBox.
        /// </summary>
        private void InitializeHobbiesList()
        {
            if (HobbiesListBox == null)
                return;

            HobbiesListBox.Items.Clear();
            HobbiesListBox.Items.Add(new ListBoxItem { Content = "Спорт" });
            HobbiesListBox.Items.Add(new ListBoxItem { Content = "Чтение" });
            HobbiesListBox.Items.Add(new ListBoxItem { Content = "Музыка" });
            HobbiesListBox.Items.Add(new ListBoxItem { Content = "Программирование" });
            HobbiesListBox.Items.Add(new ListBoxItem { Content = "Путешествия" });
        }

        /// <summary>
        /// Инициализация ListView статистики.
        /// </summary>
        private void InitializeStats()
        {
            if (StatsListView == null)
                return;

            StatsListView.ItemsSource = _dailyStats.Values.OrderBy(d => d.Date).ToList();
        }

        /// <summary>
        /// Обновление всей статистики в интерфейсе.
        /// </summary>
        private void UpdateAllStatistics()
        {
            UpdateOverallStats();
            UpdateTodayStats();
            UpdateStatsListView();
            UpdateProductivityChart();
            UpdateRecentHabits();
        }

        /// <summary>
        /// Обновление общей сводки привычек.
        /// </summary>
        private void UpdateOverallStats()
        {
            if (TotalHabitsText == null)
                return;

            int total = _habits.Count;
            int completed = _habits.Count(h => h.IsCompleted);
            int pending = total - completed;
            double rate = total > 0 ? (double)completed / total * 100 : 0;

            TotalHabitsText.Text = $"Всего привычек: {total}";
            CompletedHabitsText.Text = $"Выполнено: {completed}";
            PendingHabitsText.Text = $"Не выполнено: {pending}";
            CompletionRateText.Text = $"Процент выполнения: {rate:F1}%";
        }

        /// <summary>
        /// Обновление сводки за сегодня.
        /// </summary>
        private void UpdateTodayStats()
        {
            if (TodayTotalText == null)
                return;

            var today = DateTime.Today;
            var todayHabits = _habits.Where(h => h.Date.Date == today).ToList();
            int total = todayHabits.Count;
            int completed = todayHabits.Count(h => h.IsCompleted);
            double rate = total > 0 ? (double)completed / total * 100 : 0;

            TodayTotalText.Text = $"Всего привычек: {total}";
            TodayCompletedText.Text = $"Выполнено: {completed}";
            TodayProgressBar.Value = rate;
            TodayPercentText.Text = $"{rate:F0}%";
        }

        /// <summary>
        /// Обновление ListView статистики по дням.
        /// </summary>
        private void UpdateStatsListView()
        {
            if (StatsListView == null)
                return;

            var stats = new ObservableCollection<DayStats>();

            // Группируем привычки по датам
            var groupedHabits = _habits.GroupBy(h => h.Date.Date)
                .OrderBy(g => g.Key);

            foreach (var group in groupedHabits)
            {
                var date = group.Key;
                var dayStats = _dailyStats.ContainsKey(date) ? _dailyStats[date] : new DayStats { Date = date };

                stats.Add(new DayStats
                {
                    Date = date,
                    TotalHabits = group.Count(),
                    CompletedHabits = group.Count(h => h.IsCompleted),
                    Productivity = dayStats.Productivity,
                    Satisfaction = dayStats.Satisfaction
                });
            }

            StatsListView.ItemsSource = stats;
        }

        /// <summary>
        /// Построение графика продуктивности за неделю.
        /// </summary>
        private void UpdateProductivityChart()
        {
            if (ProductivityChart == null)
                return;

            ProductivityChart.Children.Clear();

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .ToList();

            var dataPoints = new List<DayStats>();
            foreach (var date in last7Days)
            {
                if (_dailyStats.ContainsKey(date))
                {
                    dataPoints.Add(_dailyStats[date]);
                }
                else
                {
                    dataPoints.Add(new DayStats { Date = date, Productivity = 0, Satisfaction = 0 });
                }
            }

            if (dataPoints.Count == 0)
                return;

            double maxValue = dataPoints.Max(d => Math.Max(d.Productivity, d.Satisfaction));
            if (maxValue == 0) maxValue = 100;

            double chartWidth = ProductivityChart.ActualWidth > 0 ? ProductivityChart.ActualWidth : 400;
            double chartHeight = ProductivityChart.ActualHeight > 0 ? ProductivityChart.ActualHeight : 150;
            double padding = 30;
            double usableWidth = chartWidth - padding * 2;
            double usableHeight = chartHeight - padding * 2;

            // Рисуем оси
            var xAxis = new Line
            {
                X1 = padding,
                Y1 = chartHeight - padding,
                X2 = chartWidth - padding,
                Y2 = chartHeight - padding,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };
            ProductivityChart.Children.Add(xAxis);

            var yAxis = new Line
            {
                X1 = padding,
                Y1 = padding,
                X2 = padding,
                Y2 = chartHeight - padding,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };
            ProductivityChart.Children.Add(yAxis);

            // Рисуем линию продуктивности
            if (dataPoints.Count >= 2)
            {
                for (int i = 0; i < dataPoints.Count - 1; i++)
                {
                    double x1 = padding + (i * usableWidth / (dataPoints.Count - 1));
                    double y1 = chartHeight - padding - (dataPoints[i].Productivity / maxValue * usableHeight);
                    double x2 = padding + ((i + 1) * usableWidth / (dataPoints.Count - 1));
                    double y2 = chartHeight - padding - (dataPoints[i + 1].Productivity / maxValue * usableHeight);

                    var line = new Line
                    {
                        X1 = x1,
                        Y1 = y1,
                        X2 = x2,
                        Y2 = y2,
                        Stroke = Brushes.Blue,
                        StrokeThickness = 2
                    };
                    ProductivityChart.Children.Add(line);

                    // Точка
                    var point = new Ellipse
                    {
                        Width = 6,
                        Height = 6,
                        Fill = Brushes.Blue
                    };
                    Canvas.SetLeft(point, x1 - 3);
                    Canvas.SetTop(point, y1 - 3);
                    ProductivityChart.Children.Add(point);

                    // Подпись даты
                    var label = new TextBlock
                    {
                        Text = dataPoints[i].Date.ToString("dd.MM"),
                        FontSize = 8,
                        Foreground = Brushes.Gray
                    };
                    Canvas.SetLeft(label, x1 - 15);
                    Canvas.SetTop(label, chartHeight - padding + 5);
                    ProductivityChart.Children.Add(label);
                }

                // Последняя точка и подпись
                double lastX = padding + ((dataPoints.Count - 1) * usableWidth / (dataPoints.Count - 1));
                double lastY = chartHeight - padding - (dataPoints[dataPoints.Count - 1].Productivity / maxValue * usableHeight);
                var lastPoint = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = Brushes.Blue
                };
                Canvas.SetLeft(lastPoint, lastX - 3);
                Canvas.SetTop(lastPoint, lastY - 3);
                ProductivityChart.Children.Add(lastPoint);

                var lastLabel = new TextBlock
                {
                    Text = dataPoints[dataPoints.Count - 1].Date.ToString("dd.MM"),
                    FontSize = 8,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(lastLabel, lastX - 15);
                Canvas.SetTop(lastLabel, chartHeight - padding + 5);
                ProductivityChart.Children.Add(lastLabel);

                // Рисуем линию удовлетворённости (пунктирная оранжевая)
                for (int i = 0; i < dataPoints.Count - 1; i++)
                {
                    double x1 = padding + (i * usableWidth / (dataPoints.Count - 1));
                    double y1 = chartHeight - padding - (dataPoints[i].Satisfaction / maxValue * usableHeight);
                    double x2 = padding + ((i + 1) * usableWidth / (dataPoints.Count - 1));
                    double y2 = chartHeight - padding - (dataPoints[i + 1].Satisfaction / maxValue * usableHeight);

                    var line = new Line
                    {
                        X1 = x1,
                        Y1 = y1,
                        X2 = x2,
                        Y2 = y2,
                        Stroke = Brushes.Orange,
                        StrokeThickness = 2,
                        StrokeDashArray = new DoubleCollection { 4, 2 }
                    };
                    ProductivityChart.Children.Add(line);
                }

                // Легенда
                var legendProductivity = new TextBlock
                {
                    Text = "― Продуктивность",
                    FontSize = 9,
                    Foreground = Brushes.Blue
                };
                Canvas.SetLeft(legendProductivity, padding + 10);
                Canvas.SetTop(legendProductivity, 5);
                ProductivityChart.Children.Add(legendProductivity);

                var legendSatisfaction = new TextBlock
                {
                    Text = "- - Удовлетворённость",
                    FontSize = 9,
                    Foreground = Brushes.Orange
                };
                Canvas.SetLeft(legendSatisfaction, padding + 10);
                Canvas.SetTop(legendSatisfaction, 20);
                ProductivityChart.Children.Add(legendSatisfaction);
            }
        }

        /// <summary>
        /// Обновление списка последних привычек.
        /// </summary>
        private void UpdateRecentHabits()
        {
            if (RecentHabitsList == null)
                return;

            var recent = _habits.OrderByDescending(h => h.Date).ThenByDescending(h => h.Time).Take(10).ToList();
            RecentHabitsList.ItemsSource = recent;
        }

        // ==================== Сохранение и загрузка данных ====================

        /// <summary>
        /// Сохранение всех данных в JSON-файл.
        /// </summary>
        private void SaveUserData()
        {
            try
            {
                var data = new AppData();

                // Сохраняем профиль
                data.UserProfile = new UserProfileData
                {
                    FirstName = _userProfile.FirstName,
                    LastName = _userProfile.LastName,
                    Password = _userProfile.Password,
                    BirthDate = _userProfile.BirthDate,
                    Education = _userProfile.Education,
                    Hobbies = _userProfile.Hobbies?.ToList() ?? new List<string>(),
                    ReceiveNotifications = _userProfile.ReceiveNotifications,
                    ShowPublicStats = _userProfile.ShowPublicStats,
                    AutoSave = _userProfile.AutoSave,
                    ActivityLevel = _userProfile.ActivityLevel,
                    AvatarPath = _userProfile.AvatarPath
                };

                // Сохраняем привычки
                data.Habits = _habits.Select(h => new HabitData
                {
                    Name = h.Name,
                    Date = h.Date,
                    Time = h.Time,
                    IsCompleted = h.IsCompleted
                }).ToList();

                // Сохраняем статистику
                data.DailyStats = _dailyStats.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new DayStatsData
                    {
                        Date = kvp.Value.Date,
                        TotalHabits = kvp.Value.TotalHabits,
                        CompletedHabits = kvp.Value.CompletedHabits,
                        Productivity = kvp.Value.Productivity,
                        Satisfaction = kvp.Value.Satisfaction
                    }
                );

                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(DATA_FILE, json);

                System.Diagnostics.Debug.WriteLine($"Данные сохранены в {DATA_FILE}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка всех данных из JSON-файла.
        /// </summary>
        private void LoadUserData()
        {
            try
            {
                if (!File.Exists(DATA_FILE))
                {
                    MessageBox.Show("Файл данных не найден. Загружены демо-данные.", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDemoData();
                    UpdateAllStatistics();
                    return;
                }

                string json = File.ReadAllText(DATA_FILE);
                var data = JsonConvert.DeserializeObject<AppData>(json);

                if (data == null)
                {
                    MessageBox.Show("Файл данных повреждён.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Загружаем профиль
                if (data.UserProfile != null)
                {
                    _userProfile.FirstName = data.UserProfile.FirstName ?? "";
                    _userProfile.LastName = data.UserProfile.LastName ?? "";
                    _userProfile.Password = data.UserProfile.Password ?? "";
                    _userProfile.BirthDate = data.UserProfile.BirthDate;
                    _userProfile.Education = data.UserProfile.Education ?? "Среднее";
                    _userProfile.ReceiveNotifications = data.UserProfile.ReceiveNotifications;
                    _userProfile.ShowPublicStats = data.UserProfile.ShowPublicStats;
                    _userProfile.AutoSave = data.UserProfile.AutoSave;
                    _userProfile.ActivityLevel = data.UserProfile.ActivityLevel ?? "Средний";

                    if (data.UserProfile.Hobbies != null)
                    {
                        _userProfile.Hobbies = new ObservableCollection<string>(data.UserProfile.Hobbies);
                    }

                    PasswordBox.Password = _userProfile.Password ?? "";
                    BirthDatePicker.SelectedDate = _userProfile.BirthDate;
                    EducationCombo.Text = _userProfile.Education;

                    // Восстанавливаем выбранные хобби
                    HobbiesListBox.SelectedItems.Clear();
                    foreach (ListBoxItem item in HobbiesListBox.Items)
                    {
                        if (data.UserProfile.Hobbies?.Contains(item.Content.ToString()) == true)
                        {
                            HobbiesListBox.SelectedItems.Add(item);
                        }
                    }
                }

                // Загружаем привычки
                _habits.Clear();
                if (data.Habits != null)
                {
                    foreach (var h in data.Habits)
                    {
                        _habits.Add(new Habit
                        {
                            Name = h.Name,
                            Date = h.Date,
                            Time = h.Time,
                            IsCompleted = h.IsCompleted
                        });
                    }
                }

                // Загружаем статистику
                _dailyStats.Clear();
                if (data.DailyStats != null)
                {
                    foreach (var kvp in data.DailyStats)
                    {
                        _dailyStats[kvp.Key] = new DayStats
                        {
                            Date = kvp.Value.Date,
                            TotalHabits = kvp.Value.TotalHabits,
                            CompletedHabits = kvp.Value.CompletedHabits,
                            Productivity = kvp.Value.Productivity,
                            Satisfaction = kvp.Value.Satisfaction
                        };
                    }
                }

                UpdateAllStatistics();

                if (StatusBarText != null)
                    StatusBarText.Text = $"Данные загружены из {DATA_FILE}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== Обработчики меню и тулбара ====================

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Сохраняем профиль
            if (_userProfile != null)
            {
                string validationError = _userProfile.Validate();
                if (validationError != null)
                {
                    MessageBox.Show(validationError, "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            SaveUserData();
            if (LastSaveText != null)
                LastSaveText.Text = $"Сохранено: {DateTime.Now:HH:mm:ss}";
            if (StatusBarText != null)
                StatusBarText.Text = "Данные сохранены в файл";

            MessageBox.Show("Данные успешно сохранены!", "Сохранение",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            LoadUserData();

            if (File.Exists(DATA_FILE))
            {
                MessageBox.Show("Данные успешно загружены!", "Загрузка",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Данные скопированы в буфер обмена (имитация).", "Копировать",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Данные вставлены из буфера обмена (имитация).", "Вставить",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LightTheme_Click(object sender, RoutedEventArgs e)
        {
            this.Background = Brushes.White;
            if (StatusBarText != null)
                StatusBarText.Text = "Включена светлая тема";
        }

        private void DarkTheme_Click(object sender, RoutedEventArgs e)
        {
            this.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            if (StatusBarText != null)
                StatusBarText.Text = "Включена темная тема";
        }

        // ==================== Личные данные ====================

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_userProfile != null && PasswordBox != null)
                _userProfile.Password = PasswordBox.Password;
        }

        private void LoadAvatar_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                if (_userProfile != null)
                    _userProfile.AvatarPath = openFileDialog.FileName;
                if (AvatarImage != null)
                    AvatarImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(openFileDialog.FileName));
                if (StatusBarText != null)
                    StatusBarText.Text = "Аватар загружен";
            }
        }

        private void HobbiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_userProfile == null || HobbiesListBox == null)
                return;

            _userProfile.Hobbies.Clear();
            foreach (var item in HobbiesListBox.SelectedItems)
            {
                if (item is ListBoxItem listBoxItem)
                    _userProfile.Hobbies.Add(listBoxItem.Content.ToString());
            }
        }

        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_userProfile == null)
                return;

            string validationError = _userProfile.Validate();
            if (validationError != null)
            {
                MessageBox.Show(validationError, "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveUserData();
            if (LastSaveText != null)
                LastSaveText.Text = $"Сохранено: {DateTime.Now:HH:mm:ss}";
            if (StatusBarText != null)
                StatusBarText.Text = "Профиль сохранен";
        }

        private void ResetProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_userProfile == null)
                return;

            _userProfile.Reset();
            if (PasswordBox != null)
                PasswordBox.Password = string.Empty;
            if (AvatarImage != null)
                AvatarImage.Source = null;
            if (HobbiesListBox != null)
                HobbiesListBox.SelectedItems.Clear();
            if (LastSaveText != null)
                LastSaveText.Text = "Никогда";
            if (StatusBarText != null)
                StatusBarText.Text = "Данные сброшены";
        }

        // ==================== Привычки ====================

        /// <summary>
        /// Добавление новой привычки с возможностью выбора даты.
        /// </summary>
        private void AddHabit_Click(object sender, RoutedEventArgs e)
        {
            if (NewHabitTextBox == null || _habits == null)
                return;

            string habitName = NewHabitTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(habitName))
            {
                DateTime selectedDate = HabitDatePicker?.SelectedDate ?? DateTime.Today;

                _habits.Add(new Habit
                {
                    Name = habitName,
                    Date = selectedDate,
                    Time = DateTime.Now.TimeOfDay,
                    IsCompleted = false
                });

                NewHabitTextBox.Clear();

                if (StatusBarText != null)
                    StatusBarText.Text = $"Добавлена привычка: {habitName} на {selectedDate:dd.MM.yyyy}";

                UpdateAllStatistics();
            }
            else
            {
                MessageBox.Show("Введите название привычки.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!this.IsLoaded || DayProgressBar == null || ProgressTextBlock == null)
                return;
            if (ProductivitySlider == null || SatisfactionSlider == null)
                return;

            double productivity = ProductivitySlider.Value;
            double satisfaction = SatisfactionSlider.Value;
            double average = (productivity + satisfaction) / 2;

            DayProgressBar.Value = average;
            ProgressTextBlock.Text = $"{average:F0}%";
        }

        /// <summary>
        /// Сохранение оценки дня в статистику.
        /// </summary>
        private void SaveDayRating_Click(object sender, RoutedEventArgs e)
        {
            if (_userProfile == null || ProductivitySlider == null || SatisfactionSlider == null)
                return;

            var today = DateTime.Today;
            _dailyStats[today] = new DayStats
            {
                Date = today,
                Productivity = (int)ProductivitySlider.Value,
                Satisfaction = (int)SatisfactionSlider.Value
            };

            UpdateAllStatistics();

            if (StatusBarText != null)
                StatusBarText.Text = $"Оценка дня сохранена: П={ProductivitySlider.Value:F0}%, У={SatisfactionSlider.Value:F0}%";
        }

        private void MainCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainCalendar != null && MainCalendar.SelectedDate.HasValue && StatusBarText != null)
            {
                var selectedDate = MainCalendar.SelectedDate.Value;
                StatusBarText.Text = $"Выбрана дата: {selectedDate.ToShortDateString()}";

                // Фильтруем привычки по выбранной дате
                if (HabitsDataGrid != null)
                {
                    var filteredHabits = _habits.Where(h => h.Date.Date == selectedDate.Date).ToList();
                    HabitsDataGrid.ItemsSource = filteredHabits;
                }
            }
        }

        private void EditModeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton && StatusBarText != null)
            {
                var parentGrid = this.Content as Grid;
                var grid = parentGrid?.Children.OfType<Grid>()
                    .FirstOrDefault(g => g.Children.Count > 2);
                if (grid != null)
                {
                    var rightPanel = grid.Children.OfType<FrameworkElement>()
                        .FirstOrDefault(c => Grid.GetColumn(c) == 2);
                    if (rightPanel != null)
                    {
                        rightPanel.Visibility = toggleButton.IsChecked == true ? Visibility.Hidden : Visibility.Visible;
                        StatusBarText.Text = toggleButton.IsChecked == true ? "Режим редактирования включен" : "Режим редактирования выключен";
                    }
                }
            }
        }

        private void IncreaseProductivity_Click(object sender, RoutedEventArgs e)
        {
            if (ProductivitySlider != null && ProductivitySlider.Value <= 95)
                ProductivitySlider.Value += 5;
        }

        // ==================== Статистика ====================

        private void StatsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item)
            {
                if (StatusBarText != null)
                    StatusBarText.Text = $"Категория: {item.Header}";

                // Показываем статистику по выбранной категории
                var categoryHabits = _habits.Where(h =>
                    h.Name.Contains(item.Header.ToString()) ||
                    (item.Tag != null && h.Name.ToLower().Contains(item.Tag.ToString().ToLower())))
                    .ToList();

                int total = categoryHabits.Count;
                int completed = categoryHabits.Count(h => h.IsCompleted);
                double rate = total > 0 ? (double)completed / total * 100 : 0;

                if (StatusInfoText != null)
                {
                    StatusInfoText.Text = $"Категория: {item.Header}\n" +
                                         $"Всего: {total}\n" +
                                         $"Выполнено: {completed}\n" +
                                         $"Успешность: {rate:F1}%\n" +
                                         $"Тег: {item.Tag}";
                }
            }
        }
    }

    /// <summary>
    /// Вспомогательный класс для представления привычки в DataGrid.
    /// </summary>
    public class Habit : INotifyPropertyChanged
    {
        private string _name;
        private DateTime _date;
        private TimeSpan _time;
        private bool _isCompleted;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }
        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(nameof(Date)); }
        }
        public TimeSpan Time
        {
            get => _time;
            set { _time = value; OnPropertyChanged(nameof(Time)); }
        }
        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                OnPropertyChanged(nameof(IsCompleted));

                // При изменении статуса обновляем статистику
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.GetType().GetMethod("UpdateAllStatistics",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.Invoke(mainWindow, null);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Вспомогательный класс для статистики за день.
    /// </summary>
    public class DayStats
    {
        public DateTime Date { get; set; }
        public int TotalHabits { get; set; }
        public int CompletedHabits { get; set; }
        public int Productivity { get; set; }
        public int Satisfaction { get; set; }
    }
    }

