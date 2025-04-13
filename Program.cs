using DeafAssistant.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DeafAssistant;

// TODO: Secure the connection string in production
// TODO: Add logging
// TODO: Add error handling
// [x]: Add authentication and authorization
// TODO: Add Swagger documentation
// TODO: Add unit tests
// [x]: Cors configuration
public static class Program
{
  public static async Task Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container using extension methods
    builder
      .Services.AddApplicationServices(builder.Configuration)
      .AddDatabaseServices(builder.Configuration)
      .AddIdentityServices()
      .AddSwaggerServices()
      .AddCorsConfiguration()
      .AddRoleInitializer(); // Add role initializer service

    var app = builder.Build();

    app.UseCors("CorsPolicy");

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
      app.UseSwaggerMiddleware();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Initialize roles and admin user before starting the app
    await app.UseRoleInitializer();

    await app.RunAsync();
  }
}
