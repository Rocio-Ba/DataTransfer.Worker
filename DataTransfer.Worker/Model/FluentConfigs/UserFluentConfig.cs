using DataTransfer.Worker.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataTransfer.Worker.Model.FluentConfigs;

public class UserFluentConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {

        builder.ToTable("User", "Account");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.SecurityKey).HasColumnType("nvarchar(256)").IsRequired();

    }
}
