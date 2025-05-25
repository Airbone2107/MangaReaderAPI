using MediatR;

namespace Application.Features.Chapters.Commands.UploadChapterPageImage
{
    public class UploadChapterPageImageCommand : IRequest<string> // Trả về PublicId của ảnh
    {
        public Guid ChapterPageId { get; set; } // ID của ChapterPage entry đã được tạo
        
        // Các thông tin này sẽ được Controller chuẩn bị từ IFormFile
        public Stream ImageStream { get; set; } = null!;
        public string OriginalFileName { get; set; } = string.Empty; 
        public string ContentType { get; set; } = string.Empty; 
    }
} 