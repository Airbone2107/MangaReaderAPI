using Domain.Entities;

namespace Application.Contracts.Persistence
{
    public interface IMangaRepository : IGenericRepository<Manga>
    {
        /// <summary>
        /// Lấy thông tin chi tiết của Manga bao gồm Tags, Authors, CoverArts, TranslatedMangas.
        /// </summary>
        Task<Manga?> GetMangaWithDetailsAsync(Guid mangaId);
    }
} 