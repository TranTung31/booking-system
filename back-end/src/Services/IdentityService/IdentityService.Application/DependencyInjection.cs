using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace IdentityService.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Đăng ký AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Đăng ký MediatR, tự quét tất cả handler trong assembly này
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // (Tùy chọn) Đăng ký FluentValidation + pipeline behavior
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
