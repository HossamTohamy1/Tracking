namespace Domain.Constants
{

    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Support = "Support";
        public const string ImportOffice = "ImportOffice";
        public const string Exporter = "Exporter";
        public const string Customer = "Customer";

        public static readonly IReadOnlyList<string> All = new[]
        {
            Admin,
            Support,
            ImportOffice,
            Exporter,
            Customer
        };

        public static readonly IReadOnlyList<string> AllowedForRegistration = new[]
        {
            Customer,
            ImportOffice,
            Exporter
        };
    }
}