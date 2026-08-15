using PaymentTrackerApi.DTOs;
using PaymentTrackerApi.Models;
using System.Text.Json;

namespace PaymentTrackerApi.Services
{
    public interface IAiSensyService
    {
        /// <summary>
        /// Sends a campaign message via AiSensy's API, then persists the
        /// request + response as a CampaignLog row and returns it.
        /// </summary>
        Task<CampaignLog> SendCampaignAsync(SendCampaignRequestDto request, string createdByUserId);

        /// <summary>
        /// Pulls bill/train detail data from AiSensy's API (e.g. a form-response
        /// or contact-attributes endpoint) and returns the raw JSON for parsing.
        /// </summary>
        Task<JsonElement> FetchBillDetailsAsync(string? referenceId = null);
    }
}
