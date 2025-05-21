// Project Structure.md
MangaReaderAPI/
├── Application/
│   ├── Common/
│   │   ├── DTOs/                     // Chứa các Data Transfer Objects
│   │   │   ├── PagedResult.cs        // DTO cho kết quả phân trang chung
│   │   │   ├── Authors/              // DTOs cho Author
│   │   │   │   ├── AuthorDto.cs
│   │   │   │   ├── CreateAuthorDto.cs
│   │   │   │   └── UpdateAuthorDto.cs
│   │   │   ├── Mangas/               // DTOs cho Manga
│   │   │   │   ├── MangaDto.cs
│   │   │   │   ├── CreateMangaDto.cs
│   │   │   │   ├── UpdateMangaDto.cs
│   │   │   │   ├── MangaAuthorInputDto.cs // DTO con cho input Author khi tạo/cập nhật Manga (chứa AuthorId và Role)
│   │   │   │   └── MangaTagInputDto.cs    // DTO con cho input TagId khi tạo/cập nhật Manga
│   │   │   ├── Chapters/             // DTOs cho Chapter và ChapterPage
│   │   │   │   ├── ChapterDto.cs
│   │   │   │   ├── CreateChapterDto.cs
│   │   │   │   ├── UpdateChapterDto.cs
│   │   │   │   ├── ChapterPageDto.cs
│   │   │   │   ├── CreateChapterPageDto.cs // DTO cho việc tạo metadata trang, không chứa file
│   │   │   │   └── UpdateChapterPageDto.cs // DTO cho việc cập nhật metadata trang
│   │   │   ├── Tags/                 // DTOs cho Tag
│   │   │   │   ├── TagDto.cs
│   │   │   │   ├── CreateTagDto.cs
│   │   │   │   └── UpdateTagDto.cs
│   │   │   ├── TagGroups/            // DTOs cho TagGroup
│   │   │   │   ├── TagGroupDto.cs
│   │   │   │   ├── CreateTagGroupDto.cs
│   │   │   │   └── UpdateTagGroupDto.cs
│   │   │   ├── CoverArts/            // DTOs cho CoverArt
│   │   │   │   ├── CoverArtDto.cs
│   │   │   │   └── CreateCoverArtDto.cs // DTO cho việc tạo metadata ảnh bìa, không chứa file
│   │   │   ├── TranslatedMangas/     // DTOs cho TranslatedManga
│   │   │   │   ├── TranslatedMangaDto.cs
│   │   │   │   ├── CreateTranslatedMangaDto.cs
│   │   │   │   └── UpdateTranslatedMangaDto.cs
│   │   │   └── Users/                // DTOs cho User (chỉ dùng để hiển thị)
│   │   │       └── UserDto.cs        // DTO đơn giản để hiển thị thông tin User trong ChapterDto
│   │   ├── Interfaces/
│   │   │   ├── IApplicationDbContext.cs // Interface cho DbContext. ApplicationDbContext (Persistence) sẽ implement interface này.
│   │   │   └── IPhotoAccessor.cs      // ĐÃ CÓ
│   │   ├── Mappings/
│   │   │   └── MappingProfile.cs     // Cấu hình AutoMapper
│   │   └── Models/
│   │       └── PhotoUploadResult.cs   // ĐÃ CÓ
│   ├── Contracts/                    // MỚI: Chứa các hợp đồng (interfaces) cho tầng Persistence và các dịch vụ khác
│   │   └── Persistence/              // MỚI: Interfaces cho Unit of Work và Repositories
│   │       ├── IUnitOfWork.cs        // MỚI
│   │       ├── IGenericRepository.cs // MỚI (Tùy chọn, nếu muốn dùng Generic Repository pattern)
│   │       ├── IMangaRepository.cs   // MỚI
│   │       ├── IAuthorRepository.cs  // MỚI
│   │       ├── IChapterRepository.cs // MỚI (Bao gồm các phương thức cho Chapter và ChapterPage)
│   │       ├── ITagRepository.cs     // MỚI
│   │       ├── ITagGroupRepository.cs// MỚI
│   │       ├── ICoverArtRepository.cs// MỚI
│   │       └── ITranslatedMangaRepository.cs // MỚI
│   ├── Features/                     // Tổ chức theo CQRS. Các CommandHandler và QueryHandler sẽ inject IUnitOfWork (và qua đó truy cập các IRepository cần thiết).
│   │   ├── Mangas/                   // Feature cho quản lý Manga
│   │   │   ├── Commands/
│   │   │   │   ├── CreateManga/
│   │   │   │   │   ├── CreateMangaCommand.cs
│   │   │   │   │   ├── CreateMangaCommandHandler.cs
│   │   │   │   │   └── CreateMangaCommandValidator.cs
│   │   │   │   ├── UpdateManga/
│   │   │   │   │   ├── UpdateMangaCommand.cs
│   │   │   │   │   ├── UpdateMangaCommandHandler.cs
│   │   │   │   │   └── UpdateMangaCommandValidator.cs
│   │   │   │   ├── DeleteManga/
│   │   │   │   │   ├── DeleteMangaCommand.cs
│   │   │   │   │   └── DeleteMangaCommandHandler.cs
│   │   │   │   ├── AddMangaTag/
│   │   │   │   │   ├── AddMangaTagCommand.cs
│   │   │   │   │   └── AddMangaTagCommandHandler.cs
│   │   │   │   ├── RemoveMangaTag/
│   │   │   │   │   ├── RemoveMangaTagCommand.cs
│   │   │   │   │   └── RemoveMangaTagCommandHandler.cs
│   │   │   │   ├── AddMangaAuthor/
│   │   │   │   │   ├── AddMangaAuthorCommand.cs
│   │   │   │   │   └── AddMangaAuthorCommandHandler.cs
│   │   │   │   └── RemoveMangaAuthor/
│   │   │   │       ├── RemoveMangaAuthorCommand.cs
│   │   │   │       └── RemoveMangaAuthorCommandHandler.cs
│   │   │   └── Queries/
│   │   │       ├── GetMangaById/
│   │   │       │   ├── GetMangaByIdQuery.cs
│   │   │       │   └── GetMangaByIdQueryHandler.cs
│   │   │       └── GetMangas/
│   │   │           ├── GetMangasQuery.cs
│   │   │           └── GetMangasQueryHandler.cs
│   │   ├── Authors/                  // Feature cho quản lý Author
│   │   │   ├── Commands/
│   │   │   │   ├── CreateAuthor/
│   │   │   │   │   ├── CreateAuthorCommand.cs
│   │   │   │   │   ├── CreateAuthorCommandHandler.cs
│   │   │   │   │   └── CreateAuthorCommandValidator.cs
│   │   │   │   ├── UpdateAuthor/
│   │   │   │   │   ├── UpdateAuthorCommand.cs
│   │   │   │   │   ├── UpdateAuthorCommandHandler.cs
│   │   │   │   │   └── UpdateAuthorCommandValidator.cs
│   │   │   │   └── DeleteAuthor/
│   │   │   │       ├── DeleteAuthorCommand.cs
│   │   │   │       └── DeleteAuthorCommandHandler.cs
│   │   │   └── Queries/
│   │   │       ├── GetAuthorById/
│   │   │       │   ├── GetAuthorByIdQuery.cs
│   │   │       │   └── GetAuthorByIdQueryHandler.cs
│   │   │       └── GetAuthors/
│   │   │           ├── GetAuthorsQuery.cs
│   │   │           └── GetAuthorsQueryHandler.cs
│   │   ├── Chapters/                 // Feature cho quản lý Chapter và ChapterPage
│   │   │   ├── Commands/
│   │   │   │   ├── CreateChapter/
│   │   │   │   │   ├── CreateChapterCommand.cs
│   │   │   │   │   ├── CreateChapterCommandHandler.cs
│   │   │   │   │   └── CreateChapterCommandValidator.cs
│   │   │   │   ├── UpdateChapter/
│   │   │   │   │   ├── UpdateChapterCommand.cs
│   │   │   │   │   ├── UpdateChapterCommandHandler.cs
│   │   │   │   │   └── UpdateChapterCommandValidator.cs
│   │   │   │   ├── DeleteChapter/
│   │   │   │   │   ├── DeleteChapterCommand.cs
│   │   │   │   │   └── DeleteChapterCommandHandler.cs
│   │   │   │   ├── UploadChapterPageImage/
│   │   │   │   │   ├── UploadChapterPageImageCommand.cs
│   │   │   │   │   ├── UploadChapterPageImageCommandHandler.cs
│   │   │   │   │   └── UploadChapterPageImageCommandValidator.cs
│   │   │   │   ├── CreateChapterPageEntry/
│   │   │   │   │   ├── CreateChapterPageEntryCommand.cs
│   │   │   │   │   ├── CreateChapterPageEntryCommandHandler.cs
│   │   │   │   │   └── CreateChapterPageEntryCommandValidator.cs
│   │   │   │   ├── UpdateChapterPageDetails/
│   │   │   │   │   ├── UpdateChapterPageDetailsCommand.cs
│   │   │   │   │   ├── UpdateChapterPageDetailsCommandHandler.cs
│   │   │   │   │   └── UpdateChapterPageDetailsCommandValidator.cs
│   │   │   │   └── DeleteChapterPage/
│   │   │   │       ├── DeleteChapterPageCommand.cs
│   │   │   │       └── DeleteChapterPageCommandHandler.cs
│   │   │   └── Queries/
│   │   │       ├── GetChapterById/
│   │   │       │   ├── GetChapterByIdQuery.cs
│   │   │       │   └── GetChapterByIdQueryHandler.cs
│   │   │       ├── GetChaptersByTranslatedManga/
│   │   │       │   ├── GetChaptersByTranslatedMangaQuery.cs
│   │   │       │   └── GetChaptersByTranslatedMangaQueryHandler.cs
│   │   │       └── GetChapterPages/
│   │   │           ├── GetChapterPagesQuery.cs
│   │   │           └── GetChapterPagesQueryHandler.cs
│   │   ├── CoverArts/                // Feature cho quản lý CoverArt
│   │   │   ├── Commands/
│   │   │   │   ├── UploadCoverArtImage/
│   │   │   │   │   ├── UploadCoverArtImageCommand.cs
│   │   │   │   │   ├── UploadCoverArtImageCommandHandler.cs
│   │   │   │   │   └── UploadCoverArtImageCommandValidator.cs
│   │   │   │   └── DeleteCoverArt/
│   │   │   │       ├── DeleteCoverArtCommand.cs
│   │   │   │       └── DeleteCoverArtCommandHandler.cs
│   │   │   └── Queries/
│   │   │       ├── GetCoverArtById/
│   │   │       │   ├── GetCoverArtByIdQuery.cs
│   │   │       │   └── GetCoverArtByIdQueryHandler.cs
│   │   │       └── GetCoverArtsByManga/
│   │   │           ├── GetCoverArtsByMangaQuery.cs
│   │   │           └── GetCoverArtsByMangaQueryHandler.cs
│   │   ├── Tags/                     // Feature cho quản lý Tag
│   │   │   ├── Commands/
│   │   │   │   ├── CreateTag/
│   │   │   │   │   ├── CreateTagCommand.cs
│   │   │   │   │   ├── CreateTagCommandHandler.cs
│   │   │   │   │   └── CreateTagCommandValidator.cs
│   │   │   │   ├── UpdateTag/
│   │   │   │   │   ├── UpdateTagCommand.cs
│   │   │   │   │   ├── UpdateTagCommandHandler.cs
│   │   │   │   │   └── UpdateTagCommandValidator.cs
│   │   │   │   └── DeleteTag/
│   │   │   │       ├── DeleteTagCommand.cs
│   │   │   │       └── DeleteTagCommandHandler.cs
│   │   │   └── Queries/
│   │   │       ├── GetTagById/
│   │   │       │   ├── GetTagByIdQuery.cs
│   │   │       │   └── GetTagByIdQueryHandler.cs
│   │   │       └── GetTags/
│   │   │           ├── GetTagsQuery.cs
│   │   │           └── GetTagsQueryHandler.cs
│   │   ├── TagGroups/                // Feature cho quản lý TagGroup
│   │   │   ├── Commands/
│   │   │   │   ├── CreateTagGroup/
│   │   │   │   │   ├── CreateTagGroupCommand.cs
│   │   │   │   │   ├── CreateTagGroupCommandHandler.cs
│   │   │   │   │   └── CreateTagGroupCommandValidator.cs
│   │   │   │   ├── UpdateTagGroup/
│   │   │   │   │   ├── UpdateTagGroupCommand.cs
│   │   │   │   │   ├── UpdateTagGroupCommandHandler.cs
│   │   │   │   │   └── UpdateTagGroupCommandValidator.cs
│   │   │   │   └── DeleteTagGroup/
│   │   │   │       ├── DeleteTagGroupCommand.cs
│   │   │   │       └── DeleteTagGroupCommandHandler.cs
│   │   │   └── Queries/
│   │   │       ├── GetTagGroupById/
│   │   │       │   ├── GetTagGroupByIdQuery.cs
│   │   │       │   └── GetTagGroupByIdQueryHandler.cs
│   │   │       └── GetTagGroups/
│   │   │           ├── GetTagGroupsQuery.cs
│   │   │           └── GetTagGroupsQueryHandler.cs
│   │   ├── TranslatedMangas/         // Feature cho quản lý TranslatedManga
│   │   │   ├── Commands/
│   │   │   │   ├── CreateTranslatedManga/
│   │   │   │   │   ├── CreateTranslatedMangaCommand.cs
│   │   │   │   │   ├── CreateTranslatedMangaCommandHandler.cs
│   │   │   │   │   └── CreateTranslatedMangaCommandValidator.cs
│   │   │   │   ├── UpdateTranslatedManga/
│   │   │   │   │   ├── UpdateTranslatedMangaCommand.cs
│   │   │   │   │   ├── UpdateTranslatedMangaCommandHandler.cs
│   │   │   │   │   └── UpdateTranslatedMangaCommandValidator.cs
│   │   │   │   └── DeleteTranslatedManga/
│   │   │   │       ├── DeleteTranslatedMangaCommand.cs
│   │   │   │       └── DeleteTranslatedMangaCommandHandler.cs
│   │   │   └── Queries/
│   │   │       ├── GetTranslatedMangaById/
│   │   │       │   ├── GetTranslatedMangaByIdQuery.cs
│   │   │       │   └── GetTranslatedMangaByIdQueryHandler.cs
│   │   │       └── GetTranslatedMangasByManga/
│   │   │           ├── GetTranslatedMangasByMangaQuery.cs
│   │   │           └── GetTranslatedMangasByMangaQueryHandler.cs
│   └── Application.csproj          // ĐÃ CÓ (cần dependencies: MediatR, AutoMapper, FluentValidation.DependencyInjectionExtensions)
├── Domain/                         // ĐÃ CÓ
│   ├── Common/
│   │   └── AuditableEntity.cs
│   ├── Entities/
│   │   ├── Author.cs
│   │   ├── Chapter.cs
│   │   ├── ChapterPage.cs
│   │   ├── CoverArt.cs
│   │   ├── Manga.cs
│   │   ├── MangaAuthor.cs
│   │   ├── MangaTag.cs
│   │   ├── Tag.cs
│   │   ├── TagGroup.cs
│   │   ├── TranslatedManga.cs
│   │   └── User.cs
│   ├── Enums/
│   │   ├── ContentRating.cs
│   │   ├── MangaStaffRole.cs
│   │   ├── MangaStatus.cs
│   │   └── PublicationDemographic.cs
│   └── Domain.csproj
├── Infrastructure/                 // ĐÃ CÓ
│   ├── Photos/
│   │   ├── CloudinarySettings.cs
│   │   └── PhotoAccessor.cs
│   └── Infrastructure.csproj
├── Persistence/                    // ĐÃ CÓ
│   ├── Data/
│   │   ├── ApplicationDbContext.cs // ĐÃ CÓ, sẽ implement IApplicationDbContext từ Application Layer
│   │   └── Interceptors/
│   │       └── AuditableEntitySaveChangesInterceptor.cs // ĐÃ CÓ
│   ├── Repositories/                 // MỚI: Triển khai các Repository và Unit of Work
│   │   ├── UnitOfWork.cs             // MỚI: Implement IUnitOfWork, inject ApplicationDbContext
│   │   ├── GenericRepository.cs      // MỚI (Tùy chọn, implement IGenericRepository, inject ApplicationDbContext)
│   │   ├── MangaRepository.cs        // MỚI: Implement IMangaRepository, inject ApplicationDbContext
│   │   ├── AuthorRepository.cs       // MỚI
│   │   ├── ChapterRepository.cs      // MỚI
│   │   ├── TagRepository.cs          // MỚI
│   │   ├── TagGroupRepository.cs     // MỚI
│   │   ├── CoverArtRepository.cs     // MỚI
│   │   └── TranslatedMangaRepository.cs // MỚI
│   └── Persistence.csproj          // ĐÃ CÓ
├── MangaReaderDB/                  // (Web API Project - API Quản lý)
│   ├── Controllers/                // Các controller cho API quản lý
│   │   ├── BaseApiController.cs    // Controller cơ sở, chứa Mediator
│   │   ├── MangasController.cs
│   │   ├── AuthorsController.cs
│   │   ├── ChaptersController.cs
│   │   ├── CoverArtsController.cs
│   │   ├── TagsController.cs
│   │   ├── TagGroupsController.cs
│   │   └── TranslatedMangasController.cs
│   ├── Program.cs                  // ĐÃ CÓ (cần cập nhật đăng ký services cho MediatR, AutoMapper, FluentValidation, và các Repository/UnitOfWork mới)
│   └── MangaReaderDB.csproj        // ĐÃ CÓ
├── MangaReaderUserAPI/             // (Web API Project - API cho người dùng đọc truyện) - SẼ LÀM SAU
│   ├── Controllers/
│   │   └── // ...
│   ├── Program.cs
│   └── MangaReaderUserAPI.csproj
└── .gitignore