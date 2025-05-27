// MangaReaderDB/Controllers/TagsController.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Tags;
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.Tags.Commands.CreateTag;
using Application.Features.Tags.Commands.DeleteTag;
using Application.Features.Tags.Commands.UpdateTag;
using Application.Features.Tags.Queries.GetTagById;
using Application.Features.Tags.Queries.GetTags;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required for .Select on validationResult.Errors
using Microsoft.Extensions.Logging; // Đảm bảo có using này

namespace MangaReaderDB.Controllers
{
    public class TagsController : BaseApiController
    {
        private readonly IValidator<CreateTagDto> _createTagDtoValidator;
        private readonly IValidator<UpdateTagDto> _updateTagDtoValidator;
        private readonly ILogger<TagsController> _logger; // Thêm logger

        public TagsController(
            IValidator<CreateTagDto> createTagDtoValidator,
            IValidator<UpdateTagDto> updateTagDtoValidator,
            ILogger<TagsController> logger) // Inject logger
        {
            _createTagDtoValidator = createTagDtoValidator;
            _updateTagDtoValidator = updateTagDtoValidator;
            _logger = logger; // Gán logger
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TagDto>), StatusCodes.Status201Created)] // Sửa ProducesResponseType
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] 
        public async Task<IActionResult> CreateTag([FromBody] CreateTagDto createDto)
        {
            var validationResult = await _createTagDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new CreateTagCommand { Name = createDto.Name, TagGroupId = createDto.TagGroupId };
            var id = await Mediator.Send(command);
            var tagDto = await Mediator.Send(new GetTagByIdQuery { TagId = id });
            
            if (tagDto == null)
            {
                _logger.LogError($"FATAL: Tag with ID {id} was not found after creation! This indicates a critical issue.");
                throw new InvalidOperationException($"Could not retrieve Tag with ID {id} after creation. This is an unexpected error.");
            }
            return Created(nameof(GetTagById), new { id }, tagDto);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TagDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTagById(Guid id)
        {
            var query = new GetTagByIdQuery { TagId = id };
            var result = await Mediator.Send(query);
            if (result == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Tag), id);
            }
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiCollectionResponse<TagDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTags([FromQuery] GetTagsQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTag(Guid id, [FromBody] UpdateTagDto updateDto)
        {
            var validationResult = await _updateTagDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateTagCommand { TagId = id, Name = updateDto.Name, TagGroupId = updateDto.TagGroupId };
            // Handler sẽ throw NotFoundException hoặc ValidationException
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTag(Guid id)
        {
            var command = new DeleteTagCommand { TagId = id };
            // Handler sẽ throw NotFoundException
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 