using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PaymentTrackerApi.DTOs;
using PaymentTrackerApi.Enums;
using PaymentTrackerApi.Models;
using PaymentTrackerApi.Services;

namespace PaymentTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Login for every user type (Admin, User, Supplier, InHouseTeam, AccountsTeam).
        /// The returned JWT carries the user's role(s) as claims, which the
        /// [Authorize(Roles = "...")] attributes on other controllers check.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserNameOrEmail)
                       ?? await _userManager.FindByEmailAsync(dto.UserNameOrEmail);

            if (user is null || !user.IsActive)
                return Unauthorized(new { message = "Invalid credentials or inactive account." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid credentials." });

            var roles = await _userManager.GetRolesAsync(user);
            var (token, expiresAt) = _tokenService.CreateToken(user, roles);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Roles = roles
            });
        }

        /// <summary>
        /// Only Admins can register new users (point 5 in the brief).
        /// Role must be one of the constants in UserRoles.
        /// </summary>
        [HttpPost("register")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<UserSummaryDto>> Register(RegisterDto dto)
        {
            if (!UserRoles.All.Contains(dto.Role))
                return BadRequest(new { message = $"Role must be one of: {string.Join(", ", UserRoles.All)}" });

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                FullName = dto.FullName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);
            if (!createResult.Succeeded)
                return BadRequest(new { errors = createResult.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok(new UserSummaryDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                FullName = user.FullName,
                Email = user.Email!,
                IsActive = user.IsActive,
                Roles = new List<string> { dto.Role }
            });
        }

        /// <summary>
        /// Admin-only: list every user in the system with their role(s)
        /// (point 5 - "Admin will have functionality to see all functionality").
        /// </summary>
        [HttpGet("users")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<ActionResult<List<UserSummaryDto>>> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var result = new List<UserSummaryDto>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new UserSummaryDto
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    IsActive = u.IsActive,
                    Roles = roles
                });
            }

            return Ok(result);
        }

        /// <summary>
        /// Lets the currently logged-in user change their own password.
        /// Requires the correct current password - use a separate
        /// admin-reset endpoint if you need to force-reset someone else's.
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId ?? string.Empty);
            if (user is null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            return Ok(new { message = "Password changed successfully." });
        }


        /// <summary>Returns the currently logged-in user's own profile + roles.</summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserSummaryDto>> Me()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId ?? string.Empty);
            if (user is null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new UserSummaryDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles
            });
        }
    }
}
