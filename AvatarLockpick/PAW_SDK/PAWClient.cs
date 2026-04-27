using System.Runtime.Serialization;
using System.Text.Json;
using System.Web;
using PAW_SDK.Exceptions;
using PAW_SDK.Models;

namespace PAW_SDK
{
    public sealed class PAWClient : IDisposable
    {
        private readonly HttpClient _http;

        private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        private bool _disposed;

        private string baseUrl = "https://paw-api.amelia.fun";

        public PAWClient(string? userAgent = null, TimeSpan? timeout = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);

            _http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = timeout ?? TimeSpan.FromSeconds(30)
            };

            _http.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }

        public async Task<Avatar?> GetAvatarAsync(string avatarId, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(avatarId);

            var url = $"avatar?avatarId={Uri.EscapeDataString(avatarId)}";

            var response = await GetAsync<ApiResult<Avatar>>(url, ct);

            return response.Result;
        }

        public async Task<IReadOnlyList<Avatar>> GetRandomAvatarsAsync(CancellationToken ct = default)
        {
            var response = await GetAsync<ApiResults<Avatar>>("random", ct);

            return response.Results;
        }

        public async Task<IReadOnlyList<Avatar>> GetRecentAvatarsAsync(RecentType type = RecentType.Added, CancellationToken ct = default)
        {
            var segment = type switch
            {
                RecentType.Added => "added",
                RecentType.Updated => "updated",
                RecentType.Checked => "checked",
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            var response = await GetAsync<ApiResults<Avatar>>($"recent/{segment}", ct);

            return response.Results;
        }

        public async Task<(IReadOnlyList<Avatar> Avatars, Pagination? Pagination)> SearchAsync(SearchParams parameters, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(parameters);
            ArgumentException.ThrowIfNullOrWhiteSpace(parameters.Query);

            var qs = HttpUtility.ParseQueryString(string.Empty);

            qs["query"] = parameters.Query;
            qs["page"] = parameters.Page.ToString();
            qs["size"] = parameters.PageSize.ToString();
            qs["order"] = GetEnumValue(parameters.Order);

            if (parameters.Type.HasValue) qs["type"] = GetEnumValue(parameters.Type.Value);

            if (parameters.Platforms.Count > 0) qs["platforms"] = string.Join(',', parameters.Platforms.Select(GetEnumValue));

            var response = await GetAsync<ApiResults<Avatar>>($"search?{qs}", ct);

            return (response.Results, response.Pagination);
        }

        public async Task<(IReadOnlyList<Avatar> Avatars, Pagination? Pagination)> GetSimilarAsync(string avatarId, int page = 1, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(avatarId);

            var url = $"similar?avatar_id={Uri.EscapeDataString(avatarId)}&page={page}";

            var response = await GetAsync<ApiResults<Avatar>>(url, ct);

            return (response.Results, response.Pagination);
        }

        public async Task<UpdateResult> RequestUpdateAsync(string avatarId, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(avatarId);

            var url = $"update?avatarId={Uri.EscapeDataString(avatarId)}";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            using var httpResponse = await _http.SendAsync(request, ct);

            return await DeserializeAsync<UpdateResult>(httpResponse, ct);
        }

        public Uri BuildProxyUrl(string imageUrl)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);

            var encoded = Uri.EscapeDataString(imageUrl);

            return new Uri(_http.BaseAddress!, $"proxy?url={encoded}");
        }


        public async Task<(byte[] Data, string ContentType)> FetchProxiedImageAsync(string imageUrl, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(imageUrl);

            var encoded = Uri.EscapeDataString(imageUrl);

            using var response = await _http.GetAsync($"proxy?url={encoded}", ct);

            if (!response.IsSuccessStatusCode) throw new APIException((int)response.StatusCode, $"Proxy request failed: {(int)response.StatusCode} {response.ReasonPhrase}");

            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            return (bytes, contentType);
        }

        private async Task<T> GetAsync<T>(string relativeUrl, CancellationToken ct)
        {
            using var response = await _http.GetAsync(relativeUrl, ct);

            return await DeserializeAsync<T>(response, ct);
        }

        private async Task<T> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken ct)
        {
            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                string? message = null;

                try
                {
                    using var doc = JsonDocument.Parse(body);

                    if (doc.RootElement.TryGetProperty("message", out var m) || doc.RootElement.TryGetProperty("result", out m))
                    {
                        message = m.GetString();
                    }
                }
                catch {}

                throw new APIException((int)response.StatusCode, message ?? $"API error {(int)response.StatusCode}: {response.ReasonPhrase}");
            }

            return JsonSerializer.Deserialize<T>(body, _json) ?? throw new APIException((int)response.StatusCode, "API returned an empty body.");
        }

        private static string GetEnumValue<TEnum>(TEnum value) where TEnum : Enum
        {
            var name = value.ToString();
            var member = typeof(TEnum).GetField(name);
            var attr = member?.GetCustomAttributes(typeof(EnumMemberAttribute), false).OfType<EnumMemberAttribute>().FirstOrDefault();

            return attr?.Value ?? name.ToLowerInvariant();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _http.Dispose();

            _disposed = true;
        }
    }
}