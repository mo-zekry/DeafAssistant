namespace DeafAssistant.Extensions;

public static class SwaggerConfiguration
{
  public static IApplicationBuilder UseSwaggerMiddleware(this IApplicationBuilder app)
  {
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
      // Set the Swagger UI to use the XML comments file.
      c.SwaggerEndpoint("/swagger/v1/swagger.json", "DeafAssistant API v1");
      c.RoutePrefix = string.Empty; // Set Swagger UI at the root
    });

    return app;
  }
}
