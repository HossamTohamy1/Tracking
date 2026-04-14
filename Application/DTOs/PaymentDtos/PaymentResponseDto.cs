namespace Application.DTOs.PaymentDtos
{
    public class PaymentResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? ImportRequestId { get; set; }
        public Guid? ContainerId { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? GatewayTransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}