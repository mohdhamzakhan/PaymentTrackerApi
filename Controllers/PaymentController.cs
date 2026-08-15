using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentTrackerApi.Data;
using PaymentTrackerApi.DTOs;
using PaymentTrackerApi.Enums;
using PaymentTrackerApi.Models;

namespace PaymentTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PaymentController(ApplicationDbContext db)
        {
            _db = db;
        }

        private static PaymentDetailDto ToDto(PaymentDetail p) => new()
        {
            Id = p.Id,
            CampaignLogId = p.CampaignLogId,
            PhoneNumber = p.PhoneNumber,
            Amount = p.Amount,
            UtrNumber = p.UtrNumber,
            PaymentDate = p.PaymentDate,
            PaymentMode = p.PaymentMode,
            Status = p.Status,
            Remarks = p.Remarks,
            UpdatedByUserId = p.UpdatedByUserId,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };

        /// <summary>
        /// Creates a payment record. Restricted to AccountsTeam/Admin - they're
        /// the ones entering amount/UTR/date after a payment is made.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.AccountsTeam}")]
        public async Task<ActionResult<PaymentDetailDto>> Create(CreatePaymentDetailDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var entity = new PaymentDetail
            {
                CampaignLogId = dto.CampaignLogId,
                PhoneNumber = dto.PhoneNumber,
                Amount = dto.Amount,
                UtrNumber = dto.UtrNumber,
                PaymentDate = dto.PaymentDate,
                PaymentMode = dto.PaymentMode,
                Remarks = dto.Remarks,
                Status = "Pending",
                UpdatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _db.PaymentDetails.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
        }

        /// <summary>
        /// Updates an existing payment record (e.g. Accounts marking it Verified,
        /// or correcting the UTR/amount).
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.AccountsTeam}")]
        public async Task<ActionResult<PaymentDetailDto>> Update(int id, UpdatePaymentDetailDto dto)
        {
            var entity = await _db.PaymentDetails.FindAsync(id);
            if (entity is null) return NotFound();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (dto.Amount.HasValue) entity.Amount = dto.Amount.Value;
            if (dto.UtrNumber is not null) entity.UtrNumber = dto.UtrNumber;
            if (dto.PaymentDate.HasValue) entity.PaymentDate = dto.PaymentDate.Value;
            if (dto.PaymentMode is not null) entity.PaymentMode = dto.PaymentMode;
            if (dto.Status is not null) entity.Status = dto.Status;
            if (dto.Remarks is not null) entity.Remarks = dto.Remarks;

            entity.UpdatedByUserId = userId;
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(ToDto(entity));
        }

        /// <summary>Fetch a single payment record by its id.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PaymentDetailDto>> GetById(int id)
        {
            var entity = await _db.PaymentDetails.FindAsync(id);
            return entity is null ? NotFound() : Ok(ToDto(entity));
        }

        /// <summary>
        /// Point 3 in the brief: look up payment details by phone number
        /// and/or UTR number. Any authenticated user can search - restrict
        /// further with role checks if suppliers should only see their own.
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<PaymentDetailDto>>> Search([FromQuery] string? phone, [FromQuery] string? utr)
        {
            if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(utr))
                return BadRequest(new { message = "Provide at least one of: phone, utr." });

            var query = _db.PaymentDetails.AsQueryable();

            if (!string.IsNullOrWhiteSpace(phone))
                query = query.Where(p => p.PhoneNumber.Contains(phone));

            if (!string.IsNullOrWhiteSpace(utr))
                query = query.Where(p => p.UtrNumber == utr);

            var results = await query
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => ToDto(p))
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>Admin/Accounts: full list, most recent first.</summary>
        [HttpGet]
        [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.AccountsTeam},{UserRoles.InHouseTeam}")]
        public async Task<ActionResult<List<PaymentDetailDto>>> GetAll()
        {
            var results = await _db.PaymentDetails
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => ToDto(p))
                .ToListAsync();

            return Ok(results);
        }
    }
}
