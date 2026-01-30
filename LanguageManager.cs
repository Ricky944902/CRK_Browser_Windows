using System;
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace CRK_Browser
{
    /// <summary>
    /// 语言管理器，用于管理多语言支持和无缝切换功能
    /// </summary>
    public class LanguageManager
    {
        // 支持的语言列表
        public enum Language
        {
            Chinese,
            English
        }
        
        // 当前语言
        private Language currentLanguage;
        // 资源管理器
        private ResourceManager resourceManager;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public LanguageManager()
        {
            // 初始化资源管理器
            resourceManager = new ResourceManager("CRK_Browser.Resources.Locales", Assembly.GetExecutingAssembly());
            
            // 初始化当前语言为系统默认语言
            if (CultureInfo.CurrentCulture.Name.StartsWith("zh"))
            {
                currentLanguage = Language.Chinese;
            }
            else
            {
                currentLanguage = Language.English;
            }
        }
        
        /// <summary>
        /// 获取当前语言
        /// </summary>
        /// <returns>当前语言</returns>
        public Language GetCurrentLanguage()
        {
            return currentLanguage;
        }
        
        /// <summary>
        /// 设置当前语言
        /// </summary>
        /// <param name="language">要设置的语言</param>
        public void SetCurrentLanguage(Language language)
        {
            currentLanguage = language;
            
            // 更新当前线程的文化信息
            CultureInfo cultureInfo;
            if (language == Language.Chinese)
            {
                cultureInfo = new CultureInfo("zh-CN");
            }
            else
            {
                cultureInfo = new CultureInfo("en-US");
            }
            CultureInfo.CurrentUICulture = cultureInfo;
        }
        
        /// <summary>
        /// 获取本地化的字符串
        /// </summary>
        /// <param name="key">字符串键</param>
        /// <returns>本地化的字符串</returns>
        public string GetString(string key)
        {
            try
            {
                // 根据当前语言选择相应的文化信息
                CultureInfo cultureInfo;
                if (currentLanguage == Language.Chinese)
                {
                    cultureInfo = new CultureInfo("zh-CN");
                }
                else
                {
                    cultureInfo = new CultureInfo("en-US");
                }
                
                // 从资源管理器获取字符串
                string? value = resourceManager.GetString(key, cultureInfo);
                
                // 如果找不到键，返回键本身
                return value ?? key;
            }
            catch
            {
                // 如果加载资源失败，返回键本身
                return key;
            }
        }
        
        /// <summary>
        /// 获取语言显示名称
        /// </summary>
        /// <param name="language">语言</param>
        /// <returns>语言显示名称</returns>
        public string GetLanguageDisplayName(Language language)
        {
            switch (language)
            {
                case Language.Chinese:
                    return GetString("Chinese");
                case Language.English:
                    return GetString("English");
                default:
                    return "Unknown";
            }
        }
        
        /// <summary>
        /// 切换语言
        /// </summary>
        public void ToggleLanguage()
        {
            if (currentLanguage == Language.Chinese)
            {
                SetCurrentLanguage(Language.English);
            }
            else
            {
                SetCurrentLanguage(Language.Chinese);
            }
        }
    }
}