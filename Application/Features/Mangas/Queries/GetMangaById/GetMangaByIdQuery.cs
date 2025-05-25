using Application.Common.DTOs.Mangas;
using MediatR;

namespace Application.Features.Mangas.Queries.GetMangaById
{
    public class GetMangaByIdQuery : IRequest<MangaDto?>
    {
        public Guid MangaId { get; set; }
    }
} 