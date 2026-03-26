using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using TimePlanner.Services.Interfaces;
using TimePlanner.Services.Services;


namespace TimePlanner.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IMapper, Mapper>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IAssignmentService, AssignmentService>();

            return services;
        }
    }
}