# TODO.md: Loại bỏ Đăng ký công khai và Sửa lỗi Đăng nhập

Hướng dẫn này sẽ giúp bạn thực hiện hai mục tiêu chính:
1.  **Loại bỏ endpoint `POST /api/Auth/register`**: Hướng hệ thống đến việc quản lý người dùng bởi quản trị viên thay vì cho phép đăng ký công khai.
2.  **Khắc phục lỗi `Unable to resolve service for type 'ITokenService'`**: Sửa lỗi Dependency Injection (DI) để endpoint `POST /api/Auth/login` hoạt động chính xác.

---

### Bước 1: Khắc phục lỗi Dependency Injection (DI)

Lỗi "Unable to resolve service for type 'Application.Common.Interfaces.ITokenService'" xảy ra vì `AuthController` yêu cầu một `ITokenService` và `IApplicationDbContext` trong constructor của nó, nhưng chúng ta chưa "dạy" cho hệ thống cách tạo ra các dịch vụ này. Chúng ta cần đăng ký chúng trong container DI.

**Công việc**: Cập nhật file `Program.cs` để đăng ký `ITokenService` và `IApplicationDbContext`.

**File cần chỉnh sửa**: `MangaReaderDB/Program.cs`

**Nội dung đầy đủ sau khi chỉnh sửa**:

```csharp
// path: MangaReaderDB/Program.cs
using Application.Common.Interfaces; // <<< THÊM MỚI
using Application.Contracts.Persistence;
using Domain.Constants;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Authentication;
using Infrastructure.Photos;
using MangaReaderDB.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Persistence.Data; // <<< THÊM MỚI
using Persistence.Data.Interceptors;
using Persistence.Repositories;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

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

// <<< BẮT ĐẦU KHỐI THÊM MỚI >>>
// Đăng ký các Interface và Implementation cho DI
// Đăng ký IApplicationDbContext để AuthController có thể sử dụng
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
// Đăng ký ITokenService để AuthController có thể tạo token
builder.Services.AddScoped<ITokenService, TokenService>(); 
// <<< KẾT THÚC KHỐI THÊM MỚI >>>

// Đăng ký UnitOfWork và Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMangaRepository, MangaRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITagGroupRepository, TagGroupRepository>();
builder.Services.AddScoped<ICoverArtRepository, CoverArtRepository>();
builder.Services.AddScoped<ITranslatedMangaRepository, TranslatedMangaRepository>();

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
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MangaReader API V1");
    });
    app.UseReDoc(c =>
    {
        c.DocumentTitle = "MangaReader API Documentation (ReDoc)";
        c.SpecUrl = "/swagger/v1/swagger.json";
        c.RoutePrefix = "docs";
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
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await Persistence.Data.SeedData.SeedEssentialsAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during data seeding.");
    }
}

app.Run();
```

---

### Bước 2: Loại bỏ Endpoint `POST /api/Auth/register`

Bây giờ, chúng ta sẽ xóa phương thức xử lý việc đăng ký công khai khỏi `AuthController`. Việc tạo người dùng mới sẽ được xử lý thông qua `UsersController` bởi người dùng có quyền hạn.

**Công việc**: Xóa phương thức `Register` khỏi `AuthController.cs`.

**File cần chỉnh sửa**: `MangaReaderDB/Controllers/AuthController.cs`

**Nội dung đầy đủ sau khi chỉnh sửa**:

```csharp
// path: MangaReaderDB/Controllers/AuthController.cs
using Application.Common.DTOs.Auth;
using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MangaReaderDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IApplicationDbContext context,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
            _logger = logger;
        }

        // <<< BẮT ĐẦU XÓA KHỐI CODE >>>
        // [HttpPost("register")]
        // public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        // {
        //     var user = new ApplicationUser { UserName = registerDto.Username, Email = registerDto.Email };
        //     var result = await _userManager.CreateAsync(user, registerDto.Password);
        //     if (result.Succeeded)
        //     {
        //         return Ok(new AuthResponseDto { IsSuccess = true, Message = "User registered successfully!" });
        //     }
        //     return BadRequest(new AuthResponseDto { IsSuccess = false, Message = string.Join(" | ", result.Errors.Select(e => e.Description)) });
        // }
        // <<< KẾT THÚC XÓA KHỐI CODE >>>

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid username or password." });
            }

            var accessToken = await _tokenService.CreateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync(CancellationToken.None);
            
            return Ok(new AuthResponseDto 
            { 
                IsSuccess = true, 
                Message = "Login successful!",
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var storedToken = await _context.RefreshTokens.Include(rt => rt.ApplicationUser)
                                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (storedToken == null || !storedToken.IsActive)
            {
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid or expired refresh token." });
            }
            
            var user = storedToken.ApplicationUser;
            if (user == null)
            {
                 return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "User not found for this token." });
            }

            // Tạo token mới
            var newAccessToken = await _tokenService.CreateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);
            
            // Thu hồi token cũ và thêm token mới
            storedToken.Revoked = DateTime.UtcNow;
            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Token refreshed successfully!",
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            });
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
             var storedToken = await _context.RefreshTokens
                                    .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);
            
            // User chỉ có thể thu hồi token của chính mình
            if (storedToken == null || storedToken.ApplicationUserId != userId)
            {
                 return BadRequest(new AuthResponseDto { IsSuccess = false, Message = "Invalid token." });
            }

            storedToken.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync(CancellationToken.None);

            return Ok(new AuthResponseDto { IsSuccess = true, Message = "Token revoked successfully." });
        }
    }
}
```

---

## Hoàn tất

Sau khi hoàn thành hai bước trên, bạn đã:
1.  **Khắc phục thành công lỗi DI**: `AuthController` giờ đây có thể nhận được các dịch vụ `ITokenService` và `IApplicationDbContext` mà nó cần, cho phép endpoint `login` hoạt động bình thường.
2.  **Loại bỏ endpoint đăng ký công khai**: Giúp tăng cường bảo mật và kiểm soát việc tạo người dùng mới.

**Bước tiếp theo được đề xuất**:
1.  Chạy lại ứng dụng.
2.  Thử đăng nhập bằng endpoint `POST /api/Auth/login` với tài khoản `superadmin`/`123456` đã được seed. Lần này, bạn sẽ nhận được Access Token và Refresh Token thành công.
3.  Kiểm tra rằng endpoint `POST /api/Auth/register` không còn tồn tại (sẽ trả về lỗi 404).

Chúc bạn thành công