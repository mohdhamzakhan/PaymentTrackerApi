namespace PaymentTrackerApi.Enums
{
    /// <summary>
    /// Central place to define all role names used across the system.
    /// These are seeded into AspNetRoles on startup.
    /// </summary>
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Supplier = "Supplier";
        public const string InHouseTeam = "InHouseTeam";
        public const string AccountsTeam = "AccountsTeam";

        public static readonly string[] All =
        {
            Admin, User, Supplier, InHouseTeam, AccountsTeam
        };
    }
}
