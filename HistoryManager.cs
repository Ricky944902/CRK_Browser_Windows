using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CRK_Browser
{
    /// <summary>
    /// 历史记录项
    /// </summary>
    public class HistoryItem
    {
        public string Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime VisitTime { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public HistoryItem()
        {
            Id = Guid.NewGuid().ToString();
            Url = string.Empty;
            Title = string.Empty;
            VisitTime = DateTime.Now;
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="title">标题</param>
        public HistoryItem(string url, string title)
        {
            Id = Guid.NewGuid().ToString();
            Url = url;
            Title = title;
            VisitTime = DateTime.Now;
        }
    }
    
    /// <summary>
    /// 历史记录管理器
    /// </summary>
    public class HistoryManager
    {
        // 历史记录列表
        private List<HistoryItem> history;
        // 历史记录数据文件路径
        private string historyDataFilePath;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public HistoryManager()
        {
            // 初始化历史记录列表
            history = new List<HistoryItem>();
            
            // 设置历史记录数据文件路径
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRK-Browser");
            Directory.CreateDirectory(appDataPath);
            historyDataFilePath = Path.Combine(appDataPath, "history.json");
            
            // 加载历史记录数据
            LoadHistory();
        }
        
        /// <summary>
        /// 添加历史记录
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="title">标题</param>
        public void AddHistory(string url, string title)
        {
            try
            {
                // 创建新历史记录
                HistoryItem historyItem = new HistoryItem(url, title);
                
                // 添加到历史记录列表开头
                history.Insert(0, historyItem);
                
                // 限制历史记录数量
                if (history.Count > 1000)
                {
                    history = history.Take(1000).ToList();
                }
                
                SaveHistory();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加历史记录失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 清除历史记录
        /// </summary>
        public void ClearHistory()
        {
            try
            {
                history.Clear();
                SaveHistory();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清除历史记录失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取历史记录
        /// </summary>
        /// <param name="count">数量</param>
        /// <returns>历史记录列表</returns>
        public List<HistoryItem> GetHistory(int count = 100)
        {
            return history.Take(count).ToList();
        }
        
        /// <summary>
        /// 根据日期获取历史记录
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns>历史记录列表</returns>
        public List<HistoryItem> GetHistoryByDate(DateTime date)
        {
            return history.Where(h => h.VisitTime.Date == date.Date).ToList();
        }
        
        /// <summary>
        /// 获取按日期分组的历史记录
        /// </summary>
        /// <returns>按日期分组的历史记录</returns>
        public Dictionary<DateTime, List<HistoryItem>> GetHistoryByDate()
        {
            return history.GroupBy(h => h.VisitTime.Date)
                         .OrderByDescending(g => g.Key)
                         .ToDictionary(g => g.Key, g => g.ToList());
        }
        
        /// <summary>
        /// 搜索历史记录
        /// </summary>
        /// <param name="keyword">关键词</param>
        /// <returns>历史记录列表</returns>
        public List<HistoryItem> SearchHistory(string keyword)
        {
            return history.Where(h => h.Title.Contains(keyword) || h.Url.Contains(keyword)).ToList();
        }
        
        /// <summary>
        /// 加载历史记录数据
        /// </summary>
        private void LoadHistory()
        {
            try
            {
                if (File.Exists(historyDataFilePath))
                {
                    string json = File.ReadAllText(historyDataFilePath);
                    history = JsonSerializer.Deserialize<List<HistoryItem>>(json) ?? new List<HistoryItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载历史记录失败: {ex.Message}");
                history = new List<HistoryItem>();
            }
        }
        
        /// <summary>
        /// 保存历史记录数据
        /// </summary>
        private void SaveHistory()
        {
            try
            {
                string json = JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(historyDataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存历史记录失败: {ex.Message}");
            }
        }
    }
}
