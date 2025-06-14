using Application.Common.DTOs.Tags;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Tags.Queries.GetTagById
{
    public class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, ResourceObject<TagAttributesDto>?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTagByIdQueryHandler> _logger;

        public GetTagByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTagByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ResourceObject<TagAttributesDto>?> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetTagByIdQueryHandler.Handle - Lấy tag với ID: {TagId}", request.TagId);
            
            // Cần include TagGroup để map TagGroupName trong TagDto
            var tag = await _unitOfWork.TagRepository.FindFirstOrDefaultAsync(
                t => t.TagId == request.TagId,
                includeProperties: "TagGroup" // Bao gồm TagGroup
            );

            if (tag == null)
            {
                _logger.LogWarning("Không tìm thấy tag với ID: {TagId}", request.TagId);
                return null;
            }
            
            var attributes = _mapper.Map<TagAttributesDto>(tag);
            var relationships = new List<RelationshipObject>();

            if (tag.TagGroup != null)
            {
                relationships.Add(new RelationshipObject
                {
                    Id = tag.TagGroup.TagGroupId.ToString(),
                    Type = "tag_group" 
                });
            }
            
            return new ResourceObject<TagAttributesDto>
            {
                Id = tag.TagId.ToString(),
                Type = "tag",
                Attributes = attributes,
                Relationships = relationships.Any() ? relationships : null
            };
        }
    }
} 