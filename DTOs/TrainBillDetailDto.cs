namespace PaymentTrackerApi.DTOs
{
    public class TrainBillDetailDto
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
        public Dictionary<string, string>? ExtraFields { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? BillImageUrl { get; set; }
        public bool HasBillImage { get; set; }

        public string? QrCodeImageUrl { get; set; }
        public bool HasQrCodeImage { get; set; }
    }
}