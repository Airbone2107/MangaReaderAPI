using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ChapterPage // Không cần AuditableEntity vì không có Version, CreatedAt, UpdatedAt
    {
        [Key]
        public Guid PageId { get; set; } = Guid.NewGuid(); // Khởi tạo Guid mới khi tạo đối tượng

        public Guid ChapterId { get; set; }
        public virtual Chapter Chapter { get; set; } = null!;

        public int PageNumber { get; set; } // SQL: NOT NULL

        /// <summary>
        /// Public ID của ảnh trên Cloudinary
        /// </summary>
        [Required]
        [MaxLength(255)] // Giữ MaxLength, có thể điều chỉnh nếu PublicId của Cloudinary có định dạng khác
        public string PublicId { get; set; } = string.Empty; // SQL: NOT NULL.
    }
}
