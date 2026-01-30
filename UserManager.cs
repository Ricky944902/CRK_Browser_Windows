using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CRK_Browser
{
    /// <summary>
    /// 用户类
    /// </summary>
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime RegisterTime { get; set; }
        public DateTime LastLoginTime { get; set; }
        public bool IsMember { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public User()
        {
            Id = Guid.NewGuid().ToString();
            Username = string.Empty;
            PasswordHash = string.Empty;
            RegisterTime = DateTime.Now;
            LastLoginTime = DateTime.Now;
            IsMember = false;
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        public User(string username, string password)
        {
            Id = Guid.NewGuid().ToString();
            Username = username;
            PasswordHash = HashPassword(password);
            RegisterTime = DateTime.Now;
            LastLoginTime = DateTime.Now;
            IsMember = false;
        }
        
        /// <summary>
        /// 密码哈希
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns>哈希后的密码</returns>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        
        /// <summary>
        /// 验证密码
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns>密码是否正确</returns>
        public bool VerifyPassword(string password)
        {
            string hashedPassword = HashPassword(password);
            return PasswordHash == hashedPassword;
        }
    }
    
    /// <summary>
    /// 用户管理器
    /// </summary>
    public class UserManager
    {
        // 用户列表
        private List<User> users;
        // 当前登录用户，改为可为 null
        private User? currentUser;
        // 用户数据文件路径
        private string userDataFilePath;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public UserManager()
        {
            // 初始化用户列表
            users = new List<User>();
            currentUser = null;
            
            // 设置用户数据文件路径
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRK-Browser");
            Directory.CreateDirectory(appDataPath);
            userDataFilePath = Path.Combine(appDataPath, "users.json");
            
            // 加载用户数据
            LoadUsers();
        }
        
        /// <summary>
        /// 注册用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>是否注册成功</returns>
        public bool Register(string username, string password)
        {
            try
            {
                // 检查用户名是否已存在
                if (users.Any(u => u.Username == username))
                {
                    return false;
                }
                
                // 创建新用户
                User newUser = new User(username, password);
                users.Add(newUser);
                
                // 保存用户数据
                SaveUsers();
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 验证用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>是否验证成功</returns>
        public bool ValidateUser(string username, string password)
        {
            try
            {
                // 查找用户
                User? user = users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    return false;
                }
                
                // 验证密码
                if (user.VerifyPassword(password))
                {
                    // 更新最后登录时间
                    user.LastLoginTime = DateTime.Now;
                    currentUser = user;
                    SaveUsers();
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <returns>用户对象</returns>
        public User? GetUser(string username)
        {
            return users.FirstOrDefault(u => u.Username == username);
        }
        
        /// <summary>
        /// 获取当前登录用户
        /// </summary>
        /// <returns>当前登录用户</returns>
        public User? GetCurrentUser()
        {
            return currentUser;
        }
        
        /// <summary>
        /// 登出
        /// </summary>
        public void Logout()
        {
            currentUser = null;
        }
        
        /// <summary>
        /// 加载用户数据
        /// </summary>
        private void LoadUsers()
        {
            try
            {
                if (File.Exists(userDataFilePath))
                {
                    string json = File.ReadAllText(userDataFilePath);
                    users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                }
            }
            catch
            {
                users = new List<User>();
            }
        }
        
        /// <summary>
        /// 保存用户数据
        /// </summary>
        private void SaveUsers()
        {
            try
            {
                string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(userDataFilePath, json);
            }
            catch
            {
            }
        }
        
        /// <summary>
        /// 注册用户（公开方法）
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>是否注册成功</returns>
        public bool RegisterUser(string username, string password)
        {
            return Register(username, password);
        }
        
        /// <summary>
        /// 检查用户是否已登录
        /// </summary>
        /// <returns>用户是否已登录</returns>
        public bool IsLoggedIn()
        {
            return currentUser != null;
        }
    }
}