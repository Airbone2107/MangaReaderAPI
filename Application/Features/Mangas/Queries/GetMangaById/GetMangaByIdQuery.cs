using Application.Common.DTOs.Mangas;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Mangas.Queries.GetMangaById
{
    public class GetMangaByIdQuery : IRequest<ResourceObject<MangaAttributesDto>?>
    {
        public Guid MangaId { get; set; }
    }
} 