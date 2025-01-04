using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wanvi.Contract.Repositories.IUOW;
using Wanvi.Repositories.UOW;

namespace Wanvi.Services
{
    public static class DependencyInjection
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRepositories();
        }
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
