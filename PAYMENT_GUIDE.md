# DeafAssistant Payment Integration Guide - Simplified

This guide explains how to use the simplified Stripe payment integration in the DeafAssistant API.

## Overview

The DeafAssistant API provides streamlined payment functionality using Stripe for handling subscriptions. The simplified API supports:

- Subscription plan management
- Basic payment processing with Stripe Elements
- Webhook processing for payment events

## API Documentation

Detailed API documentation is available in the [PaymentAPI.md](./docs/PaymentAPI.md) file.

## Quick Start

### Prerequisites

1. A Stripe account (test or production)
2. Stripe API keys configured in `appsettings.json`

```json
"Stripe": {
  "SecretKey": "sk_test_your_secret_key",
  "PublishableKey": "pk_test_your_publishable_key",
  "WebhookSecret": "whsec_your_webhook_secret",
  "Currency": "usd"
}
```

### Usage

#### 1. Display Available Subscription Plans

Fetch available subscription plans from the API:

```javascript
async function getSubscriptionPlans() {
  const response = await fetch('/api/Subscriptions/plans');
  return await response.json();
}
```

#### 2. Process a Payment with Stripe Elements

```javascript
// Initialize Stripe.js
const stripe = Stripe('YOUR_PUBLISHABLE_KEY');
const elements = stripe.elements();

// Create card element
const card = elements.create('card');
card.mount('#card-element');

// Process the payment
async function processPayment() {
  // Create a payment intent
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

  // Confirm the payment
  const { paymentIntent, error } = await stripe.confirmCardPayment(clientSecret, {
    payment_method: { card }
  });

  if (error) {
    // Handle error
    console.error(error.message);
  } else if (paymentIntent.status === 'succeeded') {
    // Process the subscription
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
    console.log('Payment successful!');
  }
}
```

#### 3. Handle Successful Payments

```javascript
// After a successful payment, you can check the user's subscription status
async function checkSubscriptionStatus() {
  const response = await fetch('/api/Subscriptions/me', {
    method: 'GET',
    headers: {
      'Authorization': 'Bearer YOUR_JWT_TOKEN'
    }
  });

  const subscription = await response.json();

  // Display subscription information to the user
  console.log(`Plan: ${subscription.planName}`);
  console.log(`Status: ${subscription.isActive ? 'Active' : 'Inactive'}`);
  console.log(`Next renewal: ${new Date(subscription.nextRenewalDate).toLocaleDateString()}`);
}
```

#### 4. Handle Webhook Events

Configure your Stripe webhook endpoint in the Stripe Dashboard to point to:

```
https://your-domain.com/api/Payments/webhook
```

Make sure to set up the webhook secret in your `appsettings.json` file.

For local development, you can use the Stripe CLI to forward events:

```powershell
stripe listen --forward-to https://localhost:7135/api/Payments/webhook
```

## Testing

Use Stripe's test cards for testing payments:

- **Success:** 4242 4242 4242 4242
- **Failure:** 4000 0000 0000 0002
- **Authentication Required:** 4000 0025 0000 3155
- **Insufficient Funds:** 4000 0000 0000 9995
- **Fails on Recurring Charges:** 4000 0000 0000 9987

For all test cards:

- Any future expiration date
- Any 3-digit CVC
- Any postal code

### Testing Common Scenarios

1. **New subscription**: Use a test card to sign up for a subscription plan
2. **Failed payment**: Use the failure card (4000 0000 0000 0002) to test error handling
3. **Renewal**: Use the CLI to fast-forward time and trigger renewal
4. **Cancellation**: Test the cancellation flow and confirm subscription end date is set correctly

For detailed testing procedures, see the [Testing Guide](./docs/STRIPE_TEST_ACCOUNT.md).

## Subscription Status Monitoring

### Checking Subscription Status

Users need to know their current subscription status. Implement a status dashboard that shows:

```javascript
// Fetch user's subscription
async function getSubscriptionStatus() {
  const response = await fetch('/api/Subscriptions/me', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });

  const subscription = await response.json();

  // Display subscription information
  if (subscription.isActive) {
    return {
      status: 'Active',
      plan: subscription.planName,
      renewalDate: new Date(subscription.nextRenewalDate).toLocaleDateString(),
      price: `${subscription.currency} ${subscription.price}`
    };
  } else {
    return {
      status: 'Inactive',
      message: 'Your subscription has expired or been cancelled'
    };
  }
}
```

### Alerting Users About Expiration

Send notifications when a subscription is about to expire:

1. Build a frontend mechanism to check subscription status on app launch
2. Display a banner when subscription will expire within the next 7 days
3. Provide a direct link to renew or change subscription plans

## React Integration

### Using Stripe Elements in React

To integrate Stripe Elements in a React application:

1. Install required packages:

```bash
npm install @stripe/react-stripe-js @stripe/stripe-js
```

2. Set up the Stripe provider and Elements:

```jsx
// App.js or payment component
import { Elements } from '@stripe/react-stripe-js';
import { loadStripe } from '@stripe/stripe-js';

// Load Stripe
const stripePromise = loadStripe('YOUR_PUBLISHABLE_KEY');

function App() {
  return (
    <Elements stripe={stripePromise}>
      <CheckoutForm />
    </Elements>
  );
}
```

3. Create a payment form component:

```jsx
// CheckoutForm.js
import { CardElement, useStripe, useElements } from '@stripe/react-stripe-js';

const CheckoutForm = () => {
  const stripe = useStripe();
  const elements = useElements();

  const handleSubmit = async (event) => {
    event.preventDefault();

    // Create payment intent using the API
    const response = await fetch('/api/Payments/create-payment-intent', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      },
      body: JSON.stringify({
        amount: 999,
        currency: 'usd',
        description: 'Premium Monthly Subscription',
        planName: 'premium_monthly'
      })
    });

    const { clientSecret } = await response.json();

    // Confirm payment
    const result = await stripe.confirmCardPayment(clientSecret, {
      payment_method: {
        card: elements.getElement(CardElement)
      }
    });

    if (result.error) {
      console.error(result.error.message);
    } else if (result.paymentIntent.status === 'succeeded') {
      // Process subscription
      // ...
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <CardElement />
      <button type="submit" disabled={!stripe}>Pay</button>
    </form>
  );
};
```

## Handling Subscription Lifecycle

### Subscription Renewal

Subscription renewals are handled automatically by Stripe. The webhook endpoint will receive events when:

- An invoice is created for renewal
- Payment succeeds or fails
- The subscription is updated

### Cancellation

To allow users to cancel their subscriptions:

```javascript
async function cancelSubscription(subscriptionId) {
  const response = await fetch(`/api/Subscriptions/${subscriptionId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });

  if (response.ok) {
    // Show cancellation confirmation
  } else {
    // Handle error
  }
}
```

### Upgrading or Downgrading Plans

To change a user's subscription plan:

```javascript
async function changePlan(subscriptionId, newPlanId) {
  const response = await fetch(`/api/Subscriptions/${subscriptionId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
      planId: newPlanId
    })
  });

  if (response.ok) {
    // Show success message
  } else {
    // Handle error
  }
}
```

## Further Information

For more details, see:

- [Stripe Documentation](https://stripe.com/docs)
- [API Documentation](./docs/PaymentAPI.md)
- [Stripe React Components](https://stripe.com/docs/stripe-js/react)
- [Testing Guide](./docs/STRIPE_TEST_ACCOUNT.md)
