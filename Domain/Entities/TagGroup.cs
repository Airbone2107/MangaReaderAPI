using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class TagGroup : AuditableEntity
    {
        [Key]
        public Guid TagGroupId { get; protected set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)] // Độ dài tối đa cho tên nhóm tag
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
} 