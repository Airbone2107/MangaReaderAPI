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
    using Application.Features.Chapters.Commands.CreateChapter;
    using Application.Features.Mangas.Commands.CreateManga;
    using Application.Features.TagGroups.Commands.CreateTagGroup;
    using Application.Features.Tags.Commands.CreateTag;
    using Application.Features.TranslatedMangas.Commands.CreateTranslatedManga;
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

            // TagGroup
            CreateMap<TagGroup, TagGroupAttributesDto>();
            CreateMap<CreateTagGroupDto, TagGroup>();
            CreateMap<UpdateTagGroupDto, TagGroup>();
            CreateMap<CreateTagGroupCommand, TagGroup>();

            // Tag
            CreateMap<Tag, TagAttributesDto>()
                .ForMember(dest => dest.TagGroupName, opt => opt.MapFrom(src => src.TagGroup != null ? src.TagGroup.Name : string.Empty));
            CreateMap<CreateTagDto, Tag>();
            CreateMap<UpdateTagDto, Tag>();
            CreateMap<CreateTagCommand, Tag>();

            // Manga
            CreateMap<Manga, MangaAttributesDto>();
            CreateMap<CreateMangaDto, Manga>(); // DTO to Entity
            CreateMap<UpdateMangaDto, Manga>(); // DTO to Entity
            CreateMap<CreateMangaCommand, Manga>(); // Command to Entity

            // TranslatedManga
            CreateMap<TranslatedManga, TranslatedMangaAttributesDto>();
            CreateMap<CreateTranslatedMangaDto, TranslatedManga>();
            CreateMap<UpdateTranslatedMangaDto, TranslatedManga>();
            CreateMap<CreateTranslatedMangaCommand, TranslatedManga>();

            // CoverArt
            CreateMap<CoverArt, CoverArtAttributesDto>();
            CreateMap<CreateCoverArtDto, CoverArt>(); // Dùng khi tạo entry, PublicId sẽ được cập nhật sau
            // CreateCoverArtCommand sẽ không map trực tiếp sang CoverArt vì cần xử lý file.

            // ChapterPage
            CreateMap<ChapterPage, ChapterPageAttributesDto>();
            CreateMap<CreateChapterPageDto, ChapterPage>(); // Dùng khi tạo entry
            CreateMap<UpdateChapterPageDto, ChapterPage>();
            // CreateChapterPageEntryCommand sẽ không map trực tiếp sang ChapterPage vì cần xử lý file.

            // Chapter
            CreateMap<Chapter, ChapterAttributesDto>()
                .ForMember(dest => dest.PagesCount, opt => opt.MapFrom(src => src.ChapterPages.Count));
            CreateMap<CreateChapterDto, Chapter>();
            CreateMap<UpdateChapterDto, Chapter>();
            CreateMap<CreateChapterCommand, Chapter>();
        }
    }
} 