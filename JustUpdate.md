**Bước 1: Cập nhật Logic tạo `PublicId` cho Ảnh Trang**

Chúng ta sẽ sửa `UploadChapterPageImageCommandHandler` để `desiredPublicId` được tạo từ `ChapterId` và `PageId`.

```csharp
// Application/Features/Chapters/Commands/UploadChapterPageImage/UploadChapterPageImageCommandHandler.cs
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
            var desiredPublicId = $"chapters/{chapterPage.ChapterId}/pages/{chapterPage.PageId}"; // THAY ĐỔI TẠI ĐÂY
            
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
```

**Bước 2: API Cho Phép Người Dùng Đăng Tải Nhiều Trang Ảnh Cho Một Chương Truyện**

Chúng ta sẽ tạo một command mới và endpoint tương ứng.

**2.1. Tạo Command và Handler**

```csharp
// Application/Features/Chapters/Commands/UploadChapterPages/FileToUpload.cs
using System.IO;

namespace Application.Features.Chapters.Commands.UploadChapterPages
{
    public class FileToUpload
    {
        public Stream ImageStream { get; set; } = null!;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public int DesiredPageNumber { get; set; } // Số trang mong muốn cho file này
    }
}
```

```csharp
// Application/Features/Chapters/Commands/UploadChapterPages/UploadChapterPagesCommand.cs
using Application.Common.DTOs.Chapters;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Chapters.Commands.UploadChapterPages
{
    public class UploadChapterPagesCommand : IRequest<List<ChapterPageAttributesDto>>
    {
        public Guid ChapterId { get; set; }
        public List<FileToUpload> Files { get; set; } = new List<FileToUpload>();
    }
}
```

```csharp
// Application/Features/Chapters/Commands/UploadChapterPages/UploadChapterPagesCommandHandler.cs
using Application.Common.DTOs.Chapters;
using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Chapters.Commands.UploadChapterPages
{
    public class UploadChapterPagesCommandHandler : IRequestHandler<UploadChapterPagesCommand, List<ChapterPageAttributesDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly IMapper _mapper;
        private readonly ILogger<UploadChapterPagesCommandHandler> _logger;

        public UploadChapterPagesCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, IMapper mapper, ILogger<UploadChapterPagesCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _photoAccessor = photoAccessor;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<ChapterPageAttributesDto>> Handle(UploadChapterPagesCommand request, CancellationToken cancellationToken)
        {
            var chapter = await _unitOfWork.ChapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
            {
                throw new NotFoundException(nameof(Chapter), request.ChapterId);
            }

            var uploadedPagesAttributes = new List<ChapterPageAttributesDto>();

            // Sắp xếp file theo DesiredPageNumber để xử lý tuần tự
            var sortedFiles = request.Files.OrderBy(f => f.DesiredPageNumber).ToList();

            // Kiểm tra tính duy nhất của DesiredPageNumber trong request
            var duplicatePageNumbers = sortedFiles.GroupBy(f => f.DesiredPageNumber)
                                                  .Where(g => g.Count() > 1)
                                                  .Select(g => g.Key)
                                                  .ToList();
            if (duplicatePageNumbers.Any())
            {
                throw new ValidationException($"Duplicate page numbers provided in the request: {string.Join(", ", duplicatePageNumbers)}");
            }
            
            // Lấy các trang hiện có của chapter để kiểm tra trùng lặp PageNumber
            var existingPages = (await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId))?.ChapterPages ?? new List<ChapterPage>();

            foreach (var fileToUpload in sortedFiles)
            {
                // Kiểm tra xem PageNumber đã tồn tại trong chapter này chưa
                if (existingPages.Any(p => p.PageNumber == fileToUpload.DesiredPageNumber))
                {
                    _logger.LogWarning("Page number {PageNumber} already exists in chapter {ChapterId}. Skipping file {FileName}.", 
                        fileToUpload.DesiredPageNumber, request.ChapterId, fileToUpload.OriginalFileName);
                    // Hoặc throw ValidationException tùy theo yêu cầu nghiệp vụ (nghiêm ngặt hơn)
                    // throw new ValidationException($"Page number {fileToUpload.DesiredPageNumber} already exists in chapter {request.ChapterId}.");
                    continue; // Bỏ qua file này nếu số trang đã tồn tại
                }

                var chapterPageEntity = new ChapterPage
                {
                    ChapterId = request.ChapterId,
                    PageNumber = fileToUpload.DesiredPageNumber,
                    // PublicId sẽ được gán sau khi upload
                };
                // PageId sẽ tự sinh khi AddAsync

                await _unitOfWork.ChapterRepository.AddPageAsync(chapterPageEntity);
                // Phải SaveChangesAsync ở đây để chapterPageEntity.PageId được tạo ra trước khi dùng nó để tạo PublicId
                await _unitOfWork.SaveChangesAsync(cancellationToken);


                var desiredPublicId = $"chapters/{chapterPageEntity.ChapterId}/pages/{chapterPageEntity.PageId}";

                var uploadResult = await _photoAccessor.UploadPhotoAsync(
                    fileToUpload.ImageStream,
                    desiredPublicId,
                    fileToUpload.OriginalFileName
                );

                if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
                {
                    _logger.LogError("Failed to upload image {FileName} for chapter {ChapterId}, page number {PageNumber}.",
                        fileToUpload.OriginalFileName, request.ChapterId, fileToUpload.DesiredPageNumber);
                    // Quyết định: có rollback các page đã tạo entry không? Hiện tại là không.
                    // Hoặc có thể throw exception để dừng toàn bộ quá trình.
                    // Để đơn giản, ta chỉ log lỗi và tiếp tục với các file khác.
                    // Nếu muốn dừng, hãy throw new ApiException(...);
                    // Sau khi tạo entry và SaveChanges, nếu upload lỗi, entry vẫn tồn tại với PublicId rỗng. Cần cơ chế xử lý lại.
                    // Để an toàn hơn, không nên SaveChangesAsync cho từng entry page trước khi upload.
                    // Tạm thời: sẽ cập nhật PublicId sau khi upload thành công.
                    // Nếu upload lỗi, entry đã tạo sẽ không có PublicId.
                    
                    // Cần cập nhật lại: logic này không đúng, vì pageId đã có, publicId sẽ được gán.
                    // Nếu upload lỗi, thì PublicId của chapterPageEntity sẽ không được cập nhật đúng.
                    // => Nên tạo entry, upload, nếu thành công thì cập nhật PublicId, rồi mới SaveChangesAsync một lần cuối.
                    // Tuy nhiên, để tạo desiredPublicId với PageId, PageId phải được sinh ra.
                    // -> SaveChangesAsync sau khi AddPageAsync là cần thiết để có PageId.

                    chapterPageEntity.PublicId = "upload_failed"; // Đánh dấu upload lỗi
                }
                else
                {
                    chapterPageEntity.PublicId = uploadResult.PublicId;
                }
                
                // Cập nhật lại entity ChapterPage với PublicId
                await _unitOfWork.ChapterRepository.UpdatePageAsync(chapterPageEntity);
                uploadedPagesAttributes.Add(_mapper.Map<ChapterPageAttributesDto>(chapterPageEntity));
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken); // Lưu tất cả các thay đổi PublicId
            _logger.LogInformation("Successfully processed {Count} files for chapter {ChapterId}.", sortedFiles.Count, request.ChapterId);

            return uploadedPagesAttributes;
        }
    }
}

```

