using PAW_SDK;
using PAW_SDK.Models;
using PAW_SDK.Exceptions;
using System.Net.Http;
using System.Threading.Tasks;

namespace AvatarLockpick.Utils
{
    internal static class PAWUtils
    {
        private static PAWClient _client;
        private static readonly HttpClient _healthClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(8),
            BaseAddress = new Uri("https://paw-api.amelia.fun")
        };

        public static bool IsApiOnline { get; private set; } = false;

        public static void Init()
        {
            if (_client == null)
            {
                _client = new PAWClient($"AvatarLockpick/{Program.AppVersion}");
                AppLog.Log("PAW", "PAW API client initialized.");
            }
        }

        /// <summary>
        /// Lightweight health check — sends a HEAD request to the API root.
        /// Does NOT download any avatar data.
        /// </summary>
        public static async Task CheckApiStatus()
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, "/");
                using var response = await _healthClient.SendAsync(request);
                // Any HTTP response (even 404) means the server is reachable
                IsApiOnline = true;
                AppLog.Success("PAW", $"API health check passed (HTTP {(int)response.StatusCode}).");
            }
            catch (Exception ex)
            {
                IsApiOnline = false;
                AppLog.Warn("PAW", $"API health check failed: {ex.Message}");
            }
        }

        public static async Task<Avatar?> GetAvatarAsync(string avatarId)
        {
            AppLog.Log("PAW", $"Fetching avatar info for: {avatarId}");
            try
            {
                var avatar = await _client.GetAvatarAsync(avatarId);
                if (avatar != null)
                    AppLog.Success("PAW", $"Avatar found: '{avatar.Name}' by {avatar.AuthorName}");
                else
                    AppLog.Warn("PAW", $"Avatar '{avatarId}' was not found in the PAW database.");
                return avatar;
            }
            catch (APIException apiEx)
            {
                AppLog.Error("PAW", $"API error fetching avatar '{avatarId}': {apiEx.Message}");
                return null;
            }
            catch (Exception ex)
            {
                AppLog.Error("PAW", $"Unexpected error fetching avatar '{avatarId}': {ex.Message}");
                return null;
            }
        }
    }
}
