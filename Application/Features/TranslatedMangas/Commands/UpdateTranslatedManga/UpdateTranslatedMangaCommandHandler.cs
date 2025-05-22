using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TranslatedMangas.Commands.UpdateTranslatedManga
{
    public class UpdateTranslatedMangaCommandHandler : IRequestHandler<UpdateTranslatedMangaCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateTranslatedMangaCommandHandler> _logger;

        public UpdateTranslatedMangaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateTranslatedMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateTranslatedMangaCommand request, CancellationToken cancellationToken)
        {
            var translatedMangaToUpdate = await _unitOfWork.TranslatedMangaRepository.GetByIdAsync(request.TranslatedMangaId);
            if (translatedMangaToUpdate == null)
            {
                throw new NotFoundException(nameof(TranslatedManga), request.TranslatedMangaId);
            }

            // Nếu LanguageKey thay đổi, kiểm tra xem có bị trùng với bản dịch khác của cùng Manga không
            if (!string.Equals(translatedMangaToUpdate.LanguageKey, request.LanguageKey, StringComparison.OrdinalIgnoreCase))
            {
                var existingTranslationWithNewLang = await _unitOfWork.TranslatedMangaRepository
                    .GetByMangaIdAndLanguageAsync(translatedMangaToUpdate.MangaId, request.LanguageKey);
                
                if (existingTranslationWithNewLang != null && existingTranslationWithNewLang.TranslatedMangaId != request.TranslatedMangaId)
                {
                    _logger.LogWarning("Another translation for Manga {MangaId} in language '{LanguageKey}' already exists.", 
                        translatedMangaToUpdate.MangaId, request.LanguageKey);
                    throw new Exceptions.ValidationException($"Another translation for Manga ID '{translatedMangaToUpdate.MangaId}' in language '{request.LanguageKey}' already exists.");
                }
            }

            _mapper.Map(request, translatedMangaToUpdate);

            await _unitOfWork.TranslatedMangaRepository.UpdateAsync(translatedMangaToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TranslatedManga {TranslatedMangaId} updated successfully.", request.TranslatedMangaId);
            return Unit.Value;
        }
    }
} 