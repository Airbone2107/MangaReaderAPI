using System;

namespace Application.Common.DTOs.Chapters
{
    /// <summary>
    /// DTO mô tả một hành động trên trang trong yêu cầu cập nhật batch.
    /// Được sử dụng ở Controller để nhận dữ liệu từ client.
    /// </summary>
    public class PageOperationDto
    {
        /// <summary>
        /// ID của trang hiện tại (nếu là cập nhật hoặc xóa một trang cụ thể).
        /// Để null nếu đây là một trang mới cần thêm.
        /// </summary>
        public Guid? PageId { get; set; }

        /// <summary>
        /// Số trang mong muốn (thứ tự mới). Bắt buộc cho tất cả các trang (cả mới và cũ).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Định danh file (tên file khi client gửi lên dưới dạng multipart/form-data)
        /// nếu trang này là mới hoặc cần thay thế ảnh.
        /// Client sẽ gửi các IFormFile với name trùng với giá trị này.
        /// Để null/empty nếu không thay đổi ảnh của trang hiện tại (chỉ thay đổi PageNumber).
        /// </summary>
        public string? FileIdentifier { get; set; }
    }
} 