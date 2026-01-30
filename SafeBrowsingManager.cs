using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CRK_Browser
{
    /// <summary>
    /// 安全浏览管理器
    /// </summary>
    public class SafeBrowsingManager
    {
        // 危险网站列表
        private List<string> dangerousSites;
        // 危险网站文件路径
        private string dangerousSitesFilePath;
        // HTTP客户端
        private HttpClient httpClient;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public SafeBrowsingManager()
        {
            dangerousSites = new List<string>();
            
            // 设置危险网站文件路径
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRK-Browser");
            string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "data");
            
            // 优先使用应用数据目录中的文件，如果不存在则使用资源目录中的文件
            dangerousSitesFilePath = Path.Combine(appDataPath, "dangerous_sites.json");
            if (!File.Exists(dangerousSitesFilePath))
            {
                dangerousSitesFilePath = Path.Combine(resourcesPath, "dangerous_sites.json");
            }
            
            // 初始化HTTP客户端
            httpClient = new HttpClient {
                Timeout = TimeSpan.FromSeconds(10)
            };
            
            // 加载危险网站列表
            LoadDangerousSites();
        }
        
        /// <summary>
        /// 加载危险网站列表
        /// </summary>
        private void LoadDangerousSites()
        {
            try
            {
                if (File.Exists(dangerousSitesFilePath))
                {
                    string json = File.ReadAllText(dangerousSitesFilePath);
                    var data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                    if (data != null && data.ContainsKey("dangerous_sites"))
                    {
                        dangerousSites = data["dangerous_sites"];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载危险网站列表失败: {ex.Message}");
                dangerousSites = new List<string>();
            }
        }
        
        /// <summary>
        /// 检查网站是否安全
        /// </summary>
        /// <param name="url">网站URL</param>
        /// <returns>是否安全</returns>
        public bool IsSafeSite(string url)
        {
            try
            {
                // 提取域名
                string domain = ExtractDomain(url);
                
                // 检查是否在危险网站列表中
                foreach (string dangerousSite in dangerousSites)
                {
                    if (domain.Contains(dangerousSite, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查网站安全性失败: {ex.Message}");
                return true; // 出错时默认认为网站安全
            }
        }
        
        /// <summary>
        /// 异步检查网站是否安全
        /// </summary>
        /// <param name="url">网站URL</param>
        /// <returns>是否安全</returns>
        public async Task<bool> IsSafeSiteAsync(string url)
        {
            return await Task.Run(() => IsSafeSite(url));
        }
        
        /// <summary>
        /// 提取URL中的域名
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>域名</returns>
        private string ExtractDomain(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return url;
            }
        }
        
        /// <summary>
        /// 添加危险网站
        /// </summary>
        /// <param name="url">网站URL</param>
        public void AddDangerousSite(string url)
        {
            try
            {
                string domain = ExtractDomain(url);
                if (!dangerousSites.Contains(domain))
                {
                    dangerousSites.Add(domain);
                    SaveDangerousSites();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加危险网站失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 移除危险网站
        /// </summary>
        /// <param name="url">网站URL</param>
        public void RemoveDangerousSite(string url)
        {
            try
            {
                string domain = ExtractDomain(url);
                if (dangerousSites.Contains(domain))
                {
                    dangerousSites.Remove(domain);
                    SaveDangerousSites();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"移除危险网站失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存危险网站列表
        /// </summary>
        private void SaveDangerousSites()
        {
            try
            {
                var data = new Dictionary<string, List<string>>
                {
                    { "dangerous_sites", dangerousSites }
                };
                
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                // 确保目录存在
                string? directory = Path.GetDirectoryName(dangerousSitesFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(dangerousSitesFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存危险网站列表失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 清除浏览数据
        /// </summary>
        /// <param name="clearHistory">是否清除历史记录</param>
        /// <param name="clearCookies">是否清除Cookie</param>
        /// <param name="clearCache">是否清除缓存</param>
        public void ClearBrowsingData(bool clearHistory, bool clearCookies, bool clearCache)
        {
            try
            {
                // 清除历史记录
                if (clearHistory)
                {
                    var historyManager = new HistoryManager();
                    historyManager.ClearHistory();
                }
                
                // 清除Cookie和缓存
                if (clearCookies || clearCache)
                {
                    // 注意：这里只是一个示例，实际实现需要调用WebView2的API
                    // WebView2的Cookie和缓存清除需要在每个WebView2控件上执行
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清除浏览数据失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取安全建议
        /// </summary>
        /// <param name="url">网站URL</param>
        /// <returns>安全建议</returns>
        public string GetSecurityAdvice(string url)
        {
            if (!url.StartsWith("https://"))
            {
                return "该网站使用的是HTTP协议，数据传输可能不安全。建议只在必要时访问，并避免输入敏感信息。";
            }
            
            if (!IsSafeSite(url))
            {
                return "该网站可能存在安全风险，建议谨慎访问，避免输入敏感信息。";
            }
            
            return "该网站看起来是安全的，可以正常访问。";
        }
    }
}
