namespace PaymentTrackerApi.Models
{
    /// <summary>
    /// The payment table your Accounts / InHouse team fills in, referencing
    /// a CampaignLog (optional) and/or a phone number directly. This is what
    /// gets searched by phone number or UTR number (point 3 in the brief).
    /// </summary>
    public class PaymentDetail
    {
        public int Id { get; set; }

        // Optional link back to the AiSensy message this payment relates to.
        public int? CampaignLogId { get; set; }
        public CampaignLog? CampaignLog { get; set; }

        // Kept directly on this table (not just via CampaignLog) so a payment
        // can be recorded even if there is no associated AiSensy message.
        public string PhoneNumber { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public string UtrNumber { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }

        public string PaymentMode { get; set; } = string.Empty; // e.g. NEFT, IMPS, UPI
        public string Status { get; set; } = "Pending";          // Pending, Verified, Rejected
        public string? Remarks { get; set; }

        public string UpdatedByUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