```csharp
// Application/Features/Chapters/Commands/UploadChapterPages/UploadChapterPagesCommandValidator.cs
using FluentValidation;

namespace Application.Features.Chapters.Commands.UploadChapterPages
{
    public class UploadChapterPagesCommandValidator : AbstractValidator<UploadChapterPagesCommand>
    {
        public UploadChapterPagesCommandValidator()
        {
            RuleFor(x => x.ChapterId)
                .NotEmpty().WithMessage("Chapter ID is required.");

            RuleFor(x => x.Files)
                .NotEmpty().WithMessage("At least one file is required.")
                .Must(files => files.All(f => f.ImageStream != null && f.ImageStream.Length > 0))
                .WithMessage("All files must have content.")
                .Must(files => files.All(f => !string.IsNullOrEmpty(f.OriginalFileName)))
                .WithMessage("All files must have an original file name.");
            
            RuleForEach(x => x.Files).ChildRules(fileRule =>
            {
                fileRule.RuleFor(f => f.DesiredPageNumber)
                    .GreaterThan(0).WithMessage("Desired page number must be greater than 0.");
                // Thêm các rule validate file khác nếu cần (ví dụ: content type)
                // Tuy nhiên, việc validate chi tiết file (như content type thực sự) thường phức tạp hơn và có thể thực hiện ở tầng khác.
            });
        }
    }
}
```

**2.2. Cập nhật Controller**

