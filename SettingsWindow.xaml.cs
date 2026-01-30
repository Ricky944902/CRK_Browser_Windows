using System;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace CRK_Browser
{
    public partial class SettingsWindow : Window
    {
        private string settingsFilePath;
        private SettingsData settingsData = new SettingsData();
        private HistoryManager historyManager;
        private ThemeManager themeManager;
        
        public SettingsWindow(HistoryManager historyManager)
        {
            InitializeComponent();
            this.historyManager = historyManager;
            this.themeManager = new ThemeManager();
            
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRK-Browser");
            Directory.CreateDirectory(appDataPath);
            settingsFilePath = Path.Combine(appDataPath, "settings.json");
            
            LoadSettings();
            InitializeUI();
        }
        
        private class SettingsData
        {
            public string HomePage { get; set; } = "https://www.bing.com";
            public string SearchEngine { get; set; } = "Bing";
            public string StartupMode { get; set; } = "NewTab";
            public bool NewTabHome { get; set; } = true;
            public bool CloseTabWarning { get; set; } = true;
            public bool SaveHistory { get; set; } = true;
            public bool UseCache { get; set; } = true;
            public bool AcceptCookies { get; set; } = true;
            public string Theme { get; set; } = "浅蓝色";
            public string FontSize { get; set; } = "Medium";
            public bool ShowBookmarksBar { get; set; } = false;
            public bool ShowStatusBar { get; set; } = true;
            public bool HardwareAcceleration { get; set; } = true;
            public bool ProcessPerSite { get; set; } = false;
            public bool DeveloperTools { get; set; } = true;
        }
        
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    string json = File.ReadAllText(settingsFilePath);
                    settingsData = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
                }
                else
                {
                    settingsData = new SettingsData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载设置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void InitializeUI()
        {
            HomePageTextBox.Text = settingsData.HomePage;
            SearchEngineComboBox.Text = settingsData.SearchEngine;
            // 移除 StartupModeComboBox 相关代码，因为 XAML 中不存在该控件
            NewTabHomeCheckBox.IsChecked = settingsData.NewTabHome;
            CloseTabWarningCheckBox.IsChecked = settingsData.CloseTabWarning;
            SaveHistoryCheckBox.IsChecked = settingsData.SaveHistory;
            UseCacheCheckBox.IsChecked = settingsData.UseCache;
            AcceptCookiesCheckBox.IsChecked = settingsData.AcceptCookies;
            ThemeComboBox.Text = settingsData.Theme;
            FontSizeComboBox.Text = settingsData.FontSize;
            ShowBookmarksBarCheckBox.IsChecked = settingsData.ShowBookmarksBar;
            ShowStatusBarCheckBox.IsChecked = settingsData.ShowStatusBar;
            HardwareAccelerationCheckBox.IsChecked = settingsData.HardwareAcceleration;
            ProcessPerSiteCheckBox.IsChecked = settingsData.ProcessPerSite;
            DeveloperToolsCheckBox.IsChecked = settingsData.DeveloperTools;
        }
        
        private void SaveSettings()
        {
            try
            {
                settingsData.HomePage = HomePageTextBox.Text;
                settingsData.SearchEngine = SearchEngineComboBox.Text;
                // 移除 StartupModeComboBox 相关代码
                settingsData.NewTabHome = NewTabHomeCheckBox.IsChecked ?? true;
                settingsData.CloseTabWarning = CloseTabWarningCheckBox.IsChecked ?? true;
                settingsData.SaveHistory = SaveHistoryCheckBox.IsChecked ?? true;
                settingsData.UseCache = UseCacheCheckBox.IsChecked ?? true;
                settingsData.AcceptCookies = AcceptCookiesCheckBox.IsChecked ?? true;
                settingsData.Theme = ThemeComboBox.Text;
                settingsData.FontSize = FontSizeComboBox.Text;
                settingsData.ShowBookmarksBar = ShowBookmarksBarCheckBox.IsChecked ?? false;
                settingsData.ShowStatusBar = ShowStatusBarCheckBox.IsChecked ?? true;
                settingsData.HardwareAcceleration = HardwareAccelerationCheckBox.IsChecked ?? true;
                settingsData.ProcessPerSite = ProcessPerSiteCheckBox.IsChecked ?? false;
                settingsData.DeveloperTools = DeveloperToolsCheckBox.IsChecked ?? true;
                
                string json = JsonSerializer.Serialize(settingsData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(settingsFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存设置失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            
            // 根据选择的主题切换应用程序主题
            string selectedTheme = ThemeComboBox.Text;
            if (selectedTheme == "浅蓝色")
            {
                themeManager.ChangeTheme(ThemeManager.ThemeType.LightBlue);
            }
            else if (selectedTheme == "深蓝色")
            {
                themeManager.ChangeTheme(ThemeManager.ThemeType.DarkBlue);
            }
            
            MessageBox.Show("设置已保存", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要重置所有设置为默认值吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                settingsData = new SettingsData();
                InitializeUI();
                MessageBox.Show("设置已重置为默认值", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要清除所有浏览历史记录吗？此操作不可撤销。", "确认", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                historyManager.ClearHistory();
                MessageBox.Show("浏览历史记录已清除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        private void ClearCacheButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRK-Browser\\Cache");
                if (Directory.Exists(cachePath))
                {
                    Directory.Delete(cachePath, true);
                    Directory.CreateDirectory(cachePath);
                }
                MessageBox.Show("缓存已清除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清除缓存失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ClearCookiesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string cookiesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRK-Browser\\Cookies");
                if (Directory.Exists(cookiesPath))
                {
                    Directory.Delete(cookiesPath, true);
                    Directory.CreateDirectory(cookiesPath);
                }
                MessageBox.Show("Cookie已清除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清除Cookie失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void GeneralButton_Click(object sender, RoutedEventArgs e)
        {
            GeneralSettings.Visibility = Visibility.Visible;
            PrivacySettings.Visibility = Visibility.Collapsed;
            AppearanceSettings.Visibility = Visibility.Collapsed;
            AdvancedSettings.Visibility = Visibility.Collapsed;
        }
        
        private void PrivacyButton_Click(object sender, RoutedEventArgs e)
        {
            GeneralSettings.Visibility = Visibility.Collapsed;
            PrivacySettings.Visibility = Visibility.Visible;
            AppearanceSettings.Visibility = Visibility.Collapsed;
            AdvancedSettings.Visibility = Visibility.Collapsed;
        }
        
        private void AppearanceButton_Click(object sender, RoutedEventArgs e)
        {
            GeneralSettings.Visibility = Visibility.Collapsed;
            PrivacySettings.Visibility = Visibility.Collapsed;
            AppearanceSettings.Visibility = Visibility.Visible;
            AdvancedSettings.Visibility = Visibility.Collapsed;
        }
        
        private void AdvancedButton_Click(object sender, RoutedEventArgs e)
        {
            GeneralSettings.Visibility = Visibility.Collapsed;
            PrivacySettings.Visibility = Visibility.Collapsed;
            AppearanceSettings.Visibility = Visibility.Collapsed;
            AdvancedSettings.Visibility = Visibility.Visible;
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
