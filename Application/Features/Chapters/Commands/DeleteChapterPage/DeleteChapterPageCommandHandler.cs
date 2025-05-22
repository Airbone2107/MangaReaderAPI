using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Commands.DeleteChapterPage
{
    public class DeleteChapterPageCommandHandler : IRequestHandler<DeleteChapterPageCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly ILogger<DeleteChapterPageCommandHandler> _logger;

        public DeleteChapterPageCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<DeleteChapterPageCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteChapterPageCommand request, CancellationToken cancellationToken)
        {
            var pageToDelete = await _unitOfWork.ChapterRepository.GetPageByIdAsync(request.PageId);

            if (pageToDelete == null)
            {
                _logger.LogWarning("ChapterPage with ID {PageId} not found for deletion.", request.PageId);
                throw new NotFoundException(nameof(Domain.Entities.ChapterPage), request.PageId);
            }

            // 1. Xóa ảnh khỏi Cloudinary (nếu có)
            if (!string.IsNullOrEmpty(pageToDelete.PublicId))
            {
                var deletionResult = await _photoAccessor.DeletePhotoAsync(pageToDelete.PublicId);
                if (deletionResult != "ok" && deletionResult != "not found")
                {
                    _logger.LogWarning("Failed to delete chapter page image {PublicId} from Cloudinary for PageId {PageId}. Result: {DeletionResult}", 
                        pageToDelete.PublicId, request.PageId, deletionResult);
                    // Quyết định có dừng lại không tùy thuộc vào yêu cầu. Thường thì vẫn tiếp tục xóa khỏi DB.
                }
            }

            // 2. Xóa ChapterPage khỏi DB
            await _unitOfWork.ChapterRepository.DeletePageAsync(request.PageId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ChapterPage {PageId} deleted successfully.", request.PageId);
            return Unit.Value;
        }
    }
} 