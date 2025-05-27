using Application.Common.DTOs;
using Domain.Common;
using System.Linq.Expressions;

namespace Application.Contracts.Persistence
{
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Lấy một entity bằng Id.
        /// </summary>
        /// <param name="id">Id của entity.</param>
        /// <returns>Entity nếu tìm thấy, ngược lại null.</returns>
        Task<T?> GetByIdAsync(Guid id);

        /// <summary>
        /// Lấy tất cả các entities.
        /// </summary>
        /// <returns>Danh sách chỉ đọc các entities.</returns>
        Task<IReadOnlyList<T>> GetAllAsync();

        /// <summary>
        /// Lấy danh sách các entities có phân trang sử dụng offset và limit.
        /// </summary>
        /// <param name="offset">Số lượng bản ghi bỏ qua.</param>
        /// <param name="limit">Số lượng bản ghi tối đa lấy về.</param>
        /// <param name="filter">Biểu thức lọc (tùy chọn).</param>
        /// <param name="orderBy">Hàm sắp xếp (tùy chọn).</param>
        /// <param name="includeProperties">Các navigation properties cần include, cách nhau bởi dấu phẩy (tùy chọn).</param>
        /// <returns>Kết quả phân trang của entities.</returns>
        Task<PagedResult<T>> GetPagedAsync(int offset, int limit, Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string? includeProperties = null);

        /// <summary>
        /// Thêm một entity mới. Không gọi SaveChanges.
        /// </summary>
        /// <param name="entity">Entity cần thêm.</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Cập nhật một entity hiện có. Không gọi SaveChanges.
        /// </summary>
        /// <param name="entity">Entity cần cập nhật.</param>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Xóa một entity. Không gọi SaveChanges.
        /// </summary>
        /// <param name="entity">Entity cần xóa.</param>
        Task DeleteAsync(T entity);
        
        /// <summary>
        /// Xóa một entity bằng Id. Không gọi SaveChanges.
        /// </summary>
        /// <param name="id">Id của entity cần xóa.</param>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Kiểm tra sự tồn tại của entity bằng Id.
        /// </summary>
        /// <param name="id">Id của entity.</param>
        /// <returns>True nếu tồn tại, ngược lại false.</returns>
        Task<bool> ExistsAsync(Guid id);

        /// <summary>
        /// Tìm entity đầu tiên thỏa mãn điều kiện.
        /// </summary>
        /// <param name="predicate">Điều kiện tìm kiếm.</param>
        /// <param name="includeProperties">Các navigation properties cần include (tùy chọn).</param>
        /// <returns>Entity nếu tìm thấy, ngược lại null.</returns>
        Task<T?> FindFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null);

        /// <summary>
        /// Tìm tất cả các entities thỏa mãn điều kiện.
        /// </summary>
        /// <param name="predicate">Điều kiện tìm kiếm (tùy chọn).</param>
        /// <param name="orderBy">Hàm sắp xếp (tùy chọn).</param>
        /// <param name="includeProperties">Các navigation properties cần include (tùy chọn).</param>
        /// <param name="disableTracking">True để vô hiệu hóa tracking (mặc định true cho query).</param>
        /// <returns>Danh sách chỉ đọc các entities.</returns>
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>>? predicate = null,
                                         Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                         string? includeProperties = null,
                                         bool disableTracking = true);
    }
} 