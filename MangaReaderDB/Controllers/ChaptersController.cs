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
            _logger.LogInformation("SyncChapterPages called for ChapterId: {ChapterId}", chapterId);
            _logger.LogInformation("Received pageOperationsJson: {PageOperationsJson}", pageOperationsJson);

            if (files != null && files.Any())
            {
                _logger.LogInformation("Received {FilesCount} files in IFormFileCollection:", files.Count);
                foreach (var f in files)
                {
                    _logger.LogInformation("- File Name (from IFormFile.Name): '{FormFileName}', OriginalFileName: '{OriginalFileName}', ContentType: '{ContentType}', Length: {Length} bytes",
                        f.Name, f.FileName, f.ContentType, f.Length);
                }
            }
            else
            {
                _logger.LogWarning("No files received in IFormFileCollection.");
            }

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
                throw new Application.Exceptions.ValidationException("pageOperationsJson", "Page operations cannot be null after deserialization.");
            }

            _logger.LogInformation("Deserialized {PageOperationsCount} page operations from JSON:", pageOperations.Count);
            foreach (var opLog in pageOperations)
            {
                _logger.LogInformation("- Operation: PageId='{PageId}', PageNumber={PageNumber}, FileIdentifier='{FileIdentifier}'",
                    opLog.PageId?.ToString() ?? "null", opLog.PageNumber, opLog.FileIdentifier ?? "null");
            }


            var instructions = new List<PageSyncInstruction>();
            var fileMap = files.ToDictionary(f => f.Name, f => f);

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
                    _logger.LogInformation("Processing operation for FileIdentifier: '{FileIdentifier}'", op.FileIdentifier);
                    if (fileMap.TryGetValue(op.FileIdentifier, out var formFile))
                    {
                        _logger.LogInformation("Found matching file in IFormFileCollection for FileIdentifier: '{FileIdentifier}'. OriginalFileName: {OriginalFileName}", op.FileIdentifier, formFile.FileName);

                        if (formFile.Length == 0) throw new Application.Exceptions.ValidationException(op.FileIdentifier, "File content cannot be empty.");
                        if (formFile.Length > 10 * 1024 * 1024) throw new Application.Exceptions.ValidationException(op.FileIdentifier, "File size cannot exceed 10MB.");

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
                        _logger.LogWarning("File with identifier '{FileIdentifier}' was specified in pageOperationsJson but not found in the uploaded files for chapter {ChapterId}. PageId: {PageId}, PageNumber: {PageNumber}",
                            op.FileIdentifier, chapterId, op.PageId?.ToString() ?? "new", op.PageNumber);
                        // Đây là dòng 309 gây ra lỗi trong log của bạn
                        throw new Application.Exceptions.ValidationException(op.FileIdentifier, $"File with identifier '{op.FileIdentifier}' was specified but not found in the uploaded files.");
                    }
                }
                else
                {
                    _logger.LogInformation("No FileIdentifier specified for operation with PageId: {PageId}, PageNumber: {PageNumber}. This page will not have its image updated/added unless it's an existing page and no image change is intended.",
                       op.PageId?.ToString() ?? "new", op.PageNumber);
                }

                instructions.Add(new PageSyncInstruction
                {
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
            return Ok(result);
        }
    }

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