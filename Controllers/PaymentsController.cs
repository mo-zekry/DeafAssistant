using System.Security.Claims;
using DeafAssistant.Context;
using DeafAssistant.Models;
using DeafAssistant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeafAssistant.Controllers;

/// <summary>
/// Controller for handling Stripe payments
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentsController> _logger;

    /// <summary>
    /// Constructor for PaymentsController
    /// </summary>
    public PaymentsController(
        IStripeService stripeService,
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<PaymentsController> logger
    )
    {
        _stripeService = stripeService;
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Creates a payment intent for client-side payment processing
    /// </summary>
    /// <param name="request">Payment intent create request</param>
    /// <returns>Client secret for the payment intent</returns>
    [Authorize]
    [HttpPost("create-payment-intent")]
    public async Task<ActionResult<PaymentIntentResponse>> CreatePaymentIntent(
        [FromBody] PaymentIntentCreateRequest request
    )
    {
        try
        {
            // Get user ID from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Create metadata for tracking
            var metadata = new Dictionary<string, string>
            {
                { "userId", userId },
                { "planName", request.PlanName },
                { "email", user.Email ?? string.Empty },
            };

            // Create a payment intent
            var clientSecret = await _stripeService.CreatePaymentIntentAsync(
                request.Amount,
                request.Currency,
                request.Description,
                metadata
            );

            // Return client secret and publishable key
            return new PaymentIntentResponse
            {
                ClientSecret = clientSecret,
                PublishableKey = _configuration["Stripe:PublishableKey"] ?? string.Empty,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment intent");
            return StatusCode(
                500,
                new { message = "Error creating payment intent", error = ex.Message }
            );
        }
    }

    /// <summary>
    /// Process a payment and create/update subscription
    /// </summary>
    /// <param name="request">Payment processing request</param>
    /// <returns>Payment result</returns>
    [Authorize]
    [HttpPost("process-payment")]
    public async Task<ActionResult<PaymentResponse>> ProcessPayment(
        [FromBody] ProcessPaymentRequest request
    )
    {
        try
        {
            // Get user ID from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authorized" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Check if the user already has a subscription with Stripe customer ID
            var existingSubscription = await _context.Subscription.FirstOrDefaultAsync(s =>
                s.UserId == userId
            );

            string stripeCustomerId;

            if (existingSubscription?.StripeCustomerId != null)
            {
                // Use existing Stripe customer ID
                stripeCustomerId = existingSubscription.StripeCustomerId;
            }
            else
            {
                // Create a new customer in Stripe
                stripeCustomerId = await _stripeService.CreateCustomerAsync(
                    user.Email ?? $"user_{userId}@example.com",
                    user.UserName ?? $"User {userId}"
                );
            }

            // Process the payment
            var (paymentId, status) = await _stripeService.ProcessPaymentAsync(
                request.PaymentMethodId,
                stripeCustomerId,
                request.Amount,
                request.Currency,
                request.Description
            );

            // Create or update subscription based on payment status
            if (status == "succeeded")
            {
                if (existingSubscription == null)
                {
                    // Create a new subscription
                    var subscription = new Subscription
                    {
                        UserId = userId,
                        PlanName = request.PlanName,
                        Price = request.Amount / 100m, // Convert cents to dollars
                        Currency = request.Currency.ToUpper(),
                        BillingFrequency = request.BillingFrequency,
                        PaymentMethod = "Stripe",
                        TransactionId = paymentId,
                        StripeCustomerId = stripeCustomerId,
                        StartDate = DateTime.UtcNow,
                        IsActive = true,
                        NextRenewalDate = DateTime.UtcNow.AddMonths(
                            request.BillingFrequency.ToLower() == "yearly" ? 12 : 1
                        ),
                    };

                    _context.Subscription.Add(subscription);
                }
                else
                {
                    // Update existing subscription
                    existingSubscription.PlanName = request.PlanName;
                    existingSubscription.Price = request.Amount / 100m;
                    existingSubscription.Currency = request.Currency.ToUpper();
                    existingSubscription.BillingFrequency = request.BillingFrequency;
                    existingSubscription.PaymentMethod = "Stripe";
                    existingSubscription.TransactionId = paymentId;
                    existingSubscription.StripeCustomerId = stripeCustomerId;
                    existingSubscription.IsActive = true;
                    existingSubscription.LastRenewalDate = DateTime.UtcNow;
                    existingSubscription.NextRenewalDate = DateTime.UtcNow.AddMonths(
                        request.BillingFrequency.ToLower() == "yearly" ? 12 : 1
                    );
                }

                await _context.SaveChangesAsync();

                return Ok(
                    new PaymentResponse
                    {
                        Success = true,
                        PaymentId = paymentId,
                        Status = status,
                    }
                );
            }

            return BadRequest(
                new PaymentResponse
                {
                    Success = false,
                    PaymentId = paymentId,
                    Status = status,
                    ErrorMessage = "Payment processing failed",
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return StatusCode(
                500,
                new PaymentResponse
                {
                    Success = false,
                    Status = "error",
                    ErrorMessage = ex.Message,
                }
            );
        }
    }

    /// <summary>
    /// Webhook endpoint for Stripe events
    /// </summary>
    /// <returns>OK if webhook was handled successfully</returns>
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            // Get the signature from the header
            Request.Headers.TryGetValue("Stripe-Signature", out var stripeSignature);
            var signature = stripeSignature.FirstOrDefault() ?? string.Empty;

            var success = await _stripeService.HandleWebhookEventAsync(json, signature);

            if (success)
            {
                return Ok();
            }
            else
            {
                return BadRequest(new { message = "Webhook processing failed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling webhook");
            return StatusCode(500, new { message = "Error handling webhook", error = ex.Message });
        }
    }
}
