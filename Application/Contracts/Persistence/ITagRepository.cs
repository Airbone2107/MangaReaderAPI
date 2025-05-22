using Domain.Entities;

namespace Application.Contracts.Persistence
{
    public interface ITagRepository : IGenericRepository<Tag>
    {
        /// <summary>
        /// Lấy Tag theo tên và TagGroupId.
        /// </summary>
        Task<Tag?> GetTagByNameAndGroupAsync(string name, Guid tagGroupId);

        /// <summary>
        /// Lấy danh sách Tag theo TagGroupId.
        /// </summary>
        Task<IReadOnlyList<Tag>> GetTagsByGroupIdAsync(Guid tagGroupId);
    }
} 