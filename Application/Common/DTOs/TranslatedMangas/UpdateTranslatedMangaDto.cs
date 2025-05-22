namespace Application.Common.DTOs.TranslatedMangas
{
    public class UpdateTranslatedMangaDto
    {
        // TranslatedMangaId sẽ lấy từ route
        public string LanguageKey { get; set; } = string.Empty; // Cân nhắc có cho phép đổi không, hoặc chỉ đổi Title/Description
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
} 