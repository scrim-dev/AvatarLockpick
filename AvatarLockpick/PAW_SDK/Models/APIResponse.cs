using System.Text.Json.Serialization;

namespace PAW_SDK.Models;

public class ApiResult<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("result")]
    public T? Result { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class ApiResults<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("results")]
    public List<T> Results { get; set; } = [];

    [JsonPropertyName("pagination")]
    public Pagination? Pagination { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class UpdateResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("avatar")]
    public Avatar? Avatar { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}