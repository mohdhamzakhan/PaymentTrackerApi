using System.Text.Json;

namespace PaymentTrackerApi.Services
{
    public static class TrainBillDetailParser
    {
        // Map normalized (lowercase, no spaces/punctuation) incoming keys -> our field names.
        // Add more aliases here as you see real AiSensy payloads.
        private static readonly Dictionary<string, string> KnownKeyAliases = new()
        {
            ["trainno"] = "TrainNumber",
            ["trainnumber"] = "TrainNumber",
            ["trainname"] = "TrainName",
            ["rackno"] = "RackNumber",
            ["racknumber"] = "RackNumber",
            ["managername"] = "ManagerName",
            ["managersmobileno"] = "ManagerMobileNo",
            ["managermobileno"] = "ManagerMobileNo",
            ["departuredate"] = "DepartureDate",
            ["arrivaldate"] = "ArrivalDate",
            ["vendorname"] = "VendorName",
            ["vendorsmobileno"] = "VendorMobileNo",
            ["vendormobileno"] = "VendorMobileNo",
            ["locationofpurchase"] = "LocationOfPurchase",
            ["billno"] = "BillNumber",
            ["billnumber"] = "BillNumber",
            ["billdate"] = "BillDate",
            ["totalinvoiceamount"] = "TotalInvoiceAmount",
            ["billimage"] = "BillImageUrl",
            ["billimageurl"] = "BillImageUrl",
            ["qrcode"] = "QrCodeImageUrl",
            ["qrcodeimage"] = "QrCodeImageUrl",
            ["qrcodeimageurl"] = "QrCodeImageUrl",
        };

        private static string Normalize(string key) =>
            new string(key.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

        public static Models.TrainBillDetail Parse(JsonElement payload)
        {
            var entity = new Models.TrainBillDetail
            {
                RawPayloadJson = payload.GetRawText(),
                CreatedAt = DateTime.UtcNow
            };

            var extras = new Dictionary<string, string>();

            foreach (var prop in payload.EnumerateObject())
            {
                var value = prop.Value.ValueKind == JsonValueKind.String
                    ? prop.Value.GetString() ?? string.Empty
                    : prop.Value.GetRawText();

                var normalized = Normalize(prop.Name);

                if (!KnownKeyAliases.TryGetValue(normalized, out var fieldName))
                {
                    extras[prop.Name] = value;
                    continue;
                }

                switch (fieldName)
                {
                    case "TrainNumber": entity.TrainNumber = value; break;
                    case "TrainName": entity.TrainName = value; break;
                    case "RackNumber": entity.RackNumber = value; break;
                    case "ManagerName": entity.ManagerName = value; break;
                    case "ManagerMobileNo": entity.ManagerMobileNo = value; break;
                    case "VendorName": entity.VendorName = value; break;
                    case "VendorMobileNo": entity.VendorMobileNo = value; break;
                    case "LocationOfPurchase": entity.LocationOfPurchase = value; break;
                    case "BillNumber": entity.BillNumber = value; break;
                    case "DepartureDate": entity.DepartureDate = TryParseDate(value); break;
                    case "ArrivalDate": entity.ArrivalDate = TryParseDate(value); break;
                    case "BillDate": entity.BillDate = TryParseDate(value); break;
                    case "TotalInvoiceAmount":
                        entity.TotalInvoiceAmount = decimal.TryParse(value, out var amt) ? amt : 0;
                        break;
                    case "BillImageUrl": entity.BillImageUrl = value; break;
                    case "QrCodeImageUrl": entity.QrCodeImageUrl = value; break;
                }
            }

            entity.ExtraFieldsJson = extras.Count > 0 ? JsonSerializer.Serialize(extras) : null;
            return entity;
        }

        // Handles dd/MM/yy and dd/M/yy formats like "12/08/26" and "12/8/26".
        private static DateTime? TryParseDate(string value)
        {
            string[] formats = { "dd/MM/yy", "d/M/yy", "dd/MM/yyyy", "d/M/yyyy" };
            return DateTime.TryParseExact(value, formats, null,
                System.Globalization.DateTimeStyles.None, out var date)
                ? date
                : null;
        }
    }
}