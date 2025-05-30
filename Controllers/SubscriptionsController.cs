using DeafAssistant.Context;
using DeafAssistant.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeafAssistant.Controllers;

/// <summary>
/// Controller for managing user subscriptions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Constructor for SubscriptionsController
    /// </summary>
    /// <param name="context">Application database context</param>
    public SubscriptionsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all subscriptions (admin only)
    /// </summary>
    /// <returns>List of all subscriptions</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<Subscription>>> GetSubscriptions()
    {
        return await _context.Subscription.Include(s => s.User).ToListAsync();
    }

    /// <summary>
    /// Get available subscription plans
    /// </summary>
    /// <returns>List of available subscription plans</returns>
    [HttpGet("plans")]
    [AllowAnonymous]
    public ActionResult GetSubscriptionPlans()
    {
        // In a production environment, these would typically come from a database
        var plans = new List<object>
        {
            new
            {
                Id = "free",
                Name = "Free Plan",
                Price = 0,
                PriceInCents = 0,
                Currency = "USD",
                BillingFrequency = "Monthly",
                StripePriceId = "price_free", // Would be a real Stripe price ID in production
                Features = new[]
                {
                    "Basic translation features",
                    "Limited to 10 translations per day",
                    "Standard support",
                },
            },
            new
            {
                Id = "premium_monthly",
                Name = "Premium Monthly",
                Price = 9.99,
                PriceInCents = 999,
                Currency = "USD",
                BillingFrequency = "Monthly",
                StripePriceId = "price_1PMsevGBUvhcAy8sV8vKQ9t2", // Replace with your actual Stripe price ID
                Features = new[]
                {
                    "Unlimited translations",
                    "Access to premium features",
                    "Priority support",
                    "Offline mode",
                },
            },
            new
            {
                Id = "premium_yearly",
                Name = "Premium Yearly",
                Price = 99.99,
                PriceInCents = 9999,
                Currency = "USD",
                BillingFrequency = "Yearly",
                StripePriceId = "price_1PMseyGBUvhcAy8sPROiwFnN", // Replace with your actual Stripe price ID
                Features = new[]
                {
                    "Unlimited translations",
                    "Access to premium features",
                    "Priority support",
                    "Offline mode",
                    "Save 16% compared to monthly plan",
                },
            },
        };

        return Ok(plans);
    }

    /// <summary>
    /// Get a specific subscription by ID
    /// </summary>
    /// <param name="id">Subscription ID</param>
    /// <returns>The requested subscription</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Subscription>> GetSubscription(int id)
    {
        // Get current user ID from claims
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var subscription = await _context
            .Subscription.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subscription == null)
        {
            return NotFound();
        }

        // Non-admin users can only access their own subscriptions
        if (!User.IsInRole("Admin") && subscription.UserId != userId)
        {
            return Forbid();
        }

        return subscription;
    }

    /// <summary>
    /// Get current user's subscription
    /// </summary>
    /// <returns>The current user's subscription</returns>
    [HttpGet("me")]
    public async Task<ActionResult<Subscription>> GetMySubscription()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var subscription = await _context
            .Subscription.Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (subscription == null)
        {
            // Create a default free subscription for the user
            subscription = await CreateDefaultFreeSubscription(userId);
        }

        return subscription;
    }

    /// <summary>
    /// Creates a default free subscription for a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>The created subscription</returns>
    private async Task<Subscription> CreateDefaultFreeSubscription(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var subscription = new Subscription
        {
            UserId = userId,
            PlanName = "Free Plan",
            Price = 0,
            Currency = "USD",
            BillingFrequency = "Monthly",
            StartDate = DateTime.UtcNow,
            EndDate = null, // Free plan doesn't expire
            IsActive = true,
            AutoRenew = true,
            PaymentMethod = "None", // No payment method for free plan
            LastRenewalDate = DateTime.UtcNow,
            NextRenewalDate = DateTime.UtcNow.AddMonths(1) // Just for tracking
        };

        _context.Subscription.Add(subscription);
        await _context.SaveChangesAsync();

        // Include the user in the returned subscription
        subscription.User = user;

        return subscription;
    }

    /// <summary>
    /// Create a new subscription (admin only)
    /// </summary>
    /// <param name="subscription">Subscription data</param>
    /// <returns>Created subscription with ID</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Subscription>> CreateSubscription(Subscription subscription)
    {
        subscription.StartDate = DateTime.UtcNow;

        _context.Subscription.Add(subscription);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, subscription);
    }

    /// <summary>
    /// Update an existing subscription (admin only)
    /// </summary>
    /// <param name="id">Subscription ID</param>
    /// <param name="subscription">Updated subscription data</param>
    /// <returns>No content if successful</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSubscription(int id, Subscription subscription)
    {
        if (id != subscription.Id)
        {
            return BadRequest();
        }

        _context.Entry(subscription).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!SubscriptionExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a subscription (admin only)
    /// </summary>
    /// <param name="id">Subscription ID to delete</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSubscription(int id)
    {
        var subscription = await _context.Subscription.FindAsync(id);
        if (subscription == null)
        {
            return NotFound();
        }

        _context.Subscription.Remove(subscription);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool SubscriptionExists(int id)
    {
        return _context.Subscription.Any(e => e.Id == id);
    }
}
