# Simplified Payment API Documentation

This document provides documentation for the simplified DeafAssistant API's payment-related endpoints, including subscription management and Stripe integration.

## Table of Contents

1. [Authentication](#authentication)
2. [Subscription Plans](#subscription-plans)
3. [User Subscriptions](#user-subscriptions)
4. [Stripe Payments](#stripe-payments)
5. [Testing](#testing-with-stripe)
6. [Frontend Integration Examples](#frontend-integration)

## Authentication

All payment and subscription endpoints (except where noted) require authentication using a JWT token. Include the token in the `Authorization` header as follows:

```
Authorization: Bearer {your-jwt-token}
```

## Subscription Plans

### Get Available Subscription Plans

Returns the list of available subscription plans.

**Endpoint:** `GET /api/Subscriptions/plans`

**Authentication:** None required (public endpoint)

**Response:**

```json
[
  {
    "id": "free",
    "name": "Free Plan",
    "price": 0,
    "priceInCents": 0,
    "currency": "USD",
    "billingFrequency": "Monthly",
    "stripePriceId": "price_free",
    "features": [
      "Basic translation features",
      "Limited to 10 translations per day",
      "Standard support"
    ]
  },
  {
    "id": "premium_monthly",
    "name": "Premium Monthly",
    "price": 9.99,
    "priceInCents": 999,
    "currency": "USD",
    "billingFrequency": "Monthly",
    "stripePriceId": "price_1PMsevGBUvhcAy8sV8vKQ9t2",
    "features": [
      "Unlimited translations",
      "Access to premium features",
      "Priority support",
      "Offline mode"
    ]
  },
  {
    "id": "premium_yearly",
    "name": "Premium Yearly",
    "price": 99.99,
    "priceInCents": 9999,
    "currency": "USD",
    "billingFrequency": "Yearly",
    "stripePriceId": "price_1PMseyGBUvhcAy8sPROiwFnN",
    "features": [
      "Unlimited translations",
      "Access to premium features",
      "Priority support",
      "Offline mode",
      "Save 16% compared to monthly plan"
    ]
  }
]
```

## User Subscriptions

### Get Current User's Subscription

Retrieves the current user's active subscription details.

**Endpoint:** `GET /api/Subscriptions/me`

**Authentication:** Required

**Response:**

```json
{
  "id": 1,
  "userId": "user-guid-here",
  "planName": "Premium Monthly",
  "price": 9.99,
  "currency": "USD",
  "billingFrequency": "Monthly",
  "paymentMethod": "Stripe",
  "transactionId": "pi_1234567890",
  "stripeCustomerId": "cus_1234567890",
  "startDate": "2025-05-01T10:30:00Z",
  "endDate": null,
  "isActive": true,
  "autoRenew": true,
  "lastRenewalDate": "2025-05-01T10:30:00Z",
  "nextRenewalDate": "2025-06-01T10:30:00Z"
}
```

### Get Subscription by ID

Retrieves a specific subscription by ID. Users can only access their own subscriptions unless they have an admin role.

**Endpoint:** `GET /api/Subscriptions/{id}`

**Authentication:** Required

**Parameters:**

- `id` (path parameter): The ID of the subscription to retrieve

**Response:** Same format as "Get Current User's Subscription"

### Get All Subscriptions (Admin Only)

Retrieves all subscriptions in the system.

**Endpoint:** `GET /api/Subscriptions`

**Authentication:** Required (Admin role only)

**Response:**

```json
[
  {
    "id": 1,
    "userId": "user-guid-here",
    "planName": "Premium Monthly",
    "price": 9.99,
    "currency": "USD",
    "billingFrequency": "Monthly",
    "paymentMethod": "Stripe",
    "transactionId": "pi_1234567890",
    "stripeCustomerId": "cus_1234567890",
    "startDate": "2025-05-01T10:30:00Z",
    "endDate": null,
    "isActive": true,
    "autoRenew": true,
    "lastRenewalDate": "2025-05-01T10:30:00Z",
    "nextRenewalDate": "2025-06-01T10:30:00Z"
  },
  // More subscriptions...
]
```

## Stripe Payments

### Create Payment Intent

Creates a Stripe payment intent for client-side payment processing. Use this to initiate a payment flow with Stripe Elements.

**Endpoint:** `POST /api/Payments/create-payment-intent`

**Authentication:** Required

**Request Body:**

```json
{
  "amount": 999,
  "currency": "usd",
  "description": "Premium Monthly Subscription",
  "planName": "premium_monthly"
}
```

- `amount`: The amount to charge in cents (e.g., 999 for $9.99)
- `currency`: The currency code (lowercase, e.g., "usd")
- `description`: A description of the payment
- `planName`: The name of the subscription plan

**Response:**

```json
{
  "clientSecret": "pi_1234567890_secret_1234567890",
  "publishableKey": "pk_test_51PMsevGBUvhcAy8sGG2YJdYEOOSOzt4cyzQyTyDXFYN9SJO1NmWsOZ23BZ6LL63rcLRalIepPeilXazvDkZ2eLB600ZN5py8u4"
}
```

- `clientSecret`: The client secret to use with Stripe.js to confirm the payment
- `publishableKey`: Your Stripe publishable key for the frontend

### Process Payment

Processes a payment after it has been confirmed on the client side. Use this to finalize a payment and create or update a subscription.

**Endpoint:** `POST /api/Payments/process-payment`

**Authentication:** Required

**Request Body:**

```json
{
  "paymentMethodId": "pm_1234567890",
  "amount": 999,
  "currency": "usd",
  "description": "Premium Monthly Subscription",
  "planName": "premium_monthly",
  "billingFrequency": "Monthly"
}
```

- `paymentMethodId`: The ID of the payment method created by Stripe
- `amount`: The amount to charge in cents
- `currency`: The currency code
- `description`: A description of the payment
- `planName`: The name of the subscription plan
- `billingFrequency`: The billing frequency (e.g., "Monthly", "Yearly")

**Response:**

```json
{
  "success": true,
  "paymentId": "pi_1234567890",
  "status": "succeeded",
  "errorMessage": null
}
```

### Create Checkout Session

Creates a Stripe Checkout session for redirection to Stripe's hosted checkout page.

**Endpoint:** `POST /api/Payments/create-checkout-session`

**Authentication:** Required

**Request Body:**

```json
{
  "priceId": "price_1PMsevGBUvhcAy8sV8vKQ9t2",
  "successUrl": "https://yourdomain.com/success",
  "cancelUrl": "https://yourdomain.com/cancel"
}
```

- `priceId`: The Stripe Price ID for the subscription plan
- `successUrl`: The URL to redirect to after successful payment
- `cancelUrl`: The URL to redirect to if payment is canceled

**Response:**

```json
{
  "sessionId": "cs_1234567890",
  "url": "https://checkout.stripe.com/c/pay/cs_1234567890"
}
```

- `sessionId`: The Stripe Checkout session ID
- `url`: The URL to redirect the user to for checkout

### Handle Webhook

Handles Stripe webhook events to update subscription status based on payment events.

**Endpoint:** `POST /api/Payments/webhook`

**Authentication:** None required (uses Stripe signature verification)

**Headers:**

- `Stripe-Signature`: The signature provided by Stripe to verify the webhook

**Request Body:** Raw JSON payload from Stripe

**Response:**

- 200 OK if the webhook was handled successfully
- 400 Bad Request if there was an issue processing the webhook

## Testing with Stripe

### Test Cards

When testing payments, you can use Stripe's test card numbers:

- **Successful payment**: `4242 4242 4242 4242`
- **Failed payment**: `4000 0000 0000 0002`
- **Requires authentication**: `4000 0025 0000 3155`

For all test cards:

- Any future expiration date
- Any three-digit CVC
- Any postal code

### Testing Webhooks

To test webhooks locally:

1. Install the [Stripe CLI](https://stripe.com/docs/stripe-cli)
2. Log in with `stripe login`
3. Forward events to your local server:

   ```
   stripe listen --forward-to http://localhost:7135/api/Payments/webhook
   ```

## Frontend Integration

### Direct Payment with Stripe Elements

```html
<form id="payment-form">
  <div id="card-element">
    <!-- Stripe Elements will be inserted here -->
  </div>
  <div id="card-errors" role="alert"></div>
  <button type="submit">Pay Now</button>
</form>

<script src="https://js.stripe.com/v3/"></script>
<script>
  // Initialize Stripe.js with your publishable key
  const stripe = Stripe('pk_test_51PMsevGBUvhcAy8sGG2YJdYEOOSOzt4cyzQyTyDXFYN9SJO1NmWsOZ23BZ6LL63rcLRalIepPeilXazvDkZ2eLB600ZN5py8u4');
  const elements = stripe.elements();

  // Create card element
  const card = elements.create('card');
  card.mount('#card-element');

  // Handle form submission
  document.getElementById('payment-form').addEventListener('submit', async (event) => {
    event.preventDefault();

    try {
      // Create payment intent
      const response = await fetch('/api/Payments/create-payment-intent', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer YOUR_JWT_TOKEN'
        },
        body: JSON.stringify({
          amount: 999, // $9.99
          currency: 'usd',
          description: 'Premium Monthly Subscription',
          planName: 'premium_monthly'
        })
      });

      const { clientSecret } = await response.json();

      // Confirm the card payment
      const { paymentIntent, error } = await stripe.confirmCardPayment(clientSecret, {
        payment_method: { card }
      });

      if (error) {
        // Display error to customer
        const errorElement = document.getElementById('card-errors');
        errorElement.textContent = error.message;
      } else if (paymentIntent.status === 'succeeded') {
        // Process the subscription on your server
        await fetch('/api/Payments/process-payment', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer YOUR_JWT_TOKEN'
          },
          body: JSON.stringify({
            paymentMethodId: paymentIntent.payment_method,
            amount: 999,
            currency: 'usd',
            description: 'Premium Monthly Subscription',
            planName: 'premium_monthly',
            billingFrequency: 'Monthly'
          })
        });

        // Show success message
        window.location.href = '/success';
      }
    } catch (error) {
      console.error('Error:', error);
    }
  });
</script>
```

### Stripe Checkout (Redirect Flow)

```html
<button id="checkout-button" data-price-id="price_1PMsevGBUvhcAy8sV8vKQ9t2">
  Subscribe with Stripe Checkout
</button>

<script>
  document.getElementById('checkout-button').addEventListener('click', async (event) => {
    const priceId = event.target.dataset.priceId;

    try {
      const response = await fetch('/api/Payments/create-checkout-session', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer YOUR_JWT_TOKEN'
        },
        body: JSON.stringify({
          priceId: priceId,
          successUrl: `${window.location.origin}/success`,
          cancelUrl: `${window.location.origin}/cancel`
        })
      });

      const { url } = await response.json();

      // Redirect to Stripe Checkout
      window.location.href = url;
    } catch (error) {
      console.error('Error:', error);
    }
  });
</script>
```

### React Implementation Example

```jsx
import React, { useState } from 'react';
import { loadStripe } from '@stripe/stripe-js';
import {
  CardElement,
  Elements,
  useStripe,
  useElements,
} from '@stripe/react-stripe-js';

const stripePromise = loadStripe('pk_test_51PMsevGBUvhcAy8sGG2YJdYEOOSOzt4cyzQyTyDXFYN9SJO1NmWsOZ23BZ6LL63rcLRalIepPeilXazvDkZ2eLB600ZN5py8u4');

const CheckoutForm = () => {
  const stripe = useStripe();
  const elements = useElements();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setLoading(true);
    setError(null);

    if (!stripe || !elements) {
      setLoading(false);
      return;
    }

    try {
      // Create payment intent
      const response = await fetch('/api/Payments/create-payment-intent', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
        },
        body: JSON.stringify({
          amount: 999,
          currency: 'usd',
          description: 'Premium Monthly Subscription',
          planName: 'premium_monthly'
        }),
      });

      const { clientSecret } = await response.json();

      // Confirm the payment
      const cardElement = elements.getElement(CardElement);
      const { error, paymentIntent } = await stripe.confirmCardPayment(clientSecret, {
        payment_method: {
          card: cardElement,
          billing_details: {
            name: 'Customer Name',
          },
        },
      });

      if (error) {
        setError(error.message);
      } else if (paymentIntent.status === 'succeeded') {
        // Process the payment on the server
        const processResponse = await fetch('/api/Payments/process-payment', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${localStorage.getItem('token')}`,
          },
          body: JSON.stringify({
            paymentMethodId: paymentIntent.payment_method,
            amount: 999,
            currency: 'usd',
            description: 'Premium Monthly Subscription',
            planName: 'premium_monthly',
            billingFrequency: 'Monthly'
          }),
        });

        const processResult = await processResponse.json();

        if (processResult.success) {
          setSuccess(true);
        } else {
          setError(processResult.errorMessage || 'Payment processing failed');
        }
      }
    } catch (err) {
      setError('An unexpected error occurred. Please try again.');
      console.error(err);
    }

    setLoading(false);
  };

  if (success) {
    return <div>Payment successful! Your subscription is now active.</div>;
  }

  return (
    <form onSubmit={handleSubmit}>
      <CardElement />
      {error && <div style={{ color: 'red' }}>{error}</div>}
      <button type="submit" disabled={!stripe || loading}>
        {loading ? 'Processing...' : 'Pay $9.99'}
      </button>
    </form>
  );
};

