namespace Application.Common.DTOs.Tags
{
    public class TagDto
    {
        public Guid TagId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid TagGroupId { get; set; }
        public string TagGroupName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 