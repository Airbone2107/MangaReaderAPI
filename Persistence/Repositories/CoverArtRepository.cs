using Application.Contracts.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories
{
    public class CoverArtRepository : GenericRepository<CoverArt>, ICoverArtRepository
    {
        public CoverArtRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<CoverArt>> GetCoverArtsByMangaIdAsync(Guid mangaId)
        {
            return await _dbSet
                .Where(ca => ca.MangaId == mangaId)
                .OrderBy(ca => ca.Volume) // Hoặc theo CreatedAt/UpdatedAt nếu cần
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CoverArt?> GetCoverArtByPublicIdAsync(string publicId)
        {
            return await _dbSet.FirstOrDefaultAsync(ca => ca.PublicId == publicId);
        }
    }
} 