namespace Application.Common.Models
{
    public class PhotoUploadResult
    {
        public string PublicId { get; set; } = null!;
        public string Url { get; set; } = null!; // URL có thể hữu ích cho backend, dù frontend sẽ tự build
    }
} 