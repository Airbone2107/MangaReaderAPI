using Domain.Enums;
using Application.Common.DTOs.Tags;
using Application.Common.Models; // Cần cho ResourceObject

namespace Application.Common.DTOs.Mangas
{
    public class MangaAttributesDto
    {
        public string Title { get; set; } = string.Empty;
        public string OriginalLanguage { get; set; } = string.Empty;
        public PublicationDemographic? PublicationDemographic { get; set; }
        public MangaStatus Status { get; set; }
        public int? Year { get; set; }
        public ContentRating ContentRating { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Thay đổi kiểu dữ liệu của Tags
        public List<ResourceObject<TagInMangaAttributesDto>> Tags { get; set; } = new List<ResourceObject<TagInMangaAttributesDto>>();
    }
} 