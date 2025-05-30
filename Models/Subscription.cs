using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeafAssistant.Models;

/// <summary>
/// Represents a user subscription to the application
/// </summary>
public class Subscription
{
    /// <summary>
    /// Unique identifier for the subscription
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID of the user who owns the subscription
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The user who owns the subscription
    /// </summary>
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = new ApplicationUser();

    /// <summary>
    /// Name of the subscription plan (Free, Premium, etc.)
    /// </summary>
    [Required]
    [StringLength(30)]
    public string PlanName { get; set; } = string.Empty;

    /// <summary>
    /// Price of the subscription plan
    /// </summary>
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    /// <summary>
    /// Currency of the price (USD, EUR, etc.)
    /// </summary>
    [StringLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Billing frequency (Monthly, Yearly, etc.)
    /// </summary>
    [StringLength(20)]
    public string BillingFrequency { get; set; } = "Monthly";

    /// <summary>
    /// Payment method used for subscription
    /// </summary>
    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Payment transaction ID for reference
    /// </summary>
    [StringLength(100)]
    public string? TransactionId { get; set; }

    /// <summary>
    /// Stripe customer ID for recurring payments
    /// </summary>
    [StringLength(100)]
    public string? StripeCustomerId { get; set; }

    /// <summary>
    /// Date when the subscription started
    /// </summary>
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Date when the subscription ends, if applicable
    /// </summary>
    [DataType(DataType.DateTime)]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Indicates whether the subscription is currently active
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates whether subscription renews automatically
    /// </summary>
    public bool AutoRenew { get; set; } = true;

    /// <summary>
    /// Date when the subscription was last renewed
    /// </summary>
    [DataType(DataType.DateTime)]
    public DateTime? LastRenewalDate { get; set; }

    /// <summary>
    /// Date when the subscription will next renew
    /// </summary>
    [DataType(DataType.DateTime)]
    public DateTime? NextRenewalDate { get; set; }

    /// <summary>
    /// Date when the subscription was canceled, if applicable
    /// </summary>
    [DataType(DataType.DateTime)]
    public DateTime? CancellationDate { get; set; }
}
