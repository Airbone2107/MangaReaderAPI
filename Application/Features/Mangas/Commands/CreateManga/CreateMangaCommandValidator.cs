using Domain.Enums;
using FluentValidation;

namespace Application.Features.Mangas.Commands.CreateManga
{
    public class CreateMangaCommandValidator : AbstractValidator<CreateMangaCommand>
    {
        public CreateMangaCommandValidator()
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
                
            RuleForEach(p => p.TagIds)
                .NotEmpty().WithMessage("Tag ID không được rỗng.")
                .When(p => p.TagIds != null && p.TagIds.Any());
            
            RuleForEach(p => p.Authors).ChildRules(authorRule =>
            {
                authorRule.RuleFor(a => a.AuthorId)
                    .NotEmpty().WithMessage("Author ID không được rỗng.");
                authorRule.RuleFor(a => a.Role)
                    .IsInEnum().WithMessage("Author Role không hợp lệ.");
            }).When(p => p.Authors != null && p.Authors.Any());
        }
    }
} 