using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using PaymentTrackerApi.Data;
using PaymentTrackerApi.DTOs;
using PaymentTrackerApi.Enums;
using PaymentTrackerApi.Services;
using System.Text.Json;

namespace PaymentTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AiSensyController : ControllerBase
    {
        private readonly IAiSensyService _aiSensyService;
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ImageDownloader _imageDownloader;

        public AiSensyController(
            IAiSensyService aiSensyService,
            ApplicationDbContext db,
            IConfiguration config,
            ImageDownloader imageDownloader)
        {
            _aiSensyService = aiSensyService;
            _db = db;
            _config = config;
            _imageDownloader = imageDownloader;
        }

        /// <summary>
        /// Sends a WhatsApp campaign message via AiSensy and stores the
        /// request/response as a CampaignLog. Restricted to internal roles -
        /// Suppliers shouldn't be triggering outbound messages.
        /// </summary>
        [HttpPost("send")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.InHouseTeam},{UserRoles.AccountsTeam}")]
        public async Task<ActionResult<CampaignLogDto>> Send(SendCampaignRequestDto request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            var log = await _aiSensyService.SendCampaignAsync(request, userId);

            return Ok(new CampaignLogDto
            {
                Id = log.Id,
                CampaignName = log.CampaignName,
                Destination = log.Destination,
                UserName = log.UserName,
                ResponseStatusCode = log.ResponseStatusCode,
                ResponseBody = log.ResponseBody,
                CreatedAt = log.CreatedAt
            });
        }

        /// <summary>Lists past campaign sends, most recent first, optionally filtered by phone number.</summary>
        [HttpGet]
        public async Task<ActionResult<List<CampaignLogDto>>> GetAll([FromQuery] string? phone)
        {
            var query = _db.CampaignLogs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(phone))
                query = query.Where(c => c.Destination.Contains(phone));

            var logs = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CampaignLogDto
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    Destination = c.Destination,
                    UserName = c.UserName,
                    ResponseStatusCode = c.ResponseStatusCode,
                    ResponseBody = c.ResponseBody,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(logs);
        }

        /// <summary>Lists received bill details, most recent first, optionally filtered by bill number.</summary>
        [HttpGet("bill-details")]
        public async Task<ActionResult<List<TrainBillDetailDto>>> GetBillDetails([FromQuery] string? billNo)
        {
            var query = _db.TrainBillDetails.AsQueryable();
            if (!string.IsNullOrWhiteSpace(billNo))
                query = query.Where(b => b.BillNumber == billNo);

            var results = await query
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new TrainBillDetailDto
                {
                    Id = b.Id,
                    TrainNumber = b.TrainNumber,
                    TrainName = b.TrainName,
                    RackNumber = b.RackNumber,
                    ManagerName = b.ManagerName,
                    ManagerMobileNo = b.ManagerMobileNo,
                    DepartureDate = b.DepartureDate,
                    ArrivalDate = b.ArrivalDate,
                    VendorName = b.VendorName,
                    VendorMobileNo = b.VendorMobileNo,
                    LocationOfPurchase = b.LocationOfPurchase,
                    BillNumber = b.BillNumber,
                    BillDate = b.BillDate,
                    TotalInvoiceAmount = b.TotalInvoiceAmount,
                    ExtraFields = b.ExtraFieldsJson == null
                        ? null
                        : JsonSerializer.Deserialize<Dictionary<string, string>>(b.ExtraFieldsJson, (JsonSerializerOptions?)null),
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Pull mode - a logged-in user (or scheduled job) triggers a fetch from
        /// AiSensy's API. Normal JWT auth applies, no anonymous access.
        /// </summary>
        [HttpPost("bill-details/pull")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.InHouseTeam},{UserRoles.AccountsTeam}")]
        public async Task<ActionResult<TrainBillDetailDto>> PullBillDetails([FromQuery] string? referenceId)
        {
            var payload = await _aiSensyService.FetchBillDetailsAsync(referenceId);
            var dto = await SaveBillDetailsAsync(payload);
            return Ok(dto);
        }

        /// <summary>
        /// Webhook receiver - AiSensy pushes bill/train detail data here directly.
        /// Guarded by a shared secret (since it can't carry a user JWT) and can be
        /// switched off entirely via AiSensy:Webhook:Enabled without touching code.
        /// </summary>
        [HttpPost("bill-details/webhook")]
        [AllowAnonymous]
        public async Task<ActionResult<TrainBillDetailDto>> ReceiveBillDetailsWebhook([FromBody] JsonElement payload)
        {
            var webhookEnabled = _config.GetValue<bool>("AiSensy:Webhook:Enabled");
            if (!webhookEnabled)
                return NotFound(); // pretend this route doesn't exist when webhook mode is off

            var expectedSecret = _config["AiSensy:Webhook:SharedSecret"];
            if (string.IsNullOrWhiteSpace(expectedSecret))
                return StatusCode(500, new { message = "Webhook secret is not configured." });

            if (!Request.Headers.TryGetValue("X-Webhook-Secret", out StringValues providedSecret)
                || providedSecret != expectedSecret)
                return Unauthorized(new { message = "Invalid or missing webhook secret." });

            var dto = await SaveBillDetailsAsync(payload);
            return Ok(dto);
        }

        /// <summary>Shared by both webhook and pull modes so parsing/saving isn't duplicated.</summary>
        private async Task<TrainBillDetailDto> SaveBillDetailsAsync(JsonElement payload)
        {
            var entity = TrainBillDetailParser.Parse(payload);

            var (billData, billContentType) = await _imageDownloader.TryDownloadAsync(entity.BillImageUrl);
            entity.BillImageData = billData;
            entity.BillImageContentType = billContentType;

            var (qrData, qrContentType) = await _imageDownloader.TryDownloadAsync(entity.QrCodeImageUrl);
            entity.QrCodeImageData = qrData;
            entity.QrCodeImageContentType = qrContentType;

            _db.TrainBillDetails.Add(entity);
            await _db.SaveChangesAsync();

            return new TrainBillDetailDto
            {
                Id = entity.Id,
                TrainNumber = entity.TrainNumber,
                TrainName = entity.TrainName,
                RackNumber = entity.RackNumber,
                ManagerName = entity.ManagerName,
                ManagerMobileNo = entity.ManagerMobileNo,
                DepartureDate = entity.DepartureDate,
                ArrivalDate = entity.ArrivalDate,
                VendorName = entity.VendorName,
                VendorMobileNo = entity.VendorMobileNo,
                LocationOfPurchase = entity.LocationOfPurchase,
                BillNumber = entity.BillNumber,
                BillDate = entity.BillDate,
                TotalInvoiceAmount = entity.TotalInvoiceAmount,
                BillImageUrl = entity.BillImageUrl,
                HasBillImage = entity.BillImageData is not null,
                QrCodeImageUrl = entity.QrCodeImageUrl,
                HasQrCodeImage = entity.QrCodeImageData is not null,
                ExtraFields = entity.ExtraFieldsJson is null
                    ? null
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(entity.ExtraFieldsJson),
                CreatedAt = entity.CreatedAt
            };
        }

        /// <summary>Streams the stored bill image for a given record.</summary>
        [HttpGet("bill-details/{id:int}/bill-image")]
        public async Task<IActionResult> GetBillImage(int id)
        {
            var entity = await _db.TrainBillDetails.FindAsync(id);
            if (entity?.BillImageData is null) return NotFound();
            return File(entity.BillImageData, entity.BillImageContentType ?? "application/octet-stream");
        }

        /// <summary>Streams the stored QR code image for a given record.</summary>
        [HttpGet("bill-details/{id:int}/qr-code")]
        public async Task<IActionResult> GetQrCodeImage(int id)
        {
            var entity = await _db.TrainBillDetails.FindAsync(id);
            if (entity?.QrCodeImageData is null) return NotFound();
            return File(entity.QrCodeImageData, entity.QrCodeImageContentType ?? "application/octet-stream");
        }
    }
}