using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CRK_Browser
{
    /// <summary>
    /// HistoryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryWindow : Window
    {
        // 历史记录管理器
        private HistoryManager historyManager;
        // 主窗口引用
        private MainWindow mainWindow;
        // 是否按日期分组
        private bool groupByDate;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="mainWindow">主窗口引用</param>
        public HistoryWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            
            this.mainWindow = mainWindow;
            historyManager = new HistoryManager();
            groupByDate = true;
            
            // 加载历史记录
            LoadHistory();
        }
        
        /// <summary>
        /// 加载历史记录
        /// </summary>
        private void LoadHistory()
        {
            try
            {
                // 清空列表
                HistoryListView.Items.Clear();
                
                if (groupByDate)
                {
                    // 按日期分组加载历史记录
                    var historyByDate = historyManager.GetHistoryByDate();
                    
                    // 添加分组项
                    foreach (var group in historyByDate)
                    {
                        // 添加日期分组头
                        var dateHeader = new { Title = $"{group.Key}", Url = $"共 {group.Value.Count} 条记录", VisitTime = DateTime.MinValue };
                        HistoryListView.Items.Add(dateHeader);
                        
                        // 添加分组内的历史记录
                        foreach (var item in group.Value)
                        {
                            HistoryListView.Items.Add(item);
                        }
                    }
                    
                    // 更新状态栏
                    StatusText.Text = $"共 {historyManager.GetHistory().Count} 条历史记录，按日期分组显示";
                }
                else
                {
                    // 获取历史记录
                    var historyItems = historyManager.GetHistory();
                    
                    // 添加到列表
                    foreach (var item in historyItems)
                    {
                        HistoryListView.Items.Add(item);
                    }
                    
                    // 更新状态栏
                    StatusText.Text = $"共 {historyItems.Count} 条历史记录";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载历史记录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
        /// 清除历史记录按钮点击事件
        /// </summary>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // 确认清除
            var result = MessageBox.Show("确定要清除所有历史记录吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // 清除历史记录
                historyManager.ClearHistory();
                
                // 重新加载历史记录
                LoadHistory();
                
                // 显示提示
                MessageBox.Show("历史记录已清除", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        /// <summary>
        /// 按日期分组复选框点击事件
        /// </summary>
        private void GroupByDateCheckBox_Click(object sender, RoutedEventArgs e)
        {
            groupByDate = GroupByDateCheckBox.IsChecked ?? false;
            LoadHistory();
        }
        
        /// <summary>
        /// 历史记录列表双击事件
        /// </summary>
        private void HistoryListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 获取选中的历史记录
            var selectedItem = HistoryListView.SelectedItem as HistoryItem;
            if (selectedItem != null)
            {
                // 在主窗口中打开历史记录
                mainWindow?.Navigate(selectedItem.Url);
                
                // 关闭历史记录窗口
                Close();
            }
        }
        
        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }
    }
}