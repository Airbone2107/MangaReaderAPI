using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Commands.UpdateChapterPageDetails
{
    public class UpdateChapterPageDetailsCommandHandler : IRequestHandler<UpdateChapterPageDetailsCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateChapterPageDetailsCommandHandler> _logger;

        public UpdateChapterPageDetailsCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateChapterPageDetailsCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateChapterPageDetailsCommand request, CancellationToken cancellationToken)
        {
            var pageToUpdate = await _unitOfWork.ChapterRepository.GetPageByIdAsync(request.PageId);

            if (pageToUpdate == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.ChapterPage), request.PageId);
            }

            // Kiểm tra nếu PageNumber thay đổi, có bị trùng với trang khác trong cùng chapter không
            if (pageToUpdate.PageNumber != request.PageNumber)
            {
                var chapter = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(pageToUpdate.ChapterId);
                if (chapter != null && chapter.ChapterPages.Any(p => p.PageId != request.PageId && p.PageNumber == request.PageNumber))
                {
                     _logger.LogWarning("Page number {PageNumber} already exists in chapter {ChapterId} for another page.", request.PageNumber, pageToUpdate.ChapterId);
                     throw new Exceptions.ValidationException($"Page number {request.PageNumber} already exists in chapter {pageToUpdate.ChapterId} for another page.");
                }
            }

            pageToUpdate.PageNumber = request.PageNumber;
            // Cập nhật các trường metadata khác nếu có

            await _unitOfWork.ChapterRepository.UpdatePageAsync(pageToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ChapterPage {PageId} details updated successfully.", request.PageId);
            return Unit.Value;
        }
    }
} 