Đã hiểu rõ yêu cầu của bạn! Chúng ta sẽ đảm bảo `desiredPublicId` cho `ChapterPage` được chuẩn hóa hoàn toàn chỉ dựa trên `ChapterId` và `PageNumber`, và việc ghi đè là hành vi mong muốn.

Dưới đây là file `TODO.md` được cập nhật:

```markdown
# TODO.md

## Nhiệm vụ 1: Tạo các lớp Exception tùy chỉnh

Các lớp exception này sẽ được đặt trong thư mục `Application/Exceptions/`.

### 1.1. NotFoundException

Dùng khi một tài nguyên cụ thể không được tìm thấy.

```csharp
// Application/Exceptions/NotFoundException.cs
using System;

namespace Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found.")
        {
        }

        public NotFoundException(string name, object key, string additionalInfo)
            : base($"Entity \"{name}\" ({key}) was not found. {additionalInfo}")
        {
        }
    }
}
```

### 1.2. ValidationException

Dùng khi có lỗi validation dữ liệu đầu vào.

```csharp
// Application/Exceptions/ValidationException.cs
using System;
using System.Collections.Generic;
using System.Linq; // Required for .GroupBy and .ToDictionary
using FluentValidation.Results; 

namespace Application.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException()
            : base("One or more validation failures have occurred.")
        {
            Errors = new Dictionary<string, string[]>();
        }

        // Constructor để nhận lỗi từ FluentValidation
        public ValidationException(IEnumerable<ValidationFailure> failures)
            : this()
        {
            Errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
        }
        
        // Constructor cho các lỗi validation đơn giản không dùng FluentValidation
        public ValidationException(string message) : base(message)
        {
             Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string propertyName, string errorMessage) : this()
        {
            Errors = new Dictionary<string, string[]> { { propertyName, new[] { errorMessage } } };
        }


        public IDictionary<string, string[]> Errors { get; }
    }
}
```

### 1.3. DeleteFailureException

Dùng khi việc xóa một entity thất bại do các ràng buộc hoặc lý do nghiệp vụ.

```csharp
// Application/Exceptions/DeleteFailureException.cs
using System;

namespace Application.Exceptions
{
    public class DeleteFailureException : Exception
    {
        public DeleteFailureException(string name, object key, string reason)
            : base($"Deletion of entity \"{name}\" ({key}) failed. Reason: {reason}")
        {
        }
         public DeleteFailureException(string message) : base(message)
        {
        }
    }
}
```

### 1.4. ApiException

Một exception chung cho các lỗi xảy ra ở tầng API hoặc không thuộc các loại cụ thể khác.

```csharp
// Application/Exceptions/ApiException.cs
using System;

namespace Application.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException(string message) : base(message)
        {
        }

        public ApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
```

## Nhiệm vụ 2: Cập nhật logic tạo `desiredPublicId` cho Cloudinary (ChapterPages)

Logic tạo `desiredPublicId` cho ảnh của `ChapterPage` sẽ chỉ dựa trên `ChapterID` và `PageNumber` của `ChapterPage` đó. Hành vi ghi đè là mong muốn.

### 2.1. (Không thay đổi) `UploadChapterPageImageCommand`

Định nghĩa của `UploadChapterPageImageCommand` vẫn giữ nguyên vì `OriginalFileName` vẫn cần thiết để lấy `fileExtension` và có thể được `IPhotoAccessor` sử dụng cho các mục đích khác (không phải để tạo `desiredPublicId`).

```csharp
// Application/Features/Chapters/Commands/UploadChapterPageImage/UploadChapterPageImageCommand.cs
using MediatR;
using System.IO; // Cho Stream

namespace Application.Features.Chapters.Commands.UploadChapterPageImage
{
    public class UploadChapterPageImageCommand : IRequest<string> // Trả về PublicId của ảnh
    {
        public Guid ChapterPageId { get; set; } // ID của ChapterPage entry đã được tạo
        
        // Các thông tin này sẽ được Controller chuẩn bị từ IFormFile
        public Stream ImageStream { get; set; } = null!;
        public string OriginalFileName { get; set; } = string.Empty; 
        public string ContentType { get; set; } = string.Empty; 
    }
}
```

### 2.2. Cập nhật `UploadChapterPageImageCommandHandler`

Điều chỉnh logic tạo `desiredPublicId` để chỉ sử dụng `ChapterId` và `PageNumber` từ `ChapterPage` entity, và có thể bao gồm phần mở rộng file.

```csharp
// Application/Features/Chapters/Commands/UploadChapterPageImage/UploadChapterPageImageCommandHandler.cs
using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.IO; // Cho Path.GetExtension

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
```

**Giải thích thay đổi trong `UploadChapterPageImageCommandHandler`:**
1.  **`desiredPublicId`**:
    *   Được tạo thành `$"chapters/{chapterPage.ChapterId}/pages/{chapterPage.PageNumber}{fileExtension}"`.
    *   `fileExtension` được lấy từ `request.OriginalFileName` và chuyển thành chữ thường để đồng nhất.
    *   Có một kiểm tra nhỏ để đảm bảo `fileExtension` hợp lệ và có dấu chấm ở đầu, nếu không sẽ mặc định là `.jpg` (bạn có thể thay đổi logic này, ví dụ như báo lỗi).
2.  **Ghi đè**: Với `desiredPublicId` được chuẩn hóa như trên, nếu bạn upload một ảnh mới cho cùng `ChapterId` và `PageNumber` (với cùng `fileExtension`), Cloudinary (nếu được cấu hình với `Overwrite = true` và `UniqueFilename = false` trong `PhotoAccessor`) sẽ tự động ghi đè lên ảnh cũ có cùng `PublicId`. `PhotoAccessor` hiện tại đã cấu hình như vậy.
3.  **Xóa ảnh cũ**: Bước xóa ảnh cũ (`chapterPage.PublicId`) trước khi upload vẫn quan trọng. Điều này đảm bảo rằng nếu bạn thay đổi loại file (ví dụ từ `.png` sang `.jpg`), `desiredPublicId` mới sẽ khác (`.jpg` thay vì `.png`), và ảnh cũ với `PublicId` cũ (`.png`) sẽ được dọn dẹp khỏi Cloudinary, tránh để lại rác. Nếu `desiredPublicId` mới và cũ giống hệt nhau, Cloudinary sẽ tự ghi đè.
4.  **Logging**: Thêm logging chi tiết hơn về `desiredPublicId` và `ActualPublicId` trả về.

Như vậy, `desiredPublicId` giờ đây hoàn toàn dựa trên `ChapterId`, `PageNumber` và phần mở rộng của file gốc, đảm bảo tính nhất quán và cho phép ghi đè như bạn mong muốn.
```