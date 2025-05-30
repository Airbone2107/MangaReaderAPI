using Application.Common.DTOs.TagGroups;
using Application.Common.Models; // For ResourceObject
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.TagGroups.Commands.CreateTagGroup;
using Application.Features.TagGroups.Commands.DeleteTagGroup;
using Application.Features.TagGroups.Commands.UpdateTagGroup;
using Application.Features.TagGroups.Queries.GetTagGroupById;
using Application.Features.TagGroups.Queries.GetTagGroups;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace MangaReaderDB.Controllers
{
    public class TagGroupsController : BaseApiController
    {
        private readonly IValidator<CreateTagGroupDto> _createTagGroupDtoValidator;
        private readonly IValidator<UpdateTagGroupDto> _updateTagGroupDtoValidator;
        private readonly ILogger<TagGroupsController> _logger; // Thêm logger

        public TagGroupsController(
            IValidator<CreateTagGroupDto> createTagGroupDtoValidator,
            IValidator<UpdateTagGroupDto> updateTagGroupDtoValidator,
            ILogger<TagGroupsController> logger) // Inject logger
        {
            _createTagGroupDtoValidator = createTagGroupDtoValidator;
            _updateTagGroupDtoValidator = updateTagGroupDtoValidator;
            _logger = logger; // Gán logger
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<TagGroupAttributesDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTagGroup([FromBody] CreateTagGroupDto createDto)
        {
            var validationResult = await _createTagGroupDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new CreateTagGroupCommand { Name = createDto.Name };
            var tagGroupId = await Mediator.Send(command);
            var tagGroupResource = await Mediator.Send(new GetTagGroupByIdQuery { TagGroupId = tagGroupId });

            if (tagGroupResource == null)
            {
                _logger.LogError($"FATAL: TagGroup with ID {tagGroupId} was not found after creation!");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiErrorResponse(new ApiError(500, "Creation Error", "Failed to retrieve resource after creation.")));
            }
            return Created(nameof(GetTagGroupById), new { id = tagGroupId }, tagGroupResource);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResourceObject<TagGroupAttributesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTagGroupById(Guid id)
        {
            var query = new GetTagGroupByIdQuery { TagGroupId = id };
            var tagGroupResource = await Mediator.Send(query);
            if (tagGroupResource == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.TagGroup), id);
            }
            return Ok(tagGroupResource);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiCollectionResponse<ResourceObject<TagGroupAttributesDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTagGroups([FromQuery] GetTagGroupsQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTagGroup(Guid id, [FromBody] UpdateTagGroupDto updateDto)
        {
            var validationResult = await _updateTagGroupDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new UpdateTagGroupCommand { TagGroupId = id, Name = updateDto.Name };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] 
        public async Task<IActionResult> DeleteTagGroup(Guid id)
        {
            var command = new DeleteTagGroupCommand { TagGroupId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 