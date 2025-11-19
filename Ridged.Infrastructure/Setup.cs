using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ridged.Infrastructure.Extensions;

namespace Ridged.Infrastructure
{
    public static class Setup
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Database
            services.AddDatabase(configuration);

            // Register Identity Services
            services.AddIdentityServices();

            // Register Infrastructure Services (Repositories, JWT, etc.)
            services.AddInfrastructureServices();

            return services;
        }
    }
}
