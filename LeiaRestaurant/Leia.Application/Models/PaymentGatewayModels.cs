namespace Leia.Application.Models
{
    public class CardDetails
    {
        public string CardNumber { get; set; } = string.Empty;
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public string Cvv { get; set; } = string.Empty;
        public string CardHolder { get; set; } = string.Empty;
    }

    public class GatewayChargeResult
    {
        public bool Success { get; set; }
        public string? ProviderPaymentId { get; set; }
        public string? Brand { get; set; }
        public string? Last4 { get; set; }
        public string? FailureReason { get; set; }
    }

    public class GatewayRefundResult
    {
        public bool Success { get; set; }
        public string? ProviderRefundId { get; set; }
        public string? FailureReason { get; set; }
    }
}