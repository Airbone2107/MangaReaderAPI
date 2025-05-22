using Application.Contracts.Persistence;
using Application.Exceptions;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For AnyAsync

namespace Application.Features.Mangas.Commands.AddMangaAuthor
{
    public class AddMangaAuthorCommandHandler : IRequestHandler<AddMangaAuthorCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddMangaAuthorCommandHandler> _logger;

        public AddMangaAuthorCommandHandler(IUnitOfWork unitOfWork, ILogger<AddMangaAuthorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(AddMangaAuthorCommand request, CancellationToken cancellationToken)
        {
            var manga = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);
            if (manga == null)
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            var author = await _unitOfWork.AuthorRepository.GetByIdAsync(request.AuthorId);
            if (author == null)
            {
                throw new NotFoundException(nameof(Author), request.AuthorId);
            }

            // Kiểm tra xem MangaAuthor đã tồn tại chưa
            var mangaEntity = await _unitOfWork.MangaRepository.FindFirstOrDefaultAsync(
                m => m.MangaId == request.MangaId,
                includeProperties: "MangaAuthors" // Quan trọng
            );

            if (mangaEntity == null) // Should not happen
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            if (!mangaEntity.MangaAuthors.Any(ma => ma.AuthorId == request.AuthorId && ma.Role == request.Role))
            {
                mangaEntity.MangaAuthors.Add(new MangaAuthor
                {
                    MangaId = request.MangaId,
                    AuthorId = request.AuthorId,
                    Role = request.Role
                });
            }
            else
            {
                _logger.LogInformation("Author {AuthorId} with role {Role} is already associated with Manga {MangaId}.", request.AuthorId, request.Role, request.MangaId);
                return Unit.Value;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Author {AuthorId} with role {Role} added to Manga {MangaId} successfully.", request.AuthorId, request.Role, request.MangaId);
            return Unit.Value;
        }
    }
} 