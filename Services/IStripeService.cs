using DeafAssistant.Models;

namespace DeafAssistant.Services;

/// <summary>
/// Interface for Stripe payment service operations
/// </summary>
public interface IStripeService
{
    /// <summary>
    /// Create a payment intent for a subscription
    /// </summary>
    /// <param name="amount">Amount to charge in cents</param>
    /// <param name="currency">Currency code (e.g., usd)</param>
    /// <param name="description">Description of the payment</param>
    /// <param name="metadata">Additional metadata for the payment</param>
    /// <returns>Client secret for the payment intent</returns>
    Task<string> CreatePaymentIntentAsync(
        long amount,
        string currency,
        string description,
        Dictionary<string, string>? metadata = null
    );

    /// <summary>
    /// Create a new customer in Stripe
    /// </summary>
    /// <param name="email">Customer email</param>
    /// <param name="name">Customer name</param>
    /// <returns>Stripe customer ID</returns>
    Task<string> CreateCustomerAsync(string email, string name);

    /// <summary>
    /// Process a payment for a subscription
    /// </summary>
    /// <param name="paymentMethodId">Payment method ID</param>
    /// <param name="customerId">Customer ID</param>
    /// <param name="amount">Amount to charge in cents</param>
    /// <param name="currency">Currency code</param>
    /// <param name="description">Description of payment</param>
    /// <returns>Payment ID and status</returns>
    Task<(string PaymentId, string Status)> ProcessPaymentAsync(
        string paymentMethodId,
        string customerId,
        long amount,
        string currency,
        string description
    );

    /// <summary>
    /// Handle Stripe webhook events
    /// </summary>
    /// <param name="json">JSON payload from webhook</param>
    /// <param name="signatureHeader">Stripe signature header</param>
    /// <returns>True if event was handled successfully</returns>
    Task<bool> HandleWebhookEventAsync(string json, string signatureHeader);
}
