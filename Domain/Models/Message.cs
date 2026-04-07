using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public enum MessageType
    {
        Inquiry = 0,       // استفسار عن منتج
        Offer = 1,         // عرض سعر
        Negotiation = 2,   // تفاوض
        Support = 3,       // دعم فني / مشكلة
        General = 4
    }

    /// <summary>
    /// الرسائل بين المستخدمين - غالباً استفسار عن ExportProduct
    /// يدعم Threading: ParentMessageId للردود
    /// </summary>
    public class Message : BaseEntity
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }

        // اختياري: الرسالة مرتبطة بمنتج معين
        public Guid? ExportProductId { get; set; }

        // للـ Threading: إذا كانت رداً على رسالة أخرى
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
