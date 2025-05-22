namespace Application.Common.DTOs.TranslatedMangas
{
    public class TranslatedMangaDto
    {
        public Guid TranslatedMangaId { get; set; }
        public Guid MangaId { get; set; }
        public string LanguageKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 