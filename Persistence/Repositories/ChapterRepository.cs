using Application.Contracts.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories
{
    public class ChapterRepository : GenericRepository<Chapter>, IChapterRepository
    {
        public ChapterRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Chapter>> GetChaptersByTranslatedMangaAsync(Guid translatedMangaId)
        {
            return await _dbSet
                .Where(c => c.TranslatedMangaId == translatedMangaId)
                .OrderBy(c => c.Volume)
                .ThenBy(c => c.ChapterNumber)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Chapter?> GetChapterWithPagesAsync(Guid chapterId)
        {
            return await _dbSet
                .Include(c => c.ChapterPages.OrderBy(cp => cp.PageNumber))
                .FirstOrDefaultAsync(c => c.ChapterId == chapterId);
        }
        
        public async Task<ChapterPage?> GetPageByIdAsync(Guid pageId)
        {
            return await _context.ChapterPages.FindAsync(pageId);
        }

        public async Task AddPageAsync(ChapterPage chapterPage)
        {
            await _context.ChapterPages.AddAsync(chapterPage);
            // SaveChangesAsync sẽ được gọi bởi UnitOfWork
        }

        public Task UpdatePageAsync(ChapterPage chapterPage)
        {
            _context.Entry(chapterPage).State = EntityState.Modified;
            // SaveChangesAsync sẽ được gọi bởi UnitOfWork
            return Task.CompletedTask;
        }

        public async Task DeletePageAsync(Guid pageId)
        {
            var page = await _context.ChapterPages.FindAsync(pageId);
            if (page != null)
            {
                _context.ChapterPages.Remove(page);
                // SaveChangesAsync sẽ được gọi bởi UnitOfWork
            }
        }

        public async Task<int> GetMaxPageNumberAsync(Guid chapterId)
        {
            return await _context.ChapterPages
                .Where(cp => cp.ChapterId == chapterId)
                .Select(cp => (int?)cp.PageNumber) // Cast to nullable int to handle empty sequence
                .MaxAsync() ?? 0; // Return 0 if no pages exist
        }
    }
} 