using Application.Common.DTOs;
using Application.Common.DTOs.TagGroups;
using Application.Common.Responses;
using Application.Exceptions;
using Application.Features.TagGroups.Commands.CreateTagGroup;
using Application.Features.TagGroups.Commands.DeleteTagGroup;
using Application.Features.TagGroups.Commands.UpdateTagGroup;
using Application.Features.TagGroups.Queries.GetTagGroupById;
using Application.Features.TagGroups.Queries.GetTagGroups;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Required for .Select on validationResult.Errors
using Microsoft.Extensions.Logging; // Đảm bảo có using này

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
        [ProducesResponseType(typeof(ApiResponse<TagGroupDto>), StatusCodes.Status201Created)] // Sửa ProducesResponseType để khớp DTO
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTagGroup([FromBody] CreateTagGroupDto createDto)
        {
            var validationResult = await _createTagGroupDtoValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                throw new Application.Exceptions.ValidationException(validationResult.Errors);
            }

            var command = new CreateTagGroupCommand { Name = createDto.Name };
            var id = await Mediator.Send(command);
            var tagGroupDto = await Mediator.Send(new GetTagGroupByIdQuery { TagGroupId = id });

            if (tagGroupDto == null)
            {
                _logger.LogError($"FATAL: TagGroup with ID {id} was not found after creation. This indicates a critical issue.");
                throw new InvalidOperationException($"Could not retrieve TagGroup with ID {id} after creation. This is an unexpected error.");
            }
            // Phương thức Created<T> từ BaseApiController sẽ tự động bọc tagGroupDto trong ApiResponse<TagGroupDto>
            return Created(nameof(GetTagGroupById), new { id }, tagGroupDto);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<TagGroupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTagGroupById(Guid id)
        {
            var query = new GetTagGroupByIdQuery { TagGroupId = id };
            var result = await Mediator.Send(query);
            if (result == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.TagGroup), id);
            }
            return Ok(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiCollectionResponse<TagGroupDto>), StatusCodes.Status200OK)]
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
            // Handler sẽ throw NotFoundException hoặc ValidationException (tên trùng)
            await Mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] // Cho trường hợp không xóa được do còn tag
        public async Task<IActionResult> DeleteTagGroup(Guid id)
        {
            var command = new DeleteTagGroupCommand { TagGroupId = id };
            // Handler sẽ throw NotFoundException hoặc DeleteFailureException
            await Mediator.Send(command);
            return NoContent();
        }
    }
} 