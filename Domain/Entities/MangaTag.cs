namespace Domain.Entities
{
    public class MangaTag // Không có Id riêng, khóa chính là (MangaId, TagId)
    {
        public Guid MangaId { get; set; }
        public virtual Manga Manga { get; set; } = null!;

        public Guid TagId { get; set; }
        public virtual Tag Tag { get; set; } = null!;
    }
}
