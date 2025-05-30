// MangaReaderDB/Controllers/TagsController.cs
using Application.Common.DTOs.Tags;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.Tags.Commands.CreateTag;
using Application.Features.Tags.Commands.DeleteTag;
using Application.Features.Tags.Commands.UpdateTag;
using Application.Features.Tags.Queries.GetTagById;
using Application.Features.Tags.Queries.GetTags;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

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
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<TagAttributesDto>>), StatusCodes.Status201Created)]
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
            var tagId = await Mediator.Send(command);
            var tagResource = await Mediator.Send(new GetTagByIdQuery { TagId = tagId });
            
            if (tagResource == null)
            {
                _logger.LogError($"FATAL: Tag with ID {tagId} was not found after creation!");
                 return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetTagById), new { id = tagId }, tagResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<TagAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTagById(Guid id)
        {
            var query = new GetTagByIdQuery { TagId = id };
            var tagResource = await Mediator.Send(query);
            if (tagResource == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Tag), id);
            }
            return Ok(tagResource);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<TagAttributesDto>>), StatusCodes.Status200OK)]
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
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTag(Guid id)
        {
            var command = new DeleteTagCommand { TagId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 