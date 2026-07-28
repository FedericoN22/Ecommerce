
using E_commerceApi.extension;
using E_commerceApi.Infrastructure.Services;
using E_commerceApi.Middleware;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using FluentValidation;
using Stripe;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDatabase(builder.Configuration);

// Add Identity services
builder.Services.AddIdentityServices();

// Add authentication services
builder.Services.AddJwtServices(builder.Configuration);

// Add CORS services
builder.Services.AddCorsServices();

// Add Swagger services
builder.Services.AddSwaggerServices();

builder.Services.AddServices();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddProblemDetails();
builder.Services.AddScoped<ExceptionMiddleware>();
builder.Services.AddHostedService<PendingOrderExpirationService>();

// Stripe
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

var app = builder.Build();

// Add swagger use
app.UseSwaggerServices();

app.UseMiddleware<ExceptionMiddleware>();

// Add CORS use 
app.UseCorsServices();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapAdminEndpoints();
app.MapPublicCatalogEndpoints();
app.MapCartEndpoints();
app.MapOrderEndpoints();


app.Run();
