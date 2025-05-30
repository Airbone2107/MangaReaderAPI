namespace Application.Contracts.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IMangaRepository MangaRepository { get; }
        IAuthorRepository AuthorRepository { get; }
        IChapterRepository ChapterRepository { get; }
        ITagRepository TagRepository { get; }
        ITagGroupRepository TagGroupRepository { get; }
        ICoverArtRepository CoverArtRepository { get; }
        ITranslatedMangaRepository TranslatedMangaRepository { get; }

        /// <summary>
        /// Lưu tất cả các thay đổi được thực hiện trong unit of work này vào database.
        /// </summary>
        /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
        /// <returns>Số lượng state entries được ghi vào database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
} 