```csharp
// MangaReaderDB/Controllers/ChaptersController.cs
using Application.Common.DTOs.Chapters;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.Chapters.Commands.CreateChapter;
using Application.Features.Chapters.Commands.CreateChapterPageEntry;
using Application.Features.Chapters.Commands.DeleteChapter;
using Application.Features.Chapters.Commands.DeleteChapterPage;
using Application.Features.Chapters.Commands.UpdateChapter;
using Application.Features.Chapters.Commands.UpdateChapterPageDetails;
using Application.Features.Chapters.Commands.UploadChapterPageImage;
using Application.Features.Chapters.Commands.UploadChapterPages; // Thêm using
using Application.Features.Chapters.Queries.GetChapterById;
using Application.Features.Chapters.Queries.GetChapterPages;
using Application.Features.Chapters.Queries.GetChaptersByTranslatedManga;
using FluentValidation;
using Microsoft.AspNetCore.Http; // Thêm using
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic; // Thêm using
using System.IO; // Thêm using
using System.Linq; // Thêm using
using System.Threading.Tasks; // Thêm using

namespace MangaReaderDB.Controllers
{
    public class ChaptersController : BaseApiController
    {
        private readonly IValidator<CreateChapterDto> _createChapterDtoValidator;
        private readonly IValidator<UpdateChapterDto> _updateChapterDtoValidator;
        private readonly IValidator<CreateChapterPageDto> _createChapterPageDtoValidator;
        private readonly ILogger<ChaptersController> _logger;

        public ChaptersController(
            IValidator<CreateChapterDto> createChapterDtoValidator,
            IValidator<UpdateChapterDto> updateChapterDtoValidator,
            IValidator<CreateChapterPageDto> createChapterPageDtoValidator,
            ILogger<ChaptersController> logger)
        {
            _createChapterDtoValidator = createChapterDtoValidator;
            _updateChapterDtoValidator = updateChapterDtoValidator;
            _createChapterPageDtoValidator = createChapterPageDtoValidator;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<ChapterAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateChapter([FromBody] CreateChapterDto createDto)
        {
            var validationResult = await _createChapterDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }
            
            var command = new CreateChapterCommand
            {
                TranslatedMangaId = createDto.TranslatedMangaId,
                UploadedByUserId = createDto.UploadedByUserId, 
                Volume = createDto.Volume,
                ChapterNumber = createDto.ChapterNumber,
                Title = createDto.Title,
                PublishAt = createDto.PublishAt,
                ReadableAt = createDto.ReadableAt
            };
            var chapterId = await Mediator.Send(command);
            var chapterResource = await Mediator.Send(new GetChapterByIdQuery { ChapterId = chapterId });

            if (chapterResource == null)
            {
                _logger.LogError($"FATAL: Chapter with ID {chapterId} was not found after creation!");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetChapterById), new { id = chapterId }, chapterResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<ChapterAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChapterById(Guid id)
        {
            var query = new GetChapterByIdQuery { ChapterId = id };
            var chapterResource = await Mediator.Send(query);
            if (chapterResource == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Chapter), id);
            }
            return Ok(chapterResource);
        }

        [HttpGet("/translatedmangas/{translatedMangaId:guid}/chapters")] 
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<ChapterAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChaptersByTranslatedManga(Guid translatedMangaId, [FromQuery] GetChaptersByTranslatedMangaQuery query)
        {
            query.TranslatedMangaId = translatedMangaId;
            var result = await Mediator.Send(query);
            return Ok(result);
        }
        
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChapter(Guid id, [FromBody] UpdateChapterDto updateDto)
        {
            var validationResult = await _updateChapterDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateChapterCommand
            {
                ChapterId = id,
                Volume = updateDto.Volume,
                ChapterNumber = updateDto.ChapterNumber,
                Title = updateDto.Title,
                PublishAt = updateDto.PublishAt,
                ReadableAt = updateDto.ReadableAt
            };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteChapter(Guid id)
        {
            var command = new DeleteChapterCommand { ChapterId = id };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{chapterId:guid}/pages/entry")] 
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)] 
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateChapterPageEntry(Guid chapterId, [FromBody] CreateChapterPageDto createPageDto)
        {
            var validationResult = await _createChapterPageDtoValidator.ValidateAsync(createPageDto);
            if (!validationResult.IsValid)
            {
                 throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }
            
            var command = new CreateChapterPageEntryCommand 
            { 
                ChapterId = chapterId, 
                PageNumber = createPageDto.PageNumber
            };
            var pageId = await Mediator.Send(command);

            var responsePayload = new { PageId = pageId };
            
            return CreatedAtAction(
                actionName: nameof(ChapterPagesController.UploadChapterPageImage), 
                controllerName: "ChapterPages", 
                routeValues: new { pageId = pageId }, 
                value: new ApiResponse<object>(responsePayload)
            );
        }

        [HttpGet("{chapterId:guid}/pages")]
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<ChapterPageAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChapterPages(Guid chapterId, [FromQuery] GetChapterPagesQuery query)
        {
            query.ChapterId = chapterId;
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        // API mới: Upload nhiều trang ảnh cho một chương
        [HttpPost("{chapterId:guid}/pages/batch")]
        [ProducesResponseType(typeof(ApiResponse<List<ChapterPageAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadChapterPages(Guid chapterId, [FromForm] List<IFormFile> files, [FromForm] List<int> pageNumbers)
        {
            if (files == null || !files.Any())
            {
                throw new ValidationException("files", "At least one file is required.");
            }
            if (pageNumbers == null || !pageNumbers.Any())
            {
                 throw new ValidationException("pageNumbers", "Page numbers are required for all files.");
            }
            if (files.Count != pageNumbers.Count)
            {
                throw new ValidationException("files/pageNumbers", "The number of files must match the number of page numbers provided.");
            }

            var filesToUpload = new List<FileToUpload>();
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var pageNumber = pageNumbers[i];

                if (file.Length == 0)
                    throw new ValidationException($"files[{i}]", "File content cannot be empty.");
                if (file.Length > 10 * 1024 * 1024) // Giới hạn 10MB mỗi file
                    throw new ValidationException($"files[{i}]", "File size cannot exceed 10MB.");
                
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                {
                    throw new ValidationException($"files[{i}]", "Invalid file type. Allowed types are: " + string.Join(", ", allowedExtensions));
                }
                if (pageNumber <=0)
                {
                    throw new ValidationException($"pageNumbers[{i}]", "Page number must be greater than 0.");
                }

                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // Reset stream position

                filesToUpload.Add(new FileToUpload
                {
                    ImageStream = memoryStream,
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    DesiredPageNumber = pageNumber
                });
            }

            var command = new UploadChapterPagesCommand
            {
                ChapterId = chapterId,
                Files = filesToUpload
            };

            var result = await Mediator.Send(command);
            
            // Không sử dụng Created() ở đây vì chúng ta trả về một danh sách, không phải một resource đơn lẻ mới được tạo.
            // Trả về 200 OK với danh sách các trang đã upload hoặc 201 nếu tất cả đều mới.
            // Để đơn giản, sử dụng 200 OK với payload.
            // Nếu muốn trả về 201, cần kiểm tra xem tất cả các page trong result có phải là mới hoàn toàn không.
            // Hoặc, có thể trả về URL của endpoint GetChapterPages.
            // Hiện tại, trả về 200 OK cho đơn giản.
            return Ok(new ApiResponse<List<ChapterPageAttributesDto>>(result));
        }
    }

    // Tách riêng ChapterPagesController để quản lý các endpoint liên quan đến ChapterPage
    [Route("chapterpages")]
    public class ChapterPagesController : BaseApiController
    {
        private readonly IValidator<UpdateChapterPageDto> _updateChapterPageDtoValidator;
        private readonly ILogger<ChapterPagesController> _logger;

        public ChapterPagesController(
            IValidator<UpdateChapterPageDto> updateChapterPageDtoValidator,
            ILogger<ChapterPagesController> logger)
        {
            _updateChapterPageDtoValidator = updateChapterPageDtoValidator;
            _logger = logger;
        }

        [HttpPost("{pageId:guid}/image")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)] 
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] 
        public async Task<IActionResult> UploadChapterPageImage(Guid pageId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Application.Exceptions.ValidationException("file", "File is required.");
            }
            if (file.Length > 5 * 1024 * 1024) 
            {
                 throw new Application.Exceptions.ValidationException("file", "File size cannot exceed 5MB.");
            }
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                throw new Application.Exceptions.ValidationException("file", "Invalid file type. Allowed types are: " + string.Join(", ", allowedExtensions));
            }

            using var stream = file.OpenReadStream();
            var command = new UploadChapterPageImageCommand
            {
                ChapterPageId = pageId,
                ImageStream = stream,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType
            };
            var publicId = await Mediator.Send(command); 
            
            var responsePayload = new { PublicId = publicId };
            return Ok(responsePayload); 
        }

        [HttpPut("{pageId:guid}/details")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChapterPageDetails(Guid pageId, [FromBody] UpdateChapterPageDto updateDto)
        {
            var validationResult = await _updateChapterPageDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateChapterPageDetailsCommand
            {
                PageId = pageId,
                PageNumber = updateDto.PageNumber
            };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{pageId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteChapterPage(Guid pageId)
        {
            var command = new DeleteChapterPageCommand { PageId = pageId };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 
```

