using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
using Application.Common.Models;
using Domain.Enums;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Mangas.Queries.GetMangas
{
    public class GetMangasQuery : IRequest<PagedResult<ResourceObject<MangaAttributesDto>>>
    {
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20;
        public string? TitleFilter { get; set; }
        public MangaStatus? StatusFilter { get; set; }
        public ContentRating? ContentRatingFilter { get; set; }
        public List<PublicationDemographic>? PublicationDemographicsFilter { get; set; }
        public string? OriginalLanguageFilter { get; set; }
        public int? YearFilter { get; set; }
        public List<Guid>? AuthorIdsFilter { get; set; } // Lọc manga chứa BẤT KỲ author nào trong danh sách này
        
        // TODO: [Improvement] Thêm bộ lọc cho TranslatedManga.LanguageKey? (Ví dụ: lấy manga có bản dịch tiếng Việt)

        // --- Các thuộc tính mới cho lọc tag nâng cao ---
        /// <summary>
        /// Danh sách các ID của tag mà manga PHẢI BAO GỒM.
        /// </summary>
        public List<Guid>? IncludedTags { get; set; }

        /// <summary>
        /// Chế độ lọc cho IncludedTags. Có thể là "AND" hoặc "OR".
        /// "AND": Manga phải chứa TẤT CẢ các tag trong IncludedTags.
        /// "OR": Manga phải chứa ÍT NHẤT MỘT tag trong IncludedTags.
        /// Mặc định là "AND".
        /// </summary>
        public string? IncludedTagsMode { get; set; } // Mặc định "AND" trong Handler

        /// <summary>
        /// Danh sách các ID của tag mà manga KHÔNG ĐƯỢC BAO GỒM.
        /// </summary>
        public List<Guid>? ExcludedTags { get; set; }

        /// <summary>
        /// Chế độ lọc cho ExcludedTags. Có thể là "AND" hoặc "OR".
        /// "AND": Manga không được chứa TẤT CẢ các tag trong ExcludedTags.
        /// "OR": Manga không được chứa BẤT KỲ tag nào trong ExcludedTags.
        /// Mặc định là "OR".
        /// </summary>
        public string? ExcludedTagsMode { get; set; } // Mặc định "OR" trong Handler
        // --- Kết thúc các thuộc tính mới ---

        public string OrderBy { get; set; } = "UpdatedAt"; // title, year, createdAt, updatedAt
        public bool Ascending { get; set; } = false; // Mặc định giảm dần cho UpdatedAt

        // Thêm tham số Includes
        public List<string>? Includes { get; set; } // Ví dụ: ["cover_art", "author", "artist"]
    }
} 