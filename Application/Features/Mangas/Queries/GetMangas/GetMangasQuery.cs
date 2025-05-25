using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
using Domain.Enums;
using MediatR;
using System.Collections.Generic; // Cần cho List

namespace Application.Features.Mangas.Queries.GetMangas
{
    public class GetMangasQuery : IRequest<PagedResult<MangaDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? TitleFilter { get; set; }
        public MangaStatus? StatusFilter { get; set; }
        public ContentRating? ContentRatingFilter { get; set; }
        public PublicationDemographic? DemographicFilter { get; set; }
        public string? OriginalLanguageFilter { get; set; }
        public int? YearFilter { get; set; }
        public List<Guid>? TagIdsFilter { get; set; } // Lọc manga chứa BẤT KỲ tag nào trong danh sách này
        public List<Guid>? AuthorIdsFilter { get; set; } // Lọc manga chứa BẤT KỲ author nào trong danh sách này
        
        // TODO: [Improvement] Thêm bộ lọc cho TranslatedManga.LanguageKey? (Ví dụ: lấy manga có bản dịch tiếng Việt)

        public string OrderBy { get; set; } = "UpdatedAt"; // title, year, createdAt, updatedAt
        public bool Ascending { get; set; } = false; // Mặc định giảm dần cho UpdatedAt
    }
} 