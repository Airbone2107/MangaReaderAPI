// MangaReaderDB/Controllers/AuthorsController.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Authors;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses; // Cần cho ApiResponse, ApiCollectionResponse, ApiErrorResponse
using Application.Exceptions; // Cần cho NotFoundException, ValidationException
using Application.Features.Authors.Commands.CreateAuthor;
using Application.Features.Authors.Commands.DeleteAuthor;
using Application.Features.Authors.Commands.UpdateAuthor;
using Application.Features.Authors.Queries.GetAuthorById;
using Application.Features.Authors.Queries.GetAuthors;
using FluentValidation;
using MediatR; // For Unit
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq; // Required for .Select on validationResult.Errors

namespace MangaReaderDB.Controllers
{
    public class AuthorsController : BaseApiController
    {
        private readonly IValidator<CreateAuthorDto> _createAuthorDtoValidator;
        private readonly IValidator<UpdateAuthorDto> _updateAuthorDtoValidator;
        private readonly ILogger<AuthorsController> _logger;

        public AuthorsController(
            IValidator<CreateAuthorDto> createAuthorDtoValidator,
            IValidator<UpdateAuthorDto> updateAuthorDtoValidator,
            ILogger<AuthorsController> logger)
        {
            _createAuthorDtoValidator = createAuthorDtoValidator;
            _updateAuthorDtoValidator = updateAuthorDtoValidator;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<AuthorAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDto createAuthorDto)
        {
            var validationResult = await _createAuthorDtoValidator.ValidateAsync(createAuthorDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new CreateAuthorCommand 
            { 
                Name = createAuthorDto.Name, 
                Biography = createAuthorDto.Biography 
            };
            var authorId = await Mediator.Send(command);
            
            var authorResource = await Mediator.Send(new GetAuthorByIdQuery { AuthorId = authorId });
            if (authorResource == null)
            {
                 _logger.LogError($"FATAL: Author with ID {authorId} was not found after creation!");
                 return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetAuthorById), new { id = authorId }, authorResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<AuthorAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAuthorById(Guid id)
        {
            var query = new GetAuthorByIdQuery { AuthorId = id };
            var authorResource = await Mediator.Send(query);
            if (authorResource == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Author), id);
            }
            return Ok(authorResource);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<AuthorAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuthors([FromQuery] GetAuthorsQuery query)
        {
            var result = await Mediator.Send(query); // QueryHandler trả về PagedResult<ResourceObject<AuthorAttributesDto>>
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAuthor(Guid id, [FromBody] UpdateAuthorDto updateAuthorDto)
        {
            var validationResult = await _updateAuthorDtoValidator.ValidateAsync(updateAuthorDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateAuthorCommand
            {
                AuthorId = id,
                Name = updateAuthorDto.Name,
                Biography = updateAuthorDto.Biography
            };
            await Mediator.Send(command); 
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAuthor(Guid id)
        {
            var command = new DeleteAuthorCommand { AuthorId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 