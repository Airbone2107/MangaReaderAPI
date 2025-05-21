using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain.Entities
{
    public class MangaAuthor // Không có Id riêng, khóa chính là (MangaId, AuthorId, Role)
    {
        public Guid MangaId { get; set; }
        public virtual Manga Manga { get; set; } = null!;

        public Guid AuthorId { get; set; }
        public virtual Author Author { get; set; } = null!;

        [Required]
        public MangaStaffRole Role { get; set; }
    }
}
