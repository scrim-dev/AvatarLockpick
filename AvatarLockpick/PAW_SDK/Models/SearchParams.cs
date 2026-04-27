namespace PAW_SDK.Models
{
    public class SearchParams
    {
        public required string Query { get; set; }

        public SearchType? Type { get; set; }

        public IReadOnlyList<Platform> Platforms { get; set; } = [];

        public SearchOrder Order { get; set; } = SearchOrder.Oldest;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}