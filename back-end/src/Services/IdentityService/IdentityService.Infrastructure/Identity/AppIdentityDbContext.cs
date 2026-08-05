using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace IdentityService.Infrastructure.Identity
{
    public class AppIdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
            : base(options) { }

        // Tùy chỉnh các quy ước cho Oracle, ép kiểu dữ liệu về đúng định dạng mong muốn
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            configurationBuilder.Properties<bool>()
                .HaveColumnType("NUMBER(1)");

            configurationBuilder.Properties<decimal>()
                .HaveColumnType("NUMBER(20, 4)");

            configurationBuilder.Properties<double>()
                .HaveColumnType("BINARY_DOUBLE");

            configurationBuilder.Properties<float>()
                .HaveColumnType("BINARY_FLOAT");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Áp dụng tất cả các lớp IEntityTypeConfiguration trong cùng assembly
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
