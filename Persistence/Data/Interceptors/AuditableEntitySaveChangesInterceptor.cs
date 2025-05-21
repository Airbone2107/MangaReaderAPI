using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Persistence.Data.Interceptors
{
    /// <summary>
    /// Interceptor tự động cập nhật các trường audit (CreatedAt, UpdatedAt) cho các entity
    /// </summary>
    public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
    {
        /// <summary>
        /// Xử lý cập nhật thông tin audit khi lưu thay đổi đồng bộ
        /// </summary>
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin audit khi lưu thay đổi bất đồng bộ
        /// </summary>
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        /// <summary>
        /// Cập nhật các trường audit cho tất cả entity kế thừa AuditableEntity
        /// </summary>
        /// <param name="context">DbContext đang được xử lý</param>
        private void UpdateEntities(DbContext? context)
        {
            if (context == null) return;

            var utcNow = DateTime.UtcNow;

            // Xử lý cho các entity kế thừa AuditableEntity
            foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = utcNow;
                }
            }
        }
    }
}
