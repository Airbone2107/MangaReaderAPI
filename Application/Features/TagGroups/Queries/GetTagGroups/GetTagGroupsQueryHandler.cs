using Application.Common.DTOs;
using Application.Common.DTOs.TagGroups;
using Application.Common.Models;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Application.Features.TagGroups.Queries.GetTagGroups
{
    public class GetTagGroupsQueryHandler : IRequestHandler<GetTagGroupsQuery, PagedResult<ResourceObject<TagGroupAttributesDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTagGroupsQueryHandler> _logger;

        public GetTagGroupsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTagGroupsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<ResourceObject<TagGroupAttributesDto>>> Handle(GetTagGroupsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetTagGroupsQueryHandler.Handle called with request: {@GetTagGroupsQuery}", request);

            // Build filter predicate
            Expression<Func<TagGroup, bool>>? filter = null;
            if (!string.IsNullOrWhiteSpace(request.NameFilter))
            {
                 // TODO: [Improvement] Cân nhắc tìm kiếm không dấu/chính xác hơn nếu cần
                filter = tg => tg.Name.Contains(request.NameFilter);
            }
            
            // Build OrderBy function
            Func<IQueryable<TagGroup>, IOrderedQueryable<TagGroup>> orderBy;
             switch (request.OrderBy?.ToLowerInvariant())
            {
                case "name":
                default: // Mặc định sắp xếp theo tên
                    orderBy = q => request.Ascending ? q.OrderBy(tg => tg.Name) : q.OrderByDescending(tg => tg.Name);
                    break;
            }

            // Use GetPagedAsync with filter and orderby
            // TODO: [Improvement] Nếu Query có tùy chọn IncludeTags, cần thêm includes: "Tags" vào đây.
            var pagedTagGroups = await _unitOfWork.TagGroupRepository.GetPagedAsync(
                request.Offset,
                request.Limit,
                filter,
                orderBy
            );

            var resourceObjects = new List<ResourceObject<TagGroupAttributesDto>>();
            foreach(var tg in pagedTagGroups.Items)
            {
                var attributes = _mapper.Map<TagGroupAttributesDto>(tg);
                var relationships = new List<RelationshipObject>();
                // Logic to add relationships if IncludeTags was true and implemented
                resourceObjects.Add(new ResourceObject<TagGroupAttributesDto>
                {
                    Id = tg.TagGroupId.ToString(),
                    Type = "tag_group",
                    Attributes = attributes,
                    Relationships = relationships.Any() ? relationships : null
                });
            }
            return new PagedResult<ResourceObject<TagGroupAttributesDto>>(resourceObjects, pagedTagGroups.Total, request.Offset, request.Limit);
        }
    }
} 