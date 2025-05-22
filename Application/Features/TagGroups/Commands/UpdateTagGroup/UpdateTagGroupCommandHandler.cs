using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TagGroups.Commands.UpdateTagGroup
{
    public class UpdateTagGroupCommandHandler : IRequestHandler<UpdateTagGroupCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateTagGroupCommandHandler> _logger;

        public UpdateTagGroupCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateTagGroupCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateTagGroupCommand request, CancellationToken cancellationToken)
        {
            var tagGroupToUpdate = await _unitOfWork.TagGroupRepository.GetByIdAsync(request.TagGroupId);
            if (tagGroupToUpdate == null)
            {
                throw new NotFoundException(nameof(TagGroup), request.TagGroupId);
            }

            if (!string.Equals(tagGroupToUpdate.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            {
                var existingTagGroupWithNewName = await _unitOfWork.TagGroupRepository.GetTagGroupByNameAsync(request.Name);
                if (existingTagGroupWithNewName != null && existingTagGroupWithNewName.TagGroupId != request.TagGroupId)
                {
                    _logger.LogWarning("Another TagGroup with name '{TagGroupName}' already exists.", request.Name);
                    throw new Exceptions.ValidationException($"Another TagGroup with name '{request.Name}' already exists.");
                }
            }

            _mapper.Map(request, tagGroupToUpdate);

            await _unitOfWork.TagGroupRepository.UpdateAsync(tagGroupToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TagGroup {TagGroupId} updated successfully.", request.TagGroupId);
            return Unit.Value;
        }
    }
} 