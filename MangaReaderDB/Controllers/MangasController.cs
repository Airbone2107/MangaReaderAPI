using Application.Common.DTOs;
using Application.Common.DTOs.Mangas;
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

namespace MangaReaderDB.Controllers
{
    public class MangasController : BaseApiController
    {
        private readonly IValidator<CreateMangaDto> _createMangaDtoValidator;
        private readonly IValidator<UpdateMangaDto> _updateMangaDtoValidator;
        // Nếu MangaTagInputDto và MangaAuthorInputDto có validator riêng, bạn cũng cần inject chúng.
        // Hiện tại, chúng khá đơn giản và có thể validate trực tiếp trong action nếu cần.

        public MangasController(
            IValidator<CreateMangaDto> createMangaDtoValidator,
            IValidator<UpdateMangaDto> updateMangaDtoValidator)
        {
            _createMangaDtoValidator = createMangaDtoValidator;
            _updateMangaDtoValidator = updateMangaDtoValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateManga([FromBody] CreateMangaDto createDto)
        {
            var validationResult = await _createMangaDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }

            // Map CreateMangaDto to CreateMangaCommand
            var command = new CreateMangaCommand
            {
                Title = createDto.Title,
                OriginalLanguage = createDto.OriginalLanguage,
                PublicationDemographic = createDto.PublicationDemographic,
                Status = createDto.Status,
                Year = createDto.Year,
                ContentRating = createDto.ContentRating
            };
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetMangaById), new { id }, new { MangaId = id });
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(MangaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MangaDto>> GetMangaById(Guid id)
        {
            var query = new GetMangaByIdQuery { MangaId = id };
            var result = await Mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<MangaDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<MangaDto>>> GetMangas([FromQuery] GetMangasQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateManga(Guid id, [FromBody] UpdateMangaDto updateDto)
        {
            var validationResult = await _updateMangaDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteManga(Guid id)
        {
            var command = new DeleteMangaCommand { MangaId = id };
            await Mediator.Send(command);
            return NoContent();
        }

        // --- Manga Tags ---
        [HttpPost("{mangaId:guid}/tags")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddMangaTag(Guid mangaId, [FromBody] MangaTagInputDto input)
        {
            if (input.TagId == Guid.Empty) 
            {
                return BadRequest(new { Title = "Validation Failed", Errors = new[] { new { PropertyName = nameof(input.TagId), ErrorMessage = "TagId is required." } } });
            }
            var command = new AddMangaTagCommand { MangaId = mangaId, TagId = input.TagId };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{mangaId:guid}/tags/{tagId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveMangaTag(Guid mangaId, Guid tagId)
        {
            var command = new RemoveMangaTagCommand { MangaId = mangaId, TagId = tagId };
            await Mediator.Send(command);
            return NoContent();
        }

        // --- Manga Authors ---
        [HttpPost("{mangaId:guid}/authors")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddMangaAuthor(Guid mangaId, [FromBody] MangaAuthorInputDto input)
        {
            if (input.AuthorId == Guid.Empty)
            {
                 return BadRequest(new { Title = "Validation Failed", Errors = new[] { new { PropertyName = nameof(input.AuthorId), ErrorMessage = "AuthorId is required." } } });
            }
            // Validate Role enum if necessary
            if (!Enum.IsDefined(typeof(MangaStaffRole), input.Role))
            {
                return BadRequest(new { Title = "Validation Failed", Errors = new[] { new { PropertyName = nameof(input.Role), ErrorMessage = "Invalid Role." } } });
            }
            var command = new AddMangaAuthorCommand { MangaId = mangaId, AuthorId = input.AuthorId, Role = input.Role };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{mangaId:guid}/authors/{authorId:guid}/role/{role}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveMangaAuthor(Guid mangaId, Guid authorId, MangaStaffRole role)
        {
            var command = new RemoveMangaAuthorCommand { MangaId = mangaId, AuthorId = authorId, Role = role };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 