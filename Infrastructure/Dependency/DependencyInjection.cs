using Domain.Interfaces; // واجهات موجودة في Domain Layer
using Infrastructure.Repositories; // الريبو اللي بيعمل implement للواجهات
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    // Extension method لتسجيل كل الخدمات الخاصة بالـ Infrastructure
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // تسجيل الـ Generic Repository
            // أي حاجة محتاجة IGeneralRepository<> هتاخد GeneralRepository<> تلقائي
            services.AddScoped(typeof(IGeneralRepository<>), typeof(GeneralRepository<>));

            return services;
        }
    }
}