using System.IO;

namespace Application.Features.Chapters.Commands.UploadChapterPages
{
    public class FileToUpload
    {
        public Stream ImageStream { get; set; } = null!;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int DesiredPageNumber { get; set; } // Số trang mong muốn cho file này
    }
} 