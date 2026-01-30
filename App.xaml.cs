using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace CRK_Browser
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 处理未捕获的异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            
            // 初始化应用程序设置
            InitializeSettings();
            
            // 检查必要的目录是否存在
            EnsureDirectoriesExist();
        }
        
        /// <summary>
        /// 确保必要的目录存在
        /// </summary>
        private void EnsureDirectoriesExist()
        {
            try
            {
                // 确保应用程序数据目录存在
                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRK-Browser");
                Directory.CreateDirectory(appDataPath);
                
                // 确保WebView2目录存在
                string webView2Path = Path.Combine(appDataPath, "WebView2");
                Directory.CreateDirectory(webView2Path);
                
                // 确保用户数据目录存在
                string userDataPath = Path.Combine(appDataPath, "UserData");
                Directory.CreateDirectory(userDataPath);
                
                // 确保历史记录和书签数据目录存在
                string dataPath = Path.Combine(appDataPath, "Data");
                Directory.CreateDirectory(dataPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化目录失败: {ex.Message}\n\n应用程序将尝试继续运行，但某些功能可能不可用。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            
            // 保存应用程序设置
            SaveSettings();
        }
        
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // 处理非UI线程的未捕获异常
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"应用程序发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // 处理UI线程的未捕获异常
            MessageBox.Show($"应用程序发生错误: {e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
        
        private void InitializeSettings()
        {
            // 初始化应用程序设置
            // 这里可以加载配置文件、用户设置等
        }
        
        private void SaveSettings()
        {
            // 保存应用程序设置
            // 这里可以保存配置文件、用户设置等
        }
    }
}