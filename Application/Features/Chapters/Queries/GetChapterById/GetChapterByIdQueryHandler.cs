using Application.Common.DTOs.Chapters;
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Queries.GetChapterById
{
    public class GetChapterByIdQueryHandler : IRequestHandler<GetChapterByIdQuery, ChapterDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetChapterByIdQueryHandler> _logger;

        public GetChapterByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetChapterByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ChapterDto?> Handle(GetChapterByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetChapterByIdQueryHandler.Handle - Lấy chapter với ID: {ChapterId}", request.ChapterId);
            
            // Cần include User và ChapterPages cho mapping ChapterDto
            // Sử dụng FindFirstOrDefaultAsync để bao gồm các navigation properties
            var chapter = await _unitOfWork.ChapterRepository.FindFirstOrDefaultAsync(
                predicate: c => c.ChapterId == request.ChapterId,
                includeProperties: "User,ChapterPages" // Bao gồm User và ChapterPages
            );

            if (chapter == null)
            {
                _logger.LogWarning("Không tìm thấy chapter với ID: {ChapterId}", request.ChapterId);
                return null;
            }
            
            // AutoMapper sẽ tự động map Chapter sang ChapterDto.
            // Đảm bảo MappingProfile đã cấu hình để map User sang UserDto và ChapterPages sang List<ChapterPageDto>,
            // và sắp xếp ChapterPages theo PageNumber.
            // Ví dụ trong MappingProfile:
            // CreateMap<Chapter, ChapterDto>()
            //     .ForMember(dest => dest.Uploader, opt => opt.MapFrom(src => src.User))
            //     .ForMember(dest => dest.PagesCount, opt => opt.MapFrom(src => src.ChapterPages.Count))
            //     .ForMember(dest => dest.ChapterPages, opt => opt.MapFrom(src => src.ChapterPages.OrderBy(p => p.PageNumber)));
            // CreateMap<User, UserDto>();
            // CreateMap<ChapterPage, ChapterPageDto>();

            return _mapper.Map<ChapterDto>(chapter);
        }
    }
} 