using Application.Common.DTOs.Mangas;
using Application.Contracts.Persistence;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Mangas.Queries.GetMangaById
{
    public class GetMangaByIdQueryHandler : IRequestHandler<GetMangaByIdQuery, MangaDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetMangaByIdQueryHandler> _logger;

        public GetMangaByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetMangaByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<MangaDto?> Handle(GetMangaByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("GetMangaByIdQueryHandler.Handle - Lấy manga với ID: {MangaId}", request.MangaId);
            
            // Sử dụng phương thức GetMangaWithDetailsAsync từ MangaRepository
            // Phương thức này đã được thiết kế để eager load các navigation properties cần thiết cho MangaDto.
            var manga = await _unitOfWork.MangaRepository.GetMangaWithDetailsAsync(request.MangaId);

            if (manga == null)
            {
                _logger.LogWarning("Không tìm thấy manga với ID: {MangaId}", request.MangaId);
                return null;
            }

            // AutoMapper sẽ tự động map Manga và các collection đã được load
            // sang MangaDto và các DTO tương ứng (TagDto, AuthorDto, CoverArtDto, TranslatedMangaDto).
            // Đảm bảo MappingProfile đã cấu hình đúng cho các mối quan hệ này.
            // Ví dụ:
            // CreateMap<Manga, MangaDto>()
            //     .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.MangaTags.Select(mt => mt.Tag)))
            //     .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.MangaAuthors.Select(ma => ma.Author)))
            //     ...
            // CreateMap<Tag, TagDto>();
            // CreateMap<Author, AuthorDto>();
            // ...
            
            return _mapper.Map<MangaDto>(manga);
        }
    }
} 