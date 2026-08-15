using System.ComponentModel.DataAnnotations;

namespace PaymentTrackerApi.DTOs
{
    public class CreatePaymentDetailDto
    {
        public int? CampaignLogId { get; set; }
        [Required] public string PhoneNumber { get; set; } = string.Empty;
        [Required] public decimal Amount { get; set; }
        [Required] public string UtrNumber { get; set; } = string.Empty;
        [Required] public DateTime PaymentDate { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }

    public class UpdatePaymentDetailDto
    {
        public decimal? Amount { get; set; }
        public string? UtrNumber { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentMode { get; set; }
        public string? Status { get; set; } // Pending, Verified, Rejected
        public string? Remarks { get; set; }
    }

    public class PaymentDetailDto
    {
        public int Id { get; set; }
        public int? CampaignLogId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string UtrNumber { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string UpdatedByUserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
