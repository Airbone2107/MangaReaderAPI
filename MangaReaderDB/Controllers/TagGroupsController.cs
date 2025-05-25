using Application.Common.DTOs;
using Application.Common.DTOs.TagGroups;
using Application.Features.TagGroups.Commands.CreateTagGroup;
using Application.Features.TagGroups.Commands.DeleteTagGroup;
using Application.Features.TagGroups.Commands.UpdateTagGroup;
using Application.Features.TagGroups.Queries.GetTagGroupById;
using Application.Features.TagGroups.Queries.GetTagGroups;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required for .Select on validationResult.Errors

namespace MangaReaderDB.Controllers
{
    public class TagGroupsController : BaseApiController
    {
        private readonly IValidator<CreateTagGroupDto> _createTagGroupDtoValidator;
        private readonly IValidator<UpdateTagGroupDto> _updateTagGroupDtoValidator;

        public TagGroupsController(
            IValidator<CreateTagGroupDto> createTagGroupDtoValidator,
            IValidator<UpdateTagGroupDto> updateTagGroupDtoValidator)
        {
            _createTagGroupDtoValidator = createTagGroupDtoValidator;
            _updateTagGroupDtoValidator = updateTagGroupDtoValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTagGroup([FromBody] CreateTagGroupDto createDto)
        {
            var validationResult = await _createTagGroupDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }

            var command = new CreateTagGroupCommand { Name = createDto.Name };
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetTagGroupById), new { id }, new { TagGroupId = id });
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TagGroupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TagGroupDto>> GetTagGroupById(Guid id/*, [FromQuery] bool includeTags = false*/)
        {
            var query = new GetTagGroupByIdQuery { TagGroupId = id /*, IncludeTags = includeTags*/ };
            var result = await Mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<TagGroupDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<TagGroupDto>>> GetTagGroups([FromQuery] GetTagGroupsQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTagGroup(Guid id, [FromBody] UpdateTagGroupDto updateDto)
        {
            var validationResult = await _updateTagGroupDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }

            var command = new UpdateTagGroupCommand { TagGroupId = id, Name = updateDto.Name };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Cho trường hợp không xóa được do còn tag
        public async Task<IActionResult> DeleteTagGroup(Guid id)
        {
            try
            {
                var command = new DeleteTagGroupCommand { TagGroupId = id };
                await Mediator.Send(command);
                return NoContent();
            }
            catch (Application.Exceptions.DeleteFailureException ex)
            {
                return BadRequest(new { Title = "Delete Failed", Errors = new[] { new { PropertyName = "TagGroup", ErrorMessage = ex.Message } } });
            }
        }
    }
} 