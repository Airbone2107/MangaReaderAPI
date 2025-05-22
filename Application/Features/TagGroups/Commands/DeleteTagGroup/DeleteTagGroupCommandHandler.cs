using Application.Contracts.Persistence;
using Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.TagGroups.Commands.DeleteTagGroup
{
    public class DeleteTagGroupCommandHandler : IRequestHandler<DeleteTagGroupCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteTagGroupCommandHandler> _logger;

        public DeleteTagGroupCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteTagGroupCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(DeleteTagGroupCommand request, CancellationToken cancellationToken)
        {
            var tagGroupToDelete = await _unitOfWork.TagGroupRepository.GetTagGroupWithTagsAsync(request.TagGroupId);
            if (tagGroupToDelete == null)
            {
                throw new NotFoundException(nameof(Domain.Entities.TagGroup), request.TagGroupId);
            }

            // Kiểm tra xem TagGroup có chứa Tags nào không.
            // Theo OnModelCreating, Tags.TagGroupId có OnDelete(DeleteBehavior.Restrict)
            // Điều này có nghĩa là DB sẽ không cho xóa TagGroup nếu nó còn chứa Tags.
            // Chúng ta cần kiểm tra ở đây để trả về lỗi thân thiện hơn.
            if (tagGroupToDelete.Tags != null && tagGroupToDelete.Tags.Any())
            {
                _logger.LogWarning("Attempted to delete TagGroup {TagGroupId} which still contains tags.", request.TagGroupId);
                throw new Exceptions.DeleteFailureException(nameof(Domain.Entities.TagGroup), request.TagGroupId, "Cannot delete TagGroup because it still contains tags. Please delete or reassign tags first.");
            }

            await _unitOfWork.TagGroupRepository.DeleteAsync(tagGroupToDelete);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("TagGroup {TagGroupId} deleted successfully.", request.TagGroupId);
            return Unit.Value;
        }
    }
} 