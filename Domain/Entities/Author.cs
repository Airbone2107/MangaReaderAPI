using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Author : AuditableEntity // Id là Guid, Version, CreatedAt, UpdatedAt sẽ được quản lý bởi Interceptor
    {
        [Key]
        public Guid AuthorId { get; protected set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty; // SQL: [Name] [nvarchar](255) NOT NULL

        public string? Biography { get; set; } // SQL: [Biography] [nvarchar](max) NULL

        // Version, CreatedAt, UpdatedAt sẽ được xử lý bởi AuditableEntity và Interceptor

        // Navigation properties
        public virtual ICollection<MangaAuthor> MangaAuthors { get; set; } = new List<MangaAuthor>();

        public Author() // Constructor để khởi tạo giá trị default nếu cần 
        {
            // CreatedAt = DateTime.UtcNow; // Sẽ được xử lý bởi Interceptor
            // UpdatedAt = DateTime.UtcNow; // Sẽ được xử lý bởi Interceptor
            // Version = 1;                 // Sẽ được xử lý bởi Interceptor
        }
    }
}
