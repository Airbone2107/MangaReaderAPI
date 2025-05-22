using MediatR;

namespace Application.Features.Mangas.Commands.DeleteManga
{
    public class DeleteMangaCommand : IRequest<Unit>
    {
        public Guid MangaId { get; set; }
    }
} 