const StripePaymentForm = () => (
  <Elements stripe={stripePromise}>
    <CheckoutForm />
  </Elements>
);

export default StripePaymentForm;
```

## Webhook Integration

Webhooks are used to receive notifications from Stripe about payment events, such as successful payments, failed payments, and subscription renewals. The webhook endpoint processes these events and updates your database accordingly.

### Supported Events

- `payment_intent.succeeded`: Triggered when a payment is successful
- `payment_intent.payment_failed`: Triggered when a payment fails
- `customer.subscription.created`: Triggered when a subscription is created
- `customer.subscription.updated`: Triggered when a subscription is updated
- `customer.subscription.deleted`: Triggered when a subscription is canceled
- `invoice.paid`: Triggered when an invoice is paid successfully
- `invoice.payment_failed`: Triggered when an invoice payment fails
- `checkout.session.completed`: Triggered when a checkout session completes

### Configuring Webhooks in Stripe Dashboard

1. Go to the [Stripe Dashboard](https://dashboard.stripe.com/webhooks)
2. Click "Add endpoint"
3. Enter your webhook URL: `https://yourdomain.com/api/Payments/webhook`
4. Select the events you want to receive
5. Click "Add endpoint"
6. Copy the signing secret and add it to your `appsettings.json` file under `Stripe:WebhookSecret`