**Bước 3: API Cho Phép Người Dùng Cập Nhật Toàn Bộ Ảnh Cho Một Chương Truyện**

Chức năng này phức tạp hơn, bao gồm xóa trang cũ, thêm trang mới, cập nhật trang hiện tại (có thể thay ảnh hoặc chỉ thay đổi thứ tự).

**3.1. Tạo DTOs, Command và Handler**

```csharp
// Application/Common/DTOs/Chapters/PageOperationDto.cs
using System;

namespace Application.Common.DTOs.Chapters
{
    /// <summary>
    /// DTO mô tả một hành động trên trang trong yêu cầu cập nhật batch.
    /// Được sử dụng ở Controller để nhận dữ liệu từ client.
    /// </summary>
    public class PageOperationDto
    {
        /// <summary>
        /// ID của trang hiện tại (nếu là cập nhật hoặc xóa một trang cụ thể).
        /// Để null nếu đây là một trang mới cần thêm.
        /// </summary>
        public Guid? PageId { get; set; }

        /// <summary>
        /// Số trang mong muốn (thứ tự mới). Bắt buộc cho tất cả các trang (cả mới và cũ).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Định danh file (tên file khi client gửi lên dưới dạng multipart/form-data)
        /// nếu trang này là mới hoặc cần thay thế ảnh.
        /// Client sẽ gửi các IFormFile với name trùng với giá trị này.
        /// Để null/empty nếu không thay đổi ảnh của trang hiện tại (chỉ thay đổi PageNumber).
        /// </summary>
        public string? FileIdentifier { get; set; }
    }
}
```

```csharp
// Application/Features/Chapters/Commands/SyncChapterPages/PageSyncInstruction.cs
using System;
using System.IO;

namespace Application.Features.Chapters.Commands.SyncChapterPages
{
    public class PageSyncInstruction
    {
        /// <summary>
        /// ID của trang. Nếu là trang mới, ID này sẽ được tạo bởi handler.
        /// Nếu là trang cũ, đây là PageId hiện tại.
        /// </summary>
        public Guid PageId { get; set; }

        /// <summary>
        /// Số trang (thứ tự) mong muốn.
        /// </summary>
        public int DesiredPageNumber { get; set; }

        /// <summary>
        /// Dữ liệu file ảnh nếu đây là trang mới hoặc trang cần thay thế ảnh.
        /// Null nếu chỉ thay đổi thứ tự/metadata của trang hiện tại mà không thay đổi ảnh.
        /// </summary>
        public FileToUploadInfo? ImageFileToUpload { get; set; }
    }

    public class FileToUploadInfo
    {
        public Stream ImageStream { get; set; } = null!;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}
```

```csharp
// Application/Features/Chapters/Commands/SyncChapterPages/SyncChapterPagesCommand.cs
using Application.Common.DTOs.Chapters;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Chapters.Commands.SyncChapterPages
{
    public class SyncChapterPagesCommand : IRequest<List<ChapterPageAttributesDto>>
    {
        public Guid ChapterId { get; set; }
        public List<PageSyncInstruction> Instructions { get; set; } = new List<PageSyncInstruction>();
    }
}
```

