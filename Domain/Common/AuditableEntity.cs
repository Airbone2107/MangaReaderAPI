using System.ComponentModel.DataAnnotations;

namespace Domain.Common
{
    /// <summary>
    /// Lớp cơ sở cho các entity có theo dõi thông tin audit
    /// </summary>
    public abstract class AuditableEntity
    {
        /// <summary>
        /// Thời điểm tạo entity
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Thời điểm cập nhật entity gần nhất
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}