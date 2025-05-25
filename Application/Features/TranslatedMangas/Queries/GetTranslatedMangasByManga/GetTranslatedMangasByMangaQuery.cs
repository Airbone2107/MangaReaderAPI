using Application.Common.DTOs;
using Application.Common.DTOs.TranslatedMangas;
using MediatR;
using System; // Cần cho Guid

namespace Application.Features.TranslatedMangas.Queries.GetTranslatedMangasByManga
{
    public class GetTranslatedMangasByMangaQuery : IRequest<PagedResult<TranslatedMangaDto>>
    {
        public Guid MangaId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        // Thêm OrderBy nếu cần (ví dụ: LanguageKey, Title)
        public string OrderBy { get; set; } = "LanguageKey"; 
        public bool Ascending { get; set; } = true;
        // TODO: [Improvement] Thêm bộ lọc theo LanguageKey
    }
} 