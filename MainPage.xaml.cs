using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SQLite;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace no_time
{
    public sealed partial class MainPage : Page
    {
        private string dbPath;

        public MainPage()
        {
            this.InitializeComponent();
            InitializeDatabase();
            LoadMissions();
        }

        private void InitializeDatabase()
        {
            dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "missions.db");
            using (var db = new SQLiteConnection(dbPath))
            {
                db.CreateTable<Mission>();
            }
        }

        private async void saveButtonClick(object sender, RoutedEventArgs e)
        {
            string missionTitle = this.missionTitle.Text;
            DateTimeOffset? missionDeadline = this.CalendarDatePicker.Date;

            var mission = new Mission
            {
                // MissionId 會自動遞增，無需手動設置
                UserId = 123, // Replace with actual user ID
                MissionTitle = missionTitle,
                MissionDeadline = missionDeadline?.DateTime ?? DateTime.Now,
                IsDeleted = false,
                CreateId = 123, // Replace with actual user ID
                CreateTime = DateTime.Now,
                UpdateId = 123, // Replace with actual user ID
                UpdateTime = DateTime.Now
            };

            await Task.Run(() =>
            {
                using (var db = new SQLiteConnection(dbPath))
                {
                    db.Insert(mission);
                }
            });
            LoadMissions();
        }

        private async void LoadMissions()
        {
            List<Mission> missions = await Task.Run(() =>
            {
                using (var db = new SQLiteConnection(dbPath))
                {
                    return db.Table<Mission>().ToList();
                }
            });

            MissionListView.ItemsSource = missions;
        }

    }


    public class Mission
    {
        [PrimaryKey, AutoIncrement]
        public int MissionId { get; set; }
        public int UserId { get; set; }
        public string MissionTitle { get; set; }
        public DateTime MissionDeadline { get; set; }
        public bool IsDeleted { get; set; }
        public int CreateId { get; set; }
        public DateTime CreateTime { get; set; }
        public int UpdateId { get; set; }
        public DateTime UpdateTime { get; set; }

    }

    public class DaysLeftConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime deadline)
            {
                var daysLeft = (deadline - DateTime.Now).Days;
                return $"{daysLeft} 天";
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
