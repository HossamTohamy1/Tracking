using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public enum CustomsStatus
    {
        PendingDocuments = 0,   // في انتظار رفع المستندات
        DocumentsSubmitted = 1, // تم تقديم المستندات
        UnderReview = 2,        // قيد المراجعة من الجمارك
        RequiresAction = 3,     // يتطلب إجراء (نواقص)
        Approved = 4,           // موافق عليه
        Released = 5,           // أُفرج عنه
        Rejected = 6            // مرفوض
    }

    /// <summary>
    /// التخليص الجمركي - يُنشأ عند وصول الشحنة للميناء
    /// 1-to-1 مع ImportRequest
    /// المكتب يرفع المستندات، الجمارك يراجعها
    /// </summary>
    public class CustomsClearance : BaseEntity
    {
        public Guid ImportRequestId { get; set; }
        public Guid HandledByOfficeId { get; set; }

        public CustomsStatus Status { get; set; } = CustomsStatus.PendingDocuments;

        public string? DeclarationNumber { get; set; }   // رقم البيان الجمركي
        public string? CustomsBroker { get; set; }       // اسم المخلِّص الجمركي

        public decimal CustomsValue { get; set; }        // القيمة الجمركية المُقدَّرة
        public decimal DutyAmount { get; set; }          // قيمة الرسوم الجمركية
        public decimal TaxAmount { get; set; }           // الضرائب

        public DateTime? DocumentsSubmittedAt { get; set; }
        public DateTime? CustomsApprovedAt { get; set; }
        public DateTime? ReleasedAt { get; set; }

        public string? Notes { get; set; }
        public string? RejectionReason { get; set; }

        // Navigation Properties
        public virtual ImportRequest ImportRequest { get; set; } = null!;
        public virtual ApplicationUser HandledByOffice { get; set; } = null!;
        public virtual ICollection<CustomsDocument> Documents { get; set; } = new List<CustomsDocument>();
    }

    public enum DocumentType
    {
        CommercialInvoice,   // فاتورة تجارية
        PackingList,         // قائمة التعبئة
        BillOfLading,        // بوليصة الشحن
        CertificateOfOrigin, // شهادة المنشأ
        ImportLicense,       // ترخيص الاستيراد
        InsuranceCertificate,
        Other
    }

    /// <summary>
    /// مستندات التخليص الجمركي
    /// المكتب يرفعها، Admin أو الجمارك يتحقق منها
    /// </summary>
    public class CustomsDocument : BaseEntity
    {
        public Guid CustomsClearanceId { get; set; }
        public Guid UploadedByUserId { get; set; }

        public DocumentType DocumentType { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }

        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerificationNotes { get; set; }

        // Navigation Properties
        public virtual CustomsClearance CustomsClearance { get; set; } = null!;
        public virtual ApplicationUser UploadedBy { get; set; } = null!;
        public virtual ApplicationUser? VerifiedBy { get; set; }
    }
}
