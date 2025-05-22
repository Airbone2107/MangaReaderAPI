using Application.Contracts.Persistence;
using Application.Exceptions;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For SingleOrDefaultAsync

namespace Application.Features.Mangas.Commands.RemoveMangaAuthor
{
    public class RemoveMangaAuthorCommandHandler : IRequestHandler<RemoveMangaAuthorCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RemoveMangaAuthorCommandHandler> _logger;

        public RemoveMangaAuthorCommandHandler(IUnitOfWork unitOfWork, ILogger<RemoveMangaAuthorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(RemoveMangaAuthorCommand request, CancellationToken cancellationToken)
        {
            var manga = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);
            if (manga == null)
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            // Tác giả có thể không cần check vì ta chỉ xóa record join
            // var author = await _unitOfWork.AuthorRepository.GetByIdAsync(request.AuthorId);
            // if (author == null)
            // {
            //     throw new NotFoundException(nameof(Author), request.AuthorId);
            // }

            var mangaEntity = await _unitOfWork.MangaRepository.FindFirstOrDefaultAsync(
                m => m.MangaId == request.MangaId,
                includeProperties: "MangaAuthors" // Quan trọng
            );

            if (mangaEntity == null) // Should not happen
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }
            
            var mangaAuthorToRemove = mangaEntity.MangaAuthors.SingleOrDefault(ma => ma.AuthorId == request.AuthorId && ma.Role == request.Role);

            if (mangaAuthorToRemove != null)
            {
                mangaEntity.MangaAuthors.Remove(mangaAuthorToRemove);
            }
            else
            {
                _logger.LogWarning("Author {AuthorId} with role {Role} not found on Manga {MangaId} for removal.", request.AuthorId, request.Role, request.MangaId);
                return Unit.Value;
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Author {AuthorId} with role {Role} removed from Manga {MangaId} successfully.", request.AuthorId, request.Role, request.MangaId);
            return Unit.Value;
        }
    }
} 