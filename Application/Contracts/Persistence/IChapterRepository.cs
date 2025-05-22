using Domain.Entities;

namespace Application.Contracts.Persistence
{
    public interface IChapterRepository : IGenericRepository<Chapter>
    {
        /// <summary>
        /// Lấy danh sách các Chapter theo TranslatedMangaId, sắp xếp theo các tiêu chí (ví dụ: Volume, ChapterNumber).
        /// </summary>
        Task<IReadOnlyList<Chapter>> GetChaptersByTranslatedMangaAsync(Guid translatedMangaId);

        /// <summary>
        /// Lấy thông tin Chapter bao gồm cả các ChapterPages của nó.
        /// </summary>
        Task<Chapter?> GetChapterWithPagesAsync(Guid chapterId);
        
        /// <summary>
        /// Lấy một ChapterPage bằng PageId.
        /// </summary>
        Task<ChapterPage?> GetPageByIdAsync(Guid pageId);
        
        /// <summary>
        /// Thêm một ChapterPage mới.
        /// </summary>
        Task AddPageAsync(ChapterPage chapterPage);

        /// <summary>
        /// Cập nhật một ChapterPage.
        /// </summary>
        Task UpdatePageAsync(ChapterPage chapterPage);
        
        /// <summary>
        /// Xóa một ChapterPage bằng PageId.
        /// </summary>
        Task DeletePageAsync(Guid pageId);
        
        /// <summary>
        /// Lấy số trang lớn nhất của một chapter.
        /// </summary>
        Task<int> GetMaxPageNumberAsync(Guid chapterId);
    }
} 