using MediatR;

namespace Application.Features.TranslatedMangas.Commands.UpdateTranslatedManga
{
    public class UpdateTranslatedMangaCommand : IRequest<Unit>
    {
        public Guid TranslatedMangaId { get; set; } // Lấy từ route
        public string LanguageKey { get; set; } = string.Empty; // Cân nhắc có cho đổi không
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
} 