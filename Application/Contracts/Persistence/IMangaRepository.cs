using Domain.Entities;

namespace Application.Contracts.Persistence
{
    public interface IMangaRepository : IGenericRepository<Manga>
    {
        /// <summary>
        /// Lấy thông tin chi tiết của Manga bao gồm Tags, Authors, CoverArts, TranslatedMangas.
        /// </summary>
        Task<Manga?> GetMangaWithDetailsAsync(Guid mangaId);

        /// <summary>
        /// Lấy thông tin chi tiết của Manga bao gồm Tags, Authors, CoverArts, TranslatedMangas.
        /// Dùng cho mục đích cập nhật, không sử dụng AsNoTracking.
        /// </summary>
        Task<Manga?> GetMangaWithDetailsForUpdateAsync(Guid mangaId);
    }
} 