using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

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
            // Việc này là cần thiết vì desiredPublicId mới có thể giống hệt cũ (nếu file extension giống nhau)
            // hoặc khác (nếu file extension khác), và ta muốn đảm bảo chỉ có 1 ảnh được liên kết.
            if (!string.IsNullOrEmpty(chapterPage.PublicId))
            {
                var deletionResult = await _photoAccessor.DeletePhotoAsync(chapterPage.PublicId);
                if (deletionResult != "ok" && deletionResult != "not found") // "not found" có thể chấp nhận được
                {
                    _logger.LogWarning("Failed to delete old chapter page image {OldPublicId} from Cloudinary for ChapterPage {ChapterPageId}. Result: {DeletionResult}",
                        chapterPage.PublicId, request.ChapterPageId, deletionResult);
                    // Quyết định có dừng lại không tùy thuộc vào yêu cầu. Thường thì vẫn tiếp tục upload ảnh mới.
                }
            }
            
            // Tạo desiredPublicId cho Cloudinary dựa trên ChapterId và PageNumber.
            // Phần mở rộng file được thêm vào để Cloudinary dễ dàng nhận diện và xử lý,
            // và cũng để phân biệt nếu bạn upload các loại file khác nhau cho cùng một trang (mặc dù ít xảy ra).
            // Cloudinary sẽ tự động ghi đè nếu public_id (bao gồm cả phần mở rộng) giống hệt.
            var fileExtension = Path.GetExtension(request.OriginalFileName)?.ToLowerInvariant(); // .jpg, .png (và chuyển thành chữ thường)
            
            // Đảm bảo fileExtension không rỗng và bắt đầu bằng dấu chấm.
            if (string.IsNullOrEmpty(fileExtension) || !fileExtension.StartsWith("."))
            {
                // Nếu không có phần mở rộng hoặc không hợp lệ, có thể đặt một mặc định hoặc báo lỗi.
                // Ví dụ, sử dụng ".jpg" làm mặc định nếu không có.
                // Hoặc throw exception: throw new ValidationException("OriginalFileName", "File extension is missing or invalid.");
                // Trong ví dụ này, ta sẽ mặc định là ".jpg" nếu không có
                _logger.LogWarning("OriginalFileName '{OriginalFileName}' for ChapterPageId '{ChapterPageId}' has no valid extension. Defaulting to .jpg for public_id construction.", 
                                   request.OriginalFileName, request.ChapterPageId);
                fileExtension = ".jpg"; 
            }

            var desiredPublicId = $"chapters/{chapterPage.ChapterId}/pages/{chapterPage.PageNumber}{fileExtension}";
            
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

            // Cloudinary sẽ trả về PublicId. Nó NÊN giống với desiredPublicId nếu Overwrite=true và UniqueFilename=false.
            // Nếu có sự khác biệt, cần kiểm tra cấu hình Cloudinary SDK hoặc PhotoAccessor.
            // Để đảm bảo, chúng ta sẽ lưu PublicId được trả về từ Cloudinary.
            chapterPage.PublicId = uploadResult.PublicId; 
            await _unitOfWork.ChapterRepository.UpdatePageAsync(chapterPage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Image uploaded. ChapterPage {ChapterPageId} (ChapterID: {ChapterId}, PageNumber: {PageNumber}) now has PublicId: {ActualPublicId}.", 
                request.ChapterPageId, chapterPage.ChapterId, chapterPage.PageNumber, uploadResult.PublicId);
            return uploadResult.PublicId;
        }
    }
} 