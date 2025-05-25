using Application.Common.DTOs;
using Application.Common.DTOs.CoverArts;
using MediatR;

namespace Application.Features.CoverArts.Queries.GetCoverArtsByManga
{
    public class GetCoverArtsByMangaQuery : IRequest<PagedResult<CoverArtDto>>
    {
        public Guid MangaId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        // TODO: [Improvement] Thêm bộ lọc theo Volume
        // TODO: [Improvement] Thêm sắp xếp (ví dụ: Volume, CreatedAt)
    }
} 