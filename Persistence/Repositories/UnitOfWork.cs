using Application.Contracts.Persistence;
using Persistence.Data;

namespace Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IMangaRepository? _mangaRepository;
        private IAuthorRepository? _authorRepository;
        private IChapterRepository? _chapterRepository;
        private ITagRepository? _tagRepository;
        private ITagGroupRepository? _tagGroupRepository;
        private ICoverArtRepository? _coverArtRepository;
        private ITranslatedMangaRepository? _translatedMangaRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IMangaRepository MangaRepository => 
            _mangaRepository ??= new MangaRepository(_context);

        public IAuthorRepository AuthorRepository => 
            _authorRepository ??= new AuthorRepository(_context);

        public IChapterRepository ChapterRepository => 
            _chapterRepository ??= new ChapterRepository(_context);

        public ITagRepository TagRepository => 
            _tagRepository ??= new TagRepository(_context);

        public ITagGroupRepository TagGroupRepository => 
            _tagGroupRepository ??= new TagGroupRepository(_context);

        public ICoverArtRepository CoverArtRepository => 
            _coverArtRepository ??= new CoverArtRepository(_context);

        public ITranslatedMangaRepository TranslatedMangaRepository => 
            _translatedMangaRepository ??= new TranslatedMangaRepository(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // AuditableEntitySaveChangesInterceptor sẽ tự động xử lý CreatedAt, UpdatedAt
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
} 