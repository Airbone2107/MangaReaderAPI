using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.Common.Responses
{
    public class ApiErrorResponse
    {
        [JsonPropertyOrder(1)]
        [JsonPropertyName("result")]
        public string Result { get; set; } = "error";

        [JsonPropertyOrder(2)]
        [JsonPropertyName("errors")]
        public List<ApiError> Errors { get; set; } = new List<ApiError>();

        public ApiErrorResponse() { }

        public ApiErrorResponse(ApiError error)
        {
            Errors.Add(error);
        }

        public ApiErrorResponse(IEnumerable<ApiError> errors)
        {
            Errors.AddRange(errors);
        }
    }
} 