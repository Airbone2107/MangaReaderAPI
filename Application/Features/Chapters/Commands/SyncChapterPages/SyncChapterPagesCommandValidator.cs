using FluentValidation;

namespace Application.Features.Chapters.Commands.SyncChapterPages
{
    public class SyncChapterPagesCommandValidator : AbstractValidator<SyncChapterPagesCommand>
    {
        public SyncChapterPagesCommandValidator()
        {
            RuleFor(x => x.ChapterId)
                .NotEmpty().WithMessage("Chapter ID is required.");

            RuleFor(x => x.Instructions)
                .NotNull().WithMessage("Page instructions are required.");
                // Có thể thêm rule để đảm bảo PageNumbers là duy nhất trong Instructions
                // .Must(instructions => instructions.Select(i => i.DesiredPageNumber).Distinct().Count() == instructions.Count)
                // .WithMessage("Desired page numbers must be unique within the instructions set.")
                // .When(x => x.Instructions != null && x.Instructions.Any());

            RuleForEach(x => x.Instructions).ChildRules(instr =>
            {
                instr.RuleFor(i => i.DesiredPageNumber)
                    .GreaterThan(0).WithMessage("Desired page number must be greater than 0.");
                
                // PageId có thể null cho trang mới, không cần rule NotEmpty.
                // Nếu ImageFileToUpload không null, thì các thuộc tính của nó phải hợp lệ
                instr.When(i => i.ImageFileToUpload != null, () => {
                    instr.RuleFor(i => i.ImageFileToUpload!.ImageStream)
                        .NotNull().WithMessage("Image stream is required if image file is provided.")
                        .Must(stream => stream.Length > 0).WithMessage("Image stream cannot be empty.");
                    instr.RuleFor(i => i.ImageFileToUpload!.OriginalFileName)
                        .NotEmpty().WithMessage("Original file name is required if image file is provided.");
                });
            });
        }
    }
} 