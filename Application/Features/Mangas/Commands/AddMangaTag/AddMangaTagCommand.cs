using MediatR;

namespace Application.Features.Mangas.Commands.AddMangaTag
{
    public class AddMangaTagCommand : IRequest<Unit>
    {
        public Guid MangaId { get; set; }
        public Guid TagId { get; set; }
    }
} 