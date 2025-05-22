namespace Application.Common.DTOs.TagGroups
{
    public class TagGroupDto
    {
        public Guid TagGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 