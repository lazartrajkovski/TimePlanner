using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TimePlanner.Domain.Entities;
using TimePlanner.Services.Interfaces;
using TimePlanner.Services.Services;

namespace TimePlanner.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IMapper, Mapper>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IAssignmentService, AssignmentService>();
            services.AddScoped<ITimeEntryService, TimeEntryService>();

            return services;
        }
    }
}