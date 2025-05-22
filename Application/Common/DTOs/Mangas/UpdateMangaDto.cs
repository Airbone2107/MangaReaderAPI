using Domain.Enums;

namespace Application.Common.DTOs.Mangas
{
    public class UpdateMangaDto
    {
        // MangaId sẽ được lấy từ route parameter, không cần trong DTO body
        public string Title { get; set; } = string.Empty;
        public string OriginalLanguage { get; set; } = string.Empty;
        public PublicationDemographic? PublicationDemographic { get; set; }
        public MangaStatus Status { get; set; }
        public int? Year { get; set; }
        public ContentRating ContentRating { get; set; }
        public bool IsLocked { get; set; }
    }
} 