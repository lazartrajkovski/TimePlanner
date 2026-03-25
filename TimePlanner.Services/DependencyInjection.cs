using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;


namespace TimePlanner.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
