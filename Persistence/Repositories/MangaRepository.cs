using Application.Contracts.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories
{
    public class MangaRepository : GenericRepository<Manga>, IMangaRepository
    {
        public MangaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Manga?> GetMangaWithDetailsAsync(Guid mangaId)
        {
            return await _dbSet
                .Include(m => m.MangaTags)
                    .ThenInclude(mt => mt.Tag)
                        .ThenInclude(t => t.TagGroup) // Bao gồm cả TagGroup của Tag
                .Include(m => m.MangaAuthors)
                    .ThenInclude(ma => ma.Author)
                .Include(m => m.CoverArts)
                .Include(m => m.TranslatedMangas)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MangaId == mangaId);
        }

        public async Task<Manga?> GetMangaWithDetailsForUpdateAsync(Guid mangaId)
        {
            // Truy vấn tương tự GetMangaWithDetailsAsync nhưng BỎ AsNoTracking()
            // để Entity Framework theo dõi các thay đổi.
            return await _dbSet
                .Include(m => m.MangaTags)
                    .ThenInclude(mt => mt.Tag)
                        .ThenInclude(t => t.TagGroup)
                .Include(m => m.MangaAuthors)
                    .ThenInclude(ma => ma.Author)
                .Include(m => m.CoverArts)
                .Include(m => m.TranslatedMangas)
                // KHÔNG sử dụng .AsNoTracking() ở đây
                .FirstOrDefaultAsync(m => m.MangaId == mangaId);
        }
    }
} 