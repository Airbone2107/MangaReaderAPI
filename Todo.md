

Dựa trên các yêu cầu mới của bạn, chúng ta đã có câu trả lời cho các câu hỏi trước. Tôi xin tóm tắt lại các điểm chính chúng ta sẽ thực hiện:
*   **Access Token (JWT):** Thời hạn 1 ngày.
*   **Refresh Token:** Sẽ được triển khai để làm mới Access Token, có thời hạn 7 ngày.
*   **Di dời dữ liệu:** Sẽ tạo một người dùng "Admin" mới trong hệ thống Identity, sau đó cập nhật tất cả các bản ghi `Chapter` cũ để trỏ đến người dùng "Admin" này.
*   **Mật khẩu:** Sẽ được cấu hình ở mức đơn giản nhất để tiện cho việc demo.

Dưới đây là tệp `TODO.md` chi tiết và hoàn chỉnh.

***

# TODO.md: Triển Khai ASP.NET Core Identity với JWT và Refresh Token

Hướng dẫn này sẽ giúp bạn tích hợp ASP.NET Core Identity, xác thực bằng JSON Web Tokens (JWT), triển khai Refresh Token, và thực hiện di dời dữ liệu người dùng cũ một cách an toàn.

## Mục tiêu

*   Thay thế entity `User` hiện tại bằng `ApplicationUser` của ASP.NET Core Identity.
*   Thêm cơ chế Refresh Token để duy trì phiên đăng nhập.
*   Cấu hình cơ sở dữ liệu để hoạt động với Identity.
*   Tạo các API endpoint cho việc đăng ký (`/api/auth/register`), đăng nhập (`/api/auth/login`), làm mới token (`/api/auth/refresh`), và thu hồi token (`/api/auth/revoke`).
*   Tạo một tài khoản Admin mặc định và di dời các bản ghi cũ trỏ đến tài khoản này.
*   Bảo vệ các API endpoint bằng JWT.

---

### Bước 1: Cài đặt Gói NuGet Bổ Sung cho JWT

Bạn đã cài đặt các gói Identity cơ bản. Giờ chúng ta cần thêm gói NuGet để xử lý JWT.

1.  Mở **Package Manager Console**.
2.  Chọn dự án **`MangaReaderDB`** làm dự án mặc định (Default project).
3.  Chạy lệnh sau:

    ```powershell
    Install-Package Microsoft.AspNetCore.Authentication.JwtBearer -ProjectName MangaReaderDB
    ```

### Bước 2: Cập nhật `Domain` Layer

Chúng ta sẽ thay thế `User` cũ bằng `ApplicationUser` và thêm một entity mới cho `RefreshToken`.

1.  **Xóa file `User.cs` cũ:**
    *   Trong Solution Explorer, tìm đến file `Domain\Entities\User.cs` và xóa nó.

2.  **Tạo file `RefreshToken.cs` mới:**
    *   Trong thư mục `Domain\Entities`, tạo file mới `RefreshToken.cs`. Đây là entity để lưu trữ các refresh token trong database.

    ```csharp
    // path: Domain/Entities/RefreshToken.cs
    using Microsoft.EntityFrameworkCore;
    using System;

    namespace Domain.Entities
    {
        [Owned] // Đánh dấu là Owned Entity Type nếu bạn muốn nó được chứa trong bảng AspNetUsers
                 // Hoặc bỏ đi để tạo bảng riêng. Tạo bảng riêng sẽ linh hoạt hơn.
        public class RefreshToken
        {
            public int Id { get; set; }
            public string Token { get; set; } = string.Empty;
            public DateTime Expires { get; set; }
            public bool IsExpired => DateTime.UtcNow >= Expires;
            public DateTime Created { get; set; }
            public DateTime? Revoked { get; set; }
            public bool IsActive => Revoked == null && !IsExpired;

            // Foreign Key
            public string ApplicationUserId { get; set; } = string.Empty;
            public ApplicationUser? ApplicationUser { get; set; }
        }
    }
    ```

3.  **Cập nhật file `ApplicationUser.cs`:**
    *   Tạo file `ApplicationUser.cs` trong `Domain\Entities`.
    *   Thêm quan hệ điều hướng đến `RefreshToken` và `Chapter`.

    ```csharp
    // path: Domain/Entities/ApplicationUser.cs
    using Microsoft.AspNetCore.Identity;
    using System.Collections.Generic;

    namespace Domain.Entities
    {
        public class ApplicationUser : IdentityUser
        {
            public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
            public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        }
    }
    ```

