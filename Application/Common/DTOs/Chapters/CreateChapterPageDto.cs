namespace Application.Common.DTOs.Chapters
{
    public class CreateChapterPageDto // Dùng cho command tạo entry ChapterPage
    {
        public Guid ChapterId { get; set; }
        public int PageNumber { get; set; }
        // PublicId sẽ được gán sau khi upload ảnh thành công
    }
} 