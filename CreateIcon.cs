using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

public class IconCreator
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ICONDIR
    {
        public ushort idReserved; // Reserved (must be 0)
        public ushort idType;     // Resource type (1 for icons)
        public ushort idCount;    // How many images?
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ICONDIRENTRY
    {
        public byte bWidth;          // Width, in pixels, of the image
        public byte bHeight;         // Height, in pixels, of the image
        public byte bColorCount;     // Number of colors in image (0 if >=8bpp)
        public byte bReserved;       // Reserved (must be 0)
        public ushort wPlanes;       // Color Planes
        public ushort wBitCount;     // Bits per pixel
        public uint dwBytesInRes;     // How many bytes in this resource?
        public uint dwImageOffset;   // Where in the file is this image?
    }

    public static void Main(string[] args)
    {
        string path = "d:\\编程\\C++\\课外\\rickexpriment\\CRK-Browser.Net\\Resources\\app.ico";
        CreateMultiSizeRainbowIcon(path);
        Console.WriteLine("Icon created successfully at: " + path);
    }

    public static void CreateMultiSizeRainbowIcon(string path)
    {
        // 需要的图标尺寸
        int[] sizes = { 16, 32, 48, 128 };
        
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            // 写入图标目录
            ICONDIR iconDir = new ICONDIR
            {
                idReserved = 0,
                idType = 1,
                idCount = (ushort)sizes.Length
            };
            
            // 计算图标目录大小
            int iconDirSize = Marshal.SizeOf(typeof(ICONDIR));
            int iconEntrySize = Marshal.SizeOf(typeof(ICONDIRENTRY));
            int totalIconEntrySize = iconEntrySize * sizes.Length;
            
            // 写入图标目录
            WriteStruct(fs, iconDir);
            
            // 保存每个图标的偏移量
            List<uint> imageOffsets = new List<uint>();
            List<byte[]> imageDataList = new List<byte[]>();
            
            // 计算第一个图像的偏移量
            uint currentOffset = (uint)(iconDirSize + totalIconEntrySize);
            
            // 生成每个尺寸的图标
            foreach (int size in sizes)
            {
                // 创建位图
                using (Bitmap bmp = new Bitmap(size, size))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        // 填充背景
                        g.Clear(Color.Transparent);
                        
                        // 绘制彩虹渐变
                        for (int i = 0; i < size; i++)
                        {
                            float hue = (i / (float)size) * 360.0f;
                            Color color = ColorFromHSV(hue, 1.0f, 1.0f);
                            using (Pen pen = new Pen(color, 1))
                            {
                                g.DrawLine(pen, i, 0, i, size - 1);
                            }
                        }
                        
                        // 绘制圆形轮廓
                        int borderSize = size / 16;
                        if (borderSize < 1) borderSize = 1;
                        using (Pen pen = new Pen(Color.White, borderSize))
                        {
                            int margin = borderSize;
                            g.DrawEllipse(pen, margin, margin, size - 2 * margin, size - 2 * margin);
                        }
                        
                        // 绘制中心浏览器图标
                        DrawBrowserIcon(g, size);
                    }
                    
                    // 保存为PNG到内存流
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        byte[] imageData = ms.ToArray();
                        imageDataList.Add(imageData);
                        imageOffsets.Add(currentOffset);
                        currentOffset += (uint)imageData.Length;
                    }
                }
            }
            
            // 写入图标条目
            for (int i = 0; i < sizes.Length; i++)
            {
                int size = sizes[i];
                byte[] imageData = imageDataList[i];
                
                ICONDIRENTRY entry = new ICONDIRENTRY
                {
                    bWidth = (byte)size,
                    bHeight = (byte)size,
                    bColorCount = 0,
                    bReserved = 0,
                    wPlanes = 1,
                    wBitCount = 32,
                    dwBytesInRes = (uint)imageData.Length,
                    dwImageOffset = imageOffsets[i]
                };
                
                WriteStruct(fs, entry);
            }
            
            // 写入图像数据
            foreach (byte[] imageData in imageDataList)
            {
                fs.Write(imageData, 0, imageData.Length);
            }
        }
    }
    
    private static void DrawBrowserIcon(Graphics g, int size)
    {
        // 绘制简化的浏览器图标
        int centerX = size / 2;
        int centerY = size / 2;
        int radius = size / 4;
        
        // 绘制浏览器窗口
        using (Pen pen = new Pen(Color.White, 2))
        {
            // 绘制窗口边框
            int windowX = centerX - radius;
            int windowY = centerY - radius;
            int windowWidth = radius * 2;
            int windowHeight = radius * 2;
            
            g.DrawRectangle(pen, windowX, windowY, windowWidth, windowHeight);
            
            // 绘制地址栏
            int addressBarHeight = windowHeight / 4;
            g.DrawLine(pen, windowX, windowY + addressBarHeight, windowX + windowWidth, windowY + addressBarHeight);
        }
    }
    
    private static Color ColorFromHSV(float hue, float saturation, float value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);
        
        value = value * 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));
        
        if (hi == 0) return Color.FromArgb(255, v, t, p);
        else if (hi == 1) return Color.FromArgb(255, q, v, p);
        else if (hi == 2) return Color.FromArgb(255, p, v, t);
        else if (hi == 3) return Color.FromArgb(255, p, q, v);
        else if (hi == 4) return Color.FromArgb(255, t, p, v);
        else return Color.FromArgb(255, v, p, q);
    }
    
    private static void WriteStruct<T>(Stream stream, T structure)
    {
        int size = Marshal.SizeOf(typeof(T));
        byte[] buffer = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        
        try
        {
            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, buffer, 0, size);
            stream.Write(buffer, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}