### Security

- Webhooks are secured using Stripe's signature verification
- The webhook secret is used to verify that requests are coming from Stripe
- Always use HTTPS for your webhook endpoint to ensure secure communication

### Processing Webhook Events

The webhook endpoint in DeafAssistant handles various Stripe events to update the database accordingly:

#### Example Webhook Handling Flow

1. Stripe sends an event to your webhook endpoint
2. The API verifies the signature to ensure the event is from Stripe
3. The event is processed based on its type:

   For `payment_intent.succeeded`:

   ```csharp
   // Pseudo-code for webhook handling
   if (stripeEvent.Type == "payment_intent.succeeded")
   {
       var paymentIntent = stripeEvent.Data.Object as Stripe.PaymentIntent;
       var userId = paymentIntent.Metadata["userId"];

       // Update the user's subscription status
       var subscription = await _context.Subscription.FirstOrDefaultAsync(s => s.UserId == userId);
       if (subscription != null)
       {
           subscription.IsActive = true;
           subscription.LastRenewalDate = DateTime.UtcNow;
           subscription.NextRenewalDate = CalculateNextRenewalDate(DateTime.UtcNow, subscription.BillingFrequency);
           await _context.SaveChangesAsync();
       }
   }
   ```

4. The API returns a 200 OK response to acknowledge receipt of the event

