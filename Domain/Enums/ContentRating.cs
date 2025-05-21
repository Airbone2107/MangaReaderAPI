namespace Domain.Enums
{
    /// <summary>
    /// Đánh giá nội dung của manga
    /// </summary>
    public enum ContentRating
    {
        /// <summary>Nội dung an toàn, phù hợp với mọi đối tượng</summary>
        Safe,        // safe
        /// <summary>Nội dung gợi ý, có thể không phù hợp với trẻ em</summary>
        Suggestive,  // suggestive
        /// <summary>Nội dung khiêu dâm nghệ thuật</summary>
        Erotica,     // erotica
        /// <summary>Nội dung người lớn</summary>
        Pornographic // pornographic
    }
}
