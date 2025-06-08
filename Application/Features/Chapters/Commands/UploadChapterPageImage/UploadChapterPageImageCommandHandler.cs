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

            // Nếu ChapterPage đã có ảnh (PublicId không rỗng), và PublicId này khác với PublicId mới sẽ được tạo
            // (trường hợp này ít xảy ra nếu PublicId luôn dựa trên PageId, nhưng để an toàn, ta có thể xóa ảnh cũ nếu nó không trống)
            // Với Overwrite = true trong PhotoAccessor, việc xóa ảnh cũ trước khi upload ảnh mới với cùng PublicId là không bắt buộc.
            // Tuy nhiên, nếu PublicId cũ khác (ví dụ: do thay đổi logic tạo PublicId), thì nên xóa.
            // Hiện tại, UploadPhotoAsync đã có Overwrite=true, nên nếu PublicId mới trùng PublicId cũ, ảnh sẽ được ghi đè.
            // Nếu PublicId mới khác PublicId cũ (do chapterPage.PageId thay đổi - không thể xảy ra, hoặc logic tạo publicId thay đổi),
            // thì ảnh cũ sẽ không được xóa tự động. Logic này cần xem xét kỹ tùy theo yêu cầu chính xác.
            // Với logic mới PublicId = ".../pages/{chapterPage.PageId}", PublicId sẽ không thay đổi cho một PageId cụ thể.
            // Việc xóa ảnh cũ chỉ cần thiết nếu bạn muốn dọn dẹp Cloudinary khi ảnh không còn được tham chiếu.
            // Hiện tại, ta chỉ cần đảm bảo ghi đè nếu PublicId giống nhau.

            // Tạo desiredPublicId cho Cloudinary dựa trên ChapterId và PageId.
            // KHÔNG BAO GỒM ĐUÔI FILE.
            var desiredPublicId = $"chapters/{chapterPage.ChapterId}/pages/{chapterPage.PageId}";
            
            _logger.LogInformation("Attempting to upload image for ChapterPageId '{ChapterPageId}' with desiredPublicId '{DesiredPublicId}'.", 
                                   request.ChapterPageId, desiredPublicId);

            var uploadResult = await _photoAccessor.UploadPhotoAsync(
                request.ImageStream,
                desiredPublicId, 
                request.OriginalFileName
            );

            if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
            {
                _logger.LogError("Failed to upload image for ChapterPage {ChapterPageId} (ChapterID: {ChapterId}). Desired PublicId was: {DesiredPublicId}", 
                    request.ChapterPageId, chapterPage.ChapterId, desiredPublicId);
                throw new ApiException($"Image upload failed for chapter page of chapter {chapterPage.ChapterId}.");
            }
            
            chapterPage.PublicId = uploadResult.PublicId; 
            await _unitOfWork.ChapterRepository.UpdatePageAsync(chapterPage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Image uploaded. ChapterPage {ChapterPageId} (ChapterID: {ChapterId}) now has PublicId: {ActualPublicId}.", 
                request.ChapterPageId, chapterPage.ChapterId, uploadResult.PublicId);
            return uploadResult.PublicId;
        }
    }
} 