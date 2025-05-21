using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Entity đại diện cho một tác phẩm manga
    /// </summary>
    public class Manga : AuditableEntity // Version, CreatedAt, UpdatedAt sẽ được quản lý bởi Interceptor
    {
        /// <summary>
        /// Định danh duy nhất của manga
        /// </summary>
        [Key]
        public Guid MangaId { get; protected set; } = Guid.NewGuid();

        /// <summary>
        /// Tiêu đề chính của manga (ngôn ngữ gốc)
        /// </summary>
        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mã ngôn ngữ gốc của manga (ISO 639-1)
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string OriginalLanguage { get; set; } = string.Empty;

        /// <summary>
        /// Nhóm đối tượng độc giả hướng đến
        /// </summary>
        public PublicationDemographic? PublicationDemographic { get; set; } // Enum, cho phép NULL

        /// <summary>
        /// Trạng thái xuất bản hiện tại
        /// </summary>
        [Required]
        public MangaStatus Status { get; set; } // Enum

        /// <summary>
        /// Năm xuất bản ban đầu
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Đánh giá nội dung
        /// </summary>
        [Required]
        public ContentRating ContentRating { get; set; } // Enum

        /// <summary>
        /// Cờ đánh dấu manga đã bị khóa
        /// </summary>
        public bool IsLocked { get; set; } = false; // SQL: DEFAULT 0

        // Version, CreatedAt, UpdatedAt sẽ được xử lý bởi AuditableEntity và Interceptor

        // Navigation properties
        /// <summary>
        /// Danh sách các phiên bản dịch của manga
        /// </summary>
        public virtual ICollection<TranslatedManga> TranslatedMangas { get; set; } = new List<TranslatedManga>();
        
        /// <summary>
        /// Danh sách ảnh bìa của manga
        /// </summary>
        public virtual ICollection<CoverArt> CoverArts { get; set; } = new List<CoverArt>();
        
        /// <summary>
        /// Danh sách quan hệ với tác giả
        /// </summary>
        public virtual ICollection<MangaAuthor> MangaAuthors { get; set; } = new List<MangaAuthor>();
        
        /// <summary>
        /// Danh sách thẻ gắn với manga
        /// </summary>
        public virtual ICollection<MangaTag> MangaTags { get; set; } = new List<MangaTag>();
    }
}
