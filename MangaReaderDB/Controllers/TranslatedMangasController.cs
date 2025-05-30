using Application.Common.DTOs.TranslatedMangas;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.TranslatedMangas.Commands.CreateTranslatedManga;
using Application.Features.TranslatedMangas.Commands.DeleteTranslatedManga;
using Application.Features.TranslatedMangas.Commands.UpdateTranslatedManga;
using Application.Features.TranslatedMangas.Queries.GetTranslatedMangaById;
using Application.Features.TranslatedMangas.Queries.GetTranslatedMangasByManga;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace MangaReaderDB.Controllers
{
    public class TranslatedMangasController : BaseApiController
    {
        private readonly IValidator<CreateTranslatedMangaDto> _createDtoValidator;
        private readonly IValidator<UpdateTranslatedMangaDto> _updateDtoValidator;
        private readonly ILogger<TranslatedMangasController> _logger; // Thêm logger

        public TranslatedMangasController(
            IValidator<CreateTranslatedMangaDto> createDtoValidator,
            IValidator<UpdateTranslatedMangaDto> updateDtoValidator,
            ILogger<TranslatedMangasController> logger) // Inject logger
        {
            _createDtoValidator = createDtoValidator;
            _updateDtoValidator = updateDtoValidator;
            _logger = logger; // Gán logger
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<TranslatedMangaAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] 
        public async Task<IActionResult> CreateTranslatedManga([FromBody] CreateTranslatedMangaDto createDto)
        {
            var validationResult = await _createDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new CreateTranslatedMangaCommand
            {
                MangaId = createDto.MangaId,
                LanguageKey = createDto.LanguageKey,
                Title = createDto.Title,
                Description = createDto.Description
            };
            var translatedMangaId = await Mediator.Send(command);
            var translatedMangaResource = await Mediator.Send(new GetTranslatedMangaByIdQuery { TranslatedMangaId = translatedMangaId });
            
            if(translatedMangaResource == null)
            {
                _logger.LogError($"FATAL: TranslatedManga with ID {translatedMangaId} was not found after creation!");
                 return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetTranslatedMangaById), new { id = translatedMangaId }, translatedMangaResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<TranslatedMangaAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTranslatedMangaById(Guid id)
        {
            var query = new GetTranslatedMangaByIdQuery { TranslatedMangaId = id };
            var translatedMangaResource = await Mediator.Send(query);
            if (translatedMangaResource == null)
            {
                 throw new NotFoundException(nameof(Domain.Entities.TranslatedManga), id);
            }
            return Ok(translatedMangaResource);
        }

        [HttpGet("/mangas/{mangaId:guid}/translations")] 
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<TranslatedMangaAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTranslatedMangasByManga(Guid mangaId, [FromQuery] GetTranslatedMangasByMangaQuery query)
        {
            query.MangaId = mangaId;
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTranslatedManga(Guid id, [FromBody] UpdateTranslatedMangaDto updateDto)
        {
            var validationResult = await _updateDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateTranslatedMangaCommand
            {
                TranslatedMangaId = id,
                LanguageKey = updateDto.LanguageKey,
                Title = updateDto.Title,
                Description = updateDto.Description
            };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTranslatedManga(Guid id)
        {
            var command = new DeleteTranslatedMangaCommand { TranslatedMangaId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 