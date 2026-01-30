using System;
using System.Windows;
using System.Windows.Media;

namespace CRK_Browser
{
    /// <summary>
    /// 主题管理器，用于管理和切换应用程序主题
    /// </summary>
    public class ThemeManager
    {
        /// <summary>
        /// 主题类型
        /// </summary>
        public enum ThemeType
        {
            LightBlue,   // 浅蓝色主题
            DarkBlue     // 深蓝色主题
        }

        /// <summary>
        /// 当前主题
        /// </summary>
        public ThemeType CurrentTheme { get; private set; }

        /// <summary>
        /// 主题更改事件
        /// </summary>
        public event EventHandler<ThemeType>? ThemeChanged;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ThemeManager()
        {
            // 默认使用浅蓝色主题
            CurrentTheme = ThemeType.LightBlue;
        }
        
        /// <summary>
        /// 初始化主题
        /// </summary>
        public void InitializeTheme()
        {
            ApplyTheme(CurrentTheme);
        }

        /// <summary>
        /// 切换主题
        /// </summary>
        /// <param name="themeType">主题类型</param>
        public void ChangeTheme(ThemeType themeType)
        {
            if (CurrentTheme != themeType)
            {
                CurrentTheme = themeType;
                ApplyTheme(themeType);
                ThemeChanged?.Invoke(this, themeType);
            }
        }

        /// <summary>
        /// 应用主题
        /// </summary>
        /// <param name="themeType">主题类型</param>
        private void ApplyTheme(ThemeType themeType)
        {
            try
            {
                if (Application.Current == null)
                    return;

                // 更新主题资源
                switch (themeType)
                {
                    case ThemeType.LightBlue:
                        // 浅蓝色主题（现代Edge风格）
                        Application.Current.Resources["PrimaryColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a73e8"));
                        Application.Current.Resources["PrimaryLightColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8f0fe"));
                        Application.Current.Resources["PrimaryDarkColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1557b0"));
                        Application.Current.Resources["BackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffffff"));
                        Application.Current.Resources["TextColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202124"));
                        Application.Current.Resources["TextLightColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5f6368"));
                        Application.Current.Resources["BorderColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dadce0"));
                        Application.Current.Resources["AccentColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34a853"));
                        Application.Current.Resources["TitleBarColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a73e8"));
                        break;

                    case ThemeType.DarkBlue:
                        // 深蓝色主题（现代Edge深色风格）
                        Application.Current.Resources["PrimaryColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2954d6"));
                        Application.Current.Resources["PrimaryLightColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1e293b"));
                        Application.Current.Resources["PrimaryDarkColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a2542"));
                        Application.Current.Resources["BackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121212"));
                        Application.Current.Resources["TextColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8eaed"));
                        Application.Current.Resources["TextLightColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9aa0a6"));
                        Application.Current.Resources["BorderColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3c4043"));
                        Application.Current.Resources["AccentColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#34a853"));
                        Application.Current.Resources["TitleBarColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1f1f1f"));
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"应用主题失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 移除现有的主题资源
        /// </summary>
        private void RemoveThemeResources()
        {
            try
            {
                if (Application.Current == null)
                    return;

                // 移除所有主题相关的资源
                string[] themeResourceKeys = {
                    "PrimaryColor",
                    "PrimaryLightColor",
                    "PrimaryDarkColor",
                    "BackgroundColor",
                    "TextColor",
                    "TextLightColor",
                    "BorderColor",
                    "AccentColor",
                    "TitleBarColor"
                };

                foreach (string key in themeResourceKeys)
                {
                    if (Application.Current.Resources.Contains(key))
                    {
                        Application.Current.Resources.Remove(key);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"移除主题资源失败: {ex.Message}");
            }
        }
    }
}