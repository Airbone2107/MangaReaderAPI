using Application.Common.DTOs.Mangas;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.Mangas.Commands.AddMangaAuthor;
using Application.Features.Mangas.Commands.AddMangaTag;
using Application.Features.Mangas.Commands.CreateManga;
using Application.Features.Mangas.Commands.DeleteManga;
using Application.Features.Mangas.Commands.RemoveMangaAuthor;
using Application.Features.Mangas.Commands.RemoveMangaTag;
using Application.Features.Mangas.Commands.UpdateManga;
using Application.Features.Mangas.Queries.GetMangaById;
using Application.Features.Mangas.Queries.GetMangas;
using Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required for .Select on validationResult.Errors
using Microsoft.Extensions.Logging; // Đảm bảo có using này

namespace MangaReaderDB.Controllers
{
    public class MangasController : BaseApiController
    {
        private readonly IValidator<CreateMangaDto> _createMangaDtoValidator;
        private readonly IValidator<UpdateMangaDto> _updateMangaDtoValidator;
        private readonly ILogger<MangasController> _logger; // Thêm logger
        // Nếu MangaTagInputDto và MangaAuthorInputDto có validator riêng, bạn cũng cần inject chúng.
        // Hiện tại, chúng khá đơn giản và có thể validate trực tiếp trong action nếu cần.

        public MangasController(
            IValidator<CreateMangaDto> createMangaDtoValidator,
            IValidator<UpdateMangaDto> updateMangaDtoValidator,
            ILogger<MangasController> logger) // Inject logger
        {
            _createMangaDtoValidator = createMangaDtoValidator;
            _updateMangaDtoValidator = updateMangaDtoValidator;
            _logger = logger; // Gán logger
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<MangaAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateManga([FromBody] CreateMangaDto createDto)
        {
            var validationResult = await _createMangaDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new CreateMangaCommand
            {
                Title = createDto.Title,
                OriginalLanguage = createDto.OriginalLanguage,
                PublicationDemographic = createDto.PublicationDemographic,
                Status = createDto.Status,
                Year = createDto.Year,
                ContentRating = createDto.ContentRating
            };
            var mangaId = await Mediator.Send(command);
            var mangaResource = await Mediator.Send(new GetMangaByIdQuery { MangaId = mangaId });

            if (mangaResource == null)
            {
                 _logger.LogError($"FATAL: Manga with ID {mangaId} was not found after creation!");
                 return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetMangaById), new { id = mangaId }, mangaResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<MangaAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMangaById(Guid id)
        {
            var query = new GetMangaByIdQuery { MangaId = id };
            var mangaResource = await Mediator.Send(query);
            if (mangaResource == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Manga), id);
            }
            return Ok(mangaResource);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<MangaAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMangas([FromQuery] GetMangasQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateManga(Guid id, [FromBody] UpdateMangaDto updateDto)
        {
            var validationResult = await _updateMangaDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateMangaCommand
            {
                MangaId = id,
                Title = updateDto.Title,
                OriginalLanguage = updateDto.OriginalLanguage,
                PublicationDemographic = updateDto.PublicationDemographic,
                Status = updateDto.Status,
                Year = updateDto.Year,
                ContentRating = updateDto.ContentRating,
                IsLocked = updateDto.IsLocked
            };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteManga(Guid id)
        {
            var command = new DeleteMangaCommand { MangaId = id };
            await Mediator.Send(command);
            return NoContent();
        }

        // --- Manga Tags ---
        [HttpPost("{mangaId:guid}/tags")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddMangaTag(Guid mangaId, [FromBody] MangaTagInputDto input)
        {
            if (input.TagId == Guid.Empty) 
            {
                throw new Application.Exceptions.ValidationException(nameof(input.TagId), "TagId is required.");
            }
            var command = new AddMangaTagCommand { MangaId = mangaId, TagId = input.TagId };
            // Handler sẽ throw NotFoundException nếu mangaId hoặc tagId không tồn tại
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{mangaId:guid}/tags/{tagId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveMangaTag(Guid mangaId, Guid tagId)
        {
            var command = new RemoveMangaTagCommand { MangaId = mangaId, TagId = tagId };
            // Handler sẽ throw NotFoundException nếu mangaId hoặc tagId không tồn tại, hoặc tag không được gán cho manga
            await Mediator.Send(command);
            return NoContent();
        }

        // --- Manga Authors ---
        [HttpPost("{mangaId:guid}/authors")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddMangaAuthor(Guid mangaId, [FromBody] MangaAuthorInputDto input)
        {
            if (input.AuthorId == Guid.Empty)
            {
                 throw new Application.Exceptions.ValidationException(nameof(input.AuthorId), "AuthorId is required.");
            }
            if (!Enum.IsDefined(typeof(MangaStaffRole), input.Role))
            {
                throw new Application.Exceptions.ValidationException(nameof(input.Role), "Invalid Role.");
            }
            var command = new AddMangaAuthorCommand { MangaId = mangaId, AuthorId = input.AuthorId, Role = input.Role };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{mangaId:guid}/authors/{authorId:guid}/role/{role}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveMangaAuthor(Guid mangaId, Guid authorId, MangaStaffRole role)
        {
            var command = new RemoveMangaAuthorCommand { MangaId = mangaId, AuthorId = authorId, Role = role };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 