### Testing Webhooks

To test webhooks during development:

1. Install the Stripe CLI:

   ```bash
   # On Windows with Chocolatey
   choco install stripe-cli

   # On macOS with Homebrew
   brew install stripe/stripe-cli/stripe-cli
   ```

2. Log in to your Stripe account:

   ```bash
   stripe login
   ```

3. Forward events to your local server:

   ```bash
   stripe listen --forward-to http://localhost:7135/api/Payments/webhook
   ```

4. In a separate terminal, trigger test events:

   ```bash
   stripe trigger payment_intent.succeeded
   ```

### Webhook Reliability

To ensure reliable processing of webhook events:

1. **Implement retries:** Stripe will retry delivering events that fail
2. **Handle duplicate events:** The same event may be delivered multiple times
3. **Process events asynchronously:** Use a queue for processing webhook events
4. **Implement logging:** Log all webhook events for debugging and auditing
5. **Monitor webhook failures:** Set up alerts for webhook delivery failures

## Error Handling

All payment endpoints return appropriate HTTP status codes and error messages:

| Status Code | Description | Example |
|-------------|-------------|---------|
| 400 Bad Request | Invalid request parameters | Missing required fields, invalid data format |
| 401 Unauthorized | Missing or invalid authentication | Expired token, invalid token |
| 403 Forbidden | Permission denied | Attempting to access another user's subscription |
| 404 Not Found | Resource not found | Subscription not found, user not found |
| 500 Internal Server Error | Server-side error | Database error, Stripe API error |

