namespace Application.Common.Mappings
{
    using Application.Common.DTOs.Authors;
    using Application.Common.DTOs.Chapters;
    using Application.Common.DTOs.CoverArts;
    using Application.Common.DTOs.Mangas;
    using Application.Common.DTOs.TagGroups;
    using Application.Common.DTOs.Tags;
    using Application.Common.DTOs.TranslatedMangas;
    using Application.Common.DTOs.Users;
    using Application.Features.Authors.Commands.CreateAuthor;
    using Application.Features.Authors.Commands.UpdateAuthor;
    using Application.Features.Chapters.Commands.CreateChapter;
    using Application.Features.Chapters.Commands.UpdateChapter;
    using Application.Features.Mangas.Commands.CreateManga;
    using Application.Features.Mangas.Commands.UpdateManga;
    using Application.Features.TagGroups.Commands.CreateTagGroup;
    using Application.Features.TagGroups.Commands.UpdateTagGroup;
    using Application.Features.Tags.Commands.CreateTag;
    using Application.Features.Tags.Commands.UpdateTag;
    using Application.Features.TranslatedMangas.Commands.CreateTranslatedManga;
    using Application.Features.TranslatedMangas.Commands.UpdateTranslatedManga;
    using AutoMapper;
    using Domain.Entities;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User
            CreateMap<User, UserAttributesDto>();

            // Author
            CreateMap<Author, AuthorAttributesDto>();
            CreateMap<CreateAuthorDto, Author>(); // DTO to Entity
            CreateMap<UpdateAuthorDto, Author>(); // DTO to Entity
            CreateMap<CreateAuthorCommand, Author>(); // Command to Entity
            CreateMap<UpdateAuthorCommand, Author>(); // Command to Entity (for updating existing entity)

            // TagGroup
            CreateMap<TagGroup, TagGroupAttributesDto>();
            CreateMap<CreateTagGroupDto, TagGroup>(); // DTO to Entity
            CreateMap<UpdateTagGroupDto, TagGroup>(); // DTO to Entity
            CreateMap<CreateTagGroupCommand, TagGroup>(); // Command to Entity
            CreateMap<UpdateTagGroupCommand, TagGroup>(); // Command to Entity (for updating existing entity)

            // Tag
            CreateMap<Tag, TagAttributesDto>()
                .ForMember(dest => dest.TagGroupName, opt => opt.MapFrom(src => src.TagGroup != null ? src.TagGroup.Name : string.Empty));
            CreateMap<Tag, TagInMangaAttributesDto>()
                .ForMember(dest => dest.TagGroupName, opt => opt.MapFrom(src => src.TagGroup != null ? src.TagGroup.Name : string.Empty));
            CreateMap<CreateTagDto, Tag>(); // DTO to Entity
            CreateMap<UpdateTagDto, Tag>(); // DTO to Entity
            CreateMap<CreateTagCommand, Tag>(); // Command to Entity
            CreateMap<UpdateTagCommand, Tag>(); // Command to Entity (for updating existing entity)

            // Manga
            CreateMap<Manga, MangaAttributesDto>();
            CreateMap<CreateMangaDto, Manga>(); // DTO to Entity
            CreateMap<UpdateMangaDto, Manga>(); // DTO to Entity
            CreateMap<CreateMangaCommand, Manga>(); // Command to Entity
            CreateMap<UpdateMangaCommand, Manga>(); // Command to Entity (for updating existing entity)

            // TranslatedManga
            CreateMap<TranslatedManga, TranslatedMangaAttributesDto>();
            CreateMap<CreateTranslatedMangaDto, TranslatedManga>(); // DTO to Entity
            CreateMap<UpdateTranslatedMangaDto, TranslatedManga>(); // DTO to Entity
            CreateMap<CreateTranslatedMangaCommand, TranslatedManga>(); // Command to Entity
            CreateMap<UpdateTranslatedMangaCommand, TranslatedManga>(); // Command to Entity (for updating existing entity)

            // CoverArt
            CreateMap<CoverArt, CoverArtAttributesDto>();
            CreateMap<CreateCoverArtDto, CoverArt>(); // Dùng khi tạo entry, PublicId sẽ được cập nhật sau
            // UpdateCoverArtDto và UpdateCoverArtCommand không có trong các file cung cấp, nếu có sẽ cần mapping tương ứng.
            // CreateCoverArtCommand sẽ không map trực tiếp sang CoverArt vì cần xử lý file.

            // ChapterPage
            CreateMap<ChapterPage, ChapterPageAttributesDto>();
            CreateMap<CreateChapterPageDto, ChapterPage>(); // Dùng khi tạo entry
            CreateMap<UpdateChapterPageDto, ChapterPage>(); // DTO to Entity
            // UpdateChapterPageDetailsCommand được xử lý thủ công trong Handler, không dùng AutoMapper trực tiếp từ Command sang Entity.
            // CreateChapterPageEntryCommand sẽ không map trực tiếp sang ChapterPage vì cần xử lý file.

            // Chapter
            CreateMap<Chapter, ChapterAttributesDto>()
                .ForMember(dest => dest.PagesCount, opt => opt.MapFrom(src => src.ChapterPages.Count));
            CreateMap<CreateChapterDto, Chapter>(); // DTO to Entity
            CreateMap<UpdateChapterDto, Chapter>(); // DTO to Entity
            CreateMap<CreateChapterCommand, Chapter>(); // Command to Entity
            CreateMap<UpdateChapterCommand, Chapter>(); // Command to Entity (for updating existing entity)
        }
    }
}