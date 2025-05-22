using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Authors.Commands.DeleteAuthor
{
    public class DeleteAuthorCommandHandler : IRequestHandler<DeleteAuthorCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteAuthorCommandHandler> _logger;

        public DeleteAuthorCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteAuthorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteAuthorCommand request, CancellationToken cancellationToken)
        {
            var authorToDelete = await _unitOfWork.AuthorRepository.GetByIdAsync(request.AuthorId);

            if (authorToDelete == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found for deletion.", request.AuthorId);
                throw new NotFoundException(nameof(Domain.Entities.Author), request.AuthorId);
            }

            // Cân nhắc nghiệp vụ: có cho phép xóa tác giả nếu đang được gán cho manga không?
            // Nếu có MangaAuthors liên quan, bạn có thể muốn ngăn chặn việc xóa hoặc xử lý logic liên quan.
            // Ví dụ:
            // var mangaAuthors = await _unitOfWork.MangaRepository.HasAuthorAssociatedAsync(request.AuthorId);
            // if (mangaAuthors)
            // {
            //     throw new Exceptions.DeleteFailureException(nameof(Domain.Entities.Author), request.AuthorId, "Author is associated with existing mangas.");
            // }

            await _unitOfWork.AuthorRepository.DeleteAsync(authorToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Author {AuthorId} deleted successfully.", request.AuthorId);
            return Unit.Value;
        }
    }
} 