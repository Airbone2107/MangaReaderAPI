using Application.Common.DTOs.TagGroups;
using Application.Common.DTOs.Tags; // Cần cho TagDto nếu IncludeTags = true
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Entities; // Cần cho TagGroup

namespace Application.Features.TagGroups.Queries.GetTagGroupById
{
    public class GetTagGroupByIdQueryHandler : IRequestHandler<GetTagGroupByIdQuery, TagGroupDto?>
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

        public async Task<TagGroupDto?> Handle(GetTagGroupByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetTagGroupByIdQueryHandler.Handle - Lấy tag group với ID: {TagGroupId}", request.TagGroupId);
            
            TagGroup? tagGroup;

            // TODO: [Improvement] Nếu GetTagGroupByIdQuery có IncludeTags = true, sử dụng ITagGroupRepository.GetTagGroupWithTagsAsync(request.TagGroupId);
            // Nếu không có tùy chọn IncludeTags, hoặc IncludeTags = false, chỉ cần GetByIdAsync.
            // Hiện tại chỉ implement trường hợp cơ bản không include Tags.
            tagGroup = await _unitOfWork.TagGroupRepository.GetByIdAsync(request.TagGroupId);
            
            if (tagGroup == null)
            {
                _logger.LogWarning("Không tìm thấy tag group với ID: {TagGroupId}", request.TagGroupId);
                return null;
            }

            var tagGroupDto = _mapper.Map<TagGroupDto>(tagGroup);

            // TODO: [Improvement] Nếu Query có IncludeTags = true VÀ TagGroupDto được cập nhật để chứa List<TagDto>,
            // cần đảm bảo mapping profile xử lý việc map TagGroup.Tags sang TagGroupDto.Tags.
            // MappingProfile hiện tại chỉ map TagGroup -> TagGroupDto mà không có property Tags.
            // Nếu cập nhật TagGroupDto và MappingProfile, phần này sẽ tự động hoạt động khi TagGroupWithTagsAsync được gọi.
            
            return tagGroupDto;
        }
    }
} 