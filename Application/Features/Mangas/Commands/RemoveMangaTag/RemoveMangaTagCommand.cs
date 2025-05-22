using MediatR;

namespace Application.Features.Mangas.Commands.RemoveMangaTag
{
    public class RemoveMangaTagCommand : IRequest<Unit>
    {
        public Guid MangaId { get; set; }
        public Guid TagId { get; set; }
    }
} 