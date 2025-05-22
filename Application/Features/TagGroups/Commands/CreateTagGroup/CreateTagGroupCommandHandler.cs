using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TagGroups.Commands.CreateTagGroup
{
    public class CreateTagGroupCommandHandler : IRequestHandler<CreateTagGroupCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateTagGroupCommandHandler> _logger;

        public CreateTagGroupCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateTagGroupCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateTagGroupCommand request, CancellationToken cancellationToken)
        {
            var existingTagGroup = await _unitOfWork.TagGroupRepository.GetTagGroupByNameAsync(request.Name);
            if (existingTagGroup != null)
            {
                _logger.LogWarning("TagGroup with name '{TagGroupName}' already exists.", request.Name);
                throw new Exceptions.ValidationException($"TagGroup with name '{request.Name}' already exists.");
            }

            var tagGroup = _mapper.Map<TagGroup>(request);
            // TagGroupId sẽ tự sinh

            await _unitOfWork.TagGroupRepository.AddAsync(tagGroup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TagGroup {TagGroupId} created successfully.", tagGroup.TagGroupId);
            return tagGroup.TagGroupId;
        }
    }
} 