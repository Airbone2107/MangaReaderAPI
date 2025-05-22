using Domain.Enums;
using MediatR;

namespace Application.Features.Mangas.Commands.CreateManga
{
    public class CreateMangaCommand : IRequest<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string OriginalLanguage { get; set; } = string.Empty; // ISO 639-1 code
        public PublicationDemographic? PublicationDemographic { get; set; }
        public MangaStatus Status { get; set; }
        public int? Year { get; set; }
        public ContentRating ContentRating { get; set; }
        
        // Tags và Authors sẽ được thêm qua các command riêng (AddMangaTagCommand, AddMangaAuthorCommand)
        // sau khi Manga được tạo.
    }
} 