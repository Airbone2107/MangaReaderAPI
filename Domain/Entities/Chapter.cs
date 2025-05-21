using Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities
{
    /// <summary>
    /// Entity đại diện cho một chương của manga
    /// </summary>
    public class Chapter : AuditableEntity // Version, CreatedAt, UpdatedAt sẽ được quản lý
    {
        /// <summary>
        /// Định danh duy nhất của chương
        /// </summary>
        [Key]
        public Guid ChapterId { get; protected set; } = Guid.NewGuid();

        /// <summary>
        /// Định danh của TranslatedManga chứa chương này
        /// </summary>
        [Required]
        public Guid TranslatedMangaId { get; set; }
        
        /// <summary>
        /// Reference đến TranslatedManga chứa chương này
        /// </summary>
        public virtual TranslatedManga TranslatedManga { get; set; } = null!;

        /// <summary>
        /// ID của user đã đăng tải chương
        /// </summary>
        public int UploadedByUserId { get; set; }
        
        /// <summary>
        /// Reference đến user đã đăng tải chương
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Số tập của chương (nếu có)
        /// </summary>
        [MaxLength(50)]
        public string? Volume { get; set; }

        /// <summary>
        /// Số chương (có thể là string để hỗ trợ format như 12.5, 12a)
        /// </summary>
        [MaxLength(50)]
        public string? ChapterNumber { get; set; }

        /// <summary>
        /// Tiêu đề của chương
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Số trang của chương, tính từ collection ChapterPages
        /// </summary>
        [NotMapped]
        public int Pages => ChapterPages?.Count ?? 0;

        /// <summary>
        /// Thời điểm xuất bản chương
        /// </summary>
        public DateTime PublishAt { get; set; } // SQL: NOT NULL
        
        /// <summary>
        /// Thời điểm chương có thể đọc được
        /// </summary>
        public DateTime ReadableAt { get; set; } // SQL: NOT NULL

        // Version, CreatedAt, UpdatedAt sẽ được xử lý bởi AuditableEntity và Interceptor

        // Navigation properties
        /// <summary>
        /// Danh sách các trang của chương
        /// </summary>
        public virtual ICollection<ChapterPage> ChapterPages { get; set; } = new List<ChapterPage>();
    }
}
