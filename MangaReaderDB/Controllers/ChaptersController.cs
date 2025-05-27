using Application.Common.DTOs.Chapters;
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
        [ProducesResponseType(typeof(ApiResponse<ChapterDto>), StatusCodes.Status201Created)]
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
            var id = await Mediator.Send(command);
            var chapterDto = await Mediator.Send(new GetChapterByIdQuery { ChapterId = id });

            if (chapterDto == null)
            {
                _logger.LogError($"FATAL: Chapter with ID {id} was not found after creation! This indicates a critical issue.");
                throw new InvalidOperationException($"Could not retrieve Chapter with ID {id} after creation. This is an unexpected error.");
            }
            return Created(nameof(GetChapterById), new { id }, chapterDto);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ChapterDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChapterById(Guid id)
        {
            var query = new GetChapterByIdQuery { ChapterId = id };
            var result = await Mediator.Send(query);
            if (result == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Chapter), id);
            }
            return Ok(result);
        }

        [HttpGet("/translatedmangas/{translatedMangaId:guid}/chapters")] 
        [ProducesResponseType(typeof(ApiCollectionResponse<ChapterDto>), StatusCodes.Status200OK)]
        // GetChaptersByTranslatedMangaQueryHandler sẽ trả về PagedResult rỗng nếu translatedMangaId không tồn tại, không throw NotFound.
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
            // UpdateChapterCommandHandler sẽ throw NotFoundException
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteChapter(Guid id)
        {
            var command = new DeleteChapterCommand { ChapterId = id };
            // DeleteChapterCommandHandler sẽ throw NotFoundException
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{chapterId:guid}/pages/entry")] 
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Cho chapterId
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

            // Payload trả về là một object chứa PageId
            var responsePayload = new { PageId = pageId };

            // Sử dụng CreatedAtAction của ControllerBase để có Location header đúng
            // Action "UploadChapterPageImage" nằm trong "ChapterPagesController"
            return CreatedAtAction(
                actionName: nameof(ChapterPagesController.UploadChapterPageImage), // Tên action trong ChapterPagesController
                controllerName: "ChapterPages", // Tên controller (ASP.NET Core sẽ tự động bỏ suffix "Controller")
                routeValues: new { pageId }, // Route values cho action UploadChapterPageImage
                value: new ApiResponse<object>(responsePayload) // Bọc payload trong ApiResponse, kiểu dữ liệu là object
            );
        }

        [HttpGet("{chapterId:guid}/pages")]
        [ProducesResponseType(typeof(ApiCollectionResponse<ChapterPageDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChapterPages(Guid chapterId, [FromQuery] GetChapterPagesQuery query)
        {
            query.ChapterId = chapterId;
            var result = await Mediator.Send(query);
            // Handler trả về PagedResult rỗng nếu chapter không có page hoặc không tồn tại
            return Ok(result);
        }
    }

    [Route("chapterpages")]
    public class ChapterPagesController : BaseApiController
    {
        private readonly IValidator<UpdateChapterPageDto> _updateChapterPageDtoValidator;

        public ChapterPagesController(IValidator<UpdateChapterPageDto> updateChapterPageDtoValidator)
        {
            _updateChapterPageDtoValidator = updateChapterPageDtoValidator;
        }

        [HttpPost("{pageId:guid}/image")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)] // Trả về PublicId
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
            // UploadChapterPageImageCommandHandler sẽ throw NotFoundException
            var publicId = await Mediator.Send(command);
            return Ok(new { PublicId = publicId });
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
            // UpdateChapterPageDetailsCommandHandler sẽ throw NotFoundException hoặc ValidationException
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{pageId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteChapterPage(Guid pageId)
        {
            var command = new DeleteChapterPageCommand { PageId = pageId };
            // DeleteChapterPageCommandHandler sẽ throw NotFoundException
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 