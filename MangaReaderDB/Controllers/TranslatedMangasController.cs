using Application.Common.DTOs;
using Application.Common.DTOs.TranslatedMangas;
using Application.Features.TranslatedMangas.Commands.CreateTranslatedManga;
using Application.Features.TranslatedMangas.Commands.DeleteTranslatedManga;
using Application.Features.TranslatedMangas.Commands.UpdateTranslatedManga;
using Application.Features.TranslatedMangas.Queries.GetTranslatedMangaById;
using Application.Features.TranslatedMangas.Queries.GetTranslatedMangasByManga;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required for .Select on validationResult.Errors

namespace MangaReaderDB.Controllers
{
    public class TranslatedMangasController : BaseApiController
    {
        private readonly IValidator<CreateTranslatedMangaDto> _createDtoValidator;
        private readonly IValidator<UpdateTranslatedMangaDto> _updateDtoValidator;

        public TranslatedMangasController(
            IValidator<CreateTranslatedMangaDto> createDtoValidator,
            IValidator<UpdateTranslatedMangaDto> updateDtoValidator)
        {
            _createDtoValidator = createDtoValidator;
            _updateDtoValidator = updateDtoValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTranslatedManga([FromBody] CreateTranslatedMangaDto createDto)
        {
            var validationResult = await _createDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }

            var command = new CreateTranslatedMangaCommand
            {
                MangaId = createDto.MangaId,
                LanguageKey = createDto.LanguageKey,
                Title = createDto.Title,
                Description = createDto.Description
            };
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetTranslatedMangaById), new { id }, new { TranslatedMangaId = id });
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TranslatedMangaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TranslatedMangaDto>> GetTranslatedMangaById(Guid id)
        {
            var query = new GetTranslatedMangaByIdQuery { TranslatedMangaId = id };
            var result = await Mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("/api/mangas/{mangaId:guid}/translations")] // Custom route
        [ProducesResponseType(typeof(PagedResult<TranslatedMangaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<TranslatedMangaDto>>> GetTranslatedMangasByManga(Guid mangaId, [FromQuery] GetTranslatedMangasByMangaQuery query)
        {
            query.MangaId = mangaId; // Set MangaId from route
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTranslatedManga(Guid id, [FromBody] UpdateTranslatedMangaDto updateDto)
        {
            var validationResult = await _updateDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTranslatedManga(Guid id)
        {
            var command = new DeleteTranslatedMangaCommand { TranslatedMangaId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 