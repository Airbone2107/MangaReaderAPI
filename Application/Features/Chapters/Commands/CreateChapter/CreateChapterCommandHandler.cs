using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Commands.CreateChapter
{
    public class CreateChapterCommandHandler : IRequestHandler<CreateChapterCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateChapterCommandHandler> _logger;

        public CreateChapterCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateChapterCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateChapterCommand request, CancellationToken cancellationToken)
        {
            var translatedManga = await _unitOfWork.TranslatedMangaRepository.GetByIdAsync(request.TranslatedMangaId);
            if (translatedManga == null)
            {
                throw new NotFoundException(nameof(TranslatedManga), request.TranslatedMangaId);
            }

            // Kiểm tra User (UploadedByUserId) có tồn tại không.
            // Hiện tại, ta giả định User ID là hợp lệ và được truyền vào.
            // Nếu cần, bạn có thể thêm IUserRepository và kiểm tra.
            // var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UploadedByUserId);
            // if (user == null)
            // {
            //     throw new NotFoundException(nameof(User), request.UploadedByUserId);
            // }

            var chapter = _mapper.Map<Chapter>(request);
            // ChapterId sẽ được tự sinh.
            // ChapterPages sẽ được thêm sau.

            await _unitOfWork.ChapterRepository.AddAsync(chapter);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Chapter {ChapterId} for TranslatedManga {TranslatedMangaId} created successfully by User {UserId}.",
                chapter.ChapterId, request.TranslatedMangaId, request.UploadedByUserId);
            return chapter.ChapterId;
        }
    }
} 