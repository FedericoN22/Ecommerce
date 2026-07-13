using E_commerceApi.Application.Interfaces;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            IAuthService authService) =>
        {
            var result = await authService.RegisterAsync(request);
            return Results.Ok(result);
        });

        group.MapPost("/login", async (
            LoginRequest request,
            IAuthService authService) =>
        {
            var result = await authService.LoginAsync(request);
            return Results.Ok(result);
        });

    }

}