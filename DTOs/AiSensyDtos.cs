using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PaymentTrackerApi.DTOs
{
    /// <summary>
    /// What OUR API accepts from our own frontend to trigger a campaign send.
    /// We keep apiKey out of this - it's read server-side from config so the
    /// frontend/clients never need to know it.
    /// </summary>
    public class SendCampaignRequestDto
    {
        [Required] public string CampaignName { get; set; } = string.Empty;
        [Required] public string Destination { get; set; } = string.Empty;
        [Required] public string UserName { get; set; } = string.Empty;
        public string? Source { get; set; }
        public MediaDto? Media { get; set; }
        public List<string>? TemplateParams { get; set; }
        public List<string>? Tags { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
    }

    public class MediaDto
    {
        public string Url { get; set; } = string.Empty;
        public string Filename { get; set; } = string.Empty;
    }

    /// <summary>
    /// Exact shape AiSensy's API expects (apiKey included), built server-side
    /// from SendCampaignRequestDto + the configured key before we POST it.
    /// </summary>
    public class AiSensyApiPayload
    {
        [JsonPropertyName("apiKey")] public string ApiKey { get; set; } = string.Empty;
        [JsonPropertyName("campaignName")] public string CampaignName { get; set; } = string.Empty;
        [JsonPropertyName("destination")] public string Destination { get; set; } = string.Empty;
        [JsonPropertyName("userName")] public string UserName { get; set; } = string.Empty;
        [JsonPropertyName("source")] public string? Source { get; set; }
        [JsonPropertyName("media")] public MediaDto? Media { get; set; }
        [JsonPropertyName("templateParams")] public List<string>? TemplateParams { get; set; }
        [JsonPropertyName("tags")] public List<string>? Tags { get; set; }
        [JsonPropertyName("attributes")] public Dictionary<string, string>? Attributes { get; set; }
    }

    public class CampaignLogDto
    {
        public int Id { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int ResponseStatusCode { get; set; }
        public string? ResponseBody { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
