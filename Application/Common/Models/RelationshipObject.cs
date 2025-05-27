using System.Text.Json.Serialization;

namespace Application.Common.Models
{
    public class RelationshipObject
    {
        [JsonPropertyOrder(1)]
        public string Id { get; set; } = string.Empty; // ID của thực thể liên quan

        [JsonPropertyOrder(2)]
        public string Type { get; set; } = string.Empty; // Loại của mối quan hệ hoặc vai trò, ví dụ "author", "artist", "cover_art"
    }
} 