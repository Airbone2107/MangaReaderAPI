using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TranslatedMangas.Commands.DeleteTranslatedManga
{
    public class DeleteTranslatedMangaCommandHandler : IRequestHandler<DeleteTranslatedMangaCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor; // Cần để xóa ảnh của các chapter pages
        private readonly ILogger<DeleteTranslatedMangaCommandHandler> _logger;

        public DeleteTranslatedMangaCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<DeleteTranslatedMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteTranslatedMangaCommand request, CancellationToken cancellationToken)
        {
            var translatedMangaToDelete = await _unitOfWork.TranslatedMangaRepository.GetByIdAsync(request.TranslatedMangaId);
            if (translatedMangaToDelete == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.TranslatedManga), request.TranslatedMangaId);
            }

            // Lấy tất cả các chapter của TranslatedManga này để xóa ảnh
            var chapters = await _unitOfWork.ChapterRepository.GetChaptersByTranslatedMangaAsync(request.TranslatedMangaId);
            if (chapters != null && chapters.Any())
            {
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
                                    _logger.LogWarning("Failed to delete chapter page image {PublicId} from Cloudinary for chapter {ChapterId} (during TranslatedManga deletion). Result: {DeletionResult}", 
                                        page.PublicId, chapter.ChapterId, deletionResult);
                                }
                            }
                        }
                    }
                    // Chapter entities sẽ được xóa cùng TranslatedManga do Cascade Delete
                }
            }

            // Xóa TranslatedManga (DB sẽ tự động xóa Chapters và ChapterPages liên quan do cấu hình Cascade Delete)
            await _unitOfWork.TranslatedMangaRepository.DeleteAsync(translatedMangaToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TranslatedManga {TranslatedMangaId} and its related chapters/pages deleted successfully.", request.TranslatedMangaId);
            return Unit.Value;
        }
    }
} 