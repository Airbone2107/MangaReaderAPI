using Application.Common.DTOs;
using Application.Common.DTOs.CoverArts;
using Application.Features.CoverArts.Commands.DeleteCoverArt;
using Application.Features.CoverArts.Commands.UploadCoverArtImage;
using Application.Features.CoverArts.Queries.GetCoverArtById;
using Application.Features.CoverArts.Queries.GetCoverArtsByManga;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required for .Select on validationResult.Errors

namespace MangaReaderDB.Controllers
{
    public class CoverArtsController : BaseApiController
    {
        // Validator for CreateCoverArtDto (used in UploadCoverArtImageCommand)
        private readonly IValidator<CreateCoverArtDto> _createCoverArtDtoValidator;

        public CoverArtsController(IValidator<CreateCoverArtDto> createCoverArtDtoValidator)
        {
            _createCoverArtDtoValidator = createCoverArtDtoValidator;
        }

        [HttpPost("/api/mangas/{mangaId:guid}/covers")] // Custom route to associate with manga
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadCoverArtImage(Guid mangaId, IFormFile file, [FromForm] string? volume, [FromForm] string? description) // Sử dụng FromForm cho metadata
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


            // Validate metadata (volume, description) bằng CreateCoverArtDtoValidator
            var metadataDto = new CreateCoverArtDto { MangaId = mangaId, Volume = volume, Description = description };
            var validationResult = await _createCoverArtDtoValidator.ValidateAsync(metadataDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }

            using var stream = file.OpenReadStream();
            var command = new UploadCoverArtImageCommand
            {
                MangaId = mangaId,
                Volume = volume, // metadataDto.Volume
                Description = description, // metadataDto.Description
                ImageStream = stream,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType
            };

            var coverId = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetCoverArtById), new { id = coverId }, new { CoverId = coverId });
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CoverArtDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CoverArtDto>> GetCoverArtById(Guid id)
        {
            var query = new GetCoverArtByIdQuery { CoverId = id };
            var result = await Mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("/api/mangas/{mangaId:guid}/covers")] // Custom route
        [ProducesResponseType(typeof(PagedResult<CoverArtDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CoverArtDto>>> GetCoverArtsByManga(Guid mangaId, [FromQuery] GetCoverArtsByMangaQuery query)
        {
            query.MangaId = mangaId;
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCoverArt(Guid id)
        {
            var command = new DeleteCoverArtCommand { CoverId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 