using Application.Contracts.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;

namespace Persistence.Repositories
{
    public class TagGroupRepository : GenericRepository<TagGroup>, ITagGroupRepository
    {
        public TagGroupRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<TagGroup?> GetTagGroupByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(tg => tg.Name == name);
        }

        public async Task<TagGroup?> GetTagGroupWithTagsAsync(Guid tagGroupId)
        {
            return await _dbSet
                .Include(tg => tg.Tags)
                .AsNoTracking()
                .FirstOrDefaultAsync(tg => tg.TagGroupId == tagGroupId);
        }
    }
} 