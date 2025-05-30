using System.Text.Json.Serialization;

namespace Application.Common.Models
{
    public class ResourceObject<TAttributes> where TAttributes : class
    {
        [JsonPropertyOrder(1)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyOrder(2)]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyOrder(3)]
        public TAttributes Attributes { get; set; } = null!;

        [JsonPropertyOrder(4)]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Không hiển thị nếu không có relationships
        public List<RelationshipObject>? Relationships { get; set; }
    }
} 