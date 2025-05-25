using Application.Common.DTOs.TranslatedMangas;
using FluentValidation;

namespace Application.Validation.TranslatedMangas
{
    public class CreateTranslatedMangaDtoValidator : AbstractValidator<CreateTranslatedMangaDto>
    {
        public CreateTranslatedMangaDtoValidator()
        {
            RuleFor(p => p.MangaId)
                .NotEmpty().WithMessage("MangaId không được để trống.");

            RuleFor(p => p.LanguageKey)
                .NotEmpty().WithMessage("Mã ngôn ngữ không được để trống.")
                .Length(2, 10).WithMessage("Mã ngôn ngữ phải có từ 2 đến 10 ký tự.");

            RuleFor(p => p.Title)
                .NotEmpty().WithMessage("Tiêu đề dịch không được để trống.")
                .MaximumLength(500).WithMessage("Tiêu đề dịch không được vượt quá 500 ký tự.");

            RuleFor(p => p.Description)
                .MaximumLength(4000).WithMessage("Mô tả dịch không được vượt quá 4000 ký tự.");
        }
    }
} 