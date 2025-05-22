using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Tags.Commands.UpdateTag
{
    public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateTagCommandHandler> _logger;

        public UpdateTagCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateTagCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
        {
            var tagToUpdate = await _unitOfWork.TagRepository.GetByIdAsync(request.TagId);
            if (tagToUpdate == null)
            {
                throw new NotFoundException(nameof(Tag), request.TagId);
            }

            // Kiểm tra TagGroup mới có tồn tại không
            if (tagToUpdate.TagGroupId != request.TagGroupId)
            {
                var newTagGroup = await _unitOfWork.TagGroupRepository.GetByIdAsync(request.TagGroupId);
                if (newTagGroup == null)
                {
                    throw new NotFoundException(nameof(TagGroup), request.TagGroupId, "New TagGroup for Tag update not found.");
                }
            }

            // Kiểm tra nếu tên hoặc TagGroup thay đổi, có bị trùng với tag khác không
            if (!string.Equals(tagToUpdate.Name, request.Name, StringComparison.OrdinalIgnoreCase) || tagToUpdate.TagGroupId != request.TagGroupId)
            {
                var existingTagWithNewProps = await _unitOfWork.TagRepository.GetTagByNameAndGroupAsync(request.Name, request.TagGroupId);
                if (existingTagWithNewProps != null && existingTagWithNewProps.TagId != request.TagId)
                {
                    var tagGroupName = (await _unitOfWork.TagGroupRepository.GetByIdAsync(request.TagGroupId))?.Name ?? request.TagGroupId.ToString();
                    _logger.LogWarning("Another Tag with name '{TagName}' already exists in TagGroup '{TagGroupName}'.", request.Name, tagGroupName);
                    throw new Exceptions.ValidationException($"Another Tag with name '{request.Name}' already exists in TagGroup '{tagGroupName}'.");
                }
            }

            _mapper.Map(request, tagToUpdate);

            await _unitOfWork.TagRepository.UpdateAsync(tagToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tag {TagId} updated successfully.", request.TagId);
            return Unit.Value;
        }
    }
} 