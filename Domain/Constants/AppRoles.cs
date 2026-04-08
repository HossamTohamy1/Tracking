namespace Domain.Constants
{
    /// <summary>
    /// الأدوار الثابتة في النظام
    /// ─────────────────────────────────────────────────────────
    /// Admin        : مدير التطبيق — صلاحيات كاملة
    /// Support      : فريق الدعم الفني — يشوف الشكاوى والمحادثات فقط
    /// ImportOffice : مكتب/شركة استيراد مسجَّلة — يدير الشحنات والتخليص والكونتنرات
    /// Exporter     : صاحب منتج قابل للتصدير — يعرض منتجاته ويتواصل مع المشترين
    /// Customer     : مستخدم عادي — يطلب استيراد ويتابع شحناته
    /// ─────────────────────────────────────────────────────────
    /// ملاحظة: Admin و Support لا يُنشَآن من API — يُنشَآن من Seed فقط.
    /// </summary>
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Support = "Support";
        public const string ImportOffice = "ImportOffice";
        public const string Exporter = "Exporter";
        public const string Customer = "Customer";

        /// <summary>كل الأدوار — للـ Seed</summary>
        public static readonly IReadOnlyList<string> All = new[]
        {
            Admin,
            Support,
            ImportOffice,
            Exporter,
            Customer
        };

        /// <summary>الأدوار المسموح بالتسجيل بيها من خلال API</summary>
        public static readonly IReadOnlyList<string> AllowedForRegistration = new[]
        {
            Customer,
            ImportOffice,
            Exporter
        };
    }
}