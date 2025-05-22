using Application.Contracts.Persistence;
using Application.Exceptions;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // For checking existing MangaTag

namespace Application.Features.Mangas.Commands.AddMangaTag
{
    public class AddMangaTagCommandHandler : IRequestHandler<AddMangaTagCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddMangaTagCommandHandler> _logger;

        public AddMangaTagCommandHandler(IUnitOfWork unitOfWork, ILogger<AddMangaTagCommandHandler> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(AddMangaTagCommand request, CancellationToken cancellationToken)
        {
            var manga = await _unitOfWork.MangaRepository.GetByIdAsync(request.MangaId);
            if (manga == null)
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            var tag = await _unitOfWork.TagRepository.GetByIdAsync(request.TagId);
            if (tag == null)
            {
                throw new NotFoundException(nameof(Tag), request.TagId);
            }

            // Kiểm tra xem MangaTag đã tồn tại chưa
            // EF Core không cung cấp phương thức AddIfNotExists trực tiếp cho collection.
            // Ta cần load collection hoặc kiểm tra trước.
            // Cách 1: Load collection (có thể không hiệu quả nếu collection lớn)
            // var mangaWithTags = await _unitOfWork.MangaRepository.GetMangaWithDetailsAsync(request.MangaId); // Lấy manga với tags
            // if (mangaWithTags.MangaTags.Any(mt => mt.TagId == request.TagId))
            // {
            //    _logger.LogInformation("Tag {TagId} already associated with Manga {MangaId}.", request.TagId, request.MangaId);
            //    return Unit.Value; // Hoặc throw lỗi nếu muốn
            // }

            // Cách 2: Kiểm tra trực tiếp trong bảng join MangaTags (cần ApplicationDbContext để truy cập trực tiếp, hoặc thêm phương thức vào IGenericRepository/IMangaRepository)
            // Vì chúng ta dùng UnitOfWork, và không muốn Repository biết về các Repository khác,
            // cách tốt nhất là thêm một phương thức vào IMangaRepository hoặc sử dụng DbSet trực tiếp nếu có trong UnitOfWork.
            // Tuy nhiên, để đơn giản, chúng ta sẽ cố gắng thêm và để DB xử lý constraint (nếu có Unique Key trên MangaId, TagId)
            // Hoặc, nếu ApplicationDbContext được inject vào IUnitOfWork (không phải chỉ các IRepository) thì có thể truy cập
            // _unitOfWork.Context.MangaTags.AnyAsync(...)
            // Hiện tại, chúng ta sẽ thêm một MangaTag mới.
            
            // Kiểm tra xem record đã tồn tại trong bảng MangaTags chưa
            var existingMangaTag = await _unitOfWork.MangaRepository.FindFirstOrDefaultAsync(
                m => m.MangaId == request.MangaId && m.MangaTags.Any(mt => mt.TagId == request.TagId),
                includeProperties: "MangaTags" // Cần include để kiểm tra collection
            );
            
            // Hoặc nếu bạn có IApplicationDbContext trong UnitOfWork (không khuyến khích)
            // var dbContext = (_unitOfWork as UnitOfWork)?.GetContext(); // Cần ép kiểu và GetContext() public
            // if (dbContext != null)
            // {
            //     bool exists = await dbContext.MangaTags.AnyAsync(mt => mt.MangaId == request.MangaId && mt.TagId == request.TagId, cancellationToken);
            //     if (exists)
            //     {
            //         _logger.LogInformation("Tag {TagId} is already associated with Manga {MangaId}.", request.TagId, request.MangaId);
            //         return Unit.Value;
            //     }
            // }
            // Cách đơn giản nhất là cố gắng thêm và dựa vào primary key constraint của MangaTag
            // Nhưng vì MangaTag không có Id riêng, chúng ta cần thêm một bản ghi vào bảng join.

            // Nếu không có MangaTag entity trong UnitOfWork, chúng ta tạo mới
            var mangaTag = new MangaTag { MangaId = request.MangaId, TagId = request.TagId };
            
            // Vì MangaTags là một ICollection trên Manga, ta có thể thêm vào đó
            // Nhưng chúng ta cần đảm bảo không thêm trùng lặp.
            // Cách tốt nhất là thêm một repository riêng cho MangaTag hoặc thêm phương thức vào MangaRepository.
            // Giả sử MangaRepository có phương thức AddTagAsync
            // await _unitOfWork.MangaRepository.AddTagAsync(manga, tag);

            // Hoặc, ta làm thủ công bằng cách thêm vào bảng MangaTags.
            // Cần có DbSet<MangaTag> trong IApplicationDbContext và một repository cho nó, hoặc truy cập qua context.
            // Để giữ cấu trúc hiện tại, ta sẽ tìm manga, rồi thêm tag vào collection của nó nếu chưa có.
            
            // Lấy Manga bao gồm MangaTags
            var mangaEntity = await _unitOfWork.MangaRepository.FindFirstOrDefaultAsync(
                m => m.MangaId == request.MangaId,
                includeProperties: "MangaTags" // Rất quan trọng: phải include collection này
            );

            if (mangaEntity == null) // Should not happen if previous check passed, but good practice
            {
                throw new NotFoundException(nameof(Manga), request.MangaId);
            }

            if (!mangaEntity.MangaTags.Any(mt => mt.TagId == request.TagId))
            {
                mangaEntity.MangaTags.Add(new MangaTag { MangaId = request.MangaId, TagId = request.TagId });
                // UpdateAsync không cần thiết vì EF Core theo dõi thay đổi của collection trên tracked entity mangaEntity
                // await _unitOfWork.MangaRepository.UpdateAsync(mangaEntity); // Không cần thiết nếu mangaEntity đã được track
            }
            else
            {
                _logger.LogInformation("Tag {TagId} is already associated with Manga {MangaId}.", request.TagId, request.MangaId);
                return Unit.Value; // Tag đã được gán, không làm gì thêm
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tag {TagId} added to Manga {MangaId} successfully.", request.TagId, request.MangaId);
            return Unit.Value;
        }
    }
} 