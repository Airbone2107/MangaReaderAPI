// MangaReaderDB/Program.cs
using Application.Common.Interfaces;
using Application.Contracts.Persistence; // Thêm using cho IUnitOfWork và các IRepository
using FluentValidation;
using Infrastructure.Photos;
using MangaReaderDB.Middleware; // Thêm using cho Middleware
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
using Persistence.Data.Interceptors;
using Persistence.Repositories; // Thêm using cho UnitOfWork và các Repository
using System.Text.Json.Serialization; // Thêm using này

var builder = WebApplication.CreateBuilder(args);

// Đăng ký Interceptor như một Singleton
builder.Services.AddSingleton<AuditableEntitySaveChangesInterceptor>();

// Đăng ký ApplicationDbContext và thêm Interceptor
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
               // Cấu hình Query Splitting Behavior
               sqlServerOptionsAction: sqlOptions =>
               {
                   sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
               })
           .AddInterceptors(interceptor);
});

// Cấu hình CloudinarySettings
// Đọc từ section "CloudinarySettings" trong appsettings.json
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Đăng ký PhotoAccessor
builder.Services.AddScoped<IPhotoAccessor, PhotoAccessor>();

// Đăng ký MediatR từ Application assembly
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly));

// Đăng ký AutoMapper từ Application assembly
builder.Services.AddAutoMapper(typeof(Application.AssemblyReference).Assembly);

// Đăng ký FluentValidation validators từ Application assembly
builder.Services.AddValidatorsFromAssembly(typeof(Application.AssemblyReference).Assembly, ServiceLifetime.Scoped);

// Đăng ký UnitOfWork và Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMangaRepository, MangaRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITagGroupRepository, TagGroupRepository>();
builder.Services.AddScoped<ICoverArtRepository, CoverArtRepository>();
builder.Services.AddScoped<ITranslatedMangaRepository, TranslatedMangaRepository>();
// Không cần đăng ký IGenericRepository vì nó thường được sử dụng như một base class

// Các services khác của ASP.NET Core
builder.Services.AddControllers()
    .AddJsonOptions(options => // THÊM KHỐI NÀY
    {
        // Sử dụng JsonStringEnumConverter để chuyển đổi enum thành chuỗi và ngược lại
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Đảm bảo Property Naming Policy là camelCase (mặc định của ASP.NET Core API)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // Các tùy chọn JsonSerializerOptions khác có thể được thêm vào đây nếu cần
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Đăng ký ExceptionMiddleware ở đầu pipeline
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // Sử dụng Swagger UI (giao diện mặc định của Swashbuckle)
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MangaReader API V1");
        // Bạn có thể giữ lại Swagger UI hoặc chỉ dùng ReDoc
    });

    // Thêm cấu hình để sử dụng ReDoc UI
    app.UseReDoc(c =>
    {
        c.DocumentTitle = "MangaReader API Documentation (ReDoc)";
        c.SpecUrl = "/swagger/v1/swagger.json"; // Đường dẫn đến file swagger.json
        c.RoutePrefix = "docs"; // Đường dẫn để truy cập ReDoc UI, ví dụ: /docs
        // Các tùy chọn khác của ReDoc có thể được cấu hình ở đây
        // c.EnableUntrustedSpec();
        // c.HideHostname();
        // c.HideDownloadButton();
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();