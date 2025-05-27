// MangaReaderDB/Controllers/AuthorsController.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Authors;
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
using System.Linq; // Required for .Select on validationResult.Errors

namespace MangaReaderDB.Controllers
{
    public class AuthorsController : BaseApiController
    {
        private readonly IValidator<CreateAuthorDto> _createAuthorDtoValidator;
        private readonly IValidator<UpdateAuthorDto> _updateAuthorDtoValidator;

        public AuthorsController(
            IValidator<CreateAuthorDto> createAuthorDtoValidator,
            IValidator<UpdateAuthorDto> updateAuthorDtoValidator)
        {
            _createAuthorDtoValidator = createAuthorDtoValidator;
            _updateAuthorDtoValidator = updateAuthorDtoValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)] // Trả về object chứa AuthorId
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAuthor([FromBody] CreateAuthorDto createAuthorDto)
        {
            var validationResult = await _createAuthorDtoValidator.ValidateAsync(createAuthorDto);
            if (!validationResult.IsValid)
            {
                // Để ExceptionMiddleware xử lý
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new CreateAuthorCommand 
            { 
                Name = createAuthorDto.Name, 
                Biography = createAuthorDto.Biography 
            };
            var authorId = await Mediator.Send(command);
            
            // Lấy thông tin author vừa tạo để trả về (hoặc chỉ trả về ID nếu quy ước API cho phép)
            var authorDto = await Mediator.Send(new GetAuthorByIdQuery { AuthorId = authorId });
            if (authorDto == null) // Trường hợp hiếm, nhưng để đảm bảo
            {
                 return Created(nameof(GetAuthorById), new { id = authorId }, new { AuthorId = authorId });
            }
            return Created(nameof(GetAuthorById), new { id = authorId }, authorDto);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AuthorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAuthorById(Guid id)
        {
            var query = new GetAuthorByIdQuery { AuthorId = id };
            var author = await Mediator.Send(query);
            if (author == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Author), id);
            }
            return Ok(author);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiCollectionResponse<AuthorDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuthors([FromQuery] GetAuthorsQuery query)
        {
            var result = await Mediator.Send(query);
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
            // UpdateAuthorCommandHandler sẽ throw NotFoundException nếu không tìm thấy
            await Mediator.Send(command); 
            return NoContent(); // Hoặc return OkResponseForAction(); nếu muốn có body {"result":"ok"}
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAuthor(Guid id)
        {
            var command = new DeleteAuthorCommand { AuthorId = id };
            // DeleteAuthorCommandHandler sẽ throw NotFoundException nếu không tìm thấy
            await Mediator.Send(command);
            return NoContent(); // Hoặc return OkResponseForAction();
        }
    }
} 