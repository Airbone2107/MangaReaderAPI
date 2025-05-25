using Application.Common.DTOs;
using Application.Common.DTOs.Tags;
using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Application.Common.Extensions;

namespace Application.Features.Tags.Queries.GetTags
{
    public class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, PagedResult<TagDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetTagsQueryHandler> _logger;

        public GetTagsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetTagsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<TagDto>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetTagsQueryHandler.Handle called with request: {@GetTagsQuery}", request);

            // Build filter predicate
            Expression<Func<Tag, bool>> predicate = t => true; // Start with a true condition
            if (request.TagGroupId.HasValue)
            {
                predicate = predicate.And(t => t.TagGroupId == request.TagGroupId.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.NameFilter))
            {
                 // TODO: [Improvement] Cân nhắc tìm kiếm không dấu/chính xác hơn nếu cần
                predicate = predicate.And(t => t.Name.Contains(request.NameFilter));
            }

            // Build OrderBy function
            Func<IQueryable<Tag>, IOrderedQueryable<Tag>> orderBy;
            switch (request.OrderBy?.ToLowerInvariant())
            {
                case "taggroupname":
                     // Để sort theo TagGroup.Name, cần include TagGroup
                     // Lưu ý: Sắp xếp trên navigation property có thể kém hiệu quả hơn sắp xếp trên cột trực tiếp.
                     orderBy = q => request.Ascending ? q.OrderBy(t => t.TagGroup.Name) : q.OrderByDescending(t => t.TagGroup.Name);
                    break;
                case "name":
                default:
                    orderBy = q => request.Ascending ? q.OrderBy(t => t.Name) : q.OrderByDescending(t => t.Name);
                    break;
            }

             // Use GetPagedAsync with filter, orderby, and includes
            // Cần include TagGroup để map TagGroupName trong TagDto
            // Ensure includes are configured in GenericRepository.GetPagedAsync
            var pagedTags = await _unitOfWork.TagRepository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                predicate,
                orderBy,
                includeProperties: "TagGroup" // Bao gồm TagGroup
            );

            var tagDtos = _mapper.Map<List<TagDto>>(pagedTags.Items);
            return new PagedResult<TagDto>(tagDtos, pagedTags.TotalCount, request.PageNumber, request.PageSize);
        }
    }
} 