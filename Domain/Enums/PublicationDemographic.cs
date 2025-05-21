namespace Domain.Enums
{
    /// <summary>
    /// Nhóm đối tượng độc giả hướng đến của manga
    /// </summary>
    public enum PublicationDemographic
    {
        /// <summary>Dành cho nam thanh thiếu niên</summary>
        Shounen,    // shounen
        /// <summary>Dành cho nữ thanh thiếu niên</summary>
        Shoujo,     // shoujo
        /// <summary>Dành cho phụ nữ trưởng thành</summary>
        Josei,      // josei
        /// <summary>Dành cho nam giới trưởng thành</summary>
        Seinen,     // seinen
        /// <summary>Không xác định</summary>
        None        // none (để xử lý trường hợp NULL nếu cột PublicationDemographic có thể null)
    }
}