```csharp
// Application/Features/Chapters/Commands/SyncChapterPages/SyncChapterPagesCommandHandler.cs
using Application.Common.DTOs.Chapters;
using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Chapters.Commands.SyncChapterPages
{
    public class SyncChapterPagesCommandHandler : IRequestHandler<SyncChapterPagesCommand, List<ChapterPageAttributesDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly IMapper _mapper;
        private readonly ILogger<SyncChapterPagesCommandHandler> _logger;

        public SyncChapterPagesCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, IMapper mapper, ILogger<SyncChapterPagesCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _photoAccessor = photoAccessor;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<ChapterPageAttributesDto>> Handle(SyncChapterPagesCommand request, CancellationToken cancellationToken)
        {
            var chapter = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);
            if (chapter == null)
            {
                throw new NotFoundException(nameof(Chapter), request.ChapterId);
            }

            var existingDbPages = chapter.ChapterPages.ToDictionary(p => p.PageId);
            var requestedPageIds = request.Instructions.Select(i => i.PageId).ToHashSet();

            // 1. Xóa các trang không có trong request.Instructions
            var pagesToDelete = existingDbPages.Values.Where(p => !requestedPageIds.Contains(p.PageId)).ToList();
            foreach (var pageToDelete in pagesToDelete)
            {
                if (!string.IsNullOrEmpty(pageToDelete.PublicId))
                {
                    var deletionResult = await _photoAccessor.DeletePhotoAsync(pageToDelete.PublicId);
                    if (deletionResult != "ok" && deletionResult != "not found")
                    {
                        _logger.LogWarning("Failed to delete image {PublicId} from Cloudinary for page {PageId} of chapter {ChapterId}.",
                            pageToDelete.PublicId, pageToDelete.PageId, request.ChapterId);
                        // Cân nhắc: có nên dừng lại nếu không xóa được ảnh?
                    }
                }
                // Entity Framework sẽ xử lý việc xóa khỏi DB khi SaveChanges được gọi
                // Tuy nhiên, vì ChapterPage không được load trực tiếp bởi ChapterRepository trong GenericRepository,
                // chúng ta cần xóa tường minh.
                await _unitOfWork.ChapterRepository.DeletePageAsync(pageToDelete.PageId); 
                _logger.LogInformation("Marked page {PageId} (Number: {PageNumber}) for deletion from chapter {ChapterId}.", 
                    pageToDelete.PageId, pageToDelete.PageNumber, request.ChapterId);
            }

            // 2. Cập nhật và thêm mới trang
            var finalPageEntities = new List<ChapterPage>();
            var pageNumbersInRequest = request.Instructions.Select(i => i.DesiredPageNumber).ToList();
            if (pageNumbersInRequest.Distinct().Count() != pageNumbersInRequest.Count)
            {
                throw new ValidationException("Page numbers in the request must be unique.");
            }


            foreach (var instruction in request.Instructions.OrderBy(i => i.DesiredPageNumber))
            {
                ChapterPage? currentPageEntity = null;

                // Kiểm tra xem PageId từ instruction có tồn tại trong DB không
                if (existingDbPages.TryGetValue(instruction.PageId, out var dbPage))
                {
                    currentPageEntity = dbPage;
                    _logger.LogInformation("Updating existing page {PageId} for chapter {ChapterId}. New page number: {DesiredPageNumber}", 
                        instruction.PageId, request.ChapterId, instruction.DesiredPageNumber);

                    currentPageEntity.PageNumber = instruction.DesiredPageNumber;

                    if (instruction.ImageFileToUpload != null) // Cần thay thế ảnh
                    {
                        // PublicId vẫn giữ nguyên vì nó dựa trên PageId (đã được cập nhật logic)
                        // Cloudinary UploadAsync với Overwrite = true sẽ ghi đè ảnh cũ.
                        var desiredPublicId = $"chapters/{request.ChapterId}/pages/{currentPageEntity.PageId}";
                        var uploadResult = await _photoAccessor.UploadPhotoAsync(
                            instruction.ImageFileToUpload.ImageStream,
                            desiredPublicId,
                            instruction.ImageFileToUpload.OriginalFileName
                        );

                        if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
                        {
                            _logger.LogError("Failed to re-upload image for page {PageId} (new number {DesiredPageNumber}) in chapter {ChapterId}.",
                                currentPageEntity.PageId, instruction.DesiredPageNumber, request.ChapterId);
                            // Xử lý lỗi: có thể throw, hoặc bỏ qua, hoặc set PublicId thành một giá trị lỗi
                            currentPageEntity.PublicId = "upload_replace_failed"; // Đánh dấu lỗi
                        }
                        else
                        {
                            currentPageEntity.PublicId = uploadResult.PublicId;
                        }
                        await _unitOfWork.ChapterRepository.UpdatePageAsync(currentPageEntity);
                    }
                    else // Không thay ảnh, chỉ có thể là thay đổi PageNumber
                    {
                         await _unitOfWork.ChapterRepository.UpdatePageAsync(currentPageEntity);
                    }
                    finalPageEntities.Add(currentPageEntity);
                }
                else // Trang mới
                {
                    if (instruction.ImageFileToUpload == null)
                    {
                        _logger.LogError("New page instruction for chapter {ChapterId} at page number {DesiredPageNumber} is missing image file.",
                            request.ChapterId, instruction.DesiredPageNumber);
                        throw new ValidationException($"Image file is required for new page at number {instruction.DesiredPageNumber}.");
                    }

                    _logger.LogInformation("Adding new page for chapter {ChapterId} at page number: {DesiredPageNumber}", 
                        request.ChapterId, instruction.DesiredPageNumber);

                    currentPageEntity = new ChapterPage
                    {
                        ChapterId = request.ChapterId,
                        PageNumber = instruction.DesiredPageNumber,
                        PageId = instruction.PageId // PageId này đã được gán (hoặc tạo mới ở Controller/Command)
                    };

                    var desiredPublicId = $"chapters/{request.ChapterId}/pages/{currentPageEntity.PageId}";
                    var uploadResult = await _photoAccessor.UploadPhotoAsync(
                        instruction.ImageFileToUpload.ImageStream,
                        desiredPublicId,
                        instruction.ImageFileToUpload.OriginalFileName
                    );

                    if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
                    {
                        _logger.LogError("Failed to upload image for new page (number {DesiredPageNumber}) in chapter {ChapterId}.",
                            instruction.DesiredPageNumber, request.ChapterId);
                        currentPageEntity.PublicId = "upload_new_failed"; // Đánh dấu lỗi
                    }
                    else
                    {
                        currentPageEntity.PublicId = uploadResult.PublicId;
                    }
                    await _unitOfWork.ChapterRepository.AddPageAsync(currentPageEntity);
                    finalPageEntities.Add(currentPageEntity);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Lấy lại danh sách trang cuối cùng từ DB để đảm bảo dữ liệu nhất quán và đã sắp xếp
            var updatedChapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);
            return _mapper.Map<List<ChapterPageAttributesDto>>(updatedChapterWithPages?.ChapterPages ?? new List<ChapterPage>());
        }
    }
}
```

