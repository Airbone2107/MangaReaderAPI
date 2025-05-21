using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Tag : AuditableEntity // Id là Guid, Version sẽ được quản lý bởi Interceptor
    {
        [Key]
        public Guid TagId { get; protected set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; } = string.Empty; // SQL: [Name] [nvarchar](max) NOT NULL

        // Thuộc tính Group cũ đã được loại bỏ
        // [Required]
        // [MaxLength(50)]
        // public string Group { get; set; } = string.Empty; 

        // Khóa ngoại đến TagGroup
        [Required]
        public Guid TagGroupId { get; set; }
        public virtual TagGroup TagGroup { get; set; } = null!;

        // Version, CreatedAt, UpdatedAt sẽ được xử lý bởi AuditableEntity và Interceptor

        // Navigation properties
        public virtual ICollection<MangaTag> MangaTags { get; set; } = new List<MangaTag>();
    }
}
