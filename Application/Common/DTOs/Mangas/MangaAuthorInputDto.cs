using Domain.Enums;

namespace Application.Common.DTOs.Mangas
{
    public class MangaAuthorInputDto
    {
        public Guid AuthorId { get; set; }
        public MangaStaffRole Role { get; set; }
    }
} 