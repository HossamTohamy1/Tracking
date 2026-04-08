using Domain.Enums.Enums_Models;
using System;
using System.Collections.Generic;

namespace Domain.Models
{
 

    public class CustomsClearance : BaseEntity
    {
        public Guid ImportRequestId { get; set; }

        public Guid HandledByOfficeId { get; set; }

        public CustomsStatus Status { get; set; } = CustomsStatus.PendingDocuments;

        public string? DeclarationNumber { get; set; }   
        public string? CustomsBroker { get; set; }       

        public decimal CustomsValue { get; set; }        
        public decimal DutyAmount { get; set; }          
        public decimal TaxAmount { get; set; }           

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