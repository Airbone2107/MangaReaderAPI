namespace Application.Common.DTOs.Tags
{
    public class UpdateTagDto
    {
        // TagId sẽ lấy từ route
        public string Name { get; set; } = string.Empty;
        public Guid TagGroupId { get; set; }
    }
} 