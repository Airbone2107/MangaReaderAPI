using Application.Common.DTOs.Mangas;
using Domain.Enums;
using FluentValidation;

namespace Application.Validation.Mangas
{
    public class UpdateMangaDtoValidator : AbstractValidator<UpdateMangaDto>
    {
        public UpdateMangaDtoValidator()
        {
            RuleFor(p => p.Title)
                .NotEmpty().WithMessage("Tiêu đề không được để trống.")
                .MaximumLength(500).WithMessage("Tiêu đề không được vượt quá 500 ký tự.");

            RuleFor(p => p.OriginalLanguage)
                .NotEmpty().WithMessage("Ngôn ngữ gốc không được để trống.")
                .Length(2, 10).WithMessage("Mã ngôn ngữ gốc phải có từ 2 đến 10 ký tự.");

            RuleFor(p => p.PublicationDemographic)
                .Must(x => x == null || Enum.IsDefined(typeof(PublicationDemographic), x))
                .When(p => p.PublicationDemographic.HasValue)
                .WithMessage("Đối tượng độc giả không hợp lệ.");

            RuleFor(p => p.Status)
                .IsInEnum().WithMessage("Trạng thái manga không hợp lệ.");

            RuleFor(p => p.Year)
                .InclusiveBetween(1900, DateTime.UtcNow.Year + 5).When(p => p.Year.HasValue)
                .WithMessage($"Năm xuất bản phải từ 1900 đến {DateTime.UtcNow.Year + 5}.");

            RuleFor(p => p.ContentRating)
                .IsInEnum().WithMessage("Đánh giá nội dung không hợp lệ.");
            
            // IsLocked là boolean, không cần rule đặc biệt trừ khi có logic cụ thể
        }
    }
} 