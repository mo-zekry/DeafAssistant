# Simplified Payment System Documentation

This document provides documentation for the simplified DeafAssistant payment system.

## Available Endpoints

### 1. Create Payment Intent

```
POST /api/Payments/create-payment-intent
```

This endpoint creates a payment intent for client-side payment processing using Stripe Elements.

**Request body:**

```json
{
  "amount": 999,
  "currency": "usd",
  "description": "Premium Monthly Subscription",
  "planName": "Premium Monthly"
}
```

**Response:**

```json
{
  "clientSecret": "pi_xxxxx_secret_xxxxx",
  "publishableKey": "pk_test_xxxxx"
}
```

### 2. Process Payment

```
POST /api/Payments/process-payment
```

This endpoint processes a payment and creates or updates a subscription for the user.

**Request body:**

```json
{
  "paymentMethodId": "pm_xxxxx",
  "amount": 999,
  "currency": "usd",
  "description": "Premium Monthly Subscription",
  "planName": "Premium Monthly",
  "billingFrequency": "Monthly"
}
```

**Response:**

```json
{
  "success": true,
  "paymentId": "pi_xxxxx",
  "status": "succeeded"
}
```

### 3. Webhook Handler

```
POST /api/Payments/webhook
```

This endpoint handles webhook events from Stripe, such as successful payments or failed payments.

## Client-Side Integration Example

```javascript
// Initialize Stripe
const stripe = Stripe('YOUR_PUBLISHABLE_KEY');
const elements = stripe.elements();
const card = elements.create('card');
card.mount('#card-element');

// Handle form submission
document.getElementById('payment-form').addEventListener('submit', async (event) => {
  event.preventDefault();

  // Step 1: Create a payment intent
  const intentResponse = await fetch('/api/Payments/create-payment-intent', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer YOUR_JWT_TOKEN'
    },
    body: JSON.stringify({
      amount: 999,
      currency: 'usd',
      description: 'Premium Monthly Subscription',
      planName: 'premium_monthly'
    })
  });

  const { clientSecret } = await intentResponse.json();

  // Step 2: Confirm the payment with the card
  const { paymentIntent, error } = await stripe.confirmCardPayment(clientSecret, {
    payment_method: { card }
  });

  if (error) {
    console.error(error.message);
  } else if (paymentIntent.status === 'succeeded') {
    // Step 3: Process the subscription
    const processResponse = await fetch('/api/Payments/process-payment', {
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

    const result = await processResponse.json();
    if (result.success) {
      console.log('Payment successful!');
    }
  }
});
```

## Testing

You can test the payment system using the Stripe test mode and test cards:

- **Test card success**: 4242 4242 4242 4242
- **Test card decline**: 4000 0000 0000 0002

Remember to use a future expiry date, any 3-digit CVC, and any postal code.
