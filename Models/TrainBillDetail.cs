namespace PaymentTrackerApi.Models
{
    /// <summary>
    /// Stores the structured train/bill data that comes in the AiSensy JSON.
    /// Known fields get their own columns (fast to query/filter on).
    /// Anything else in the payload that isn't mapped below goes into
    /// ExtraFieldsJson so no data is ever silently dropped, even if AiSensy
    /// adds more fields later.
    /// </summary>
    public class TrainBillDetail
    {
        public int Id { get; set; }

        public string TrainNumber { get; set; } = string.Empty;
        public string TrainName { get; set; } = string.Empty;
        public string RackNumber { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerMobileNo { get; set; } = string.Empty;
        public DateTime? DepartureDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string? VendorMobileNo { get; set; }
        public string LocationOfPurchase { get; set; } = string.Empty;
        public string BillNumber { get; set; } = string.Empty;
        public DateTime? BillDate { get; set; }
        public decimal TotalInvoiceAmount { get; set; }

        /// <summary>Any fields present in the incoming JSON that aren't mapped above.</summary>
        public string? ExtraFieldsJson { get; set; }

        /// <summary>Full original payload, kept as-is for audit/debugging.</summary>
        public string RawPayloadJson { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? BillImageUrl { get; set; }
        public byte[]? BillImageData { get; set; }
        public string? BillImageContentType { get; set; }

        public string? QrCodeImageUrl { get; set; }
        public byte[]? QrCodeImageData { get; set; }
        public string? QrCodeImageContentType { get; set; }
    }
}