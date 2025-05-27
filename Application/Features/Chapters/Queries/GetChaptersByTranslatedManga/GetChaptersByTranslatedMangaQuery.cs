using Application.Common.DTOs;
using Application.Common.DTOs.Chapters;
using MediatR;
using System.Collections.Generic; // Cần cho List

namespace Application.Features.Chapters.Queries.GetChaptersByTranslatedManga
{
    public class GetChaptersByTranslatedMangaQuery : IRequest<PagedResult<ChapterDto>>
    {
        public Guid TranslatedMangaId { get; set; }
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 20; // Thường hiển thị nhiều chapter hơn
        // Thêm OrderBy nếu cần (ví dụ: PublishAt, ChapterNumber)
        public string OrderBy { get; set; } = "ChapterNumber"; // volume, chapterNumber, publishAt
        public bool Ascending { get; set; } = true;
        // TODO: [Improvement] Thêm bộ lọc theo Volume, ChapterNumber
    }
} 