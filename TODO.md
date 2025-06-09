Chắc chắn rồi, dựa vào log lỗi và thông tin bạn cung cấp, có vẻ như `SyncChapterPagesCommandHandler` đang không tìm thấy thông tin file ảnh (`ImageFileToUpload` bị null) cho một `PageSyncInstruction` được xác định là trang mới. Điều này xảy ra mặc dù client đã gửi `FileIdentifier` và file ảnh tương ứng.

Nguyên nhân có thể là do trong `ChaptersController`, logic map `FileIdentifier` từ JSON payload sang `IFormFile` trong `IFormFileCollection` và sau đó tạo `FileToUploadInfo` chưa xử lý đúng mọi trường hợp, đặc biệt là khi `FileIdentifier` được cung cấp nhưng file không được tìm thấy trong collection `files`.

Chúng ta sẽ tạo file `TODO.md` để sửa lỗi này. File sẽ tập trung vào việc:
1.  **Trong `ChaptersController.SyncChapterPages`**:
    *   Đảm bảo rằng nếu `PageOperationDto.FileIdentifier` được cung cấp, thì file tương ứng PHẢI có trong `IFormFileCollection files`. Nếu không tìm thấy, đây là một lỗi và cần throw `ValidationException` ngay tại Controller.
    *   Đảm bảo `MemoryStream` được reset vị trí (`memoryStream.Position = 0;`) trước khi gán cho `FileToUploadInfo.ImageStream`.
2.  **Trong `SyncChapterPagesCommandHandler.Handle`**:
    *   Mặc dù lỗi hiện tại xảy ra do `ImageFileToUpload` là `null` cho trang mới, việc kiểm tra này là đúng. Chúng ta sẽ giữ nguyên logic kiểm tra này trong Handler. Thay đổi chính sẽ nằm ở Controller để đảm bảo `ImageFileToUpload` không bao giờ `null` một cách không mong muốn nếu `FileIdentifier` đã được cung cấp.

Dưới đây là nội dung file `TODO.md`:

