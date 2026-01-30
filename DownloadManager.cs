using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CRK_Browser
{
    /// <summary>
    /// 下载任务状态
    /// </summary>
    public enum DownloadStatus
    {
        Waiting,
        Downloading,
        Paused,
        Completed,
        Failed,
        Canceled
    }

    /// <summary>
    /// 下载任务类
    /// </summary>
    public class DownloadTask : INotifyPropertyChanged
    {
        private string _id;
        private string _url;
        private string _fileName;
        private string _savePath;
        private long _totalSize;
        private long _downloadedSize;
        private DownloadStatus _status;
        private string _errorMessage;
        private double _progress;
        private double _downloadSpeed;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string SavePath
        {
            get => _savePath;
            set => SetProperty(ref _savePath, value);
        }

        public long TotalSize
        {
            get => _totalSize;
            set => SetProperty(ref _totalSize, value);
        }

        public long DownloadedSize
        {
            get => _downloadedSize;
            set
            {
                if (SetProperty(ref _downloadedSize, value))
                {
                    if (TotalSize > 0)
                    {
                        Progress = (double)DownloadedSize / TotalSize * 100;
                    }
                }
            }
        }

        public DownloadStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public double DownloadSpeed
        {
            get => _downloadSpeed;
            set => SetProperty(ref _downloadSpeed, value);
        }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public DownloadTask()
        {
            Id = Guid.NewGuid().ToString();
            Status = DownloadStatus.Waiting;
            CancellationTokenSource = new CancellationTokenSource();
        }

        protected bool SetProperty<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(nameof(T));
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 下载管理器类
    /// </summary>
    public class DownloadManager
    {
        private List<DownloadTask> _downloadTasks;
        private HttpClient _httpClient;
        private const int BufferSize = 8192;
        private const int MaxConcurrentDownloads = 5;
        private SemaphoreSlim _semaphore;

        public event EventHandler<DownloadTask> TaskAdded;
        public event EventHandler<DownloadTask> TaskUpdated;
        public event EventHandler<DownloadTask> TaskCompleted;

        public List<DownloadTask> DownloadTasks
        {
            get => _downloadTasks;
        }

        public DownloadManager()
        {
            _downloadTasks = new List<DownloadTask>();
            _semaphore = new SemaphoreSlim(MaxConcurrentDownloads, MaxConcurrentDownloads);

            _httpClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseDefaultCredentials = true
            })
            {
                Timeout = TimeSpan.FromMinutes(30)
            };
        }

        /// <summary>
        /// 添加下载任务
        /// </summary>
        public async Task<DownloadTask> AddDownloadTask(string url, string savePath)
        {
            var task = new DownloadTask
            {
                Url = url,
                SavePath = savePath,
                FileName = Path.GetFileName(url)
            };

            _downloadTasks.Add(task);
            TaskAdded?.Invoke(this, task);

            await StartDownloadAsync(task);
            return task;
        }

        /// <summary>
        /// 开始下载任务
        /// </summary>
        public async Task StartDownloadAsync(DownloadTask task)
        {
            if (task.Status == DownloadStatus.Downloading)
                return;

            task.Status = DownloadStatus.Downloading;
            TaskUpdated?.Invoke(this, task);

            await _semaphore.WaitAsync();

            try
            {
                await DownloadFileAsync(task);
            }
            catch (Exception ex)
            {
                task.Status = DownloadStatus.Failed;
                task.ErrorMessage = ex.Message;
                TaskUpdated?.Invoke(this, task);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 暂停下载任务
        /// </summary>
        public void PauseDownload(DownloadTask task)
        {
            if (task.Status == DownloadStatus.Downloading)
            {
                task.CancellationTokenSource.Cancel();
                task.Status = DownloadStatus.Paused;
                TaskUpdated?.Invoke(this, task);
            }
        }

        /// <summary>
        /// 取消下载任务
        /// </summary>
        public void CancelDownload(DownloadTask task)
        {
            if (task.Status == DownloadStatus.Downloading || task.Status == DownloadStatus.Paused)
            {
                task.CancellationTokenSource.Cancel();
                task.Status = DownloadStatus.Canceled;
                TaskUpdated?.Invoke(this, task);

                // 删除部分下载的文件
                if (File.Exists(Path.Combine(task.SavePath, task.FileName)))
                {
                    try
                    {
                        File.Delete(Path.Combine(task.SavePath, task.FileName));
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 移除下载任务
        /// </summary>
        public void RemoveDownloadTask(DownloadTask task)
        {
            if (task.Status == DownloadStatus.Downloading)
            {
                CancelDownload(task);
            }

            _downloadTasks.Remove(task);
        }

        /// <summary>
        /// 下载文件的核心方法
        /// </summary>
        private async Task DownloadFileAsync(DownloadTask task)
        {
            // 确保保存目录存在
            Directory.CreateDirectory(task.SavePath);

            var filePath = Path.Combine(task.SavePath, task.FileName);
            var tempFilePath = filePath + ".tmp";

            // 检查是否有部分下载的文件
            long resumePosition = 0;
            if (File.Exists(tempFilePath))
            {
                resumePosition = new FileInfo(tempFilePath).Length;
            }

            using (var request = new HttpRequestMessage(HttpMethod.Get, task.Url))
            {
                // 设置Range头以支持断点续传
                if (resumePosition > 0)
                {
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(resumePosition, null);
                }

                using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, task.CancellationTokenSource.Token))
                {
                    response.EnsureSuccessStatusCode();

                    // 获取文件总大小
                    if (response.Content.Headers.ContentLength.HasValue)
                    {
                        task.TotalSize = response.Content.Headers.ContentLength.Value + resumePosition;
                    }

                    // 打开文件流
                    using (var fileStream = new FileStream(tempFilePath, resumePosition > 0 ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            var buffer = new byte[BufferSize];
                            int bytesRead;
                            long totalRead = resumePosition;
                            var startTime = DateTime.Now;
                            var lastUpdateTime = startTime;
                            var lastReadBytes = 0L;

                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, task.CancellationTokenSource.Token)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead, task.CancellationTokenSource.Token);
                                totalRead += bytesRead;
                                task.DownloadedSize = totalRead;

                                // 计算下载速度
                                var currentTime = DateTime.Now;
                                var elapsedSinceLastUpdate = currentTime - lastUpdateTime;
                                if (elapsedSinceLastUpdate.TotalSeconds >= 1)
                                {
                                    var bytesSinceLastUpdate = totalRead - lastReadBytes;
                                    task.DownloadSpeed = bytesSinceLastUpdate / elapsedSinceLastUpdate.TotalSeconds / 1024;
                                    lastUpdateTime = currentTime;
                                    lastReadBytes = totalRead;
                                    TaskUpdated?.Invoke(this, task);
                                }
                            }

                            // 下载完成，重命名临时文件
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                            File.Move(tempFilePath, filePath);

                            task.Status = DownloadStatus.Completed;
                            TaskUpdated?.Invoke(this, task);
                            TaskCompleted?.Invoke(this, task);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取默认下载路径
        /// </summary>
        public string GetDefaultDownloadPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        }

        /// <summary>
        /// 清理已完成的任务
        /// </summary>
        public void CleanupCompletedTasks()
        {
            _downloadTasks.RemoveAll(task => 
                task.Status == DownloadStatus.Completed || 
                task.Status == DownloadStatus.Failed || 
                task.Status == DownloadStatus.Canceled);
        }
    }
}
