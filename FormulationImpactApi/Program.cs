using FormulationImpactApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Scoped lifetime is appropriate here: a new instance is created per HTTP request,
// which suits a stateless calculation service with no shared mutable state.
builder.Services.AddScoped<FormulationService>();

// AddOpenApi() registers the built-in .NET 10 OpenAPI document generator.
// This replaces the Swashbuckle/Swagger package that was the default in .NET 8.
// The generated OpenAPI document is served at /openapi/v1.json and consumed
// by Scalar below to provide an interactive API browser.
builder.Services.AddOpenApi();

// CORS policy for the Angular dev server.
// Without this, the browser will block cross-origin requests from localhost:4200
// to this API (which runs on a different port) due to the Same-Origin Policy.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Scalar is the interactive API UI shipped with .NET 10, replacing Swagger UI.
    // Swashbuckle (Swagger UI) is no longer the default as of .NET 9+.
    // Scalar consumes the OpenAPI document at /openapi/v1.json and provides
    // a modern, feature-rich browser at /scalar/v1.
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// UseCors must be called after UseRouting and before UseAuthorization
// so that CORS preflight requests are handled in the correct pipeline order.
app.UseCors("AllowAngularDev");

app.UseAuthorization();

app.MapControllers();

app.Run();
