using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SQLite;
using Windows.Storage;
using System.Threading.Tasks;

namespace no_time
{
    public sealed partial class MainPage : Page
    {
        private string dbPath;

        public MainPage()
        {
            this.InitializeComponent();
            InitializeDatabase();
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
                MissionId = Guid.NewGuid().ToString(),
                UserId = "user_123", // Replace with actual user ID
                MissionTitle = missionTitle,
                MissionDeadline = missionDeadline?.DateTime ?? DateTime.Now,
                IsDeleted = false,
                CreateId = "user_123", // Replace with actual user ID
                CreateTime = DateTime.Now,
                UpdateId = "user_123", // Replace with actual user ID
                UpdateTime = DateTime.Now
            };

            await Task.Run(() =>
            {
                using (var db = new SQLiteConnection(dbPath))
                {
                    db.Insert(mission);
                }
            });
        }

        private async void loadButtonClick(object sender, RoutedEventArgs e)
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
        public int Id { get; set; }
        public string MissionId { get; set; }
        public string UserId { get; set; }
        public string MissionTitle { get; set; }
        public DateTime MissionDeadline { get; set; }
        public bool IsDeleted { get; set; }
        public string CreateId { get; set; }
        public DateTime CreateTime { get; set; }
        public string UpdateId { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
