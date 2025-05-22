using Domain.Enums;
using MediatR;

namespace Application.Features.Mangas.Commands.AddMangaAuthor
{
    public class AddMangaAuthorCommand : IRequest<Unit>
    {
        public Guid MangaId { get; set; }
        public Guid AuthorId { get; set; }
        public MangaStaffRole Role { get; set; }
    }
} 