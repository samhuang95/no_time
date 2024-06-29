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

        // This db setting is store to local.
        //private void InitializeDatabase()
        //{
        //    dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "missions.db");
        //    using (var db = new SQLiteConnection(dbPath))
        //    {
        //        db.CreateTable<Mission>();
        //    }
        //}

        // This DB setting is store to current project folder.
        private void InitializeDatabase()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dbFolder = Path.Combine(baseDirectory, "db");

            // 如果資料夾不存在，則建立資料夾
            if (!Directory.Exists(dbFolder))
            {
                Directory.CreateDirectory(dbFolder);
            }

            dbPath = Path.Combine(dbFolder, "missions.db");

            // 檢查資料庫檔案是否存在，如果不存在則創建
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);

                using (var db = new SQLiteConnection(dbPath))
                {
                    db.CreateTable<Mission>();
                }
            }

            // 將 dbPath 賦值給全局變量
            this.dbPath = dbPath;
        }



        private async void saveButtonClick(object sender, RoutedEventArgs e)
        {
            string missionTitle = this.missionTitle.Text;
            DateTimeOffset? missionDeadline = this.CalendarDatePicker.Date;

            if (string.IsNullOrWhiteSpace(missionTitle) || missionDeadline == null)
            {
                // 如果標題或截止日期未填寫，顯示警告訊息
                ShowCustomDialog("Failed", "Please enter Mission Title and Deadline.");
                return;
            }

            if (missionDeadline.Value.Date < DateTime.Today)
            {
                ShowCustomDialog("Failed", "Mission Deadline cannot be earlier than today.");
                return;
            }


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

            await SaveMission(mission);

            LoadMissions();
        }

        private async Task SaveMission(Mission mission)
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var db = new SQLiteConnection(dbPath))
                    {
                        db.Insert(mission);
                    }
                });

                // 顯示成功的提示訊息
                ShowCustomDialog("Success.", "Mission saved successfully.");
            }
            catch (Exception ex)
            {
                // 處理錯誤，例如記錄日誌或顯示錯誤訊息
                Console.WriteLine($"Error saving mission: {ex.Message}");
                ShowCustomDialog("Failed", "Failed to save mission.");
            }
        }

        private async void ShowCustomDialog(string title, string content)
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = title,
                Content = content,
                PrimaryButtonText = "OK"
            };

            await dialog.ShowAsync();
        }


        private async void LoadMissions()
        {
            List<Mission> missions = await Task.Run(() =>
            {
                using (var db = new SQLiteConnection(dbPath))
                {
                    return db.Table<Mission>()
                    .Where(m => !m.IsDeleted)
                    .OrderBy(m => m.MissionDeadline)
                    .ToList();
                }
            });

            MissionListView.ItemsSource = missions;
        }
        private async void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var missionId = button?.Tag as int?; // 將 Tag 轉換為 int

            if (missionId.HasValue)
            {
                await Task.Run(() =>
                {
                    using (var db = new SQLiteConnection(dbPath))
                    {
                        var mission = db.Table<Mission>().FirstOrDefault(m => m.MissionId == missionId);
                        if (mission != null)
                        {
                            mission.IsDeleted = true;
                            db.Update(mission);
                        }
                    }
                });
                LoadMissions();
            }
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
