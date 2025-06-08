using System;
using System.IO;

namespace Application.Features.Chapters.Commands.SyncChapterPages
{
    public class PageSyncInstruction
    {
        /// <summary>
        /// ID của trang. Nếu là trang mới, ID này sẽ được tạo bởi handler.
        /// Nếu là trang cũ, đây là PageId hiện tại.
        /// </summary>
        public Guid PageId { get; set; }

        /// <summary>
        /// Số trang (thứ tự) mong muốn.
        /// </summary>
        public int DesiredPageNumber { get; set; }

        /// <summary>
        /// Dữ liệu file ảnh nếu đây là trang mới hoặc trang cần thay thế ảnh.
        /// Null nếu chỉ thay đổi thứ tự/metadata của trang hiện tại mà không thay đổi ảnh.
        /// </summary>
        public FileToUploadInfo? ImageFileToUpload { get; set; }
    }

    public class FileToUploadInfo
    {
        public Stream ImageStream { get; set; } = null!;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
} 