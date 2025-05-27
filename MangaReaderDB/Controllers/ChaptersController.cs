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
using Application.Features.Chapters.Queries.GetChapterById;
using Application.Features.Chapters.Queries.GetChapterPages;
using Application.Features.Chapters.Queries.GetChaptersByTranslatedManga;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
            
            // Giả sử UploadedByUserId được lấy từ context người dùng đã xác thực
            // Tạm thời để trống hoặc gán một giá trị mặc định nếu cần thiết cho command.
            // int currentUserId = ... ; // Lấy từ HttpContext.User hoặc service
            var command = new CreateChapterCommand
            {
                TranslatedMangaId = createDto.TranslatedMangaId,
                UploadedByUserId = createDto.UploadedByUserId, // Cần thay thế bằng UserId từ context
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)] // Payload là { "pageId": "guid" }
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
            
            // Trả về 201 Created với Location header trỏ đến action upload image cho page này
            // Action "UploadChapterPageImage" nằm trong "ChapterPagesController"
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)] // Payload là { "publicId": "string" }
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] // Cho lỗi file
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Cho pageId
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
            var publicId = await Mediator.Send(command); // Handler sẽ throw NotFoundException nếu pageId không tồn tại
            
            var responsePayload = new { PublicId = publicId };
            return Ok(responsePayload); // BaseApiController.Ok sẽ bọc trong ApiResponse<object>
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