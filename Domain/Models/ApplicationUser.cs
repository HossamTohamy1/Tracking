using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    /// <summary>
    /// ApplicationUser — يرث من IdentityUser&lt;Guid&gt;
    ///
    /// الـ Role يتحدد من خلال ASP.NET Identity Roles:
    ///   Admin        → مدير النظام
    ///   Support      → فريق الدعم الفني
    ///   ImportOffice → مكتب / شركة استيراد
    ///   Exporter     → صاحب منتج قابل للتصدير
    ///   Customer     → مستخدم عادي / عميل
    ///
    /// ملاحظة: لا يرث من BaseEntity لأن IdentityUser لديه Id خاص به.
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>
    {
        // ── بيانات أساسية ───────────────────────────────────────────────
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }

        // ── بيانات ImportOffice / Exporter ──────────────────────────────
        /// <summary>اسم الشركة — مطلوب لـ ImportOffice، اختياري لـ Exporter</summary>
        public string? CompanyName { get; set; }

        /// <summary>العنوان — مهم لـ ImportOffice والتوصيل</summary>
        public string? Address { get; set; }

        public string? Country { get; set; }

        // ── حالة الحساب ─────────────────────────────────────────────────
        /// <summary>
        /// ImportOffice → false حتى يوافق Admin
        /// باقي الـ Roles → true افتراضياً
        /// </summary>
        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        // ── تواريخ ──────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // ── Navigation Properties ────────────────────────────────────────

        // Customer: الطلبات التي أنشأها
        public virtual ICollection<ImportRequest> MyImportRequests { get; set; } = new List<ImportRequest>();

        // ImportOffice: الطلبات المكلَّف بها
        public virtual ICollection<ImportRequest> AssignedRequests { get; set; } = new List<ImportRequest>();

        // ImportOffice: الكونتنرات التي يديرها
        public virtual ICollection<Container> ManagedContainers { get; set; } = new List<Container>();

        // ImportOffice: التخليص الجمركي الذي يتولاه
        public virtual ICollection<CustomsClearance> HandledClearances { get; set; } = new List<CustomsClearance>();

        // Exporter: المنتجات القابلة للتصدير
        public virtual ICollection<ExportProduct> ExportProducts { get; set; } = new List<ExportProduct>();

        // جلسات تسجيل الدخول
        public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();

        // المدفوعات
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // الرسائل
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

        // الإشعارات
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // التقييمات التي كتبها (Customer)
        public virtual ICollection<Review> WrittenReviews { get; set; } = new List<Review>();

        // التقييمات التي استقبلها (ImportOffice / Exporter)
        public virtual ICollection<Review> ReceivedReviews { get; set; } = new List<Review>();

        // سجل التدقيق
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        // مستندات التخليص الجمركي التي رفعها
        public virtual ICollection<CustomsDocument> UploadedDocuments { get; set; } = new List<CustomsDocument>();
    }
}