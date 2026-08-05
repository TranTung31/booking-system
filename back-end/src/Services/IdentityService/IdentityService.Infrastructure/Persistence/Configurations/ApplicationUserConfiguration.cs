using IdentityService.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityService.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // Đổi tên bảng (Oracle thường viết hoa)
        builder.ToTable("ApplicationUsers");

        // Nếu muốn thay đổi độ dài mặc định của UserName hoặc Email
        builder.Property(u => u.UserName)
            .HasMaxLength(256);

        builder.Property(u => u.Email)
            .HasMaxLength(256);
    }
}
