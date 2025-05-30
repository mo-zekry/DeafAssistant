# Stripe Test Account Setup

This document provides instructions for setting up and using the Stripe test account for payment integration testing in the DeafAssistant API.

## Test Account Details

The application is configured with the following Stripe test credentials:

```json
"Stripe": {
  "SecretKey": "sk_test_51PMsevGBUvhcAy8sVH6TeIDiX3lMjw3PpltIWPRDu8tLXtXSgVr6FZyelqrciYQgrdMV7gVPHsoFzzudAalFH50j00f0MnDbsy",
  "PublishableKey": "pk_test_51PMsevGBUvhcAy8sGG2YJdYEOOSOzt4cyzQyTyDXFYN9SJO1NmWsOZ23BZ6LL63rcLRalIepPeilXazvDkZ2eLB600ZN5py8u4",
  "WebhookSecret": "whsec_test_secret_key",
  "Currency": "usd"
}
```

**Important**: These are test keys only. For production, use live keys and store them securely.

## Available Test Plans

The following test plans are configured in the Stripe test account:

1. **Premium Monthly**
   - Price ID: `price_1PMsevGBUvhcAy8sV8vKQ9t2`
   - Amount: $9.99 (999 cents)
   - Currency: USD
   - Billing Frequency: Monthly

2. **Premium Yearly**
   - Price ID: `price_1PMseyGBUvhcAy8sPROiwFnN`
   - Amount: $99.99 (9999 cents)
   - Currency: USD
   - Billing Frequency: Yearly

## Testing Payments

### Test Cards

You can use the following test cards for payment testing:

| Card Number | Description |
|-------------|-------------|
| 4242 4242 4242 4242 | Successful payment |
| 4000 0000 0000 0002 | Declined payment |
| 4000 0025 0000 3155 | Authentication required |
| 4000 0000 0000 9995 | Insufficient funds |
| 4000 0000 0000 9987 | Successful for first charge, but fails for subsequent charges |
| 4000 1000 0000 0009 | Successful but disputes as fraudulent |

For all test cards:

- Use any future expiration date
- Use any 3-digit CVC code
- Use any postal code

### Testing Recurring Subscriptions

For testing recurring subscriptions in the test environment:

1. **Create a subscription** using the Stripe Checkout or Elements integration
2. **Fast-forward time** using the Stripe Dashboard or CLI:

   ```powershell
   # Fast-forward a subscription invoice by 1 month
   stripe subscriptions test_clock advance_to_hour --hours-from-now 730 --id={CLOCK_ID}
   ```

3. **Observe subscription renewal behavior** as Stripe processes the renewal automatically

4. **Test subscription cancellation** using the Dashboard or API:

   ```powershell
   # Cancel a subscription using the Stripe CLI
   stripe subscriptions cancel {SUBSCRIPTION_ID}
   ```

5. **Test payment failure scenarios** by using test cards that fail on recurring charges

### Testing Webhooks for Subscription Events

When testing subscription events with webhooks:

1. Start webhook forwarding:

   ```powershell
   stripe listen --forward-to https://localhost:7135/api/Payments/webhook
   ```

2. Trigger specific subscription events:

   ```powershell
   # Test subscription creation
   stripe trigger customer.subscription.created

   # Test subscription update
   stripe trigger customer.subscription.updated

   # Test subscription cancellation
   stripe trigger customer.subscription.deleted
   ```

### Testing Stripe Elements Integration

1. Create a payment intent using the API endpoint:

   ```
   POST /api/Payments/create-payment-intent
   ```

2. Use the returned client secret to confirm the payment on the frontend using Stripe.js.

3. Process the payment with the API endpoint:

   ```
   POST /api/Payments/process-payment
   ```

### Testing Stripe Checkout Integration

1. Create a checkout session using the API endpoint:

   ```
   POST /api/Payments/create-checkout-session
   ```

2. Redirect the user to the returned URL.

3. Complete the payment using one of the test cards.

## Testing Webhooks Locally

To test webhooks locally:

1. Install the Stripe CLI: [https://stripe.com/docs/stripe-cli](https://stripe.com/docs/stripe-cli)

2. Log in with your Stripe account:

   ```powershell
   stripe login
   ```

3. Start forwarding events to your local webhook endpoint:

   ```powershell
   stripe listen --forward-to https://localhost:7135/api/Payments/webhook
   ```

4. In a separate terminal, trigger a test webhook event:

   ```powershell
   stripe trigger payment_intent.succeeded
   ```

## Viewing Test Data in Stripe Dashboard

You can view all test transactions, customers, and subscriptions in the [Stripe Dashboard](https://dashboard.stripe.com/test/dashboard).

Make sure "Test Mode" is enabled to see test data.

## Transitioning to Production

When you're ready to go live:

1. Create a Stripe live account
2. Replace the test keys with live keys
3. Configure live webhooks in the Stripe Dashboard
4. Update the price IDs to match your live plans

**Important**: Never use test cards or test mode in production environments.

## Support

For issues related to Stripe integration, refer to the [Stripe Documentation](https://stripe.com/docs) or contact the development team.
