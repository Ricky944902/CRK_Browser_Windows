using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CRK_Browser
{
    /// <summary>
    /// DownloadWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadWindow : Window
    {
        private DownloadManager _downloadManager;

        public ICommand StartPauseCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public DownloadWindow(DownloadManager downloadManager)
        {
            InitializeComponent();
            _downloadManager = downloadManager;

            // 初始化命令
            StartPauseCommand = new RelayCommand(StartPauseDownload);
            CancelCommand = new RelayCommand(CancelDownload);

            // 设置数据上下文
            DataContext = this;

            // 绑定下载任务列表
            DownloadListView.ItemsSource = _downloadManager.DownloadTasks;

            // 订阅下载管理器事件
            _downloadManager.TaskAdded += DownloadManager_TaskAdded;
            _downloadManager.TaskUpdated += DownloadManager_TaskUpdated;
            _downloadManager.TaskCompleted += DownloadManager_TaskCompleted;
        }

        private void DownloadManager_TaskAdded(object sender, DownloadTask e)
        {
            // 任务添加时刷新列表
            RefreshList();
        }

        private void DownloadManager_TaskUpdated(object sender, DownloadTask e)
        {
            // 任务更新时刷新列表
            RefreshList();
        }

        private void DownloadManager_TaskCompleted(object sender, DownloadTask e)
        {
            // 任务完成时刷新列表
            RefreshList();
        }

        private void RefreshList()
        {
            // 强制刷新列表
            CollectionViewSource.GetDefaultView(DownloadListView.ItemsSource).Refresh();
        }

        private async void StartPauseDownload(object parameter)
        {
            if (parameter is DownloadTask task)
            {
                if (task.Status == DownloadStatus.Paused || task.Status == DownloadStatus.Failed || task.Status == DownloadStatus.Waiting)
                {
                    await _downloadManager.StartDownloadAsync(task);
                }
                else if (task.Status == DownloadStatus.Downloading)
                {
                    _downloadManager.PauseDownload(task);
                }
            }
        }

        private void CancelDownload(object parameter)
        {
            if (parameter is DownloadTask task)
            {
                _downloadManager.CancelDownload(task);
            }
        }

        private void CleanupButton_Click(object sender, RoutedEventArgs e)
        {
            _downloadManager.CleanupCompletedTasks();
            RefreshList();
        }

        private void OpenDownloadsFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string downloadsFolder = _downloadManager.GetDefaultDownloadPath();
            if (Directory.Exists(downloadsFolder))
            {
                System.Diagnostics.Process.Start("explorer.exe", downloadsFolder);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    /// <summary>
    /// 大小转换器
    /// </summary>
    public class SizeConverter : IValueConverter
    {
        public static readonly SizeConverter Instance = new SizeConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long size)
            {
                if (size < 1024)
                    return $"{size} B";
                else if (size < 1024 * 1024)
                    return $"{size / 1024.0:F2} KB";
                else if (size < 1024 * 1024 * 1024)
                    return $"{size / (1024.0 * 1024.0):F2} MB";
                else
                    return $"{size / (1024.0 * 1024.0 * 1024.0):F2} GB";
            }
            return "0 B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 速度转换器
    /// </summary>
    public class SpeedConverter : IValueConverter
    {
        public static readonly SpeedConverter Instance = new SpeedConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double speed)
            {
                if (speed < 1024)
                    return $"{speed:F2} KB/s";
                else
                    return $"{speed / 1024.0:F2} MB/s";
            }
            return "0 KB/s";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 状态颜色转换器
    /// </summary>
    public class StatusColorConverter : IValueConverter
    {
        public static readonly StatusColorConverter Instance = new StatusColorConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DownloadStatus status)
            {
                switch (status)
                {
                    case DownloadStatus.Downloading:
                        return System.Windows.Media.Brushes.Blue;
                    case DownloadStatus.Completed:
                        return System.Windows.Media.Brushes.Green;
                    case DownloadStatus.Paused:
                        return System.Windows.Media.Brushes.Orange;
                    case DownloadStatus.Failed:
                        return System.Windows.Media.Brushes.Red;
                    case DownloadStatus.Canceled:
                        return System.Windows.Media.Brushes.Gray;
                    default:
                        return System.Windows.Media.Brushes.Gray;
                }
            }
            return System.Windows.Media.Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 状态到按钮文本转换器
    /// </summary>
    public class StatusToButtonTextConverter : IValueConverter
    {
        public static readonly StatusToButtonTextConverter Instance = new StatusToButtonTextConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DownloadStatus status)
            {
                switch (status)
                {
                    case DownloadStatus.Downloading:
                        return "暂停";
                    case DownloadStatus.Paused:
                    case DownloadStatus.Failed:
                    case DownloadStatus.Waiting:
                        return "开始";
                    default:
                        return "";
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 能否开始/暂停转换器
    /// </summary>
    public class CanStartPauseConverter : IValueConverter
    {
        public static readonly CanStartPauseConverter Instance = new CanStartPauseConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DownloadStatus status)
            {
                return status == DownloadStatus.Downloading || 
                       status == DownloadStatus.Paused || 
                       status == DownloadStatus.Failed || 
                       status == DownloadStatus.Waiting;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 能否取消转换器
    /// </summary>
    public class CanCancelConverter : IValueConverter
    {
        public static readonly CanCancelConverter Instance = new CanCancelConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DownloadStatus status)
            {
                return status == DownloadStatus.Downloading || 
                       status == DownloadStatus.Paused || 
                       status == DownloadStatus.Waiting;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 命令实现类
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
