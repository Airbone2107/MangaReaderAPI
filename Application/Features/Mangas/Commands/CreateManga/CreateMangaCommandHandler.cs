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
            
            if (request.TagIds != null && request.TagIds.Any())
            {
                foreach (var tagId in request.TagIds)
                {
                    var tag = await _unitOfWork.TagRepository.GetByIdAsync(tagId);
                    if (tag != null)
                    {
                        manga.MangaTags.Add(new MangaTag { TagId = tagId });
                    }
                    else
                    {
                        _logger.LogWarning("Tag with ID {TagId} not found when creating manga. It will be ignored.", tagId);
                    }
                }
            }

            if (request.Authors != null && request.Authors.Any())
            {
                foreach (var authorInput in request.Authors)
                {
                    var author = await _unitOfWork.AuthorRepository.GetByIdAsync(authorInput.AuthorId);
                    if (author != null)
                    {
                        manga.MangaAuthors.Add(new MangaAuthor { AuthorId = authorInput.AuthorId, Role = authorInput.Role });
                    }
                    else
                    {
                        _logger.LogWarning("Author with ID {AuthorId} not found when creating manga. It will be ignored.", authorInput.AuthorId);
                    }
                }
            }

            await _unitOfWork.MangaRepository.AddAsync(manga);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Manga {MangaId} created successfully with its tags and authors.", manga.MangaId);
            return manga.MangaId;
        }
    }
} 