using Application.Common.DTOs.Chapters;
using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            // 1. Xóa các trang không có trong request.Instructions
            var pagesToDelete = existingDbPages.Values.Where(p => !requestedPageIds.Contains(p.PageId)).ToList();
            foreach (var pageToDelete in pagesToDelete)
            {
                if (!string.IsNullOrEmpty(pageToDelete.PublicId))
                {
                    var deletionResult = await _photoAccessor.DeletePhotoAsync(pageToDelete.PublicId);
                    if (deletionResult != "ok" && deletionResult != "not found")
                    {
                        _logger.LogWarning("Failed to delete image {PublicId} from Cloudinary for page {PageId} of chapter {ChapterId}.",
                            pageToDelete.PublicId, pageToDelete.PageId, request.ChapterId);
                        // Cân nhắc: có nên dừng lại nếu không xóa được ảnh?
                    }
                }
                // Entity Framework sẽ xử lý việc xóa khỏi DB khi SaveChanges được gọi
                // Tuy nhiên, vì ChapterPage không được load trực tiếp bởi ChapterRepository trong GenericRepository,
                // chúng ta cần xóa tường minh.
                await _unitOfWork.ChapterRepository.DeletePageAsync(pageToDelete.PageId); 
                _logger.LogInformation("Marked page {PageId} (Number: {PageNumber}) for deletion from chapter {ChapterId}.", 
                    pageToDelete.PageId, pageToDelete.PageNumber, request.ChapterId);
            }

            // 2. Cập nhật và thêm mới trang
            var finalPageEntities = new List<ChapterPage>();
            var pageNumbersInRequest = request.Instructions.Select(i => i.DesiredPageNumber).ToList();
            if (pageNumbersInRequest.Distinct().Count() != pageNumbersInRequest.Count)
            {
                throw new ValidationException("Page numbers in the request must be unique.");
            }


            foreach (var instruction in request.Instructions.OrderBy(i => i.DesiredPageNumber))
            {
                ChapterPage? currentPageEntity = null;

                // Kiểm tra xem PageId từ instruction có tồn tại trong DB không
                if (existingDbPages.TryGetValue(instruction.PageId, out var dbPage))
                {
                    currentPageEntity = dbPage;
                    _logger.LogInformation("Updating existing page {PageId} for chapter {ChapterId}. New page number: {DesiredPageNumber}", 
                        instruction.PageId, request.ChapterId, instruction.DesiredPageNumber);

                    currentPageEntity.PageNumber = instruction.DesiredPageNumber;

                    if (instruction.ImageFileToUpload != null) // Cần thay thế ảnh
                    {
                        // PublicId vẫn giữ nguyên vì nó dựa trên PageId (đã được cập nhật logic)
                        // Cloudinary UploadAsync với Overwrite = true sẽ ghi đè ảnh cũ.
                        var desiredPublicId = $"chapters/{request.ChapterId}/pages/{currentPageEntity.PageId}";
                        var uploadResult = await _photoAccessor.UploadPhotoAsync(
                            instruction.ImageFileToUpload.ImageStream,
                            desiredPublicId,
                            instruction.ImageFileToUpload.OriginalFileName
                        );

                        if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
                        {
                            _logger.LogError("Failed to re-upload image for page {PageId} (new number {DesiredPageNumber}) in chapter {ChapterId}.",
                                currentPageEntity.PageId, instruction.DesiredPageNumber, request.ChapterId);
                            // Xử lý lỗi: có thể throw, hoặc bỏ qua, hoặc set PublicId thành một giá trị lỗi
                            currentPageEntity.PublicId = "upload_replace_failed"; // Đánh dấu lỗi
                        }
                        else
                        {
                            currentPageEntity.PublicId = uploadResult.PublicId;
                        }
                        await _unitOfWork.ChapterRepository.UpdatePageAsync(currentPageEntity);
                    }
                    else // Không thay ảnh, chỉ có thể là thay đổi PageNumber
                    {
                         await _unitOfWork.ChapterRepository.UpdatePageAsync(currentPageEntity);
                    }
                    finalPageEntities.Add(currentPageEntity);
                }
                else // Trang mới
                {
                    if (instruction.ImageFileToUpload == null)
                    {
                        _logger.LogError("New page instruction for chapter {ChapterId} at page number {DesiredPageNumber} is missing image file.",
                            request.ChapterId, instruction.DesiredPageNumber);
                        throw new ValidationException($"Image file is required for new page at number {instruction.DesiredPageNumber}.");
                    }

                    _logger.LogInformation("Adding new page for chapter {ChapterId} at page number: {DesiredPageNumber}", 
                        request.ChapterId, instruction.DesiredPageNumber);

                    currentPageEntity = new ChapterPage
                    {
                        ChapterId = request.ChapterId,
                        PageNumber = instruction.DesiredPageNumber,
                        PageId = instruction.PageId // PageId này đã được gán (hoặc tạo mới ở Controller/Command)
                    };

                    var desiredPublicId = $"chapters/{request.ChapterId}/pages/{currentPageEntity.PageId}";
                    var uploadResult = await _photoAccessor.UploadPhotoAsync(
                        instruction.ImageFileToUpload.ImageStream,
                        desiredPublicId,
                        instruction.ImageFileToUpload.OriginalFileName
                    );

                    if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
                    {
                        _logger.LogError("Failed to upload image for new page (number {DesiredPageNumber}) in chapter {ChapterId}.",
                            instruction.DesiredPageNumber, request.ChapterId);
                        currentPageEntity.PublicId = "upload_new_failed"; // Đánh dấu lỗi
                    }
                    else
                    {
                        currentPageEntity.PublicId = uploadResult.PublicId;
                    }
                    await _unitOfWork.ChapterRepository.AddPageAsync(currentPageEntity);
                    finalPageEntities.Add(currentPageEntity);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Lấy lại danh sách trang cuối cùng từ DB để đảm bảo dữ liệu nhất quán và đã sắp xếp
            var updatedChapterWithPages = await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId);
            return _mapper.Map<List<ChapterPageAttributesDto>>(updatedChapterWithPages?.ChapterPages ?? new List<ChapterPage>());
        }
    }
} 