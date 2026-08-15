namespace PaymentTrackerApi.Models
{
    /// <summary>
    /// Stores every AiSensy API call we make (request + response), so we
    /// have a persistent record of what was sent and what AiSensy returned.
    /// PaymentDetail rows reference this table via CampaignLogId, OR you can
    /// just link on Destination (phone number) if a message was sent outside
    /// this system.
    /// </summary>
    public class CampaignLog
    {
        public int Id { get; set; }

        public string CampaignName { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty; // phone number, e.g. +91xxxxxxxxxx
        public string UserName { get; set; } = string.Empty;
        public string? Source { get; set; }

        // Stored as JSON strings so we don't need extra child tables for
        // variable-length arrays/objects (templateParams, tags, attributes).
        public string? TemplateParamsJson { get; set; }
        public string? TagsJson { get; set; }
        public string? AttributesJson { get; set; }

        public int ResponseStatusCode { get; set; }
        public string? ResponseBody { get; set; }

        public string CreatedByUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();
    }
}
