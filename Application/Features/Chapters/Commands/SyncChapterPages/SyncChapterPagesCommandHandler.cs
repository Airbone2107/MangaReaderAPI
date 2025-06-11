using Application.Common.DTOs.Chapters;
using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Chapters.Commands.SyncChapterPages
{
    public class SyncChapterPagesCommandHandler : IRequestHandler<SyncChapterPagesCommand, List<ChapterPageAttributesDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly IMapper _mapper;
        private readonly ILogger<SyncChapterPagesCommandHandler> _logger;

        public SyncChapterPagesCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, IMapper mapper, ILogger<SyncChapterPagesCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _photoAccessor = photoAccessor;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<ChapterPageAttributesDto>> Handle(SyncChapterPagesCommand request, CancellationToken cancellationToken)
        {
            var chapter = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);
            if (chapter == null)
            {
                throw new NotFoundException(nameof(Chapter), request.ChapterId);
            }

            var existingDbPages = chapter.ChapterPages.ToDictionary(p => p.PageId);
            var requestedPageIds = request.Instructions.Select(i => i.PageId).ToHashSet();

            // Kiểm tra PageNumber > 0 và duy nhất trong request
            var pageNumbersInRequest = request.Instructions.Select(i => i.DesiredPageNumber).ToList();
            if (pageNumbersInRequest.Any(p => p <= 0))
            {
                throw new ValidationException("Số trang phải lớn hơn 0.");
            }
            if (pageNumbersInRequest.Distinct().Count() != pageNumbersInRequest.Count)
            {
                throw new ValidationException("Số trang trong yêu cầu phải là duy nhất.");
            }

            // ----- Giai đoạn 1: Xác định các trang cần thay đổi số và cần gán giá trị tạm thời -----
            _logger.LogInformation("Giai đoạn 1: Xác định các trang cần thay đổi số cho chương {ChapterId}.", request.ChapterId);
            var pageNumberUpdates = new Dictionary<Guid, int>();
            var pagesToReceiveTemporaryNumber = new List<ChapterPage>();
            bool hasTemporaryUpdates = false;

            foreach (var instruction in request.Instructions)
            {
                if (existingDbPages.TryGetValue(instruction.PageId, out var existingPage))
                {
                    if (existingPage.PageNumber != instruction.DesiredPageNumber)
                    {
                        pageNumberUpdates[existingPage.PageId] = instruction.DesiredPageNumber;
                        pagesToReceiveTemporaryNumber.Add(existingPage);
                        _logger.LogInformation("Đánh dấu trang {PageId} để thay đổi số từ {OldPageNumber} sang {NewPageNumber}.",
                            existingPage.PageId, existingPage.PageNumber, instruction.DesiredPageNumber);
                    }
                }
            }

            // ----- Giai đoạn 2: Gán PageNumber tạm thời cho các trang cần thay đổi số -----
            _logger.LogInformation("Giai đoạn 2: Gán số trang tạm thời cho chương {ChapterId}.", request.ChapterId);
            if (pagesToReceiveTemporaryNumber.Any())
            {
                int tempNumberStart = -(existingDbPages.Count + request.Instructions.Count(i => !existingDbPages.ContainsKey(i.PageId)) + 1000);
                foreach (var pageToTemporarilyUpdate in pagesToReceiveTemporaryNumber)
                {
                    pageToTemporarilyUpdate.PageNumber = tempNumberStart--;
                    _logger.LogInformation("Tạm thời cập nhật PageNumber cho trang {PageId} thành {TemporaryPageNumber}.",
                        pageToTemporarilyUpdate.PageId, pageToTemporarilyUpdate.PageNumber);
                    await _unitOfWork.ChapterRepository.UpdatePageAsync(pageToTemporarilyUpdate);
                    hasTemporaryUpdates = true;
                }

                if (hasTemporaryUpdates)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Đã lưu các PageNumber tạm thời vào DB cho chương {ChapterId}.", request.ChapterId);
                }
            }

            // ----- Giai đoạn 3: Xóa các trang không có trong danh sách yêu cầu -----
            _logger.LogInformation("Giai đoạn 3: Xóa các trang không có trong yêu cầu cho chương {ChapterId}.", request.ChapterId);
            var pagesToDelete = existingDbPages.Values.Where(p => !requestedPageIds.Contains(p.PageId)).ToList();
            bool hasDeletions = false;
            if (pagesToDelete.Any())
            {
                foreach (var pageToDelete in pagesToDelete)
                {
                    if (!string.IsNullOrEmpty(pageToDelete.PublicId))
                    {
                        var deletionResult = await _photoAccessor.DeletePhotoAsync(pageToDelete.PublicId);
                        if (deletionResult != "ok" && deletionResult != "not found")
                        {
                            _logger.LogWarning("Không thể xóa ảnh {PublicId} từ Cloudinary cho trang {PageId} của chương {ChapterId}.",
                                pageToDelete.PublicId, pageToDelete.PageId, request.ChapterId);
                        }
                    }
                    await _unitOfWork.ChapterRepository.DeletePageAsync(pageToDelete.PageId);
                    _logger.LogInformation("Đã đánh dấu trang {PageId} (Số cũ: {PageNumber}) để xóa khỏi chương {ChapterId}.",
                        pageToDelete.PageId, pageToDelete.PageNumber, request.ChapterId);
                    existingDbPages.Remove(pageToDelete.PageId); // Xóa khỏi dictionary theo dõi
                    hasDeletions = true;
                }

                if (hasDeletions)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Đã lưu các thay đổi xóa trang vào DB cho chương {ChapterId}.", request.ChapterId);
                }
            }

            // ----- Giai đoạn 4: Cập nhật và Thêm mới các trang -----
            _logger.LogInformation("Giai đoạn 4: Xử lý thêm mới và cập nhật các trang cho chương {ChapterId}.", request.ChapterId);

            // 4.1. Thêm các trang mới
            _logger.LogInformation("Giai đoạn 4.1: Thêm các trang mới.");
            var newPagesInstructions = request.Instructions.Where(i => !existingDbPages.ContainsKey(i.PageId)).OrderBy(i => i.DesiredPageNumber).ToList();
            bool newPagesAdded = false;

            if (newPagesInstructions.Any())
            {
                foreach (var instruction in newPagesInstructions)
                {
                    if (instruction.ImageFileToUpload == null)
                    {
                        _logger.LogError("Hướng dẫn trang mới cho chương {ChapterId} ở số trang {DesiredPageNumber} không có file ảnh.",
                            request.ChapterId, instruction.DesiredPageNumber);
                        throw new ValidationException($"Yêu cầu file ảnh cho trang mới ở số {instruction.DesiredPageNumber}.");
                    }

                    _logger.LogInformation("Thêm trang mới cho chương {ChapterId} ở số trang: {DesiredPageNumber} với PageId: {PageId}",
                        request.ChapterId, instruction.DesiredPageNumber, instruction.PageId);

                    var newPageEntity = new ChapterPage
                    {
                        ChapterId = request.ChapterId,
                        PageNumber = instruction.DesiredPageNumber,
                        PageId = instruction.PageId
                    };

                    var desiredPublicId = $"chapters/{request.ChapterId}/pages/{newPageEntity.PageId}";
                    var uploadResult = await _photoAccessor.UploadPhotoAsync(
                        instruction.ImageFileToUpload.ImageStream,
                        desiredPublicId,
                        instruction.ImageFileToUpload.OriginalFileName
                    );

                    if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
                    {
                        _logger.LogError("Không thể tải ảnh cho trang mới (số {DesiredPageNumber}, PageId {PageId}) trong chương {ChapterId}.",
                            instruction.DesiredPageNumber, newPageEntity.PageId, request.ChapterId);
                        newPageEntity.PublicId = "upload_new_failed";
                    }
                    else
                    {
                        newPageEntity.PublicId = uploadResult.PublicId;
                    }
                    await _unitOfWork.ChapterRepository.AddPageAsync(newPageEntity);
                    newPagesAdded = true;
                }

                if (newPagesAdded)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Đã lưu các trang mới vào DB cho chương {ChapterId}.", request.ChapterId);
                }
            }

            // 4.2. Cập nhật các trang hiện có về PageNumber cuối cùng và cập nhật ảnh (nếu có)
            _logger.LogInformation("Giai đoạn 4.2: Cập nhật các trang hiện có về số trang cuối cùng.");
            var existingPagesToUpdateInstructions = request.Instructions.Where(i => existingDbPages.ContainsKey(i.PageId)).OrderBy(i => i.DesiredPageNumber).ToList();
            bool existingPagesUpdated = false;

            if (existingPagesToUpdateInstructions.Any())
            {
                foreach (var instruction in existingPagesToUpdateInstructions)
                {
                    var currentPageEntity = existingDbPages[instruction.PageId];

                    _logger.LogInformation("Cập nhật trang hiện có {PageId} cho chương {ChapterId}. Số trang tạm thời: {CurrentPageNumber}, Số trang mới: {DesiredPageNumber}",
                        instruction.PageId, request.ChapterId, currentPageEntity.PageNumber, instruction.DesiredPageNumber);

                    currentPageEntity.PageNumber = instruction.DesiredPageNumber;

                    if (instruction.ImageFileToUpload != null)
                    {
                        _logger.LogInformation("Thay thế ảnh cho trang {PageId} (số mới {DesiredPageNumber}).", currentPageEntity.PageId, instruction.DesiredPageNumber);
                        var desiredPublicId = $"chapters/{request.ChapterId}/pages/{currentPageEntity.PageId}";
                        var uploadResult = await _photoAccessor.UploadPhotoAsync(
                            instruction.ImageFileToUpload.ImageStream,
                            desiredPublicId,
                            instruction.ImageFileToUpload.OriginalFileName
                        );

                        if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
                        {
                            _logger.LogError("Không thể tải lại ảnh cho trang {PageId} (số mới {DesiredPageNumber}) trong chương {ChapterId}.",
                                currentPageEntity.PageId, instruction.DesiredPageNumber, request.ChapterId);
                            currentPageEntity.PublicId = "upload_replace_failed";
                        }
                        else
                        {
                            currentPageEntity.PublicId = uploadResult.PublicId;
                        }
                    }

                    await _unitOfWork.ChapterRepository.UpdatePageAsync(currentPageEntity);
                    existingPagesUpdated = true;
                }

                if (existingPagesUpdated)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Đã lưu các cập nhật cuối cùng cho trang hiện có vào DB cho chương {ChapterId}.", request.ChapterId);
                }
            }

            // ----- Giai đoạn 5: Ghi log hoàn thành -----
            _logger.LogInformation("Giai đoạn 5: Đồng bộ thành công các trang cho chương {ChapterId}. Các thay đổi đã được lưu qua nhiều bước.", request.ChapterId);

            // ----- Giai đoạn 6: Lấy lại danh sách trang đã cập nhật và trả về -----
            _logger.LogInformation("Giai đoạn 6: Lấy lại danh sách trang cuối cùng cho chương {ChapterId}.", request.ChapterId);
            var updatedChapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);
            var resultPages = updatedChapterWithPages?.ChapterPages.OrderBy(p => p.PageNumber).ToList() ?? new List<ChapterPage>();

            return _mapper.Map<List<ChapterPageAttributesDto>>(resultPages);
        }
    }
}