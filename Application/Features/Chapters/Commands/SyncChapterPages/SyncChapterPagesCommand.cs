using Application.Common.DTOs.Chapters;
using MediatR;
using System;
using System.Collections.Generic;

namespace Application.Features.Chapters.Commands.SyncChapterPages
{
    public class SyncChapterPagesCommand : IRequest<List<ChapterPageAttributesDto>>
    {
        public Guid ChapterId { get; set; }
        public List<PageSyncInstruction> Instructions { get; set; } = new List<PageSyncInstruction>();
    }
} 