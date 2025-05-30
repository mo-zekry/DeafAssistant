using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeafAssistant.Models;

/// <summary>
/// Model for creating a payment intent
/// </summary>
public class PaymentIntentCreateRequest
{
    /// <summary>
    /// Amount to charge in cents (e.g., 2000 for $20.00)
    /// </summary>
    [Required]
    public long Amount { get; set; }

    /// <summary>
    /// Currency code (e.g., usd)
    /// </summary>
    [Required]
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Description of the payment
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Subscription plan name
    /// </summary>
    [Required]
    public string PlanName { get; set; } = string.Empty;
}

/// <summary>
/// Response with payment intent client secret
/// </summary>
public class PaymentIntentResponse
{
    /// <summary>
    /// Client secret for the payment intent
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Public key for Stripe
    /// </summary>
    public string PublishableKey { get; set; } = string.Empty;
}

/// <summary>
/// Request to process a payment
/// </summary>
public class ProcessPaymentRequest
{
    /// <summary>
    /// Stripe payment method ID
    /// </summary>
    [Required]
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Amount to charge in cents
    /// </summary>
    [Required]
    public long Amount { get; set; }

    /// <summary>
    /// Currency code (e.g., usd)
    /// </summary>
    [Required]
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Description of the payment
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Subscription plan name
    /// </summary>
    [Required]
    public string PlanName { get; set; } = string.Empty;

    /// <summary>
    /// Billing frequency (e.g., Monthly, Yearly)
    /// </summary>
    [Required]
    public string BillingFrequency { get; set; } = "Monthly";
}

/// <summary>
/// Response with payment processing result
/// </summary>
public class PaymentResponse
{
    /// <summary>
    /// Success status of the payment
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Payment ID (transaction ID)
    /// </summary>
    public string? PaymentId { get; set; }

    /// <summary>
    /// Payment status (e.g., succeeded, failed)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Error message if payment failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
