using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Application.Features.Chapters.Commands.UploadChapterPageImage
{
    public class UploadChapterPageImageCommandHandler : IRequestHandler<UploadChapterPageImageCommand, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly ILogger<UploadChapterPageImageCommandHandler> _logger;

        public UploadChapterPageImageCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, ILogger<UploadChapterPageImageCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _photoAccessor = photoAccessor ?? throw new ArgumentNullException(nameof(photoAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> Handle(UploadChapterPageImageCommand request, CancellationToken cancellationToken)
        {
            var chapterPage = await _unitOfWork.ChapterRepository.GetPageByIdAsync(request.ChapterPageId);
            if (chapterPage == null)
            {
                _logger.LogWarning("ChapterPage with ID {ChapterPageId} not found for image upload.", request.ChapterPageId);
                throw new NotFoundException(nameof(Domain.Entities.ChapterPage), request.ChapterPageId);
            }

            // Nếu ChapterPage đã có ảnh (PublicId không rỗng), xóa ảnh cũ trước khi upload ảnh mới.
            if (!string.IsNullOrEmpty(chapterPage.PublicId))
            {
                var deletionResult = await _photoAccessor.DeletePhotoAsync(chapterPage.PublicId);
                if (deletionResult != "ok" && deletionResult != "not found") 
                {
                    _logger.LogWarning("Failed to delete old chapter page image {OldPublicId} from Cloudinary for ChapterPage {ChapterPageId}. Result: {DeletionResult}",
                        chapterPage.PublicId, request.ChapterPageId, deletionResult);
                }
            }
            
            // Tạo desiredPublicId cho Cloudinary dựa trên ChapterId và PageNumber.
            // KHÔNG BAO GỒM ĐUÔI FILE.
            // Cloudinary sẽ tự động thêm đuôi file thích hợp khi tạo URL hiển thị ảnh.
            var desiredPublicId = $"chapters/{chapterPage.ChapterId}/pages/{chapterPage.PageNumber}";
            
            _logger.LogInformation("Attempting to upload image for ChapterPageId '{ChapterPageId}' with desiredPublicId '{DesiredPublicId}'.", 
                                   request.ChapterPageId, desiredPublicId);

            var uploadResult = await _photoAccessor.UploadPhotoAsync(
                request.ImageStream,
                desiredPublicId, 
                request.OriginalFileName // originalFileNameForUpload được truyền vào, Cloudinary có thể dùng nó cho metadata.
            );

            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
            {
                _logger.LogError("Failed to upload image for ChapterPage {ChapterPageId} (ChapterID: {ChapterId}, PageNumber: {PageNumber}). Desired PublicId was: {DesiredPublicId}", 
                    request.ChapterPageId, chapterPage.ChapterId, chapterPage.PageNumber, desiredPublicId);
                throw new ApiException($"Image upload failed for chapter page {chapterPage.PageNumber} of chapter {chapterPage.ChapterId}.");
            }

            // PublicId trả về từ Cloudinary (uploadResult.PublicId) nên giống với desiredPublicId bạn cung cấp
            // (nếu không có đuôi file). Nếu nó vẫn chứa đuôi file, có thể do cấu hình của Cloudinary.
            // Tuy nhiên, nếu PublicId được lưu ở đây không có đuôi, thì link sẽ đúng.
            chapterPage.PublicId = uploadResult.PublicId; 
            await _unitOfWork.ChapterRepository.UpdatePageAsync(chapterPage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Image uploaded. ChapterPage {ChapterPageId} (ChapterID: {ChapterId}, PageNumber: {PageNumber}) now has PublicId: {ActualPublicId}.", 
                request.ChapterPageId, chapterPage.ChapterId, chapterPage.PageNumber, uploadResult.PublicId);
            return uploadResult.PublicId;
        }
    }
} 