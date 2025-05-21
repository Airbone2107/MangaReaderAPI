using Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    /// <summary>
    /// Lưu trữ thông tin dịch của một Manga (tiêu đề, mô tả) theo ngôn ngữ
    /// </summary>
    public class TranslatedManga : AuditableEntity // Kế thừa AuditableEntity để có CreatedAt, UpdatedAt
    {
        [Key]
        public Guid TranslatedMangaId { get; protected set; } = Guid.NewGuid();

        /// <summary>
        /// ID của Manga gốc
        /// </summary>
        [Required]
        public Guid MangaId { get; set; }
        public virtual Manga Manga { get; set; } = null!;

        /// <summary>
        /// Mã ngôn ngữ dịch (ví dụ: "en", "vi")
        /// </summary>
        [Required]
        [MaxLength(10)]
        [Column(TypeName = "varchar(10)")]
        public required string LanguageKey { get; set; }

        /// <summary>
        /// Tiêu đề đã dịch của Manga
        /// </summary>
        [Required]
        [MaxLength(500)]
        public required string Title { get; set; }

        /// <summary>
        /// Mô tả đã dịch của Manga (có thể null)
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? Description { get; set; }

        // Navigation property cho Chapter
        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    }
} 