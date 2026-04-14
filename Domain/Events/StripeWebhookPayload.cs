namespace Domain.Events
{
 
    public class StripeWebhookPayload
    {
        public string Type { get; set; } = string.Empty;      // e.g. "payment_intent.succeeded"
        public StripeEventData Data { get; set; } = new();
    }

    public class StripeEventData
    {
        public StripePaymentObject Object { get; set; } = new();
    }

    public class StripePaymentObject
    {
        public string Id { get; set; } = string.Empty;         // pi_xxx
        public long Amount { get; set; }                        // cents
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;     // "succeeded" / "canceled"

        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}