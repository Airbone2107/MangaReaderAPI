using Application.Common.DTOs;
using Application.Common.DTOs.Chapters;
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
using System.Linq; // Required for .Select on validationResult.Errors

namespace MangaReaderDB.Controllers
{
    public class ChaptersController : BaseApiController
    {
        private readonly IValidator<CreateChapterDto> _createChapterDtoValidator;
        private readonly IValidator<UpdateChapterDto> _updateChapterDtoValidator;
        private readonly IValidator<CreateChapterPageDto> _createChapterPageDtoValidator; 
        private readonly IValidator<UpdateChapterPageDto> _updateChapterPageDtoValidator;


        public ChaptersController(
            IValidator<CreateChapterDto> createChapterDtoValidator,
            IValidator<UpdateChapterDto> updateChapterDtoValidator,
            IValidator<CreateChapterPageDto> createChapterPageDtoValidator,
            IValidator<UpdateChapterPageDto> updateChapterPageDtoValidator)
        {
            _createChapterDtoValidator = createChapterDtoValidator;
            _updateChapterDtoValidator = updateChapterDtoValidator;
            _createChapterPageDtoValidator = createChapterPageDtoValidator;
            _updateChapterPageDtoValidator = updateChapterPageDtoValidator;
        }

        // --- Chapter Endpoints ---
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateChapter([FromBody] CreateChapterDto createDto)
        {
            var validationResult = await _createChapterDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }

            // TODO: Lấy UploadedByUserId từ context người dùng đã xác thực
            // Hiện tại, CreateChapterDto đang nhận UploadedByUserId. Trong thực tế, nó sẽ được lấy từ HttpContext.User
            // Ví dụ: var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            // command.UploadedByUserId = userId;
            // Hoặc Handler sẽ lấy từ ICurrentUserService (nếu bạn implement)

            var command = new CreateChapterCommand
            {
                TranslatedMangaId = createDto.TranslatedMangaId,
                UploadedByUserId = createDto.UploadedByUserId, // Tạm thời lấy từ DTO
                Volume = createDto.Volume,
                ChapterNumber = createDto.ChapterNumber,
                Title = createDto.Title,
                PublishAt = createDto.PublishAt,
                ReadableAt = createDto.ReadableAt
            };
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetChapterById), new { id }, new { ChapterId = id });
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ChapterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ChapterDto>> GetChapterById(Guid id)
        {
            var query = new GetChapterByIdQuery { ChapterId = id };
            var result = await Mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("/api/translatedmangas/{translatedMangaId:guid}/chapters")] // Custom route
        [ProducesResponseType(typeof(PagedResult<ChapterDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<ChapterDto>>> GetChaptersByTranslatedManga(Guid translatedMangaId, [FromQuery] GetChaptersByTranslatedMangaQuery query)
        {
            query.TranslatedMangaId = translatedMangaId;
            var result = await Mediator.Send(query);
            return Ok(result);
        }
        
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChapter(Guid id, [FromBody] UpdateChapterDto updateDto)
        {
            var validationResult = await _updateChapterDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteChapter(Guid id)
        {
            var command = new DeleteChapterCommand { ChapterId = id };
            await Mediator.Send(command);
            return NoContent();
        }

        // --- ChapterPage Endpoints ---
        // Route cho ChapterPage nên tách ra controller riêng (ChapterPagesController) hoặc đặt dưới /api/chapters/{chapterId}/pages
        // Hiện tại để chung cho đơn giản.

        [HttpPost("{chapterId:guid}/pages/entry")] // Tạo metadata cho trang
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateChapterPageEntry(Guid chapterId, [FromBody] CreateChapterPageDto createPageDto)
        {
            var validationResult = await _createChapterPageDtoValidator.ValidateAsync(createPageDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }
            
            var command = new CreateChapterPageEntryCommand 
            { 
                ChapterId = chapterId, 
                PageNumber = createPageDto.PageNumber // CreateChapterPageDto đã có PageNumber
            };
            var pageId = await Mediator.Send(command);
            // Trả về PageId, client sẽ dùng PageId này để upload ảnh.
            // Có thể trả về location của endpoint upload ảnh cho trang này.
            return CreatedAtAction("UploadChapterPageImage", "ChapterPages", new { pageId = pageId }, new { PageId = pageId });
        }

        [HttpGet("{chapterId:guid}/pages")]
        [ProducesResponseType(typeof(PagedResult<ChapterPageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PagedResult<ChapterPageDto>>> GetChapterPages(Guid chapterId, [FromQuery] GetChapterPagesQuery query)
        {
            query.ChapterId = chapterId;
            var result = await Mediator.Send(query);
            // Handler sẽ trả về PagedResult rỗng nếu chapter không tìm thấy hoặc không có page.
            return Ok(result);
        }
    }

    // Nên tạo Controller riêng cho ChapterPages để quản lý tốt hơn
    [Route("api/chapterpages")]
    public class ChapterPagesController : BaseApiController
    {
        private readonly IValidator<UpdateChapterPageDto> _updateChapterPageDtoValidator;

        public ChapterPagesController(IValidator<UpdateChapterPageDto> updateChapterPageDtoValidator)
        {
            _updateChapterPageDtoValidator = updateChapterPageDtoValidator;
        }

        [HttpPost("{pageId:guid}/image")] // Upload ảnh cho một ChapterPage đã có entry
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)] // Trả về PublicId
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadChapterPageImage(Guid pageId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Title = "Validation Failed", Errors = new[] { new { PropertyName = "file", ErrorMessage = "File is required." } } });
            }
             if (file.Length > 5 * 1024 * 1024) // Giới hạn 5MB
            {
                return BadRequest(new { Title = "Validation Failed", Errors = new[] { new { PropertyName = "file", ErrorMessage = "File size cannot exceed 5MB." } } });
            }
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                return BadRequest(new { Title = "Validation Failed", Errors = new[] { new { PropertyName = "file", ErrorMessage = "Invalid file type. Allowed types are: " + string.Join(", ", allowedExtensions) } } });
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
            return Ok(new { PublicId = publicId });
        }

        [HttpPut("{pageId:guid}/details")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChapterPageDetails(Guid pageId, [FromBody] UpdateChapterPageDto updateDto)
        {
            var validationResult = await _updateChapterPageDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteChapterPage(Guid pageId)
        {
            var command = new DeleteChapterPageCommand { PageId = pageId };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 