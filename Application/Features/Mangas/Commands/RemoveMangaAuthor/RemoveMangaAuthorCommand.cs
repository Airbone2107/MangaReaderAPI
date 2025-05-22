using Domain.Enums;
using MediatR;

namespace Application.Features.Mangas.Commands.RemoveMangaAuthor
{
    public class RemoveMangaAuthorCommand : IRequest<Unit>
    {
        public Guid MangaId { get; set; }
        public Guid AuthorId { get; set; }
        public MangaStaffRole Role { get; set; } // Cần Role để xác định chính xác record cần xóa
    }
} 