using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Identity
{
    public class AppIdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Tùy chỉnh tên bảng nếu muốn (Oracle hay viết hoa)
            builder.Entity<ApplicationUser>().ToTable("ApplicationUsers");
            builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            // ... các bảng Identity khác (tùy chọn)
        }
    }
}
