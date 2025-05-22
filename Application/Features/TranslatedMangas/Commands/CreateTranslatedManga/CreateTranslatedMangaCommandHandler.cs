using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TranslatedMangas.Commands.CreateTranslatedManga
{
    public class CreateTranslatedMangaCommandHandler : IRequestHandler<CreateTranslatedMangaCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateTranslatedMangaCommandHandler> _logger;

        public CreateTranslatedMangaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateTranslatedMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateTranslatedMangaCommand request, CancellationToken cancellationToken)
        {
            var manga = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);
            if (manga == null)
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            // Kiểm tra xem bản dịch cho ngôn ngữ này đã tồn tại chưa
            var existingTranslation = await _unitOfWork.TranslatedMangaRepository.GetByMangaIdAndLanguageAsync(request.MangaId, request.LanguageKey);
            if (existingTranslation != null)
            {
                _logger.LogWarning("Translation for Manga {MangaId} in language '{LanguageKey}' already exists.", request.MangaId, request.LanguageKey);
                throw new Exceptions.ValidationException($"Translation for Manga ID '{request.MangaId}' in language '{request.LanguageKey}' already exists.");
            }

            var translatedManga = _mapper.Map<TranslatedManga>(request);
            // TranslatedMangaId sẽ tự sinh

            await _unitOfWork.TranslatedMangaRepository.AddAsync(translatedManga);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TranslatedManga {TranslatedMangaId} created for Manga {MangaId} in language {LanguageKey}.",
                translatedManga.TranslatedMangaId, request.MangaId, request.LanguageKey);
            return translatedManga.TranslatedMangaId;
        }
    }
} 