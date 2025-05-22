using Domain.Entities;

namespace Application.Contracts.Persistence
{
    public interface ITagGroupRepository : IGenericRepository<TagGroup>
    {
        /// <summary>
        /// Lấy TagGroup theo tên.
        /// </summary>
        Task<TagGroup?> GetTagGroupByNameAsync(string name);

        /// <summary>
        /// Lấy TagGroup bao gồm cả danh sách Tags của nó.
        /// </summary>
        Task<TagGroup?> GetTagGroupWithTagsAsync(Guid tagGroupId);
    }
} 