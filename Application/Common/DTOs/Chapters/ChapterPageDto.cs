namespace Application.Common.DTOs.Chapters
{
    public class ChapterPageDto
    {
        public Guid PageId { get; set; }
        public Guid ChapterId { get; set; }
        public int PageNumber { get; set; }
        public string PublicId { get; set; } = string.Empty; // URL sẽ được build ở client
    }
} 