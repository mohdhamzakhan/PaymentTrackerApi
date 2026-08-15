using PaymentTrackerApi.Models;

namespace PaymentTrackerApi.Services
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) CreateToken(ApplicationUser user, IList<string> roles);
    }
}
