using Application.Common.DTOs.Mangas;
using Application.Common.Models;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Mangas.Queries.GetMangaById
{
    public class GetMangaByIdQuery : IRequest<ResourceObject<MangaAttributesDto>?>
    {
        public Guid MangaId { get; set; }
        
        public List<string>? Includes { get; set; }
    }
} 