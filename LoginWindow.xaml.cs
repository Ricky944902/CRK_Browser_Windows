using System;
using System.Windows;

namespace CRK_Browser
{
    public partial class LoginWindow : Window
    {
        private UserManager userManager;
        public bool IsLoggedIn { get; private set; }
        // 当前登录用户，改为可为 null
        public User? CurrentUser { get; private set; }
        
        public LoginWindow(UserManager manager)
        {
            InitializeComponent();
            userManager = manager;
            IsLoggedIn = false;
            CurrentUser = null;
        }
        
        /// <summary>
        /// 登录按钮点击事件
        /// </summary>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            
            // 验证输入
            if (string.IsNullOrEmpty(username))
            {
                ShowError("请输入用户名");
                return;
            }
            
            if (string.IsNullOrEmpty(password))
            {
                ShowError("请输入密码");
                return;
            }
            
            try
            {
                // 验证用户
                if (userManager.ValidateUser(username, password))
                {
                    // 登录成功
                    IsLoggedIn = true;
                    CurrentUser = userManager.GetUser(username);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError("用户名或密码错误");
                }
            }
            catch (Exception ex)
            {
                ShowError($"登录失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 注册按钮点击事件
        /// </summary>
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            
            // 验证输入
            if (string.IsNullOrEmpty(username))
            {
                ShowError("请输入用户名");
                return;
            }
            
            if (string.IsNullOrEmpty(password))
            {
                ShowError("请输入密码");
                return;
            }
            
            try
            {
                // 注册用户
                if (userManager.RegisterUser(username, password))
                {
                    ShowSuccess("注册成功，请登录");
                }
                else
                {
                    ShowError("用户名已存在");
                }
            }
            catch (Exception ex)
            {
                ShowError($"注册失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="message">错误信息</param>
        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Foreground = System.Windows.Media.Brushes.Red;
            ErrorText.Visibility = Visibility.Visible;
        }
        
        /// <summary>
        /// 显示成功信息
        /// </summary>
        /// <param name="message">成功信息</param>
        private void ShowSuccess(string message)
        {
            ErrorText.Text = message;
            ErrorText.Foreground = System.Windows.Media.Brushes.Green;
            ErrorText.Visibility = Visibility.Visible;
        }
        
        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        /// <summary>
        /// 最小化按钮点击事件
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        
        /// <summary>
        /// 忘记密码按钮点击事件
        /// </summary>
        private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            // 这里可以实现忘记密码的逻辑
            ShowError("忘记密码功能暂未实现");
        }
        
        /// <summary>
        /// 游客模式按钮点击事件
        /// </summary>
        private void GuestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            // 游客模式登录，不需要验证
            IsLoggedIn = true;
            CurrentUser = null;
            DialogResult = true;
            Close();
        }
    }
}
