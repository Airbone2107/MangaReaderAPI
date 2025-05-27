using Application.Common.DTOs.TranslatedMangas;
using Application.Common.Models;
using MediatR;

namespace Application.Features.TranslatedMangas.Queries.GetTranslatedMangaById
{
    public class GetTranslatedMangaByIdQuery : IRequest<ResourceObject<TranslatedMangaAttributesDto>?>
    {
        public Guid TranslatedMangaId { get; set; }
        // TODO: [Improvement] Thêm tùy chọn IncludeManga để có thể lấy Manga gốc không? (ít phổ biến)
        // TODO: [Improvement] Thêm tùy chọn IncludeChapters để có thể lấy danh sách Chapters không?
    }
} 