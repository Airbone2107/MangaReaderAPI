using Application.Common.DTOs.Mangas;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.Mangas.Commands.CreateManga;
using Application.Features.Mangas.Commands.DeleteManga;
using Application.Features.Mangas.Commands.UpdateManga;
using Application.Features.Mangas.Queries.GetMangaById;
using Application.Features.Mangas.Queries.GetMangas;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace MangaReaderDB.Controllers
{
    public class MangasController : BaseApiController
    {
        private readonly IValidator<CreateMangaDto> _createMangaDtoValidator;
        private readonly IValidator<UpdateMangaDto> _updateMangaDtoValidator;
        private readonly ILogger<MangasController> _logger; // Thêm logger

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
                ContentRating = createDto.ContentRating,
                TagIds = createDto.TagIds,
                Authors = createDto.Authors
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
                IsLocked = updateDto.IsLocked,
                TagIds = updateDto.TagIds,
                Authors = updateDto.Authors
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
    }
} 