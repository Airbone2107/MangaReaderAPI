namespace Application.Common.DTOs.CoverArts
{
    public class CoverArtDto
    {
        public Guid CoverId { get; set; }
        public Guid MangaId { get; set; }
        public string? Volume { get; set; }
        public string PublicId { get; set; } = string.Empty; // URL sẽ được build ở client
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 