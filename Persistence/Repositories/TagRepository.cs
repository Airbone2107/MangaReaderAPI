using Application.Contracts.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories
{
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        public TagRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Tag?> GetTagByNameAndGroupAsync(string name, Guid tagGroupId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(t => t.Name == name && t.TagGroupId == tagGroupId);
        }

        public async Task<IReadOnlyList<Tag>> GetTagsByGroupIdAsync(Guid tagGroupId)
        {
            return await _dbSet
                .Where(t => t.TagGroupId == tagGroupId)
                .OrderBy(t => t.Name)
                .AsNoTracking()
                .ToListAsync();
        }
    }
} 