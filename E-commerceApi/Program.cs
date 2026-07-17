
using E_commerceApi.extension;

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

var app = builder.Build();

// Add swagger use
app.UseSwaggerServices();

// Add CORS use 
app.UseCorsServices();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapAdminEndpoints();
app.MapPublicCatalogEndpoints();


app.Run();
