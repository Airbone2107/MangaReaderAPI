using Application.Common.DTOs.TagGroups;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities; // Cần cho TagGroup
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TagGroups.Queries.GetTagGroupById
{
    public class GetTagGroupByIdQueryHandler : IRequestHandler<GetTagGroupByIdQuery, ResourceObject<TagGroupAttributesDto>?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTagGroupByIdQueryHandler> _logger;

        public GetTagGroupByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTagGroupByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResourceObject<TagGroupAttributesDto>?> Handle(GetTagGroupByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetTagGroupByIdQueryHandler.Handle - Lấy tag group với ID: {TagGroupId}, IncludeTags: {IncludeTags}", request.TagGroupId, request.IncludeTags);
            
            TagGroup? tagGroup;
            if (request.IncludeTags)
            {
                tagGroup = await _unitOfWork.TagGroupRepository.GetTagGroupWithTagsAsync(request.TagGroupId);
            }
            else
            {
                tagGroup = await _unitOfWork.TagGroupRepository.GetByIdAsync(request.TagGroupId);
            }
            
            if (tagGroup == null)
            {
                _logger.LogWarning("Không tìm thấy tag group với ID: {TagGroupId}", request.TagGroupId);
                return null;
            }

            var attributes = _mapper.Map<TagGroupAttributesDto>(tagGroup);
            var relationships = new List<RelationshipObject>();

            if (request.IncludeTags && tagGroup.Tags != null)
            {
                foreach(var tag in tagGroup.Tags)
                {
                    relationships.Add(new RelationshipObject
                    {
                        Id = tag.TagId.ToString(),
                        Type = "tag"
                    });
                }
            }
            
            return new ResourceObject<TagGroupAttributesDto>
            {
                Id = tagGroup.TagGroupId.ToString(),
                Type = "tag_group",
                Attributes = attributes,
                Relationships = relationships.Any() ? relationships : null
            };
        }
    }
} 