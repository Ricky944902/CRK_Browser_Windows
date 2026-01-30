using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CRK_Browser
{
    public partial class BookmarkWindow : Window
    {
        private BookmarkManager bookmarkManager;
        private string currentFolder = "所有文件夹";
        
        public BookmarkWindow(BookmarkManager bookmarkManager)
        {
            InitializeComponent();
            this.bookmarkManager = bookmarkManager;
            LoadBookmarks();
        }
        
        private void LoadBookmarks()
        {
            try
            {
                BookmarkListView.Items.Clear();
                FolderComboBox.Items.Clear();
                
                // 添加文件夹选项
                FolderComboBox.Items.Add("所有文件夹");
                foreach (string folder in bookmarkManager.GetFolders())
                {
                    FolderComboBox.Items.Add(folder);
                }
                FolderComboBox.SelectedItem = currentFolder;
                
                // 获取书签
                var bookmarkItems = bookmarkManager.GetBookmarks();
                
                // 过滤书签
                if (currentFolder != "所有文件夹")
                {
                    bookmarkItems = bookmarkItems.Where(item => item.Folder == currentFolder).ToList();
                }
                
                // 添加到列表
                foreach (var item in bookmarkItems)
                {
                    BookmarkListView.Items.Add(item);
                }
                
                // 更新状态栏
                StatusText.Text = $"共 {bookmarkItems.Count} 个书签";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载书签失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 获取当前激活的浏览器窗口
                MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    string url = mainWindow.GetCurrentTabUrl();
                    string title = mainWindow.GetCurrentTabTitle();
                    
                    if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(title))
                    {
                        // 显示添加书签对话框
                        AddBookmarkDialog dialog = new AddBookmarkDialog(url, title);
                        if (dialog.ShowDialog() == true)
                        {
                            // 添加书签
                            bookmarkManager.AddBookmark(dialog.Url, dialog.Title, dialog.Folder);
                            LoadBookmarks();
                            MessageBox.Show("书签已添加", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("无法获取当前页面信息，请确保页面已加载完成", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加书签失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BookmarkListView.SelectedItem is BookmarkItem selectedItem)
                {
                    if (MessageBox.Show($"确定要删除书签 '{selectedItem.Title}' 吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        bookmarkManager.RemoveBookmark(selectedItem.Url);
                        LoadBookmarks();
                        MessageBox.Show("书签已删除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("请先选择要删除的书签", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除书签失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void FolderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FolderComboBox.SelectedItem is string folder)
            {
                currentFolder = folder;
                LoadBookmarks();
            }
        }
        
        private void BookmarkListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (BookmarkListView.SelectedItem is BookmarkItem selectedItem)
                {
                    // 打开书签
                    MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.Navigate(selectedItem.Url);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开书签失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        
        /// <summary>
        /// 添加书签对话框
        /// </summary>
        private class AddBookmarkDialog : Window
        {
            // 控件
            private TextBox titleTextBox;
            private ComboBox folderComboBox;
            private Button okButton;
            private Button cancelButton;
            
            // 属性
            public string Url { get; set; } = string.Empty;
            
            // 使用 new 关键字明确隐藏继承的 Title 属性
            public new string Title { get; set; } = string.Empty;
            
            // 文件夹属性，添加默认值
            public string Folder { get; set; } = "默认文件夹";
            
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="url">URL</param>
            /// <param name="title">标题</param>
            public AddBookmarkDialog(string url, string title)
            {
                // 设置窗口属性
                Width = 400;
                Height = 200;
                base.Title = "添加书签"; // 使用 base.Title 设置窗口标题
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                ResizeMode = ResizeMode.NoResize;
                
                // 创建布局
                Grid grid = new Grid();
                grid.Margin = new Thickness(12);
                
                // 添加行定义
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                
                // 添加标题标签
                Label titleLabel = new Label();
                titleLabel.Content = "标题:";
                titleLabel.Margin = new Thickness(0, 0, 0, 4);
                Grid.SetRow(titleLabel, 0);
                grid.Children.Add(titleLabel);
                
                // 添加标题文本框
                titleTextBox = new TextBox();
                titleTextBox.Text = title;
                titleTextBox.Margin = new Thickness(0, 0, 0, 12);
                Grid.SetRow(titleTextBox, 1);
                grid.Children.Add(titleTextBox);
                
                // 添加文件夹标签
                Label folderLabel = new Label();
                folderLabel.Content = "文件夹:";
                folderLabel.Margin = new Thickness(0, 0, 0, 4);
                Grid.SetRow(folderLabel, 2);
                grid.Children.Add(folderLabel);
                
                // 添加文件夹下拉框
                folderComboBox = new ComboBox();
                folderComboBox.Margin = new Thickness(0, 0, 0, 12);
                folderComboBox.Items.Add("默认文件夹");
                folderComboBox.Items.Add("工作");
                folderComboBox.Items.Add("个人");
                folderComboBox.Items.Add("学习");
                folderComboBox.SelectedItem = "默认文件夹";
                Grid.SetRow(folderComboBox, 3);
                grid.Children.Add(folderComboBox);
                
                // 添加按钮容器
                StackPanel buttonPanel = new StackPanel();
                buttonPanel.Orientation = Orientation.Horizontal;
                buttonPanel.HorizontalAlignment = HorizontalAlignment.Right;
                buttonPanel.Margin = new Thickness(0, 8, 0, 0);
                Grid.SetRow(buttonPanel, 4);
                grid.Children.Add(buttonPanel);
                
                // 添加取消按钮
                cancelButton = new Button();
                cancelButton.Content = "取消";
                cancelButton.Width = 80;
                cancelButton.Margin = new Thickness(0, 0, 8, 0);
                cancelButton.Click += CancelButton_Click;
                buttonPanel.Children.Add(cancelButton);
                
                // 添加确定按钮
                okButton = new Button();
                okButton.Content = "确定";
                okButton.Width = 80;
                okButton.Click += OkButton_Click;
                buttonPanel.Children.Add(okButton);
                
                // 设置内容
                Content = grid;
                
                // 保存参数
                Url = url;
                Title = title;
            }
            
            private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Title = titleTextBox.Text;
            Folder = folderComboBox.SelectedItem as string ?? "默认文件夹";
            DialogResult = true;
            Close();
        }
            
            private void CancelButton_Click(object sender, RoutedEventArgs e)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
