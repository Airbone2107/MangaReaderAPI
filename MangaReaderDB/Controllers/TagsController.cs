// MangaReaderDB/Controllers/TagsController.cs
using Application.Common.DTOs;
using Application.Common.DTOs.Tags;
using Application.Features.Tags.Commands.CreateTag;
using Application.Features.Tags.Commands.DeleteTag;
using Application.Features.Tags.Commands.UpdateTag;
using Application.Features.Tags.Queries.GetTagById;
using Application.Features.Tags.Queries.GetTags;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required for .Select on validationResult.Errors

namespace MangaReaderDB.Controllers
{
    public class TagsController : BaseApiController
    {
        private readonly IValidator<CreateTagDto> _createTagDtoValidator;
        private readonly IValidator<UpdateTagDto> _updateTagDtoValidator;

        public TagsController(
            IValidator<CreateTagDto> createTagDtoValidator,
            IValidator<UpdateTagDto> updateTagDtoValidator)
        {
            _createTagDtoValidator = createTagDtoValidator;
            _updateTagDtoValidator = updateTagDtoValidator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagDto createDto)
        {
            var validationResult = await _createTagDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }

            var command = new CreateTagCommand { Name = createDto.Name, TagGroupId = createDto.TagGroupId };
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetTagById), new { id }, new { TagId = id });
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TagDto>> GetTagById(Guid id)
        {
            var query = new GetTagByIdQuery { TagId = id };
            var result = await Mediator.Send(query);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<TagDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<TagDto>>> GetTags([FromQuery] GetTagsQuery query)
        {
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTag(Guid id, [FromBody] UpdateTagDto updateDto)
        {
            var validationResult = await _updateTagDtoValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(new { Title = "Validation Failed", Errors = errors });
            }

            var command = new UpdateTagCommand { TagId = id, Name = updateDto.Name, TagGroupId = updateDto.TagGroupId };
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTag(Guid id)
        {
            var command = new DeleteTagCommand { TagId = id };
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 