4.  **Cập nhật lớp `Chapter.cs`:**
    *   Mở file `Domain\Entities\Chapter.cs`.
    *   Thay đổi kiểu của `UploadedByUserId` từ `int` thành `string` và `User` thành `ApplicationUser`.

    ```csharp
    // path: Domain/Entities/Chapter.cs
    using Domain.Common;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace Domain.Entities
    {
        public class Chapter : AuditableEntity
        {
            [Key]
            public Guid ChapterId { get; protected set; } = Guid.NewGuid();
            [Required]
            public Guid TranslatedMangaId { get; set; }
            public virtual TranslatedManga TranslatedManga { get; set; } = null!;
            
            // <<< THAY ĐỔI >>>
            public string UploadedByUserId { get; set; } = string.Empty;
            public virtual ApplicationUser User { get; set; } = null!;

            // ... các thuộc tính khác giữ nguyên ...
        }
    }
    ```

5.  **Cập nhật `IApplicationDbContext.cs`:**
    *   Mở `Application\Common\Interfaces\IApplicationDbContext.cs`.
    *   Xóa `DbSet<User> Users` và thêm `DbSet<RefreshToken>`.

    ```csharp
    // path: Application/Common/Interfaces/IApplicationDbContext.cs
    using Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    namespace Application.Common.Interfaces
    {
        public interface IApplicationDbContext
        {
            // DbSet<User> Users { get; } // <<< XÓA DÒNG NÀY
            DbSet<RefreshToken> RefreshTokens { get; } // <<< THÊM DÒNG NÀY
            
            DbSet<Author> Authors { get; }
            // ... các DbSet khác giữ nguyên
        }
    }
    ```

### Bước 3: Cấu hình `DbContext` và Migration

1.  **Cập nhật `ApplicationDbContext.cs`:**
    *   Kế thừa `IdentityDbContext<ApplicationUser>`.
    *   Thêm `DbSet<RefreshToken>`.
    *   Gọi `base.OnModelCreating(modelBuilder)` đầu tiên.
    *   Xóa cấu hình cho `User` cũ và cập nhật cấu hình cho `Chapter`.
    *   Thêm cấu hình cho `RefreshToken`.

    ```csharp
    // path: Persistence/Data/ApplicationDbContext.cs
    using Application.Common.Interfaces;
    using Domain.Entities;
    using Domain.Enums;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    namespace Persistence.Data
    {
        public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

            public DbSet<RefreshToken> RefreshTokens { get; set; } = null!; // <<< THÊM MỚI
            public DbSet<Author> Authors { get; set; } = null!;
            // ... các DbSet khác giữ nguyên

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder); // <<< LUÔN GỌI ĐẦU TIÊN

                // Xóa cấu hình User cũ
                
                // Cấu hình RefreshToken
                modelBuilder.Entity<RefreshToken>(entity =>
                {
                    entity.HasKey(rt => rt.Id);
                    entity.HasIndex(rt => rt.Token).IsUnique();
                    entity.Property(rt => rt.Token).IsRequired();
                    entity.HasOne(rt => rt.ApplicationUser)
                          .WithMany(u => u.RefreshTokens)
                          .HasForeignKey(rt => rt.ApplicationUserId);
                });

                // Cập nhật cấu hình Chapter
                modelBuilder.Entity<Chapter>(entity =>
                {
                    // ... các cấu hình khác của Chapter
                    entity.HasOne(c => c.User)
                          .WithMany(u => u.Chapters)
                          .HasForeignKey(c => c.UploadedByUserId)
                          .OnDelete(DeleteBehavior.Restrict); // <<< THAY ĐỔI
                });

                // ... giữ lại các cấu hình entity khác của bạn ...
            }
        }
    }
    ```

2.  **Tạo và áp dụng Migration:**
    *   Mở **Package Manager Console**.
    *   Chọn dự án mặc định là **`Persistence`**.
    *   Chạy lệnh: `Add-Migration AddIdentityAndRefreshToken`
    *   Chạy lệnh: `Update-Database`

### Bước 4: Cấu hình JWT và Refresh Token

1.  **Cập nhật `appsettings.json`:**
    *   Thêm `RefreshTokenTTLInDays` và đặt `DurationInMinutes` thành `1440` (1 ngày).

    ```json
    // path: MangaReaderDB/appsettings.json
    {
      // ...
      "JwtSettings": {
        "Key": "SuperSecretKeyForYourMangaReaderAPIProjectPleaseChangeThisNow",
        "Issuer": "MangaReaderAPI",
        "Audience": "MangaReaderAPI.Users",
        "DurationInMinutes": 1440, // <<< 1 ngày
        "RefreshTokenTTLInDays": 7 // <<< THÊM MỚI
      },
      // ...
    }
    ```

