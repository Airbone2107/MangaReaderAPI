namespace Application.Common.DTOs.TranslatedMangas
{
    public class CreateTranslatedMangaDto
    {
        public Guid MangaId { get; set; }
        public string LanguageKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
} 