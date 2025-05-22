using Domain.Enums;
using MediatR;

namespace Application.Features.Mangas.Commands.UpdateManga
{
    public class UpdateMangaCommand : IRequest<Unit>
    {
        public Guid MangaId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OriginalLanguage { get; set; } = string.Empty;
        public PublicationDemographic? PublicationDemographic { get; set; }
        public MangaStatus Status { get; set; }
        public int? Year { get; set; }
        public ContentRating ContentRating { get; set; }
        public bool IsLocked { get; set; }
    }
} 