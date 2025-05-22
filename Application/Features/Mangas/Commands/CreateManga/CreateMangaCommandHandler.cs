using Application.Contracts.Persistence;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Mangas.Commands.CreateManga
{
    public class CreateMangaCommandHandler : IRequestHandler<CreateMangaCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateMangaCommandHandler> _logger;

        public CreateMangaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateMangaCommand request, CancellationToken cancellationToken)
        {
            // Kiểm tra xem Manga đã tồn tại chưa (ví dụ theo Title và OriginalLanguage)
            // var existingManga = await _unitOfWork.MangaRepository.FindFirstOrDefaultAsync(
            //     m => m.Title == request.Title && m.OriginalLanguage == request.OriginalLanguage
            // );
            // if (existingManga != null)
            // {
            //     _logger.LogWarning("Manga with title '{MangaTitle}' and language '{OriginalLanguage}' already exists.", request.Title, request.OriginalLanguage);
            //     throw new Exceptions.ValidationException($"Manga with title '{request.Title}' and language '{request.OriginalLanguage}' already exists.");
            // }

            var manga = _mapper.Map<Manga>(request);
            // IsLocked mặc định là false khi tạo mới, không cần gán lại trừ khi có logic khác

            await _unitOfWork.MangaRepository.AddAsync(manga);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Manga {MangaId} created successfully.", manga.MangaId);
            return manga.MangaId;
        }
    }
} 