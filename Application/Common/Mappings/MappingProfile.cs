namespace Application.Common.Mappings
{
    using AutoMapper;
    using Application.Common.DTOs.Authors;
    using Application.Common.DTOs.Chapters;
    using Application.Common.DTOs.CoverArts;
    using Application.Common.DTOs.Mangas;
    using Application.Common.DTOs.TagGroups;
    using Application.Common.DTOs.Tags;
    using Application.Common.DTOs.TranslatedMangas;
    using Application.Common.DTOs.Users;
    using Domain.Entities;
    using Application.Features.Mangas.Commands.CreateManga;
    using Application.Features.Authors.Commands.CreateAuthor;
    using Application.Features.TagGroups.Commands.CreateTagGroup;
    using Application.Features.Tags.Commands.CreateTag;
    using Application.Features.TranslatedMangas.Commands.CreateTranslatedManga;
    using Application.Features.Chapters.Commands.CreateChapter;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User
            CreateMap<User, UserDto>();

            // Author
            CreateMap<Author, AuthorDto>();
            CreateMap<CreateAuthorDto, Author>(); // DTO to Entity
            CreateMap<UpdateAuthorDto, Author>(); // DTO to Entity
            CreateMap<CreateAuthorCommand, Author>(); // Command to Entity

            // TagGroup
            CreateMap<TagGroup, TagGroupDto>();
            CreateMap<CreateTagGroupDto, TagGroup>();
            CreateMap<UpdateTagGroupDto, TagGroup>();
            CreateMap<CreateTagGroupCommand, TagGroup>();

            // Tag
            CreateMap<Tag, TagDto>()
                .ForMember(dest => dest.TagGroupName, opt => opt.MapFrom(src => src.TagGroup != null ? src.TagGroup.Name : string.Empty));
            CreateMap<CreateTagDto, Tag>();
            CreateMap<UpdateTagDto, Tag>();
            CreateMap<CreateTagCommand, Tag>();

            // Manga
            CreateMap<Manga, MangaDto>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.MangaTags.Select(mt => mt.Tag)))
                .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.MangaAuthors.Select(ma => ma.Author))); // Cần map Author sang AuthorDto, AutoMapper sẽ tự làm nếu có mapping Author -> AuthorDto
                // .ForMember(dest => dest.Authors, opt => opt.MapFrom(src => src.MangaAuthors.Select(ma => new AuthorDto { AuthorId = ma.Author.AuthorId, Name = ma.Author.Name, Role = ma.Role }))) // Nếu cần map Author kèm Role
            
            CreateMap<CreateMangaDto, Manga>(); // DTO to Entity
            CreateMap<UpdateMangaDto, Manga>(); // DTO to Entity
            CreateMap<CreateMangaCommand, Manga>(); // Command to Entity

            // TranslatedManga
            CreateMap<TranslatedManga, TranslatedMangaDto>();
            CreateMap<CreateTranslatedMangaDto, TranslatedManga>();
            CreateMap<UpdateTranslatedMangaDto, TranslatedManga>();
            CreateMap<CreateTranslatedMangaCommand, TranslatedManga>();

            // CoverArt
            CreateMap<CoverArt, CoverArtDto>();
            CreateMap<CreateCoverArtDto, CoverArt>(); // Dùng khi tạo entry, PublicId sẽ được cập nhật sau
            // CreateCoverArtCommand sẽ không map trực tiếp sang CoverArt vì cần xử lý file.

            // ChapterPage
            CreateMap<ChapterPage, ChapterPageDto>();
            CreateMap<CreateChapterPageDto, ChapterPage>(); // Dùng khi tạo entry
            CreateMap<UpdateChapterPageDto, ChapterPage>();
            // CreateChapterPageEntryCommand sẽ không map trực tiếp sang ChapterPage vì cần xử lý file.

            // Chapter
            CreateMap<Chapter, ChapterDto>()
                .ForMember(dest => dest.Uploader, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.PagesCount, opt => opt.MapFrom(src => src.ChapterPages.Count))
                .ForMember(dest => dest.ChapterPages, opt => opt.MapFrom(src => src.ChapterPages.OrderBy(p => p.PageNumber)));
            CreateMap<CreateChapterDto, Chapter>();
            CreateMap<UpdateChapterDto, Chapter>();
            CreateMap<CreateChapterCommand, Chapter>();
        }
    }
} 