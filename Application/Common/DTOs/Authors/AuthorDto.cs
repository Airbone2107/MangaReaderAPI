namespace Application.Common.DTOs.Authors
{
    public class AuthorDto
    {
        public Guid AuthorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Biography { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 