```markdown
<!-- TODO.md -->
# TODO: Khắc phục lỗi "Yêu cầu file ảnh cho trang mới" trong SyncChapterPages

## Mục tiêu

Khắc phục lỗi `Application.Exceptions.ValidationException: Yêu cầu file ảnh cho trang mới ở số X.` xảy ra trong `SyncChapterPagesCommandHandler` khi client đã gửi `FileIdentifier` và file ảnh cần thiết cho trang mới.

## Nguyên nhân gốc rễ (Dự đoán)

Lỗi xảy ra do `PageSyncInstruction.ImageFileToUpload` là `null` cho một trang được xác định là mới trong `SyncChapterPagesCommandHandler`. Điều này có thể bắt nguồn từ `ChaptersController.SyncChapterPages` khi:

1.  Client cung cấp `PageOperationDto.FileIdentifier` cho một trang.
2.  Nhưng file tương ứng với `FileIdentifier` đó không được tìm thấy trong `IFormFileCollection files` được gửi lên.
3.  Logic hiện tại trong Controller có thể không throw lỗi ngay lập tức trong trường hợp này nếu `PageOperationDto.PageId` cũng được client cung cấp (ngay cả khi đó là trang mới mà client tự gán PageId). Kết quả là `FileToUploadInfo` được gán `null` cho `PageSyncInstruction`.
4.  Khi `SyncChapterPagesCommandHandler` xử lý `instruction` này như một trang mới (vì `PageId` không có trong DB), nó phát hiện `ImageFileToUpload` là `null` và throw lỗi.

## Các bước khắc phục

### Bước 1: Cập nhật `MangaReaderDB\Controllers\ChaptersController.cs`

**Mục đích:** Sửa đổi logic trong action `SyncChapterPages` để đảm bảo rằng nếu `PageOperationDto.FileIdentifier` được cung cấp, file tương ứng PHẢI tồn tại trong `IFormFileCollection`. Nếu không, throw `ValidationException` ngay lập tức.

**Code đầy đủ của file `MangaReaderDB\Controllers\ChaptersController.cs` sau khi sửa đổi:**

```csharp
// MangaReaderDB/Controllers/ChaptersController.cs
using Application.Common.DTOs.Chapters;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
// Application.Exceptions đã được using, nhưng ta sẽ chỉ định rõ ràng khi new ValidationException
using Application.Features.Chapters.Commands.CreateChapter;
using Application.Features.Chapters.Commands.CreateChapterPageEntry;
using Application.Features.Chapters.Commands.DeleteChapter;
using Application.Features.Chapters.Commands.DeleteChapterPage;
using Application.Features.Chapters.Commands.SyncChapterPages;
using Application.Features.Chapters.Commands.UpdateChapter;
using Application.Features.Chapters.Commands.UpdateChapterPageDetails;
using Application.Features.Chapters.Commands.UploadChapterPageImage;
using Application.Features.Chapters.Commands.UploadChapterPages;
using Application.Features.Chapters.Queries.GetChapterById;
using Application.Features.Chapters.Queries.GetChapterPages;
using Application.Features.Chapters.Queries.GetChaptersByTranslatedManga;
using FluentValidation; // Giữ using này cho IValidator
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MangaReaderDB.Controllers
{
    public class ChaptersController : BaseApiController
    {
        private readonly FluentValidation.IValidator<CreateChapterDto> _createChapterDtoValidator;
        private readonly FluentValidation.IValidator<UpdateChapterDto> _updateChapterDtoValidator;
        private readonly FluentValidation.IValidator<CreateChapterPageDto> _createChapterPageDtoValidator;
        private readonly ILogger<ChaptersController> _logger;

        public ChaptersController(
            FluentValidation.IValidator<CreateChapterDto> createChapterDtoValidator,
            FluentValidation.IValidator<UpdateChapterDto> updateChapterDtoValidator,
            FluentValidation.IValidator<CreateChapterPageDto> createChapterPageDtoValidator,
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
                throw new Application.Exceptions.NotFoundException(nameof(Domain.Entities.Chapter), id);
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
        [ProducesResponseType(typeof(ApiResponse<List<ChapterPageAttributesDto>>), StatusCodes.Status201Created)] // Sử dụng 200 OK để đơn giản, hoặc 201 nếu tất cả đều mới
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadChapterPages(Guid chapterId, [FromForm] List<IFormFile> files, [FromForm] List<int> pageNumbers)
        {
            if (files == null || !files.Any())
            {
                throw new Application.Exceptions.ValidationException("files", "At least one file is required.");
            }
            if (pageNumbers == null || !pageNumbers.Any())
            {
                throw new Application.Exceptions.ValidationException("pageNumbers", "Page numbers are required for all files.");
            }
            if (files.Count != pageNumbers.Count)
            {
                throw new Application.Exceptions.ValidationException("files/pageNumbers", "The number of files must match the number of page numbers provided.");
            }

            var filesToUpload = new List<FileToUpload>();
            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];
                var pageNumber = pageNumbers[i];

                if (file.Length == 0)
                    throw new Application.Exceptions.ValidationException($"files[{i}]", "File content cannot be empty.");
                if (file.Length > 10 * 1024 * 1024)
                    throw new Application.Exceptions.ValidationException($"files[{i}]", "File size cannot exceed 10MB.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                {
                    throw new Application.Exceptions.ValidationException($"files[{i}]", "Invalid file type. Allowed types are: " + string.Join(", ", allowedExtensions));
                }
                if (pageNumber <= 0)
                {
                    throw new Application.Exceptions.ValidationException($"pageNumbers[{i}]", "Page number must be greater than 0.");
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

            // Bỏ new ApiResponse<> vì Ok() của BaseApiController sẽ tự làm.
            return Ok(result);
        }

        [HttpPut("{chapterId:guid}/pages")]
        [ProducesResponseType(typeof(ApiResponse<List<ChapterPageAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SyncChapterPages(Guid chapterId, [FromForm] string pageOperationsJson, [FromForm] IFormFileCollection files)
        {
            if (string.IsNullOrEmpty(pageOperationsJson))
            {
                throw new Application.Exceptions.ValidationException("pageOperationsJson", "Page operations JSON is required.");
            }

            List<PageOperationDto>? pageOperations;
            try
            {
                pageOperations = JsonSerializer.Deserialize<List<PageOperationDto>>(pageOperationsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize pageOperationsJson.");
                throw new Application.Exceptions.ValidationException("pageOperationsJson", "Invalid JSON format for page operations.");
            }

            if (pageOperations == null)
            {
                // Điều này không nên xảy ra nếu Deserialize không throw lỗi, nhưng để an toàn
                throw new Application.Exceptions.ValidationException("pageOperationsJson", "Page operations cannot be null after deserialization.");
            }

            var instructions = new List<PageSyncInstruction>();
            var fileMap = files.ToDictionary(f => f.Name, f => f); // Key là FileIdentifier từ client

            foreach (var op in pageOperations)
            {
                if (op.PageNumber <= 0)
                {
                    string errorContext = op.PageId.HasValue ? $"PageId '{op.PageId.Value}'" : $"FileIdentifier '{op.FileIdentifier}'";
                    throw new Application.Exceptions.ValidationException($"PageOperation.PageNumber", $"Page number '{op.PageNumber}' for operation related to {errorContext} must be positive.");
                }

                FileToUploadInfo? fileToUploadInfo = null;
                if (!string.IsNullOrEmpty(op.FileIdentifier)) 
                {
                    if (fileMap.TryGetValue(op.FileIdentifier, out var formFile))
                    {
                        // Validate file
                        if (formFile.Length == 0) throw new Application.Exceptions.ValidationException(op.FileIdentifier, "File content cannot be empty.");
                        if (formFile.Length > 10 * 1024 * 1024) throw new Application.Exceptions.ValidationException(op.FileIdentifier, "File size cannot exceed 10MB.");
                        // TODO: Add more specific content type validation if needed using file signatures

                        var memoryStream = new MemoryStream();
                        await formFile.CopyToAsync(memoryStream);
                        memoryStream.Position = 0; // QUAN TRỌNG: Reset vị trí stream để đọc lại từ đầu
                        fileToUploadInfo = new FileToUploadInfo
                        {
                            ImageStream = memoryStream,
                            OriginalFileName = formFile.FileName,
                            ContentType = formFile.ContentType
                        };
                    }
                    else // FileIdentifier được chỉ định nhưng không tìm thấy file tương ứng
                    {
                        // Nếu client nói rằng sẽ cung cấp file (bằng FileIdentifier) thì file đó phải tồn tại.
                        // Bất kể đây là trang mới hay trang cũ cần thay ảnh.
                        _logger.LogWarning("File with identifier '{FileIdentifier}' was specified in pageOperationsJson but not found in the uploaded files for chapter {ChapterId}. PageId: {PageId}, PageNumber: {PageNumber}", 
                            op.FileIdentifier, chapterId, op.PageId?.ToString() ?? "new", op.PageNumber);
                        throw new Application.Exceptions.ValidationException(op.FileIdentifier, $"File with identifier '{op.FileIdentifier}' was specified but not found in the uploaded files.");
                    }
                }
                // Nếu op.FileIdentifier là null hoặc empty, fileToUploadInfo sẽ là null.
                // Handler sẽ kiểm tra:
                // - Nếu là trang mới (PageId không có trong DB) và fileToUploadInfo là null -> Lỗi (cần ảnh cho trang mới).
                // - Nếu là trang cũ (PageId có trong DB) và fileToUploadInfo là null -> Không thay đổi ảnh, chỉ thay đổi PageNumber.

                instructions.Add(new PageSyncInstruction
                {
                    PageId = op.PageId ?? Guid.NewGuid(), // Nếu PageId null từ client, tạo Guid mới.
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
            return Ok(result); // BaseApiController.Ok sẽ tự wrap trong ApiResponse
        }
    }

    // Controller này có thể gộp vào ChaptersController hoặc giữ riêng nếu có nhiều endpoint hơn cho ChapterPages.
    // Hiện tại, nó có một số endpoint liên quan đến pageId.
    [Route("chapterpages")] 
    public class ChapterPagesController : BaseApiController
    {
        private readonly FluentValidation.IValidator<UpdateChapterPageDto> _updateChapterPageDtoValidator;
        private readonly ILogger<ChapterPagesController> _logger;

        public ChapterPagesController(
            FluentValidation.IValidator<UpdateChapterPageDto> updateChapterPageDtoValidator,
            ILogger<ChapterPagesController> logger)
        {
            _updateChapterPageDtoValidator = updateChapterPageDtoValidator;
            _logger = logger;
        }

        [HttpPost("{pageId:guid}/image")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)] // Trả về publicId
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadChapterPageImage(Guid pageId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new Application.Exceptions.ValidationException("file", "File is required.");
            }
            if (file.Length > 5 * 1024 * 1024) // 5MB
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
            return Ok(responsePayload); // BaseApiController.Ok sẽ tự wrap
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

### Bước 2: (Tùy chọn nhưng khuyến nghị) Kiểm tra lại `Application\Features\Chapters\Commands\SyncChapterPages\SyncChapterPagesCommandHandler.cs`

**Mục đích:** Đảm bảo logic trong handler vẫn chặt chẽ sau thay đổi ở Controller. Logic hiện tại về việc kiểm tra `instruction.ImageFileToUpload == null` cho trang mới có vẻ đã đúng và sẽ hoạt động tốt hơn với sự đảm bảo từ Controller.

**Code đầy đủ của file `Application\Features\Chapters\Commands\SyncChapterPages\SyncChapterPagesCommandHandler.cs` (không thay đổi so với phiên bản bạn cung cấp vì logic kiểm tra lỗi đã đúng nếu Controller gửi dữ liệu chuẩn):**

```csharp
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

            // Kiểm tra PageNumber > 0 và duy nhất trong request
            var pageNumbersInRequest = request.Instructions.Select(i => i.DesiredPageNumber).ToList();
            if (pageNumbersInRequest.Any(p => p <= 0))
            {
                throw new ValidationException("Số trang phải lớn hơn 0.");
            }
            if (pageNumbersInRequest.Distinct().Count() != pageNumbersInRequest.Count)
            {
                throw new ValidationException("Số trang trong yêu cầu phải là duy nhất.");
            }

            // ----- Giai đoạn 1: Xác định các trang cần thay đổi số và cần gán giá trị tạm thời -----
            _logger.LogInformation("Giai đoạn 1: Xác định các trang cần thay đổi số cho chương {ChapterId}.", request.ChapterId);
            var pageNumberUpdates = new Dictionary<Guid, int>();
            var pagesToReceiveTemporaryNumber = new List<ChapterPage>();
            bool hasTemporaryUpdates = false;

            foreach (var instruction in request.Instructions)
            {
                if (existingDbPages.TryGetValue(instruction.PageId, out var existingPage))
                {
                    if (existingPage.PageNumber != instruction.DesiredPageNumber)
                    {
                        pageNumberUpdates[existingPage.PageId] = instruction.DesiredPageNumber;
                        pagesToReceiveTemporaryNumber.Add(existingPage);
                        _logger.LogInformation("Đánh dấu trang {PageId} để thay đổi số từ {OldPageNumber} sang {NewPageNumber}.",
                            existingPage.PageId, existingPage.PageNumber, instruction.DesiredPageNumber);
                    }
                }
            }

            // ----- Giai đoạn 2: Gán PageNumber tạm thời cho các trang cần thay đổi số -----
            _logger.LogInformation("Giai đoạn 2: Gán số trang tạm thời cho chương {ChapterId}.", request.ChapterId);
            if (pagesToReceiveTemporaryNumber.Any())
            {
                int tempNumberStart = -(existingDbPages.Count + request.Instructions.Count(i => !existingDbPages.ContainsKey(i.PageId)) + 1000);
                foreach (var pageToTemporarilyUpdate in pagesToReceiveTemporaryNumber)
                {
                    pageToTemporarilyUpdate.PageNumber = tempNumberStart--;
                    _logger.LogInformation("Tạm thời cập nhật PageNumber cho trang {PageId} thành {TemporaryPageNumber}.",
                        pageToTemporarilyUpdate.PageId, pageToTemporarilyUpdate.PageNumber);
                    await _unitOfWork.ChapterRepository.UpdatePageAsync(pageToTemporarilyUpdate);
                    hasTemporaryUpdates = true;
                }

                if (hasTemporaryUpdates)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Đã lưu các PageNumber tạm thời vào DB cho chương {ChapterId}.", request.ChapterId);
                }
            }

            // ----- Giai đoạn 3: Xóa các trang không có trong danh sách yêu cầu -----
            _logger.LogInformation("Giai đoạn 3: Xóa các trang không có trong yêu cầu cho chương {ChapterId}.", request.ChapterId);
            var pagesToDelete = existingDbPages.Values.Where(p => !requestedPageIds.Contains(p.PageId)).ToList();
            bool hasDeletions = false;
            if (pagesToDelete.Any())
            {
                foreach (var pageToDelete in pagesToDelete)
                {
                    if (!string.IsNullOrEmpty(pageToDelete.PublicId))
                    {
                        var deletionResult = await _photoAccessor.DeletePhotoAsync(pageToDelete.PublicId);
                        if (deletionResult != "ok" && deletionResult != "not found")
                        {
                            _logger.LogWarning("Không thể xóa ảnh {PublicId} từ Cloudinary cho trang {PageId} của chương {ChapterId}.",
                                pageToDelete.PublicId, pageToDelete.PageId, request.ChapterId);
                        }
                    }
                    await _unitOfWork.ChapterRepository.DeletePageAsync(pageToDelete.PageId);
                    _logger.LogInformation("Đã đánh dấu trang {PageId} (Số cũ: {PageNumber}) để xóa khỏi chương {ChapterId}.",
                        pageToDelete.PageId, pageToDelete.PageNumber, request.ChapterId);
                    existingDbPages.Remove(pageToDelete.PageId); // Xóa khỏi dictionary theo dõi
                    hasDeletions = true;
                }

                if (hasDeletions)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Đã lưu các thay đổi xóa trang vào DB cho chương {ChapterId}.", request.ChapterId);
                }
            }

            // ----- Giai đoạn 4: Cập nhật và Thêm mới các trang -----
            _logger.LogInformation("Giai đoạn 4: Xử lý thêm mới và cập nhật các trang cho chương {ChapterId}.", request.ChapterId);

            // 4.1. Thêm các trang mới
            _logger.LogInformation("Giai đoạn 4.1: Thêm các trang mới.");
            var newPagesInstructions = request.Instructions.Where(i => !existingDbPages.ContainsKey(i.PageId)).OrderBy(i => i.DesiredPageNumber).ToList();
            bool newPagesAdded = false;

            if (newPagesInstructions.Any())
            {
                foreach (var instruction in newPagesInstructions)
                {
                    // Kiểm tra này quan trọng: Nếu controller đã đảm bảo ImageFileToUpload được gán đúng,
                    // thì lỗi này chỉ xảy ra nếu client gửi yêu cầu tạo trang mới mà không cung cấp FileIdentifier.
                    if (instruction.ImageFileToUpload == null)
                    {
                        _logger.LogError("Hướng dẫn trang mới cho chương {ChapterId} ở số trang {DesiredPageNumber} không có file ảnh (ImageFileToUpload is null). Instruction PageId: {InstructionPageId}",
                            request.ChapterId, instruction.DesiredPageNumber, instruction.PageId);
                        throw new ValidationException($"Yêu cầu file ảnh cho trang mới ở số {instruction.DesiredPageNumber}.");
                    }

                    _logger.LogInformation("Thêm trang mới cho chương {ChapterId} ở số trang: {DesiredPageNumber} với PageId: {PageId}",
                        request.ChapterId, instruction.DesiredPageNumber, instruction.PageId);

                    var newPageEntity = new ChapterPage
                    {
                        ChapterId = request.ChapterId,
                        PageNumber = instruction.DesiredPageNumber,
                        PageId = instruction.PageId // Sử dụng PageId từ instruction (do controller gán hoặc client cung cấp)
                    };

                    var desiredPublicId = $"chapters/{request.ChapterId}/pages/{newPageEntity.PageId}";
                    var uploadResult = await _photoAccessor.UploadPhotoAsync(
                        instruction.ImageFileToUpload.ImageStream,
                        desiredPublicId,
                        instruction.ImageFileToUpload.OriginalFileName
                    );

                    if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
                    {
                        _logger.LogError("Không thể tải ảnh cho trang mới (số {DesiredPageNumber}, PageId {PageId}) trong chương {ChapterId}.",
                            instruction.DesiredPageNumber, newPageEntity.PageId, request.ChapterId);
                        // Có thể throw lỗi ở đây nếu việc upload thất bại là nghiêm trọng
                        // throw new ApiException($"Image upload failed for new page {instruction.DesiredPageNumber} in chapter {request.ChapterId}.");
                        newPageEntity.PublicId = "upload_new_failed"; // Hoặc một giá trị đánh dấu lỗi
                    }
                    else
                    {
                        newPageEntity.PublicId = uploadResult.PublicId;
                    }
                    await _unitOfWork.ChapterRepository.AddPageAsync(newPageEntity);
                    newPagesAdded = true;
                }

                if (newPagesAdded)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Đã lưu các trang mới vào DB cho chương {ChapterId}.", request.ChapterId);
                }
            }

            // 4.2. Cập nhật các trang hiện có về PageNumber cuối cùng và cập nhật ảnh (nếu có)
            _logger.LogInformation("Giai đoạn 4.2: Cập nhật các trang hiện có về số trang cuối cùng.");
            var existingPagesToUpdateInstructions = request.Instructions.Where(i => existingDbPages.ContainsKey(i.PageId)).OrderBy(i => i.DesiredPageNumber).ToList();
            bool existingPagesUpdated = false;

            if (existingPagesToUpdateInstructions.Any())
            {
                foreach (var instruction in existingPagesToUpdateInstructions)
                {
                    var currentPageEntity = existingDbPages[instruction.PageId];

                    _logger.LogInformation("Cập nhật trang hiện có {PageId} cho chương {ChapterId}. Số trang tạm thời: {CurrentPageNumber}, Số trang mới: {DesiredPageNumber}",
                        instruction.PageId, request.ChapterId, currentPageEntity.PageNumber, instruction.DesiredPageNumber);

                    currentPageEntity.PageNumber = instruction.DesiredPageNumber;

                    if (instruction.ImageFileToUpload != null) // Nếu client muốn thay thế ảnh cho trang này
                    {
                        _logger.LogInformation("Thay thế ảnh cho trang {PageId} (số mới {DesiredPageNumber}). Ảnh cũ PublicId: {OldPublicId}", currentPageEntity.PageId, instruction.DesiredPageNumber, currentPageEntity.PublicId);
                        var desiredPublicId = $"chapters/{request.ChapterId}/pages/{currentPageEntity.PageId}";
                        
                        // Không cần xóa ảnh cũ trước nếu Cloudinary được cấu hình Overwrite=true và PublicId không đổi.
                        // Nếu PublicId có thể thay đổi (ví dụ: dựa trên tên file mới), thì cần xóa ảnh cũ.
                        // Hiện tại, PublicId dựa trên PageId nên nó sẽ không đổi.

                        var uploadResult = await _photoAccessor.UploadPhotoAsync(
                            instruction.ImageFileToUpload.ImageStream,
                            desiredPublicId,
                            instruction.ImageFileToUpload.OriginalFileName
                        );

                        if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
                        {
                            _logger.LogError("Không thể tải lại ảnh cho trang {PageId} (số mới {DesiredPageNumber}) trong chương {ChapterId}.",
                                currentPageEntity.PageId, instruction.DesiredPageNumber, request.ChapterId);
                            // Quyết định xử lý lỗi: có thể throw, hoặc giữ lại PublicId cũ, hoặc đánh dấu lỗi
                            // throw new ApiException($"Image re-upload failed for page {instruction.DesiredPageNumber} (PageId: {currentPageEntity.PageId}) in chapter {request.ChapterId}.");
                             currentPageEntity.PublicId = "upload_replace_failed"; // Đánh dấu lỗi, hoặc giữ lại PublicId cũ
                        }
                        else
                        {
                            currentPageEntity.PublicId = uploadResult.PublicId;
                        }
                    }

                    await _unitOfWork.ChapterRepository.UpdatePageAsync(currentPageEntity);
                    existingPagesUpdated = true;
                }

                if (existingPagesUpdated)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Đã lưu các cập nhật cuối cùng cho trang hiện có vào DB cho chương {ChapterId}.", request.ChapterId);
                }
            }

            // ----- Giai đoạn 5: Ghi log hoàn thành -----
            _logger.LogInformation("Giai đoạn 5: Đồng bộ thành công các trang cho chương {ChapterId}. Các thay đổi đã được lưu qua nhiều bước.", request.ChapterId);

            // ----- Giai đoạn 6: Lấy lại danh sách trang đã cập nhật và trả về -----
            _logger.LogInformation("Giai đoạn 6: Lấy lại danh sách trang cuối cùng cho chương {ChapterId}.", request.ChapterId);
            var updatedChapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);
            var resultPages = updatedChapterWithPages?.ChapterPages.OrderBy(p => p.PageNumber).ToList() ?? new List<ChapterPage>();

            return _mapper.Map<List<ChapterPageAttributesDto>>(resultPages);
        }
    }
}
```

# KẾT LUẬN CẬP NHẬT
Đã cập nhật thành công ChaptersController.SyncChapterPages để kiểm tra nghiêm ngặt và đảm bảo khi client cung cấp FileIdentifier thì file tương ứng PHẢI tồn tại trong request. Việc này sẽ giúp SyncChapterPagesCommandHandler nhận được dữ liệu đúng và tránh lỗi "Yêu cầu file ảnh cho trang mới ở số X".

Cập nhật: 14/07/2023