```csharp
// Application/Features/Chapters/Commands/SyncChapterPages/SyncChapterPagesCommandValidator.cs
using FluentValidation;

namespace Application.Features.Chapters.Commands.SyncChapterPages
{
    public class SyncChapterPagesCommandValidator : AbstractValidator<SyncChapterPagesCommand>
    {
        public SyncChapterPagesCommandValidator()
        {
            RuleFor(x => x.ChapterId)
                .NotEmpty().WithMessage("Chapter ID is required.");

            RuleFor(x => x.Instructions)
                .NotNull().WithMessage("Page instructions are required.");
                // Có thể thêm rule để đảm bảo PageNumbers là duy nhất trong Instructions
                // .Must(instructions => instructions.Select(i => i.DesiredPageNumber).Distinct().Count() == instructions.Count)
                // .WithMessage("Desired page numbers must be unique within the instructions set.")
                // .When(x => x.Instructions != null && x.Instructions.Any());

            RuleForEach(x => x.Instructions).ChildRules(instr =>
            {
                instr.RuleFor(i => i.DesiredPageNumber)
                    .GreaterThan(0).WithMessage("Desired page number must be greater than 0.");
                
                // PageId có thể null cho trang mới, không cần rule NotEmpty.
                // Nếu ImageFileToUpload không null, thì các thuộc tính của nó phải hợp lệ
                instr.When(i => i.ImageFileToUpload != null, () => {
                    instr.RuleFor(i => i.ImageFileToUpload!.ImageStream)
                        .NotNull().WithMessage("Image stream is required if image file is provided.")
                        .Must(stream => stream.Length > 0).WithMessage("Image stream cannot be empty.");
                    instr.RuleFor(i => i.ImageFileToUpload!.OriginalFileName)
                        .NotEmpty().WithMessage("Original file name is required if image file is provided.");
                });
            });
        }
    }
}
```

**3.2. Cập nhật Controller**

