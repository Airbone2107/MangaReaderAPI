// MangaReaderDB/Program.cs
using Application.Common.Interfaces;
using Application.Contracts.Persistence; // Thêm using cho IUnitOfWork và các IRepository
using Domain.Constants; // <<< THÊM MỚI
using Domain.Entities;
using FluentValidation;
using Infrastructure.Authentication;
using Infrastructure.Photos;
using MangaReaderDB.Middleware; // Thêm using cho Middleware
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistence.Data;
using Persistence.Data.Interceptors;
using Persistence.Repositories; // Thêm using cho UnitOfWork và các Repository
using System.Reflection; // <<< THÊM MỚI
using System.Text;
using System.Text.Json.Serialization; // Thêm using này

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Đăng ký Interceptor như một Singleton
builder.Services.AddSingleton<AuditableEntitySaveChangesInterceptor>();

// Đăng ký ApplicationDbContext và thêm Interceptor
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
               // Cấu hình Query Splitting Behavior
               sqlServerOptionsAction: sqlOptions =>
               {
                   sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
               })
           .AddInterceptors(interceptor);
});

// Cấu hình CloudinarySettings
// Đọc từ section "CloudinarySettings" trong appsettings.json
builder.Services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

// Đăng ký PhotoAccessor
builder.Services.AddScoped<IPhotoAccessor, PhotoAccessor>();

// Đăng ký MediatR từ Application assembly
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly));

// Đăng ký AutoMapper từ Application assembly
builder.Services.AddAutoMapper(typeof(Application.AssemblyReference).Assembly);

// Đăng ký FluentValidation validators từ Application assembly
builder.Services.AddValidatorsFromAssembly(typeof(Application.AssemblyReference).Assembly, ServiceLifetime.Scoped);

// Đăng ký các Interface và Implementation cho DI
// Đăng ký IApplicationDbContext để AuthController có thể sử dụng
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
// Đăng ký ITokenService để AuthController có thể tạo token
builder.Services.AddScoped<ITokenService, TokenService>();

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

// Cấu hình đọc Settings
builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

// Cấu hình Identity Core
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cấu hình Authentication với JWT Bearer
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JwtSettings:Issuer"],
        ValidAudience = configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!))
    };
});

// Cấu hình Authorization với Policies
builder.Services.AddAuthorization(options =>
{
    // Tự động tạo policies cho tất cả các permission
    // Lấy tất cả các hằng số chuỗi public từ lớp lồng nhau trong Permissions
    var permissions = typeof(Permissions)
        .GetNestedTypes()
        .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
        .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
        .Select(x => (string)x.GetRawConstantValue()!)
        .ToList();

    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
    }
});

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

// Cấu hình Swagger để hỗ trợ JWT
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Gọi hàm Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>(); // <<< THÊM MỚI
        await Persistence.Data.SeedData.SeedEssentialsAsync(userManager, roleManager); // <<< THAY ĐỔI TÊN HÀM
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during data seeding.");
    }
}

app.Run();