namespace Application.Common.DTOs.CoverArts
{
    public class CreateCoverArtDto // Dùng cho command tạo entry CoverArt
    {
        public Guid MangaId { get; set; }
        public string? Volume { get; set; }
        public string? Description { get; set; }
        // PublicId sẽ được gán sau khi upload ảnh thành công
    }
} 