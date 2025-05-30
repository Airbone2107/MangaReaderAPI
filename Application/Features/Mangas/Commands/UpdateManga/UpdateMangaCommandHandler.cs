using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
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
            var mangaToUpdate = await _unitOfWork.MangaRepository.GetMangaWithDetailsAsync(request.MangaId);

            if (mangaToUpdate == null)
            {
                _logger.LogWarning("Manga with ID {MangaId} not found for update.", request.MangaId);
                throw new NotFoundException(nameof(Domain.Entities.Manga), request.MangaId);
            }

            _mapper.Map(request, mangaToUpdate);

            var requestedTagIds = request.TagIds ?? new List<Guid>();
            var currentTagIds = mangaToUpdate.MangaTags.Select(mt => mt.TagId).ToList();

            var tagsToRemove = mangaToUpdate.MangaTags
                .Where(mt => !requestedTagIds.Contains(mt.TagId))
                .ToList();
            foreach (var mangaTag in tagsToRemove)
            {
                mangaToUpdate.MangaTags.Remove(mangaTag);
            }

            var newTagIds = requestedTagIds.Except(currentTagIds).ToList();
            foreach (var tagId in newTagIds)
            {
                var tagExists = await _unitOfWork.TagRepository.ExistsAsync(tagId);
                if (tagExists)
                {
                    mangaToUpdate.MangaTags.Add(new MangaTag { MangaId = mangaToUpdate.MangaId, TagId = tagId });
                }
                else
                {
                    _logger.LogWarning("Tag with ID {TagId} not found when updating manga {MangaId}. It will be ignored.", tagId, mangaToUpdate.MangaId);
                }
            }

            var requestedAuthors = request.Authors ?? new List<Application.Common.DTOs.Mangas.MangaAuthorInputDto>();
            
            var authorsToRemove = mangaToUpdate.MangaAuthors
                .Where(ma => !requestedAuthors.Any(r => r.AuthorId == ma.AuthorId && r.Role == ma.Role))
                .ToList();
            foreach (var mangaAuthor in authorsToRemove)
            {
                mangaToUpdate.MangaAuthors.Remove(mangaAuthor);
            }

            foreach (var authorInput in requestedAuthors)
            {
                if (!mangaToUpdate.MangaAuthors.Any(ma => ma.AuthorId == authorInput.AuthorId && ma.Role == authorInput.Role))
                {
                    var authorExists = await _unitOfWork.AuthorRepository.ExistsAsync(authorInput.AuthorId);
                    if (authorExists)
                    {
                        mangaToUpdate.MangaAuthors.Add(new MangaAuthor { MangaId = mangaToUpdate.MangaId, AuthorId = authorInput.AuthorId, Role = authorInput.Role });
                    }
                    else
                    {
                         _logger.LogWarning("Author with ID {AuthorId} not found when updating manga {MangaId}. It will be ignored.", authorInput.AuthorId, mangaToUpdate.MangaId);
                    }
                }
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Manga {MangaId} updated successfully with its tags and authors.", request.MangaId);
            return Unit.Value;
        }
    }
} 