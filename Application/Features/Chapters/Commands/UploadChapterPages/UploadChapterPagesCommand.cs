using Application.Common.DTOs.Chapters;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Chapters.Commands.UploadChapterPages
{
    public class UploadChapterPagesCommand : IRequest<List<ChapterPageAttributesDto>>
    {
        public Guid ChapterId { get; set; }
        public List<FileToUpload> Files { get; set; } = new List<FileToUpload>();
    }
} 