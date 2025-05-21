namespace Domain.Enums
{
    /// <summary>
    /// Trạng thái xuất bản của manga
    /// </summary>
    public enum MangaStatus
    {
        /// <summary>Đang tiếp tục xuất bản</summary>
        Ongoing,     // ongoing
        /// <summary>Đã hoàn thành</summary>
        Completed,   // completed
        /// <summary>Tạm ngưng</summary>
        Hiatus,      // hiatus
        /// <summary>Đã hủy bỏ</summary>
        Cancelled    // cancelled
    }
}
