# Deaf Assistant

A web application designed to assist deaf individuals with learning and communication.

## Features

- User Authentication and Authorization
- Email Verification
- Lesson Management
- User Subscriptions
- Feedback System

## Prerequisites

- .NET 9.0 SDK
- SQL Server
- Visual Studio, VS Code, or JetBrains Rider

## Configuration

Before running the application, you need to set up the following configuration values in your user secrets:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your_connection_string"
  },
  "JWT": {
    "Secret": "your_jwt_secret_key_min_32_chars",
    "ValidIssuer": "your_issuer",
    "ValidAudience": "your_audience"
  },
  "EmailSettings": {
    "SmtpUsername": "your_email@example.com",
    "SmtpPassword": "your_email_password",
    "SenderEmail": "your_email@example.com",
    "SenderName": "Your Sender Name"
  }
}
```

To set up user secrets:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your_connection_string"
# Add other secrets similarly
```

## Getting Started

1. Clone the repository
2. Set up user secrets as described above
3. Run database migrations:

   ```bash
   dotnet ef database update
   ```

4. Run the application:

   ```bash
   dotnet run
   ```

## API Documentation

Once running, access the Swagger documentation at:

- <http://localhost:5256/swagger> (HTTP)
- <https://localhost:7135/swagger> (HTTPS)
