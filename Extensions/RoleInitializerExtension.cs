using DeafAssistant.Services;

namespace DeafAssistant.Extensions;

/// <summary>
/// Extension methods for adding role initialization services to the application
/// </summary>
public static class RoleInitializerExtension
{
  /// <summary>
  /// Adds the role initializer service to the service collection
  /// </summary>
  /// <param name="services">The service collection</param>
  /// <returns>The service collection with role initializer service added</returns>
  public static IServiceCollection AddRoleInitializer(this IServiceCollection services)
  {
    services.AddScoped<RoleInitializerService>();
    return services;
  }

  /// <summary>
  /// Initializes application roles and default admin user
  /// </summary>
  /// <param name="app">The application builder</param>
  /// <returns>The application builder</returns>
  public static async Task<IApplicationBuilder> UseRoleInitializer(this IApplicationBuilder app)
  {
    using var scope = app.ApplicationServices.CreateScope();
    var roleInitializer = scope.ServiceProvider.GetRequiredService<RoleInitializerService>();
    await roleInitializer.InitializeAsync();
    return app;
  }
}
