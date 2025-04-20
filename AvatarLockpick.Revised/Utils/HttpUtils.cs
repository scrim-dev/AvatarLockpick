using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvatarLockpick.Revised.Utils
{
    internal class URLStuff
    {
        public static void OpenUrl(string url)
        {
            try
            {
                if (!url.StartsWith("http"))
                    url = "https://" + url;

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                // Fallback methods
                if (OperatingSystem.IsWindows())
                    Process.Start("explorer.exe", url);
                else if (OperatingSystem.IsLinux())
                    Process.Start("xdg-open", url);
                else if (OperatingSystem.IsMacOS())
                    Process.Start("open", url);
            }
        }
    }

    internal class HttpUtils : IDisposable
    {
        private HttpClient? _httpClient;
        private bool _disposed;

        public void Load(string? userAgent = null)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                userAgent ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
            );
        }

        public async Task<string> DownloadStringAsync(string url)
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Call Load() first!");

            return await _httpClient.GetStringAsync(url).ConfigureAwait(false);
        }

        public string DownloadString(string url)
        {
            if (_httpClient == null)
                throw new InvalidOperationException("Call Load() first!");

            return _httpClient.GetStringAsync(url).GetAwaiter().GetResult();
        }

        public List<string> DownloadStringList(string url)
        {
            string content = DownloadString(url);
            return [.. content.Split(
                ["\r\n", "\r", "\n"],
                StringSplitOptions.RemoveEmptyEntries
            )];
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
