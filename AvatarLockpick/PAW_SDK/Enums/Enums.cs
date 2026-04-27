using System.Runtime.Serialization;

public enum SearchType
{
    [EnumMember(Value = "name")]
    Name,

    [EnumMember(Value = "description")]
    Description,

    [EnumMember(Value = "author")]
    Author,

    [EnumMember(Value = "author_name")]
    AuthorName,

    [EnumMember(Value = "image/thumbnail")]
    ImageThumbnail,

    [EnumMember(Value = "ai_tags")]
    AiTags
}

public enum Platform
{
    [EnumMember(Value = "pc")]
    PC,

    [EnumMember(Value = "android")]
    Android,

    [EnumMember(Value = "ios")]
    iOS
}

public enum SearchOrder
{
    [EnumMember(Value = "newest")]
    Newest,

    [EnumMember(Value = "oldest")]
    Oldest
}

public enum RecentType
{
    Added,
    Updated,
    Checked
}