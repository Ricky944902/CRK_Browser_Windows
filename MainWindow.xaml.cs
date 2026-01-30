using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace CRK_Browser
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // 标签页列表
        private List<TabItem> tabs;
        // WebView2 控件列表
        private List<WebView2> webViews;
        // 当前标签页索引
        private int currentTabIndex;
        // 语言管理器
        private LanguageManager languageManager;
        // 历史记录管理器
        private HistoryManager historyManager;
        // 书签管理器
        private BookmarkManager bookmarkManager;
        // 用户管理器
        private UserManager userManager;
        // 安全浏览管理器
        private SafeBrowsingManager safeBrowsingManager;
        // 下载管理器
        private DownloadManager downloadManager;
        // WebView2环境
        private CoreWebView2Environment? webView2Environment;
        // 主题管理器
        private ThemeManager themeManager;
        
        public MainWindow()
        {
            InitializeComponent();
            
            // 初始化变量
            tabs = new List<TabItem>();
            webViews = new List<WebView2>();
            currentTabIndex = 0;
            languageManager = new LanguageManager();
            historyManager = new HistoryManager();
            bookmarkManager = new BookmarkManager();
            userManager = new UserManager();
            safeBrowsingManager = new SafeBrowsingManager();
            downloadManager = new DownloadManager();
            themeManager = new ThemeManager();
            
            // 订阅主题更改事件
            themeManager.ThemeChanged += ThemeManager_ThemeChanged;
            
            // 初始化主题
            themeManager.InitializeTheme();
            
            // 加载保存的主题设置
            LoadSavedTheme();
            
            // 初始化WebView2环境
            InitializeWebView2();
            
            // 更新界面文本
            UpdateUI();
        }
        
        /// <summary>
        /// 设置数据类，用于序列化和反序列化设置
        /// </summary>
        private class SettingsData
        {
            public string? Theme { get; set; }
        }
        
        /// <summary>
        /// 加载保存的主题设置
        /// </summary>
        private void LoadSavedTheme()
        {
            try
            {
                string settingsFilePath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    "CRK-Browser", 
                    "settings.json"
                );
                
                if (System.IO.File.Exists(settingsFilePath))
                {
                    string json = System.IO.File.ReadAllText(settingsFilePath);
                    var settingsData = System.Text.Json.JsonSerializer.Deserialize<SettingsData>(json);
                    
                    if (settingsData != null && !string.IsNullOrEmpty(settingsData.Theme))
                    {
                        if (settingsData.Theme == "浅蓝色")
                        {
                            themeManager.ChangeTheme(ThemeManager.ThemeType.LightBlue);
                        }
                        else if (settingsData.Theme == "深蓝色")
                        {
                            themeManager.ChangeTheme(ThemeManager.ThemeType.DarkBlue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载主题设置失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 初始化WebView2环境
        /// </summary>
        private async void InitializeWebView2()
        {
            try
            {
                // 确保目录存在
                string webView2Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRK-Browser", "WebView2");
                Directory.CreateDirectory(webView2Path);
                
                // 设置WebView2环境
                webView2Environment = await CoreWebView2Environment.CreateAsync(
                    null, 
                    webView2Path,
                    new CoreWebView2EnvironmentOptions
                    {
                        AllowSingleSignOnUsingOSPrimaryAccount = true,
                        Language = CultureInfo.CurrentUICulture.Name,
                        AdditionalBrowserArguments = "--disable-gpu" // 禁用GPU加速以避免某些兼容性问题
                    }
                );
                
                // 环境初始化成功后创建默认标签页
                CreateNewTab("https://www.bing.com");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化WebView2环境失败: {ex.Message}\n\n应用程序将尝试继续运行，但某些功能可能不可用。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                // 即使WebView2环境初始化失败，也创建一个空标签页
                CreateNewTab("https://www.bing.com");
            }
        }
        
        /// <summary>
        /// 创建新标签页
        /// </summary>
        /// <param name="url">初始URL</param>
        private async void CreateNewTab(string url = "https://www.bing.com")
        {
            try
            {
                // 创建标签页框架
                var tabItem = new TabItem();
                tabItem.Header = "加载中...";
                tabItem.Style = (Style)FindResource("TabItemStyle");
                tabItem.Tag = tabs.Count;
                
                // 添加到列表和控件
                tabs.Add(tabItem);
                TabControl.Items.Add(tabItem);
                
                // 选择新标签页
                TabControl.SelectedItem = tabItem;
                currentTabIndex = tabs.Count - 1;
                
                // 在UI线程上创建WebView2控件
                var webView = new WebView2();
                webView.Name = "WebView" + (tabs.Count - 1);
                webView.HorizontalAlignment = HorizontalAlignment.Stretch;
                webView.VerticalAlignment = VerticalAlignment.Stretch;
                
                // 注册WebView事件
                webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
                webView.NavigationStarting += WebView_NavigationStarting;
                webView.NavigationCompleted += WebView_NavigationCompleted;
                
                // 设置标签页内容
                tabItem.Content = webView;
                tabItem.Header = "新标签页";
                
                // 添加到WebView列表
                webViews.Add(webView);
                
                // 异步初始化WebView2
                await InitializeWebViewAsync(webView, url);
            }
            catch (Exception ex)
            {
                // 移除失败的标签页
                if (tabs.Count > 0)
                {
                    var failedTab = tabs[tabs.Count - 1];
                    tabs.Remove(failedTab);
                    TabControl.Items.Remove(failedTab);
                    
                    if (tabs.Count > 0)
                    {
                        TabControl.SelectedIndex = tabs.Count - 1;
                        currentTabIndex = tabs.Count - 1;
                    }
                }
                
                MessageBox.Show($"创建新标签页失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 异步初始化WebView2
        /// </summary>
        /// <param name="webView">WebView2控件</param>
        /// <param name="url">初始URL</param>
        private async Task InitializeWebViewAsync(WebView2 webView, string url)
        {
            try
            {
                // 初始化WebView2
                await webView.EnsureCoreWebView2Async(webView2Environment);
                
                // 配置WebView2
                if (webView.CoreWebView2 != null)
                {
                    // 禁用不必要的功能以提升性能
                    webView.CoreWebView2.Settings.IsScriptEnabled = true;
                    webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                    webView.CoreWebView2.Settings.IsZoomControlEnabled = false;
                    webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
                    
                    // 注册CoreWebView2事件
                    webView.CoreWebView2.DocumentTitleChanged += WebView_DocumentTitleChanged;
                    webView.CoreWebView2.HistoryChanged += WebView_HistoryChanged;
                    webView.CoreWebView2.DownloadStarting += WebView_DownloadStarting;
                    
                    // 导航到指定URL
                    if (!string.IsNullOrEmpty(url))
                    {
                        webView.CoreWebView2.Navigate(url);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化WebView2失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 关闭标签页
        /// </summary>
        /// <param name="index">标签页索引</param>
        private void CloseTab(int index)
        {
            if (tabs.Count <= 1) return; // 至少保留一个标签页
            
            // 移除标签页和WebView2控件
            TabControl.Items.Remove(tabs[index]);
            tabs.RemoveAt(index);
            webViews.RemoveAt(index);
            
            // 更新标签页索引
            for (int i = 0; i < tabs.Count; i++)
            {
                tabs[i].Tag = i;
            }
            
            // 选择合适的标签页
            if (currentTabIndex >= tabs.Count)
            {
                currentTabIndex = tabs.Count - 1;
            }
            TabControl.SelectedIndex = currentTabIndex;
        }
        
        /// <summary>
        /// 获取当前WebView2控件
        /// </summary>
        /// <returns>当前WebView2控件</returns>
        private WebView2? GetCurrentWebView()
        {
            if (currentTabIndex >= 0 && currentTabIndex < webViews.Count)
            {
                return webViews[currentTabIndex];
            }
            return null;
        }
        
        /// <summary>
        /// WebView2初始化完成事件
        /// </summary>
        private void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                MessageBox.Show($"WebView2初始化失败: {e.InitializationException.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            var webView = sender as WebView2;
            if (webView != null)
            {
                // 配置WebView2
                webView.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 CRK-Browser/1.0";
                webView.CoreWebView2.Settings.IsScriptEnabled = true;
                webView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                
                // 注册WebMessageReceived事件
                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            }
        }
        
        /// <summary>
        /// 导航开始事件
        /// </summary>
        private async void WebView_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            var webView = sender as WebView2;
            if (webView == null)
            {
                return;
            }
            var index = webViews.IndexOf(webView);
            
            if (index == currentTabIndex)
            {
                // 更新地址栏
                AddressBar.Text = e.Uri;
                
                // 更新状态栏
                StatusText.Text = "正在加载...";
                LoadProgress.Visibility = Visibility.Visible;
                
                // 检查网站安全性
                bool isSafe = await safeBrowsingManager.IsSafeSiteAsync(e.Uri);
                
                // 更新安全状态
                if (e.Uri.StartsWith("https://"))
                {
                    if (isSafe)
                    {
                        SecurityStatus.Text = "安全";
                        SecurityStatus.Foreground = (System.Windows.Media.Brush)FindResource("AccentColor");
                    }
                    else
                    {
                        SecurityStatus.Text = "安全但可能存在风险";
                        SecurityStatus.Foreground = System.Windows.Media.Brushes.Orange;
                        
                        // 显示安全警告
                        MessageBoxResult result = MessageBox.Show(
                            $"该网站可能存在安全风险，建议谨慎访问。\n\n{safeBrowsingManager.GetSecurityAdvice(e.Uri)}",
                            "安全警告",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning
                        );
                        
                        if (result == MessageBoxResult.No)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }
                else
                {
                    if (isSafe)
                    {
                        SecurityStatus.Text = "不安全";
                        SecurityStatus.Foreground = System.Windows.Media.Brushes.Red;
                        
                        // 显示安全警告
                        MessageBoxResult result = MessageBox.Show(
                            "该网站使用的是HTTP协议，数据传输可能不安全。建议只在必要时访问，并避免输入敏感信息。",
                            "安全警告",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning
                        );
                        
                        if (result == MessageBoxResult.No)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                    else
                    {
                        SecurityStatus.Text = "危险";
                        SecurityStatus.Foreground = System.Windows.Media.Brushes.Red;
                        
                        // 显示安全警告
                        MessageBoxResult result = MessageBox.Show(
                            $"该网站存在安全风险，不建议访问。\n\n{safeBrowsingManager.GetSecurityAdvice(e.Uri)}",
                            "危险警告",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Error
                        );
                        
                        if (result == MessageBoxResult.No)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 导航完成事件
        /// </summary>
        private void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var webView = sender as WebView2;
            if (webView == null)
            {
                return;
            }
            var index = webViews.IndexOf(webView);
            
            if (index == currentTabIndex)
            {
                // 更新状态栏
                if (e.IsSuccess)
                {
                    StatusText.Text = "已完成";
                    
                    // 添加到历史记录
                    if (webView.CoreWebView2 != null)
                    {
                        string url = webView.CoreWebView2.Source.ToString();
                        string title = webView.CoreWebView2.DocumentTitle;
                        historyManager.AddHistory(url, title);
                    }
                }
                else
                {
                    StatusText.Text = $"加载失败: {e.WebErrorStatus}";
                }
                LoadProgress.Visibility = Visibility.Collapsed;
                
                // 更新导航按钮状态
                UpdateNavigationButtons();
            }
        }
        
        /// <summary>
        /// 文档标题变更事件
        /// </summary>
        private void WebView_DocumentTitleChanged(object? sender, object e)
        {
            var webView = sender as WebView2;
            if (webView == null)
            {
                return;
            }
            var index = webViews.IndexOf(webView);
            
            if (index >= 0 && index < tabs.Count)
            {
                // 更新标签页标题
                tabs[index].Header = webView.CoreWebView2.DocumentTitle;
                
                // 更新窗口标题
                if (index == currentTabIndex)
                {
                    Title = $"{webView.CoreWebView2.DocumentTitle} - CRK Browser";
                }
            }
        }
        
        /// <summary>
        /// 历史记录变更事件
        /// </summary>
        private void WebView_HistoryChanged(object? sender, object e)
        {
            // 更新导航按钮状态
            UpdateNavigationButtons();
        }
        
        /// <summary>
        /// Web消息接收事件
        /// </summary>
        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            // 处理来自网页的消息
            string message = e.TryGetWebMessageAsString();
            // 这里可以处理网页发送的消息
        }
        
        /// <summary>
        /// 更新导航按钮状态
        /// </summary>
        private void UpdateNavigationButtons()
        {
            var webView = GetCurrentWebView();
            if (webView != null && webView.CoreWebView2 != null)
            {
                BackButton.IsEnabled = webView.CoreWebView2.CanGoBack;
                ForwardButton.IsEnabled = webView.CoreWebView2.CanGoForward;
            }
        }
        
        // 事件处理方法
        
        /// <summary>
        /// 后退按钮点击事件
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var webView = GetCurrentWebView();
            if (webView != null && webView.CoreWebView2 != null && webView.CoreWebView2.CanGoBack)
            {
                webView.CoreWebView2.GoBack();
            }
        }
        
        /// <summary>
        /// 前进按钮点击事件
        /// </summary>
        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            var webView = GetCurrentWebView();
            if (webView != null && webView.CoreWebView2 != null && webView.CoreWebView2.CanGoForward)
            {
                webView.CoreWebView2.GoForward();
            }
        }
        
        /// <summary>
        /// 刷新按钮点击事件
        /// </summary>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var webView = GetCurrentWebView();
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Reload();
            }
        }
        
        /// <summary>
        /// 主页按钮点击事件
        /// </summary>
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            var webView = GetCurrentWebView();
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate("https://www.bing.com");
            }
        }
        
        /// <summary>
        /// 地址栏按键事件
        /// </summary>
        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string url = AddressBar.Text.Trim();
                
                // 检查是否是有效的URL
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    // 检查是否是搜索查询
                    if (url.Contains(" ") || !url.Contains("."))
                    {
                        url = "https://www.bing.com/search?q=" + Uri.EscapeDataString(url);
                    }
                    else
                    {
                        url = "https://" + url;
                    }
                }
                
                // 导航到URL
                var webView = GetCurrentWebView();
                if (webView != null && webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.Navigate(url);
                }
            }
        }
        
        /// <summary>
        /// 新建标签页按钮点击事件
        /// </summary>
        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTab();
        }
        
        /// <summary>
        /// 关闭标签页按钮点击事件
        /// </summary>
        private void CloseTabButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                int index = Convert.ToInt32(button.Tag);
                CloseTab(index);
            }
        }
        
        /// <summary>
        /// 标签页选择变更事件
        /// </summary>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedIndex >= 0)
            {
                currentTabIndex = TabControl.SelectedIndex;
                
                // 更新地址栏
                var webView = GetCurrentWebView();
                if (webView != null && webView.CoreWebView2 != null)
                {
                    AddressBar.Text = webView.CoreWebView2.Source.ToString();
                    Title = $"{webView.CoreWebView2.DocumentTitle} - CRK Browser";
                    
                    // 更新导航按钮状态
                    UpdateNavigationButtons();
                    
                    // 更新安全状态
                    if (webView.CoreWebView2.Source.ToString().StartsWith("https://"))
                    {
                        SecurityStatus.Text = "安全";
                        SecurityStatus.Foreground = (System.Windows.Media.Brush)FindResource("AccentColor");
                    }
                    else
                    {
                        SecurityStatus.Text = "不安全";
                        SecurityStatus.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
            }
        }
        
        /// <summary>
        /// 书签按钮点击事件
        /// </summary>
        private void BookmarksButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开书签窗口
            BookmarkWindow bookmarkWindow = new BookmarkWindow(bookmarkManager);
            bookmarkWindow.ShowDialog();
        }
        
        /// <summary>
        /// 获取当前标签页的 URL
        /// </summary>
        /// <returns>当前标签页的 URL</returns>
        public string GetCurrentTabUrl()
        {
            var webView = GetCurrentWebView();
            if (webView != null && webView.CoreWebView2 != null)
            {
                return webView.CoreWebView2.Source.ToString();
            }
            return string.Empty;
        }
        
        /// <summary>
        /// 获取当前标签页的标题
        /// </summary>
        /// <returns>当前标签页的标题</returns>
        public string GetCurrentTabTitle()
        {
            var webView = GetCurrentWebView();
            if (webView != null && webView.CoreWebView2 != null)
            {
                return webView.CoreWebView2.DocumentTitle;
            }
            return string.Empty;
        }
        
        /// <summary>
        /// 历史记录按钮点击事件
        /// </summary>
        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开历史记录窗口
            HistoryWindow historyWindow = new HistoryWindow(this);
            historyWindow.ShowDialog();
        }
        
        /// <summary>
        /// 导航到指定的 URL
        /// </summary>
        /// <param name="url">URL</param>
        public void Navigate(string url)
        {
            // 获取当前WebView2控件
            var webView = GetCurrentWebView();
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(url);
            }
        }
        
        /// <summary>
        /// 设置按钮点击事件
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开设置窗口
            SettingsWindow settingsWindow = new SettingsWindow(historyManager);
            settingsWindow.ShowDialog();
        }
        
        /// <summary>
        /// 更新界面文本
        /// </summary>
        private void UpdateUI()
        {
            // 更新窗口标题
            Title = languageManager.GetString("AppTitle");
            
            // 更新按钮文本
            // 注意：由于我们使用了图标作为按钮内容，这里只更新工具提示
            BackButton.ToolTip = languageManager.GetString("Back");
            ForwardButton.ToolTip = languageManager.GetString("Forward");
            RefreshButton.ToolTip = languageManager.GetString("Refresh");
            HomeButton.ToolTip = languageManager.GetString("Home");
            BookmarksButton.ToolTip = languageManager.GetString("Bookmarks");
            HistoryButton.ToolTip = languageManager.GetString("History");
            DownloadsButton.ToolTip = "下载";
            SettingsButton.ToolTip = languageManager.GetString("Settings");
            LanguageButton.ToolTip = languageManager.GetString("Language");
            UserButton.ToolTip = userManager.IsLoggedIn() ? userManager.GetCurrentUser()?.Username ?? languageManager.GetString("Login") : languageManager.GetString("Login");
            NewTabButton.ToolTip = languageManager.GetString("NewTab");
            
            // 更新状态栏文本
            StatusText.Text = languageManager.GetString("Completed");
        }
        
        /// <summary>
        /// 语言按钮点击事件
        /// </summary>
        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            // 显示语言选择菜单
            ContextMenu languageMenu = new ContextMenu();
            
            // 添加中文选项
            MenuItem chineseItem = new MenuItem();
            chineseItem.Header = languageManager.GetString("Chinese");
            chineseItem.Click += (s, args) =>
            {
                languageManager.SetCurrentLanguage(LanguageManager.Language.Chinese);
                UpdateUI();
            };
            languageMenu.Items.Add(chineseItem);
            
            // 添加英文选项
            MenuItem englishItem = new MenuItem();
            englishItem.Header = languageManager.GetString("English");
            englishItem.Click += (s, args) =>
            {
                languageManager.SetCurrentLanguage(LanguageManager.Language.English);
                UpdateUI();
            };
            languageMenu.Items.Add(englishItem);
            
            // 显示菜单
            languageMenu.PlacementTarget = sender as Button;
            languageMenu.IsOpen = true;
        }
        
        /// <summary>
        /// 最小化按钮点击事件
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        
        /// <summary>
        /// 最大化按钮点击事件
        /// </summary>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }
        
        /// <summary>
        /// 用户按钮点击事件
        /// </summary>
        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (userManager.IsLoggedIn())
            {
                // 显示用户菜单
                ContextMenu userMenu = new ContextMenu();
                
                // 添加用户名菜单项
                MenuItem userItem = new MenuItem();
                var currentUser = userManager.GetCurrentUser();
                userItem.Header = currentUser?.Username ?? "未知用户";
                userItem.IsEnabled = false;
                userMenu.Items.Add(userItem);
                
                // 添加分割线
                userMenu.Items.Add(new Separator());
                
                // 添加会员状态菜单项
                MenuItem memberItem = new MenuItem();
                memberItem.Header = currentUser?.IsMember ?? false ? "会员" : "普通用户";
                memberItem.IsEnabled = false;
                userMenu.Items.Add(memberItem);
                
                // 添加分割线
                userMenu.Items.Add(new Separator());
                
                // 添加注销菜单项
                MenuItem logoutItem = new MenuItem();
                logoutItem.Header = languageManager.GetString("Logout");
                logoutItem.Click += (s, args) =>
                {
                    userManager.Logout();
                    UpdateUI();
                    MessageBox.Show(languageManager.GetString("LogoutSuccess"), "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                };
                userMenu.Items.Add(logoutItem);
                
                // 显示菜单
                userMenu.PlacementTarget = sender as Button;
                userMenu.IsOpen = true;
            }
            else
            {
                // 显示登录窗口
                LoginWindow loginWindow = new LoginWindow(userManager);
                bool? result = loginWindow.ShowDialog();
                if (result == true)
                {
                    // 登录成功
                    UpdateUI();
                    MessageBox.Show(languageManager.GetString("LoginSuccess"), "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        
        /// <summary>
        /// 下载按钮点击事件
        /// </summary>
        private void DownloadsButton_Click(object sender, RoutedEventArgs e)
        {
            // 打开下载管理窗口
            DownloadWindow downloadWindow = new DownloadWindow(downloadManager);
            downloadWindow.Show();
        }
        
        /// <summary>
        /// 下载开始事件处理
        /// </summary>
        private async void WebView_DownloadStarting(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2DownloadStartingEventArgs e)
        {
            // 取消默认下载行为
            e.Cancel = true;
            
            // 获取下载URL和文件名
            string url = e.DownloadOperation.Uri;
            string fileName = e.ResultFilePath;
            
            // 显示保存文件对话框
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = System.IO.Path.GetFileName(fileName),
                InitialDirectory = downloadManager.GetDefaultDownloadPath(),
                Filter = "所有文件|*.*"
            };
            
            if (saveFileDialog.ShowDialog() == true)
            {
                string savePath = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);
                if (!string.IsNullOrEmpty(savePath))
                {
                    // 添加到下载任务
                    await downloadManager.AddDownloadTask(url, savePath);
                    
                    // 显示下载管理窗口
                    DownloadWindow downloadWindow = new DownloadWindow(downloadManager);
                    downloadWindow.Show();
                }
            }
        }
        
        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        /// <summary>
        /// 主题更改事件处理程序
        /// </summary>
        private void ThemeManager_ThemeChanged(object? sender, ThemeManager.ThemeType themeType)
        {
            // 主题更改时更新UI
            try
            {
                // 强制重新加载窗口资源
                UpdateUI();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新主题UI失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 切换到浅蓝色主题
        /// </summary>
        public void SwitchToLightBlueTheme()
        {
            themeManager.ChangeTheme(ThemeManager.ThemeType.LightBlue);
        }
        
        /// <summary>
        /// 切换到深蓝色主题
        /// </summary>
        public void SwitchToDarkBlueTheme()
        {
            themeManager.ChangeTheme(ThemeManager.ThemeType.DarkBlue);
        }
    }
}