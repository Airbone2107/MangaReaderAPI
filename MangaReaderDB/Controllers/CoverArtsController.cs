using Application.Common.DTOs;
using Application.Common.DTOs.CoverArts;
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.CoverArts.Commands.DeleteCoverArt;
using Application.Features.CoverArts.Commands.UploadCoverArtImage;
using Application.Features.CoverArts.Queries.GetCoverArtById;
using Application.Features.CoverArts.Queries.GetCoverArtsByManga;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required for .Select on validationResult.Errors
using Microsoft.Extensions.Logging; // Đảm bảo có using này

namespace MangaReaderDB.Controllers
{
    public class CoverArtsController : BaseApiController
    {
        // Validator for CreateCoverArtDto (used in UploadCoverArtImageCommand)
        private readonly IValidator<CreateCoverArtDto> _createCoverArtDtoValidator;
        private readonly ILogger<CoverArtsController> _logger; // Thêm logger

        public CoverArtsController(
            IValidator<CreateCoverArtDto> createCoverArtDtoValidator,
            ILogger<CoverArtsController> logger) // Inject logger
        {
            _createCoverArtDtoValidator = createCoverArtDtoValidator;
            _logger = logger; // Gán logger
        }

        [HttpPost("/mangas/{mangaId:guid}/covers")] // Custom route to associate with manga
        [ProducesResponseType(typeof(ApiResponse<CoverArtDto>), StatusCodes.Status201Created)] // Sửa ProducesResponseType
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadCoverArtImage(Guid mangaId, IFormFile file, [FromForm] string? volume, [FromForm] string? description) // Sử dụng FromForm cho metadata
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

            var metadataDto = new CreateCoverArtDto { MangaId = mangaId, Volume = volume, Description = description };
            var validationResult = await _createCoverArtDtoValidator.ValidateAsync(metadataDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            using var stream = file.OpenReadStream();
            var command = new UploadCoverArtImageCommand
            {
                MangaId = mangaId,
                Volume = volume,
                Description = description,
                ImageStream = stream,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType
            };

            var coverId = await Mediator.Send(command);
            var coverArtDto = await Mediator.Send(new GetCoverArtByIdQuery { CoverId = coverId });

            if (coverArtDto == null)
            {
                _logger.LogError($"FATAL: CoverArt with ID {coverId} was not found after creation! This indicates a critical issue.");
                throw new InvalidOperationException($"Could not retrieve CoverArt with ID {coverId} after creation. This is an unexpected error.");
            }
            return Created(nameof(GetCoverArtById), new { id = coverId }, coverArtDto);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CoverArtDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCoverArtById(Guid id)
        {
            var query = new GetCoverArtByIdQuery { CoverId = id };
            var result = await Mediator.Send(query);
            if (result == null)
            {
                 throw new NotFoundException(nameof(Domain.Entities.CoverArt), id);
            }
            return Ok(result);
        }

        [HttpGet("/mangas/{mangaId:guid}/covers")] // Custom route
        [ProducesResponseType(typeof(ApiCollectionResponse<CoverArtDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCoverArtsByManga(Guid mangaId, [FromQuery] GetCoverArtsByMangaQuery query)
        {
            query.MangaId = mangaId;
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCoverArt(Guid id)
        {
            var command = new DeleteCoverArtCommand { CoverId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 