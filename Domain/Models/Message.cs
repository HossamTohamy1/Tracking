using Domain.Enums.Enums_Models;
using System;
using System.Collections.Generic;

namespace Domain.Models
{
  


    public class Message : BaseEntity
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }

        public Guid? ExportProductId { get; set; }

        public Guid? ParentMessageId { get; set; }

        public MessageType Type { get; set; } = MessageType.Inquiry;
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        // Navigation Properties
        public virtual ApplicationUser Sender { get; set; } = null!;
        public virtual ApplicationUser Receiver { get; set; } = null!;
        public virtual ExportProduct? ExportProduct { get; set; }
        public virtual Message? ParentMessage { get; set; }
        public virtual ICollection<Message> Replies { get; set; } = new List<Message>();
    }
}
