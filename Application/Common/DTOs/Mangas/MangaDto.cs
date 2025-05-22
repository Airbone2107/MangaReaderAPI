using Application.Common.DTOs.Authors;
using Application.Common.DTOs.CoverArts;
using Application.Common.DTOs.Tags;
using Application.Common.DTOs.TranslatedMangas;
using Domain.Enums;
using System.Collections.Generic;

namespace Application.Common.DTOs.Mangas
{
    public class MangaDto
    {
        public Guid MangaId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OriginalLanguage { get; set; } = string.Empty;
        public PublicationDemographic? PublicationDemographic { get; set; }
        public MangaStatus Status { get; set; }
        public int? Year { get; set; }
        public ContentRating ContentRating { get; set; }
        public bool IsLocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<TagDto> Tags { get; set; } = new();
        public List<AuthorDto> Authors { get; set; } = new(); // Hoặc DTO cụ thể hơn cho vai trò
        public List<CoverArtDto> CoverArts { get; set; } = new();
        public List<TranslatedMangaDto> TranslatedMangas { get; set; } = new();
    }
} 