using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class CoverArt : AuditableEntity // Version, CreatedAt, UpdatedAt sẽ được quản lý
    {
        [Key]
        public Guid CoverId { get; protected set; } = Guid.NewGuid();

        public Guid MangaId { get; set; }
        public virtual Manga Manga { get; set; } = null!;

        [MaxLength(50)]
        public string? Volume { get; set; }

        /// <summary>
        /// Public ID của ảnh trên Cloudinary
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string PublicId { get; set; } = string.Empty; // SQL: NOT NULL.

        [MaxLength(512)]
        public string? Description { get; set; }

        // Version, CreatedAt, UpdatedAt sẽ được xử lý bởi AuditableEntity và Interceptor
    }
}
