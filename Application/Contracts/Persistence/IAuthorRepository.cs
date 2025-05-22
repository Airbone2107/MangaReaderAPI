using Domain.Entities;

namespace Application.Contracts.Persistence
{
    public interface IAuthorRepository : IGenericRepository<Author>
    {
        /// <summary>
        /// Lấy tác giả theo tên.
        /// </summary>
        Task<Author?> GetAuthorByNameAsync(string name);
    }
} 