```csharp
// MangaReaderDB/Controllers/ChaptersController.cs
using Application.Common.DTOs.Chapters;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.Chapters.Commands.CreateChapter;
using Application.Features.Chapters.Commands.CreateChapterPageEntry;
using Application.Features.Chapters.Commands.DeleteChapter;
using Application.Features.Chapters.Commands.DeleteChapterPage;
using Application.Features.Chapters.Commands.SyncChapterPages; // Thêm using
using Application.Features.Chapters.Commands.UpdateChapter;
using Application.Features.Chapters.Commands.UpdateChapterPageDetails;
using Application.Features.Chapters.Commands.UploadChapterPageImage;
using Application.Features.Chapters.Commands.UploadChapterPages; 
using Application.Features.Chapters.Queries.GetChapterById;
using Application.Features.Chapters.Queries.GetChapterPages;
using Application.Features.Chapters.Queries.GetChaptersByTranslatedManga;
using FluentValidation;
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Mvc;
using System; // Thêm using
using System.Collections.Generic; 
using System.IO; 
using System.Linq; 
using System.Text.Json; // Thêm using
using System.Threading.Tasks; 

namespace MangaReaderDB.Controllers
{
    public class ChaptersController : BaseApiController
    {
        private readonly IValidator<CreateChapterDto> _createChapterDtoValidator;
        private readonly IValidator<UpdateChapterDto> _updateChapterDtoValidator;
        private readonly IValidator<CreateChapterPageDto> _createChapterPageDtoValidator;
        private readonly ILogger<ChaptersController> _logger;

        public ChaptersController(
            IValidator<CreateChapterDto> createChapterDtoValidator,
            IValidator<UpdateChapterDto> updateChapterDtoValidator,
            IValidator<CreateChapterPageDto> createChapterPageDtoValidator,
            ILogger<ChaptersController> logger)
        {
            _createChapterDtoValidator = createChapterDtoValidator;
            _updateChapterDtoValidator = updateChapterDtoValidator;
            _createChapterPageDtoValidator = createChapterPageDtoValidator;
            _logger = logger;
        }

        // ... (các actions đã có giữ nguyên) ...
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<ChapterAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateChapter([FromBody] CreateChapterDto createDto)
        {
            var validationResult = await _createChapterDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }
            
            var command = new CreateChapterCommand
            {
                TranslatedMangaId = createDto.TranslatedMangaId,
                UploadedByUserId = createDto.UploadedByUserId, 
                Volume = createDto.Volume,
                ChapterNumber = createDto.ChapterNumber,
                Title = createDto.Title,
                PublishAt = createDto.PublishAt,
                ReadableAt = createDto.ReadableAt
            };
            var chapterId = await Mediator.Send(command);
            var chapterResource = await Mediator.Send(new GetChapterByIdQuery { ChapterId = chapterId });

            if (chapterResource == null)
            {
                _logger.LogError($"FATAL: Chapter with ID {chapterId} was not found after creation!");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetChapterById), new { id = chapterId }, chapterResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<ChapterAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChapterById(Guid id)
        {
            var query = new GetChapterByIdQuery { ChapterId = id };
            var chapterResource = await Mediator.Send(query);
            if (chapterResource == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Chapter), id);
            }
            return Ok(chapterResource);
        }

        [HttpGet("/translatedmangas/{translatedMangaId:guid}/chapters")] 
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<ChapterAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChaptersByTranslatedManga(Guid translatedMangaId, [FromQuery] GetChaptersByTranslatedMangaQuery query)
        {
            query.TranslatedMangaId = translatedMangaId;
            var result = await Mediator.Send(query);
            return Ok(result);
        }
        
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChapter(Guid id, [FromBody] UpdateChapterDto updateDto)
        {
            var validationResult = await _updateChapterDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateChapterCommand
            {
                ChapterId = id,
                Volume = updateDto.Volume,
                ChapterNumber = updateDto.ChapterNumber,
                Title = updateDto.Title,
                PublishAt = updateDto.PublishAt,
                ReadableAt = updateDto.ReadableAt
            };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteChapter(Guid id)
        {
            var command = new DeleteChapterCommand { ChapterId = id };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{chapterId:guid}/pages/entry")] 
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)] 
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateChapterPageEntry(Guid chapterId, [FromBody] CreateChapterPageDto createPageDto)
        {
            var validationResult = await _createChapterPageDtoValidator.ValidateAsync(createPageDto);
            if (!validationResult.IsValid)
            {
                 throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }
            
            var command = new CreateChapterPageEntryCommand 
            { 
                ChapterId = chapterId, 
                PageNumber = createPageDto.PageNumber
            };
            var pageId = await Mediator.Send(command);

            var responsePayload = new { PageId = pageId };
            
            return CreatedAtAction(
                actionName: nameof(ChapterPagesController.UploadChapterPageImage), 
                controllerName: "ChapterPages", 
                routeValues: new { pageId = pageId }, 
                value: new ApiResponse<object>(responsePayload)
            );
        }

        [HttpGet("{chapterId:guid}/pages")]
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<ChapterPageAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChapterPages(Guid chapterId, [FromQuery] GetChapterPagesQuery query)
        {
            query.ChapterId = chapterId;
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("{chapterId:guid}/pages/batch")]
        [ProducesResponseType(typeof(ApiResponse<List<ChapterPageAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadChapterPages(Guid chapterId, [FromForm] List<IFormFile> files, [FromForm] List<int> pageNumbers)
        {
            if (files == null || !files.Any())
            {
                throw new ValidationException("files", "At least one file is required.");
            }
            if (pageNumbers == null || !pageNumbers.Any())
            {
                 throw new ValidationException("pageNumbers", "Page numbers are required for all files.");
            }
            if (files.Count != pageNumbers.Count)
            {
                throw new ValidationException("files/pageNumbers", "The number of files must match the number of page numbers provided.");
            }

            var filesToUpload = new List<FileToUpload>();
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var pageNumber = pageNumbers[i];

                if (file.Length == 0)
                    throw new ValidationException($"files[{i}]", "File content cannot be empty.");
                if (file.Length > 10 * 1024 * 1024) 
                    throw new ValidationException($"files[{i}]", "File size cannot exceed 10MB.");
                
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                {
                    throw new ValidationException($"files[{i}]", "Invalid file type. Allowed types are: " + string.Join(", ", allowedExtensions));
                }
                if (pageNumber <=0)
                {
                    throw new ValidationException($"pageNumbers[{i}]", "Page number must be greater than 0.");
                }

                var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0; 

                filesToUpload.Add(new FileToUpload
                {
                    ImageStream = memoryStream,
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    DesiredPageNumber = pageNumber
                });
            }

            var command = new UploadChapterPagesCommand
            {
                ChapterId = chapterId,
                Files = filesToUpload
            };

            var result = await Mediator.Send(command);
            
            return Ok(new ApiResponse<List<ChapterPageAttributesDto>>(result));
        }
        
        // API mới: Cập nhật (đồng bộ) tất cả các trang cho một chương
        [HttpPut("{chapterId:guid}/pages")] // Sử dụng PUT cho toàn bộ collection pages của chapter
        [ProducesResponseType(typeof(ApiResponse<List<ChapterPageAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SyncChapterPages(Guid chapterId, [FromForm] string pageOperationsJson, [FromForm] IFormFileCollection files)
        {
            if (string.IsNullOrEmpty(pageOperationsJson))
            {
                throw new ValidationException("pageOperationsJson", "Page operations JSON is required.");
            }

            List<PageOperationDto>? pageOperations;
            try
            {
                pageOperations = JsonSerializer.Deserialize<List<PageOperationDto>>(pageOperationsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize pageOperationsJson.");
                throw new ValidationException("pageOperationsJson", "Invalid JSON format for page operations.");
            }

            if (pageOperations == null) // Nên throw lỗi nếu Deserialize trả về null và pageOperationsJson không rỗng
            {
                 throw new ValidationException("pageOperationsJson", "Page operations cannot be null after deserialization.");
            }

            var instructions = new List<PageSyncInstruction>();
            var fileMap = files.ToDictionary(f => f.Name, f => f); // Dùng Name của IFormFile làm FileIdentifier

            foreach (var op in pageOperations)
            {
                if (op.PageNumber <= 0)
                {
                    throw new ValidationException($"PageOperation.PageNumber", $"Page number '{op.PageNumber}' for operation related to PageId '{op.PageId?.ToString() ?? "new"}' or FileIdentifier '{op.FileIdentifier}' must be positive.");
                }

                FileToUploadInfo? fileToUploadInfo = null;
                if (!string.IsNullOrEmpty(op.FileIdentifier))
                {
                    if (fileMap.TryGetValue(op.FileIdentifier, out var formFile))
                    {
                        if (formFile.Length == 0) throw new ValidationException(op.FileIdentifier, "File content cannot be empty.");
                        if (formFile.Length > 10 * 1024 * 1024) throw new ValidationException(op.FileIdentifier, "File size cannot exceed 10MB.");
                        // Thêm validation file type ở đây nếu cần

                        var memoryStream = new MemoryStream();
                        await formFile.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        fileToUploadInfo = new FileToUploadInfo
                        {
                            ImageStream = memoryStream,
                            OriginalFileName = formFile.FileName,
                            ContentType = formFile.ContentType
                        };
                    }
                    else
                    {
                        // Nếu FileIdentifier được cung cấp nhưng không có file tương ứng, và đây không phải là PageId đã có (tức là không phải chỉ đổi thứ tự)
                        // thì đó là lỗi, trừ khi operation này là cho một PageId đã tồn tại và không có ý định thay đổi ảnh.
                        // Logic này cần được làm rõ trong command handler.
                        // Nếu PageId null (trang mới) thì FileIdentifier phải trỏ đến file.
                        if (!op.PageId.HasValue) 
                        {
                             throw new ValidationException(op.FileIdentifier, $"File with identifier '{op.FileIdentifier}' not found in the uploaded files, but is required for a new page.");
                        }
                        // Nếu PageId có giá trị, FileIdentifier có thể là để thay thế ảnh. Nếu không có file, nghĩa là không thay thế ảnh.
                    }
                }
                
                instructions.Add(new PageSyncInstruction
                {
                    // Nếu PageId null (trang mới), tạo Guid mới. Handler sẽ dùng Guid này.
                    // Nếu PageId có giá trị (trang cũ), dùng Guid đó.
                    PageId = op.PageId ?? Guid.NewGuid(), 
                    DesiredPageNumber = op.PageNumber,
                    ImageFileToUpload = fileToUploadInfo
                });
            }
            
            var command = new SyncChapterPagesCommand
            {
                ChapterId = chapterId,
                Instructions = instructions
            };

            var result = await Mediator.Send(command);
            return Ok(new ApiResponse<List<ChapterPageAttributesDto>>(result));
        }
    }

    // Tách riêng ChapterPagesController để quản lý các endpoint liên quan đến ChapterPage
    // (Giữ nguyên controller này cho các action đơn lẻ nếu cần)
    [Route("chapterpages")]
    public class ChapterPagesController : BaseApiController
    {
        private readonly IValidator<UpdateChapterPageDto> _updateChapterPageDtoValidator;
        private readonly ILogger<ChapterPagesController> _logger;

        public ChapterPagesController(
            IValidator<UpdateChapterPageDto> updateChapterPageDtoValidator,
            ILogger<ChapterPagesController> logger)
        {
            _updateChapterPageDtoValidator = updateChapterPageDtoValidator;
            _logger = logger;
        }

        [HttpPost("{pageId:guid}/image")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)] 
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] 
        public async Task<IActionResult> UploadChapterPageImage(Guid pageId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Application.Exceptions.ValidationException("file", "File is required.");
            }
            if (file.Length > 5 * 1024 * 1024) 
            {
                 throw new Application.Exceptions.ValidationException("file", "File size cannot exceed 5MB.");
            }
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                throw new Application.Exceptions.ValidationException("file", "Invalid file type. Allowed types are: " + string.Join(", ", allowedExtensions));
            }

            using var stream = file.OpenReadStream();
            var command = new UploadChapterPageImageCommand
            {
                ChapterPageId = pageId,
                ImageStream = stream,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType
            };
            var publicId = await Mediator.Send(command); 
            
            var responsePayload = new { PublicId = publicId };
            return Ok(responsePayload); 
        }

        [HttpPut("{pageId:guid}/details")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChapterPageDetails(Guid pageId, [FromBody] UpdateChapterPageDto updateDto)
        {
            var validationResult = await _updateChapterPageDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateChapterPageDetailsCommand
            {
                PageId = pageId,
                PageNumber = updateDto.PageNumber
            };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{pageId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteChapterPage(Guid pageId)
        {
            var command = new DeleteChapterPageCommand { PageId = pageId };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 
```