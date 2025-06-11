using System.Text.Json.Serialization;

namespace Application.Common.Models
{
    public class RelationshipObject
    {
        [JsonPropertyOrder(1)]
        public string Id { get; set; } = string.Empty; // ID của thực thể liên quan

        [JsonPropertyOrder(2)]
        public string Type { get; set; } = string.Empty; // Loại của mối quan hệ hoặc vai trò, ví dụ "author", "artist", "cover_art"

        [JsonPropertyOrder(3)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Attributes { get; set; } // Thuộc tính chi tiết của thực thể liên quan (nếu được include)
    }
} 