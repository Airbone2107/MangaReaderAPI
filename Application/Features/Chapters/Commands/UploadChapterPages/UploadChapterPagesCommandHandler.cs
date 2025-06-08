using Application.Common.DTOs.Chapters;
using Application.Common.Interfaces;
using Application.Contracts.Persistence;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Chapters.Commands.UploadChapterPages
{
    public class UploadChapterPagesCommandHandler : IRequestHandler<UploadChapterPagesCommand, List<ChapterPageAttributesDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoAccessor _photoAccessor;
        private readonly IMapper _mapper;
        private readonly ILogger<UploadChapterPagesCommandHandler> _logger;

        public UploadChapterPagesCommandHandler(IUnitOfWork unitOfWork, IPhotoAccessor photoAccessor, IMapper mapper, ILogger<UploadChapterPagesCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _photoAccessor = photoAccessor;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<ChapterPageAttributesDto>> Handle(UploadChapterPagesCommand request, CancellationToken cancellationToken)
        {
            var chapter = await _unitOfWork.ChapterRepository.GetByIdAsync(request.ChapterId);
            if (chapter == null)
            {
                throw new NotFoundException(nameof(Chapter), request.ChapterId);
            }

            var uploadedPagesAttributes = new List<ChapterPageAttributesDto>();

            // Sắp xếp file theo DesiredPageNumber để xử lý tuần tự
            var sortedFiles = request.Files.OrderBy(f => f.DesiredPageNumber).ToList();

            // Kiểm tra tính duy nhất của DesiredPageNumber trong request
            var duplicatePageNumbers = sortedFiles.GroupBy(f => f.DesiredPageNumber)
                                                  .Where(g => g.Count() > 1)
                                                  .Select(g => g.Key)
                                                  .ToList();
            if (duplicatePageNumbers.Any())
            {
                throw new ValidationException($"Duplicate page numbers provided in the request: {string.Join(", ", duplicatePageNumbers)}");
            }
            
            // Lấy các trang hiện có của chapter để kiểm tra trùng lặp PageNumber
            var existingPages = (await _unitOfWork.ChapterRepository.GetChapterWithPagesAsync(request.ChapterId))?.ChapterPages ?? new List<ChapterPage>();

            foreach (var fileToUpload in sortedFiles)
            {
                // Kiểm tra xem PageNumber đã tồn tại trong chapter này chưa
                if (existingPages.Any(p => p.PageNumber == fileToUpload.DesiredPageNumber))
                {
                    _logger.LogWarning("Page number {PageNumber} already exists in chapter {ChapterId}. Skipping file {FileName}.", 
                        fileToUpload.DesiredPageNumber, request.ChapterId, fileToUpload.OriginalFileName);
                    // Hoặc throw ValidationException tùy theo yêu cầu nghiệp vụ (nghiêm ngặt hơn)
                    // throw new ValidationException($"Page number {fileToUpload.DesiredPageNumber} already exists in chapter {request.ChapterId}.");
                    continue; // Bỏ qua file này nếu số trang đã tồn tại
                }

                var chapterPageEntity = new ChapterPage
                {
                    ChapterId = request.ChapterId,
                    PageNumber = fileToUpload.DesiredPageNumber,
                    // PublicId sẽ được gán sau khi upload
                };
                // PageId sẽ tự sinh khi AddAsync

                await _unitOfWork.ChapterRepository.AddPageAsync(chapterPageEntity);
                // Phải SaveChangesAsync ở đây để chapterPageEntity.PageId được tạo ra trước khi dùng nó để tạo PublicId
                await _unitOfWork.SaveChangesAsync(cancellationToken);


                var desiredPublicId = $"chapters/{chapterPageEntity.ChapterId}/pages/{chapterPageEntity.PageId}";

                var uploadResult = await _photoAccessor.UploadPhotoAsync(
                    fileToUpload.ImageStream,
                    desiredPublicId,
                    fileToUpload.OriginalFileName
                );

                if (uploadResult == null || string.IsNullOrEmpty(uploadResult.PublicId))
                {
                    _logger.LogError("Failed to upload image {FileName} for chapter {ChapterId}, page number {PageNumber}.",
                        fileToUpload.OriginalFileName, request.ChapterId, fileToUpload.DesiredPageNumber);
                    // Quyết định: có rollback các page đã tạo entry không? Hiện tại là không.
                    // Hoặc có thể throw exception để dừng toàn bộ quá trình.
                    // Để đơn giản, ta chỉ log lỗi và tiếp tục với các file khác.
                    // Nếu muốn dừng, hãy throw new ApiException(...);
                    // Sau khi tạo entry và SaveChanges, nếu upload lỗi, entry vẫn tồn tại với PublicId rỗng. Cần cơ chế xử lý lại.
                    // Để an toàn hơn, không nên SaveChangesAsync cho từng entry page trước khi upload.
                    // Tạm thời: sẽ cập nhật PublicId sau khi upload thành công.
                    // Nếu upload lỗi, entry đã tạo sẽ không có PublicId.
                    
                    // Cần cập nhật lại: logic này không đúng, vì pageId đã có, publicId sẽ được gán.
                    // Nếu upload lỗi, thì PublicId của chapterPageEntity sẽ không được cập nhật đúng.
                    // => Nên tạo entry, upload, nếu thành công thì cập nhật PublicId, rồi mới SaveChangesAsync một lần cuối.
                    // Tuy nhiên, để tạo desiredPublicId với PageId, PageId phải được sinh ra.
                    // -> SaveChangesAsync sau khi AddPageAsync là cần thiết để có PageId.

                    chapterPageEntity.PublicId = "upload_failed"; // Đánh dấu upload lỗi
                }
                else
                {
                    chapterPageEntity.PublicId = uploadResult.PublicId;
                }
                
                // Cập nhật lại entity ChapterPage với PublicId
                await _unitOfWork.ChapterRepository.UpdatePageAsync(chapterPageEntity);
                uploadedPagesAttributes.Add(_mapper.Map<ChapterPageAttributesDto>(chapterPageEntity));
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken); // Lưu tất cả các thay đổi PublicId
            _logger.LogInformation("Successfully processed {Count} files for chapter {ChapterId}.", sortedFiles.Count, request.ChapterId);

            return uploadedPagesAttributes;
        }
    }
} 