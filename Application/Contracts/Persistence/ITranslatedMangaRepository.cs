using Domain.Entities;

namespace Application.Contracts.Persistence
{
    public interface ITranslatedMangaRepository : IGenericRepository<TranslatedManga>
    {
        /// <summary>
        /// Lấy TranslatedManga theo MangaId và LanguageKey.
        /// </summary>
        Task<TranslatedManga?> GetByMangaIdAndLanguageAsync(Guid mangaId, string languageKey);

        /// <summary>
        /// Lấy danh sách các TranslatedManga theo MangaId.
        /// </summary>
        Task<IReadOnlyList<TranslatedManga>> GetByMangaIdAsync(Guid mangaId);
    }
} 