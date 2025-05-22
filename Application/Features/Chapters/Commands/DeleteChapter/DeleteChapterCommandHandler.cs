using Application.Contracts.Persistence;
using Application.Common.Interfaces; // Cho IPhotoAccessor
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // Cho ToListAsync

namespace Application.Features.Chapters.Commands.DeleteChapter
{
    public class DeleteChapterCommandHandler : IRequestHandler<DeleteChapterCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly ILogger<DeleteChapterCommandHandler> _logger;

        public DeleteChapterCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<DeleteChapterCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteChapterCommand request, CancellationToken cancellationToken)
        {
            var chapterToDelete = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);

            if (chapterToDelete == null)
            {
                _logger.LogWarning("Chapter with ID {ChapterId} not found for deletion.", request.ChapterId);
                throw new NotFoundException(nameof(Domain.Entities.Chapter), request.ChapterId);
            }

            // 1. Xóa các trang (ChapterPages) khỏi Cloudinary
            if (chapterToDelete.ChapterPages != null && chapterToDelete.ChapterPages.Any())
            {
                foreach (var page in chapterToDelete.ChapterPages.ToList()) // ToList() để tránh lỗi khi modify collection
                {
                    if (!string.IsNullOrEmpty(page.PublicId))
                    {
                        var deletionResult = await _photoAccessor.DeletePhotoAsync(page.PublicId);
                        if (deletionResult != "ok" && deletionResult != "not found")
                        {
                            _logger.LogWarning("Failed to delete chapter page {PublicId} from Cloudinary for chapter {ChapterId}. Result: {DeletionResult}", page.PublicId, request.ChapterId, deletionResult);
                            // Có thể quyết định dừng lại hoặc tiếp tục tùy theo yêu cầu
                        }
                    }
                    // ChapterPage entities sẽ được xóa cùng Chapter do cấu hình Cascade Delete trong DB
                }
            }

            // 2. Xóa Chapter khỏi DB (ChapterPages sẽ tự động xóa theo cấu hình Cascade)
            await _unitOfWork.ChapterRepository.DeleteAsync(chapterToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Chapter {ChapterId} and its pages deleted successfully.", request.ChapterId);
            return Unit.Value;
        }
    }
} 