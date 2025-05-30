using Application.Common.Interfaces; // Cho IPhotoAccessor
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Mangas.Commands.DeleteManga
{
    public class DeleteMangaCommandHandler : IRequestHandler<DeleteMangaCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly ILogger<DeleteMangaCommandHandler> _logger;

        public DeleteMangaCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<DeleteMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteMangaCommand request, CancellationToken cancellationToken)
        {
            // Lấy manga với các thông tin liên quan cần xóa (CoverArts, Chapters -> ChapterPages)
            var mangaToDelete = await _unitOfWork.MangaRepository.GetMangaWithDetailsAsync(request.MangaId);

            if (mangaToDelete == null)
            {
                _logger.LogWarning("Manga with ID {MangaId} not found for deletion.", request.MangaId);
                throw new NotFoundException(nameof(Domain.Entities.Manga), request.MangaId);
            }

            // 1. Xóa ảnh bìa (CoverArts) khỏi Cloudinary và DB
            if (mangaToDelete.CoverArts != null && mangaToDelete.CoverArts.Any())
            {
                foreach (var coverArt in mangaToDelete.CoverArts.ToList()) // ToList() để tránh lỗi khi modify collection
                {
                    if (!string.IsNullOrEmpty(coverArt.PublicId))
                    {
                        var deletionResult = await _photoAccessor.DeletePhotoAsync(coverArt.PublicId);
                        if (deletionResult != "ok" && deletionResult != "not found") // "not found" có thể chấp nhận được
                        {
                            _logger.LogWarning("Failed to delete cover art {PublicId} from Cloudinary for manga {MangaId}. Result: {DeletionResult}", coverArt.PublicId, request.MangaId, deletionResult);
                        }
                    }
                    // CoverArt entities sẽ được xóa cùng Manga do cấu hình Cascade Delete
                }
            }

            // 2. Xóa các trang của chapter (ChapterPages) khỏi Cloudinary và DB
            if (mangaToDelete.TranslatedMangas != null)
            {
                foreach (var translatedManga in mangaToDelete.TranslatedMangas.ToList())
                {
                    var chapters = await _unitOfWork.ChapterRepository.GetChaptersByTranslatedMangaAsync(translatedManga.TranslatedMangaId);
                    foreach (var chapter in chapters.ToList())
                    {
                        var chapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(chapter.ChapterId);
                        if (chapterWithPages?.ChapterPages != null && chapterWithPages.ChapterPages.Any())
                        {
                            foreach (var page in chapterWithPages.ChapterPages.ToList())
                            {
                                if (!string.IsNullOrEmpty(page.PublicId))
                                {
                                    var deletionResult = await _photoAccessor.DeletePhotoAsync(page.PublicId);
                                    if (deletionResult != "ok" && deletionResult != "not found")
                                    {
                                        _logger.LogWarning("Failed to delete chapter page {PublicId} from Cloudinary for chapter {ChapterId}. Result: {DeletionResult}", page.PublicId, chapter.ChapterId, deletionResult);
                                    }
                                }
                                // ChapterPage entities sẽ được xóa cùng Chapter do Cascade Delete
                            }
                        }
                        // Chapter entities sẽ được xóa cùng TranslatedManga do Cascade Delete
                    }
                    // TranslatedManga entities sẽ được xóa cùng Manga do Cascade Delete
                }
            }
            
            // 3. Xóa Manga khỏi DB (các bảng liên quan như MangaTag, MangaAuthor, TranslatedManga, Chapter, ChapterPage, CoverArt sẽ tự động xóa theo cấu hình Cascade)
            await _unitOfWork.MangaRepository.DeleteAsync(mangaToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Manga {MangaId} and its related data deleted successfully.", request.MangaId);
            return Unit.Value;
        }
    }
} 