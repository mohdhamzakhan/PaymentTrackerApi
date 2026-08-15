using System.Text;
using System.Text.Json;
using PaymentTrackerApi.Data;
using PaymentTrackerApi.DTOs;
using PaymentTrackerApi.Models;

namespace PaymentTrackerApi.Services
{
    public class AiSensyService : IAiSensyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public AiSensyService(HttpClient httpClient, IConfiguration config, ApplicationDbContext db)
        {
            _httpClient = httpClient;
            _config = config;
            _db = db;
        }

        public async Task<CampaignLog> SendCampaignAsync(SendCampaignRequestDto request, string createdByUserId)
        {
            var apiKey = _config["AiSensy:ApiKey"]
                ?? throw new InvalidOperationException("AiSensy:ApiKey is not configured.");
            var endpoint = _config["AiSensy:Endpoint"]
                ?? "https://backend.aisensy.com/campaign/t1/api/v2";

            var payload = new AiSensyApiPayload
            {
                ApiKey = apiKey,
                CampaignName = request.CampaignName,
                Destination = request.Destination,
                UserName = request.UserName,
                Source = request.Source,
                Media = request.Media,
                TemplateParams = request.TemplateParams,
                Tags = request.Tags,
                Attributes = request.Attributes
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var log = new CampaignLog
            {
                CampaignName = request.CampaignName,
                Destination = request.Destination,
                UserName = request.UserName,
                Source = request.Source,
                TemplateParamsJson = request.TemplateParams is null ? null : JsonSerializer.Serialize(request.TemplateParams),
                TagsJson = request.Tags is null ? null : JsonSerializer.Serialize(request.Tags),
                AttributesJson = request.Attributes is null ? null : JsonSerializer.Serialize(request.Attributes),
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                log.ResponseStatusCode = (int)response.StatusCode;
                log.ResponseBody = responseBody;
            }
            catch (Exception ex)
            {
                // Network/timeout errors still get logged so nothing silently disappears.
                log.ResponseStatusCode = 0;
                log.ResponseBody = $"Request failed: {ex.Message}";
            }

            _db.CampaignLogs.Add(log);
            await _db.SaveChangesAsync();

            return log;
        }
        public async Task<JsonElement> FetchBillDetailsAsync(string? referenceId = null)
        {
            var apiKey = _config["AiSensy:ApiKey"]
                ?? throw new InvalidOperationException("AiSensy:ApiKey is not configured.");

            // Adjust this to AiSensy's actual "fetch" endpoint once confirmed - this
            // is a placeholder built from the config so it's a one-line change later.
            var baseUrl = _config["AiSensy:FetchEndpoint"]
                ?? throw new InvalidOperationException("AiSensy:FetchEndpoint is not configured.");

            var url = string.IsNullOrWhiteSpace(referenceId)
                ? $"{baseUrl}?apiKey={apiKey}"
                : $"{baseUrl}?apiKey={apiKey}&referenceId={referenceId}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            return doc.RootElement.Clone();
        }

    }
}
