using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Ridged.Application.Common.Behaviors;
using System.Reflection;

namespace Ridged.Application
{
    public static class Setup
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Register MediatR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            // Register FluentValidation validators
            services.AddValidatorsFromAssembly(assembly);

            return services;
        }
    }
}
