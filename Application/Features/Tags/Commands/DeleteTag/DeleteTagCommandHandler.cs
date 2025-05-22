using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Tags.Commands.DeleteTag
{
    public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteTagCommandHandler> _logger;

        public DeleteTagCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteTagCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
        {
            var tagToDelete = await _unitOfWork.TagRepository.GetByIdAsync(request.TagId);
            if (tagToDelete == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.Tag), request.TagId);
            }

            // Cân nhắc: nếu Tag đang được sử dụng bởi Manga (MangaTags), có cho phép xóa không?
            // OnDelete behavior trong DB đã được cấu hình là Cascade, nên MangaTags liên quan sẽ bị xóa.
            // Nếu không muốn cascade, bạn cần kiểm tra ở đây.
            // var isTagUsed = await _unitOfWork.MangaRepository.IsTagUsedAsync(request.TagId); // Cần thêm phương thức này vào IMangaRepository
            // if (isTagUsed)
            // {
            //    throw new Exceptions.DeleteFailureException(nameof(Domain.Entities.Tag), request.TagId, "Tag is currently associated with mangas.");
            // }

            await _unitOfWork.TagRepository.DeleteAsync(tagToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tag {TagId} deleted successfully.", request.TagId);
            return Unit.Value;
        }
    }
} 