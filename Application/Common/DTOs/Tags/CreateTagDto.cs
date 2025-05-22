namespace Application.Common.DTOs.Tags
{
    public class CreateTagDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid TagGroupId { get; set; }
    }
} 