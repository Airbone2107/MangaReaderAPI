using MediatR;

namespace Application.Features.TranslatedMangas.Commands.CreateTranslatedManga
{
    public class CreateTranslatedMangaCommand : IRequest<Guid>
    {
        public Guid MangaId { get; set; }
        public string LanguageKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
} 