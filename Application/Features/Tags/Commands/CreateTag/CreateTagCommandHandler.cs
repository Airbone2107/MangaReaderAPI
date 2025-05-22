using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Tags.Commands.CreateTag
{
    public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateTagCommandHandler> _logger;

        public CreateTagCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateTagCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateTagCommand request, CancellationToken cancellationToken)
        {
            var tagGroup = await _unitOfWork.TagGroupRepository.GetByIdAsync(request.TagGroupId);
            if (tagGroup == null)
            {
                throw new NotFoundException(nameof(TagGroup), request.TagGroupId);
            }

            // Kiểm tra xem Tag đã tồn tại trong TagGroup này chưa
            var existingTag = await _unitOfWork.TagRepository.GetTagByNameAndGroupAsync(request.Name, request.TagGroupId);
            if (existingTag != null)
            {
                _logger.LogWarning("Tag with name '{TagName}' already exists in TagGroup {TagGroupId}.", request.Name, request.TagGroupId);
                throw new Exceptions.ValidationException($"Tag with name '{request.Name}' already exists in TagGroup '{tagGroup.Name}'.");
            }

            var tag = _mapper.Map<Tag>(request);
            // TagId sẽ tự sinh

            await _unitOfWork.TagRepository.AddAsync(tag);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tag {TagId} created successfully in TagGroup {TagGroupId}.", tag.TagId, request.TagGroupId);
            return tag.TagId;
        }
    }
} 