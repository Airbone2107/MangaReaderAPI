using Application.Common.DTOs;
using Application.Common.DTOs.TranslatedMangas;
using Application.Common.Models;
using MediatR;
using System; // Cần cho Guid

namespace Application.Features.TranslatedMangas.Queries.GetTranslatedMangasByManga
{
    public class GetTranslatedMangasByMangaQuery : IRequest<PagedResult<ResourceObject<TranslatedMangaAttributesDto>>>
    {
        public Guid MangaId { get; set; }
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20;
        // Thêm OrderBy nếu cần (ví dụ: LanguageKey, Title)
        public string OrderBy { get; set; } = "LanguageKey"; 
        public bool Ascending { get; set; } = true;
        // TODO: [Improvement] Thêm bộ lọc theo LanguageKey
    }
} 