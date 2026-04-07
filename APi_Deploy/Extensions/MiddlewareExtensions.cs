using APi_Presentation.Middleware;

namespace APi_Presentation.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IServiceCollection AddGlobalErrorHandler(this IServiceCollection services)
        {
            services.AddScoped<GlobalErrorHandlerMiddleware>();
            return services;
        }

        public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalErrorHandlerMiddleware>();
            return app;
        }
    }
}