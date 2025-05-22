using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Commands.UpdateChapter
{
    public class UpdateChapterCommandHandler : IRequestHandler<UpdateChapterCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateChapterCommandHandler> _logger;

        public UpdateChapterCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateChapterCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UpdateChapterCommand request, CancellationToken cancellationToken)
        {
            var chapterToUpdate = await _unitOfWork.ChapterRepository.GetByIdAsync(request.ChapterId);

            if (chapterToUpdate == null)
            {
                _logger.LogWarning("Chapter with ID {ChapterId} not found for update.", request.ChapterId);
                throw new NotFoundException(nameof(Domain.Entities.Chapter), request.ChapterId);
            }

            // Chỉ map các trường được phép thay đổi.
            // AutoMapper có thể được cấu hình để bỏ qua các thuộc tính không có trong source
            // hoặc bạn có thể map thủ công từng thuộc tính.
            // Trong ví dụ này, UpdateChapterCommand chỉ chứa các trường có thể cập nhật.
            _mapper.Map(request, chapterToUpdate);

            await _unitOfWork.ChapterRepository.UpdateAsync(chapterToUpdate);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Chapter {ChapterId} updated successfully.", request.ChapterId);
            return Unit.Value;
        }
    }
} 