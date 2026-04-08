using Domain.Enums.Enums_Models;
using System;

namespace Domain.Models
{


  
    public class Review : BaseEntity
    {
        public Guid ReviewerId { get; set; }

        public ReviewTargetType TargetType { get; set; }

        public Guid? OfficeId { get; set; }

        public Guid? ExportProductId { get; set; }

        public Guid? ImportRequestId { get; set; }

        public int Rating { get; set; }   
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;

        public bool IsVerified { get; set; } = false;

        public string? ReplyText { get; set; }
        public DateTime? RepliedAt { get; set; }

        // Navigation Properties
        public virtual ApplicationUser Reviewer { get; set; } = null!;

        public virtual ApplicationUser? Office { get; set; }

        public virtual ExportProduct? ExportProduct { get; set; }
        public virtual ImportRequest? ImportRequest { get; set; }
    }
}