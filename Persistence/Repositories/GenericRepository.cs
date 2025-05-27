using Application.Common.DTOs;
using Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using System.Linq.Expressions;

namespace Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public virtual async Task<PagedResult<T>> GetPagedAsync(int offset, int limit, Expression<Func<T, bool>>? filter = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            var totalCount = await query.CountAsync();

            if (orderBy != null)
            {
                query = orderBy(query);
            }
            
            var items = await query.Skip(offset)
                                   .Take(limit)
                                   .AsNoTracking()
                                   .ToListAsync();

            return new PagedResult<T>(items, totalCount, offset, limit);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            // Không gọi SaveChangesAsync() ở đây
        }

        public virtual Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            // Không gọi SaveChangesAsync() ở đây
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            // Không gọi SaveChangesAsync() ở đây
            return Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
            // Không gọi SaveChangesAsync() ở đây
        }

        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            // Tìm kiếm bằng khóa chính hiệu quả hơn là dùng FindAsync rồi kiểm tra null
            // Tuy nhiên, cách này yêu cầu entity phải có thuộc tính Id kiểu Guid.
            // Nếu T không đảm bảo có Id, cần một cách tiếp cận khác hoặc ràng buộc T.
            // Hiện tại, FindAsync là cách chung chung nhất.
            var entity = await _dbSet.FindAsync(id);
            return entity != null;
        }

        public virtual async Task<T?> FindFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }
            
            return await query.AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>>? predicate = null,
                                                              Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                                              string? includeProperties = null,
                                                              bool disableTracking = true)
        {
            IQueryable<T> query = _dbSet;

            if (disableTracking)
            {
                query = query.AsNoTracking();
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }
    }
} 