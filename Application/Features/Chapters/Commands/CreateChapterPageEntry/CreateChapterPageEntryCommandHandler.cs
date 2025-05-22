using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper; // Nếu cần map
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Commands.CreateChapterPageEntry
{
    public class CreateChapterPageEntryCommandHandler : IRequestHandler<CreateChapterPageEntryCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        // private readonly IMapper _mapper; // Không cần mapper nếu command khớp entity
        private readonly ILogger<CreateChapterPageEntryCommandHandler> _logger;

        public CreateChapterPageEntryCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateChapterPageEntryCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateChapterPageEntryCommand request, CancellationToken cancellationToken)
        {
            var chapter = await _unitOfWork.ChapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
            {
                throw new NotFoundException(nameof(Chapter), request.ChapterId);
            }

            // Kiểm tra xem PageNumber đã tồn tại trong Chapter này chưa
            var chapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);
            if (chapterWithPages != null && chapterWithPages.ChapterPages.Any(p => p.PageNumber == request.PageNumber))
            {
                _logger.LogWarning("Page number {PageNumber} already exists in chapter {ChapterId}.", request.PageNumber, request.ChapterId);
                throw new Exceptions.ValidationException($"Page number {request.PageNumber} already exists in chapter {request.ChapterId}.");
            }

            // Nếu PageNumber được cung cấp là 0 hoặc âm, hoặc không được cung cấp, ta có thể tự động gán số trang tiếp theo.
            int pageNumberToSet = request.PageNumber;
            if (pageNumberToSet <= 0)
            {
                pageNumberToSet = await _unitOfWork.ChapterRepository.GetMaxPageNumberAsync(request.ChapterId) + 1;
            }
            
            var chapterPage = new ChapterPage
            {
                ChapterId = request.ChapterId,
                PageNumber = pageNumberToSet,
                PublicId = string.Empty // Sẽ được cập nhật sau khi upload ảnh
            };
            // PageId sẽ tự sinh

            // Thay vì _unitOfWork.ChapterPageRepository.AddAsync(chapterPage);
            // Ta sử dụng phương thức trong IChapterRepository đã định nghĩa
            await _unitOfWork.ChapterRepository.AddPageAsync(chapterPage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ChapterPage entry {PageId} (PageNumber: {PageNumber}) created for Chapter {ChapterId}.", 
                chapterPage.PageId, chapterPage.PageNumber, request.ChapterId);
            return chapterPage.PageId;
        }
    }
} 