using IdentityService.Domain.Interfaces;
using IdentityService.Infrastructure.Identity;
using IdentityService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký DbContext cho Oracle
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseOracle(configuration.GetConnectionString("OracleConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppIdentityDbContext>();

            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
