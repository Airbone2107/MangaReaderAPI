using Domain.Entities;

namespace Application.Contracts.Persistence
{
    public interface ICoverArtRepository : IGenericRepository<CoverArt>
    {
        /// <summary>
        /// Lấy danh sách CoverArt theo MangaId.
        /// </summary>
        Task<IReadOnlyList<CoverArt>> GetCoverArtsByMangaIdAsync(Guid mangaId);

        /// <summary>
        /// Lấy CoverArt theo PublicId (từ Cloudinary).
        /// </summary>
        Task<CoverArt?> GetCoverArtByPublicIdAsync(string publicId);
    }
} 