2.  **Cập nhật `JwtSettings.cs`:**

    ```csharp
    // path: Infrastructure/Authentication/JwtSettings.cs
    namespace Infrastructure.Authentication
    {
        public class JwtSettings
        {
            public string Key { get; init; } = string.Empty;
            public string Issuer { get; init; } = string.Empty;
            public string Audience { get; init; } = string.Empty;
            public int DurationInMinutes { get; init; }
            public int RefreshTokenTTLInDays { get; init; } // <<< THÊM MỚI
        }
    }
    ```

3.  **Cập nhật `ITokenService.cs`:**

    ```csharp
    // path: Application/Common/Interfaces/ITokenService.cs
    using Domain.Entities;
    using System.Threading.Tasks;

    namespace Application.Common.Interfaces
    {
        public interface ITokenService
        {
            Task<string> CreateToken(ApplicationUser user);
            RefreshToken GenerateRefreshToken(string userId); // <<< THAY ĐỔI: Nhận userId
        }
    }
    ```

4.  **Cập nhật `TokenService.cs`:**

    ```csharp
    // path: Infrastructure/Authentication/TokenService.cs
    using Application.Common.Interfaces;
    using Domain.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography; // <<< THÊM MỚI
    using System.Text;
    using System.Threading.Tasks;

    namespace Infrastructure.Authentication
    {
        public class TokenService : ITokenService
        {
            private readonly JwtSettings _jwtSettings;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly SymmetricSecurityKey _key;

            public TokenService(IOptions<JwtSettings> jwtSettings, UserManager<ApplicationUser> userManager)
            {
                _jwtSettings = jwtSettings.Value;
                _userManager = userManager;
                _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            }

            public async Task<string> CreateToken(ApplicationUser user)
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName!)
                };

                var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                    SigningCredentials = creds,
                    Issuer = _jwtSettings.Issuer,
                    Audience = _jwtSettings.Audience
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }

            public RefreshToken GenerateRefreshToken(string userId)
            {
                var randomNumber = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomNumber);
                
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomNumber),
                    Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenTTLInDays),
                    Created = DateTime.UtcNow,
                    ApplicationUserId = userId
                };
            }
        }
    }
    ```

### Bước 5: Cấu hình Services trong `Program.cs`

1.  Mở `MangaReaderDB\Program.cs` và cập nhật lại toàn bộ nội dung để đảm bảo tất cả các service được đăng ký đúng.

    ```csharp
    // path: MangaReaderDB/Program.cs
    using Application.Common.Interfaces;
    using Application.Contracts.Persistence; 
    using Domain.Entities;
    using FluentValidation;
    using Infrastructure.Authentication;
    using Infrastructure.Photos;
    using MangaReaderDB.Middleware;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models; // <<< THÊM MỚI
    using Persistence.Data;
    using Persistence.Data.Interceptors;
    using Persistence.Repositories;
    using System.Text;
    using System.Text.Json.Serialization;

    var builder = WebApplication.CreateBuilder(args);
    var configuration = builder.Configuration;

    // Đăng ký Services
    builder.Services.AddSingleton<AuditableEntitySaveChangesInterceptor>();
    builder.Services.AddDbContext<ApplicationDbContext>((sp, opt) =>
    {
        var interceptor = sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
        opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
            sqlOpt => sqlOpt.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
           .AddInterceptors(interceptor);
    });
    
    // Cấu hình đọc Settings
    builder.Services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
    builder.Services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

    // Đăng ký Services từ các tầng khác
    builder.Services.AddScoped<IPhotoAccessor, PhotoAccessor>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    // ... các repository khác được UnitOfWork quản lý, không cần đăng ký riêng
    
    // MediatR, AutoMapper, FluentValidation
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Application.AssemblyReference).Assembly));
    builder.Services.AddAutoMapper(typeof(Application.AssemblyReference).Assembly);
    builder.Services.AddValidatorsFromAssembly(typeof(Application.AssemblyReference).Assembly, ServiceLifetime.Scoped);

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

    builder.Services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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

    app.UseMiddleware<ExceptionMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => {
             c.SwaggerEndpoint("/swagger/v1/swagger.json", "MangaReader API V1");
        });
        app.UseReDoc(c => {
            c.RoutePrefix = "docs";
            c.SpecUrl = "/swagger/v1/swagger.json";
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
            await Persistence.Data.SeedData.SeedAdminUserAsync(userManager);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred during data seeding.");
        }
    }

    app.Run();
    ```

### Bước 6: Di Dời Dữ Liệu và Tạo User Admin

