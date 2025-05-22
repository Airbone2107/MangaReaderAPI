using Application.Contracts.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories
{
    public class TranslatedMangaRepository : GenericRepository<TranslatedManga>, ITranslatedMangaRepository
    {
        public TranslatedMangaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<TranslatedManga?> GetByMangaIdAndLanguageAsync(Guid mangaId, string languageKey)
        {
            return await _dbSet
                .FirstOrDefaultAsync(tm => tm.MangaId == mangaId && tm.LanguageKey == languageKey);
        }

        public async Task<IReadOnlyList<TranslatedManga>> GetByMangaIdAsync(Guid mangaId)
        {
            return await _dbSet
                .Where(tm => tm.MangaId == mangaId)
                .AsNoTracking()
                .ToListAsync();
        }
    }
} 