using MediatR;

namespace Application.Features.CoverArts.Commands.UploadCoverArtImage
{
    public class UploadCoverArtImageCommand : IRequest<Guid> // Trả về CoverId
    {
        public Guid MangaId { get; set; }
        public string? Volume { get; set; }
        public string? Description { get; set; }

        // Thông tin file sẽ được Controller chuẩn bị
        public Stream ImageStream { get; set; } = null!;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
} 