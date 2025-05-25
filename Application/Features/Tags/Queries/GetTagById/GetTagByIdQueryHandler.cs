using Application.Common.DTOs.Tags;
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Domain.Entities; // Cần thiết cho FindFirstOrDefaultAsync

namespace Application.Features.Tags.Queries.GetTagById
{
    public class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, TagDto?>
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

        public async Task<TagDto?> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
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
            // AutoMapper sẽ tự động map Tag sang TagDto, bao gồm cả TagGroup.Name nhờ include.
            return _mapper.Map<TagDto>(tag);
        }
    }
} 