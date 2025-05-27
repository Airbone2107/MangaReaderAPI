using Application.Common.DTOs;
using Application.Common.DTOs.CoverArts;
using MediatR;

namespace Application.Features.CoverArts.Queries.GetCoverArtsByManga
{
    public class GetCoverArtsByMangaQuery : IRequest<PagedResult<CoverArtDto>>
    {
        public Guid MangaId { get; set; }
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20;
        // TODO: [Improvement] Thêm bộ lọc theo Volume
        // TODO: [Improvement] Thêm sắp xếp (ví dụ: Volume, CreatedAt)
    }
} 