1.  **Tạo Lớp `SeedData.cs`:**
    *   Trong dự án **`Persistence`**, tạo một file mới trong thư mục `Data` tên là `SeedData.cs`.

    ```csharp
    // path: Persistence/Data/SeedData.cs
    using Domain.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    namespace Persistence.Data
    {
        public static class SeedData
        {
            public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
            {
                if (!userManager.Users.Any(u => u.UserName == "Admin"))
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = "Admin",
                        Email = "admin@mangareader.com",
                        EmailConfirmed = true 
                    };

                    var result = await userManager.CreateAsync(adminUser, "123456");

                    if (result.Succeeded)
                    {
                        // Thêm vai trò Admin trong tương lai
                        // await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine("Admin user created successfully.");
                    }
                    else
                    {
                         Console.WriteLine("Failed to create admin user:");
                         foreach (var error in result.Errors)
                         {
                            Console.WriteLine($"- {error.Description}");
                         }
                    }
                }
            }
        }
    }
    ```

2.  **Lấy ID của Admin và Cập nhật dữ liệu:**
    *   Sau khi chạy ứng dụng lần đầu với mã nguồn ở Bước 5, tài khoản "Admin" sẽ được tạo.
    *   Mở **SQL Server Management Studio (SSMS)** và kết nối tới database `MangaReaderAPI`.
    *   Mở một cửa sổ New Query và chạy lệnh sau để lấy `Id` của người dùng Admin:
        ```sql
        SELECT Id FROM AspNetUsers WHERE UserName = 'Admin';
        ```
    *   **Copy giá trị `Id`** (nó sẽ là một chuỗi GUID dài).
    *   Trong cửa sổ Query, chạy lệnh sau, **dán `Id` bạn vừa copy** vào vị trí `YOUR_ADMIN_USER_GUID_HERE`:

        ```sql
        UPDATE Chapters
        SET UploadedByUserId = 'YOUR_ADMIN_USER_GUID_HERE'
        WHERE UploadedByUserId IS NOT NULL; -- Chỉ cập nhật những dòng đã có user cũ
        ```
    *   Thao tác này sẽ đảm bảo tất cả các chapter cũ giờ đây đều thuộc về người dùng "Admin" mới.

### Bước 7: Cập nhật `AuthController` và Bảo vệ Endpoint

1.  **Tạo DTOs cho Refresh Token:**
    *   Trong `Application/Common/DTOs/Auth`, tạo file `RefreshTokenRequestDto.cs`.

    ```csharp
    // path: Application/Common/DTOs/Auth/RefreshTokenRequestDto.cs
    using System.ComponentModel.DataAnnotations;
    namespace Application.Common.DTOs.Auth
    {
        public class RefreshTokenRequestDto
        {
            [Required]
            public string RefreshToken { get; set; } = string.Empty;
        }
    }
    ```

2.  **Cập nhật `AuthController.cs`:**
    *   Thêm logic để tạo, lưu, sử dụng và thu hồi Refresh Token.

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
            private readonly IApplicationDbContext _context; // Inject DbContext
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

            [HttpPost("register")]
            public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
            {
                var user = new ApplicationUser { UserName = registerDto.Username, Email = registerDto.Email };
                var result = await _userManager.CreateAsync(user, registerDto.Password);
                if (result.Succeeded)
                {
                    return Ok(new AuthResponseDto { IsSuccess = true, Message = "User registered successfully!" });
                }
                return BadRequest(new AuthResponseDto { IsSuccess = false, Message = string.Join(" | ", result.Errors.Select(e => e.Description)) });
            }

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
            [Authorize] // Chỉ user đã đăng nhập mới có thể thu hồi token của mình
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

3.  **Cập nhật `CreateChapter` và các endpoint khác:**
    *   Tương tự như hướng dẫn trước, bạn cần cập nhật `ChaptersController` để lấy User ID từ `Claims` và thêm `[Authorize]` vào các endpoint cần bảo vệ. Logic này không thay đổi so với hướng dẫn trước.
    *   Xóa `UploadedByUserId` khỏi `CreateChapterDto`.

## Hoàn Tất

Bây giờ hệ thống của bạn đã có đầy đủ chức năng xác thực bằng JWT và Refresh Token. Quy trình kiểm tra như sau:
1.  Chạy API.
2.  Đăng ký user mới qua `POST /api/auth/register`.
3.  Đăng nhập qua `POST /api/auth/login` để nhận `AccessToken` và `RefreshToken`.
4.  Dùng `AccessToken` để truy cập các endpoint có `[Authorize]`.
5.  Khi `AccessToken` hết hạn, dùng `RefreshToken` để gọi `POST /api/auth/refresh` và nhận một cặp token mới.
6.  Đăng xuất bằng cách gọi `POST /api/auth/revoke` (nếu cần) và xóa token ở phía client.