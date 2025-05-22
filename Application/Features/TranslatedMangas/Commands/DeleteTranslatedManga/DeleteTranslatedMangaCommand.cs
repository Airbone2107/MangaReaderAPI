using MediatR;

namespace Application.Features.TranslatedMangas.Commands.DeleteTranslatedManga
{
    public class DeleteTranslatedMangaCommand : IRequest<Unit>
    {
        public Guid TranslatedMangaId { get; set; }
    }
} 