Error responses include a message explaining the error and, when appropriate, additional details to help troubleshoot the issue:

```json
{
  "message": "Error creating payment intent",
  "error": "Your card was declined"
}
```

### Common Error Scenarios

1. **Card Declined:** When a card is declined by the bank or payment processor

   ```json
   {
     "success": false,
     "status": "failed",
     "errorMessage": "Your card was declined"
   }
   ```

2. **Authentication Required:** When a card requires additional authentication

   ```json
   {
     "success": false,
     "status": "requires_authentication",
     "errorMessage": "This payment requires additional authentication"
   }
   ```

3. **Invalid Request:** When the request is missing required parameters

   ```json
   {
     "message": "Invalid request",
     "error": "Amount is required and must be greater than zero"
   }
   ```

### Handling Failed Payments

When a payment fails, the API will return detailed information about the failure. Your frontend application should:

1. Display the error message to the user
2. Provide clear instructions on how to resolve the issue
3. Allow the user to retry the payment

## Best Practices

1. **Always validate user input** on both client and server sides
2. **Handle payment errors gracefully** and provide clear feedback to users
3. **Use Stripe's test mode** for development and testing
4. **Implement proper error logging** for payment processing issues
5. **Keep Stripe API keys secure** and never expose them in client-side code
6. **Regularly check for failed payments** and attempt to recover them
7. **Monitor webhook deliveries** to ensure events are being processed correctly
8. **Implement idempotency** to prevent duplicate charges
9. **Provide clear receipts and confirmation** after successful payments
10. **Follow PCI compliance guidelines** by using Stripe Elements or Checkout
11. **Implement proper retry logic** for failed API calls
12. **Test the entire payment flow** before deploying to production

### Idempotency in Payments

Idempotency ensures that a payment operation is only performed once, even if the API request is sent multiple times. This is crucial to prevent duplicate charges.

When using the Stripe API directly, you can include an Idempotency-Key header:

```javascript
const response = await fetch('/api/Payments/create-payment-intent', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': 'Bearer YOUR_JWT_TOKEN',
    'Idempotency-Key': 'a-unique-identifier-for-this-operation'  // e.g., userId-timestamp
  },
  body: JSON.stringify({
    amount: 999,
    currency: 'usd',
    description: 'Premium Monthly Subscription',
    planName: 'premium_monthly'
  })
});
```

The DeafAssistant API internally uses Stripe's idempotency keys to ensure that operations like creating customers and processing payments are idempotent.
