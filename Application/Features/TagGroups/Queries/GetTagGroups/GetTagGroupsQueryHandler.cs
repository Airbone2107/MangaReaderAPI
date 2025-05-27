using Application.Common.DTOs;
using Application.Common.DTOs.TagGroups;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Application.Common.Extensions;

namespace Application.Features.TagGroups.Queries.GetTagGroups
{
    public class GetTagGroupsQueryHandler : IRequestHandler<GetTagGroupsQuery, PagedResult<TagGroupDto>>
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

        public async Task<PagedResult<TagGroupDto>> Handle(GetTagGroupsQuery request, CancellationToken cancellationToken)
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

            var tagGroupDtos = _mapper.Map<List<TagGroupDto>>(pagedTagGroups.Items);
            return new PagedResult<TagGroupDto>(tagGroupDtos, pagedTagGroups.Total, request.Offset, request.Limit);
        }
    }
} 