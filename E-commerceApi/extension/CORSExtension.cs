
namespace E_commerceApi.extension
{
    public static class CORSExtension
    {
        public static void AddCorsServices(this IServiceCollection services)
        {

            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });
        }

        public static void UseCorsServices(this WebApplication app)
        {
            app.UseCors("AllowFrontend");
        }


    }
}