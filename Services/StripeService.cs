using Stripe;

namespace DeafAssistant.Services;

/// <summary>
/// Service for handling Stripe payment operations
/// </summary>
public class StripeService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeService> _logger;

    /// <summary>
    /// Constructor for StripeService
    /// </summary>
    /// <param name="configuration">Application configuration</param>
    /// <param name="logger">Logger</param>
    public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Initialize Stripe with the API key
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
    }

    /// <summary>
    /// Create a payment intent for a subscription
    /// </summary>
    /// <param name="amount">Amount to charge in cents</param>
    /// <param name="currency">Currency code (e.g., usd)</param>
    /// <param name="description">Description of the payment</param>
    /// <param name="metadata">Additional metadata for the payment</param>
    /// <returns>Client secret for the payment intent</returns>
    public async Task<string> CreatePaymentIntentAsync(
        long amount,
        string currency,
        string description,
        Dictionary<string, string>? metadata = null
    )
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount, // Amount in cents
                Currency = currency,
                Description = description,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = metadata,
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return paymentIntent.ClientSecret;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating payment intent: {Message}", ex.Message);
            throw new Exception("Error creating payment intent", ex);
        }
    }

    /// <summary>
    /// Create a new customer in Stripe
    /// </summary>
    /// <param name="email">Customer email</param>
    /// <param name="name">Customer name</param>
    /// <returns>Stripe customer ID</returns>
    public async Task<string> CreateCustomerAsync(string email, string name)
    {
        try
        {
            var options = new CustomerCreateOptions { Email = email, Name = name };

            var service = new CustomerService();
            var customer = await service.CreateAsync(options);

            return customer.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating customer: {Message}", ex.Message);
            throw new Exception("Error creating Stripe customer", ex);
        }
    }

    /// <summary>
    /// Process a payment for a subscription
    /// </summary>
    /// <param name="paymentMethodId">Payment method ID</param>
    /// <param name="customerId">Customer ID</param>
    /// <param name="amount">Amount to charge in cents</param>
    /// <param name="currency">Currency code</param>
    /// <param name="description">Description of payment</param>
    /// <returns>Payment ID and status</returns>
    public async Task<(string PaymentId, string Status)> ProcessPaymentAsync(
        string paymentMethodId,
        string customerId,
        long amount,
        string currency,
        string description
    )
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = currency,
                Customer = customerId,
                PaymentMethod = paymentMethodId,
                Description = description,
                Confirm = true,
                ConfirmationMethod = "automatic",
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return (paymentIntent.Id, paymentIntent.Status);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error processing payment: {Message}", ex.Message);
            throw new Exception("Error processing payment", ex);
        }
    }

    /// <summary>
    /// Handle Stripe webhook events
    /// </summary>
    /// <param name="json">JSON payload from webhook</param>
    /// <param name="signatureHeader">Stripe signature header</param>
    /// <returns>True if event was handled successfully</returns>
    public Task<bool> HandleWebhookEventAsync(string json, string signatureHeader)
    {
        try
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);

            // Handle different types of events
            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                {
                    _logger.LogInformation(
                        "Payment succeeded: {PaymentIntentId}",
                        paymentIntent.Id
                    );
                    // Here you would update your database to mark the payment as successful
                }
            }
            else if (stripeEvent.Type == "payment_intent.payment_failed")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                {
                    _logger.LogWarning("Payment failed: {PaymentIntentId}", paymentIntent.Id);
                    // Here you would update your database to mark the payment as failed
                }
            }

            return Task.FromResult(true);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error handling webhook: {Message}", ex.Message);
            return Task.FromResult(false);
        }
    }
}
