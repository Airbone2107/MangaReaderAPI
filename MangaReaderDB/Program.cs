// MangaReaderDB/Program.cs
using Application.Common.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Photos;
using Microsoft.EntityFrameworkCore;
using Persistence.Data; 
using Persistence.Data.Interceptors;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký Interceptor như một Singleton
builder.Services.AddSingleton<AuditableEntitySaveChangesInterceptor>();

// Đăng ký ApplicationDbContext và thêm Interceptor
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(interceptor); // Thêm interceptor vào DbContext
});

// Cấu hình CloudinarySettings
// Đọc từ section "CloudinarySettings" trong appsettings.json
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Đăng ký PhotoAccessor
builder.Services.AddScoped<IPhotoAccessor, PhotoAccessor>();

// Đăng ký validators từ assembly Application
builder.Services.AddValidatorsFromAssembly(typeof(Application.AssemblyReference).Assembly, ServiceLifetime.Scoped);
builder.Services.AddFluentValidationAutoValidation();

// Các services khác của ASP.NET Core
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();