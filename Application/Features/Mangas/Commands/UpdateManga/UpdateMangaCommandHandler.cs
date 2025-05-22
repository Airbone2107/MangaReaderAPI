using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Mangas.Commands.UpdateManga
{
    public class UpdateMangaCommandHandler : IRequestHandler<UpdateMangaCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateMangaCommandHandler> _logger;

        public UpdateMangaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateMangaCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateMangaCommand request, CancellationToken cancellationToken)
        {
            var mangaToUpdate = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);

            if (mangaToUpdate == null)
            {
                _logger.LogWarning("Manga with ID {MangaId} not found for update.", request.MangaId);
                throw new NotFoundException(nameof(Domain.Entities.Manga), request.MangaId);
            }

            // Kiểm tra IsLocked: Nếu Manga bị khóa, có thể không cho phép một số thay đổi nhất định (tùy logic nghiệp vụ)
            // if (mangaToUpdate.IsLocked && (mangaToUpdate.Title != request.Title /* ... các trường khác ... */))
            // {
            //     _logger.LogWarning("Attempted to update a locked manga {MangaId}.", request.MangaId);
            //     throw new Exceptions.ValidationException("Cannot update a locked manga's details.");
            // }

            _mapper.Map(request, mangaToUpdate);

            await _unitOfWork.MangaRepository.UpdateAsync(mangaToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Manga {MangaId} updated successfully.", request.MangaId);
            return Unit.Value;
        }
    }
} 