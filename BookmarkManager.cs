using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CRK_Browser
{
    /// <summary>
    /// 书签项
    /// </summary>
    public class BookmarkItem
    {
        /// <summary>
        /// 书签ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// URL
        /// </summary>
        public string Url { get; set; } = string.Empty;
        
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 文件夹
        /// </summary>
        public string Folder { get; set; } = string.Empty;
        
        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BookmarkItem()
        {
            Id = Guid.NewGuid().ToString();
            Url = string.Empty;
            Title = string.Empty;
            Folder = "默认文件夹";
            AddTime = DateTime.Now;
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="title">标题</param>
        /// <param name="folder">文件夹</param>
        public BookmarkItem(string url, string title, string folder = "默认文件夹")
        {
            Id = Guid.NewGuid().ToString();
            Url = url;
            Title = title;
            Folder = folder;
            AddTime = DateTime.Now;
        }
    }
    
    /// <summary>
    /// 书签管理器
    /// </summary>
    public class BookmarkManager
    {
        // 书签列表
        private List<BookmarkItem> bookmarks;
        // 书签数据文件路径
        private string bookmarkDataFilePath;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public BookmarkManager()
        {
            // 初始化书签列表
            bookmarks = new List<BookmarkItem>();
            
            // 设置书签数据文件路径
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRK-Browser");
            Directory.CreateDirectory(appDataPath);
            bookmarkDataFilePath = Path.Combine(appDataPath, "bookmarks.json");
            
            // 加载书签数据
            LoadBookmarks();
        }
        
        /// <summary>
        /// 添加书签
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="title">标题</param>
        /// <param name="folder">文件夹</param>
        public void AddBookmark(string url, string title, string folder = "默认文件夹")
        {
            try
            {
                // 创建新书签
                BookmarkItem bookmark = new BookmarkItem(url, title, folder);
                
                // 检查是否已存在相同URL的书签
                if (!bookmarks.Any(b => b.Url == url))
                {
                    bookmarks.Add(bookmark);
                    SaveBookmarks();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加书签失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 移除书签
        /// </summary>
        /// <param name="url">URL</param>
        public void RemoveBookmark(string url)
        {
            try
            {
                bookmarks.RemoveAll(b => b.Url == url);
                SaveBookmarks();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"移除书签失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取所有书签
        /// </summary>
        /// <returns>书签列表</returns>
        public List<BookmarkItem> GetBookmarks()
        {
            return bookmarks;
        }
        
        /// <summary>
        /// 获取指定文件夹的书签
        /// </summary>
        /// <param name="folder">文件夹</param>
        /// <returns>书签列表</returns>
        public List<BookmarkItem> GetBookmarksByFolder(string folder)
        {
            return bookmarks.Where(b => b.Folder == folder).ToList();
        }
        
        /// <summary>
        /// 获取所有文件夹
        /// </summary>
        /// <returns>文件夹列表</returns>
        public List<string> GetFolders()
        {
            return bookmarks.Select(b => b.Folder).Distinct().ToList();
        }
        
        /// <summary>
        /// 加载书签数据
        /// </summary>
        private void LoadBookmarks()
        {
            try
            {
                if (File.Exists(bookmarkDataFilePath))
                {
                    string json = File.ReadAllText(bookmarkDataFilePath);
                    bookmarks = JsonSerializer.Deserialize<List<BookmarkItem>>(json) ?? new List<BookmarkItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载书签失败: {ex.Message}");
                bookmarks = new List<BookmarkItem>();
            }
        }
        
        /// <summary>
        /// 保存书签数据
        /// </summary>
        private void SaveBookmarks()
        {
            try
            {
                string json = JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(bookmarkDataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存书签失败: {ex.Message}");
            }
        }
    }
}
