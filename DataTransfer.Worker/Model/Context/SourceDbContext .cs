
using DataTransfer.Worker.Model.Entities;
using DataTransfer.Worker.Model.FluentConfigs;
using Microsoft.EntityFrameworkCore;


namespace DataTransfer.Worker.Model.Context;
public class SourceDbContext : DbContext
{
    public DbSet<User> User { get; set; }
    public SourceDbContext(DbContextOptions<SourceDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new UserFluentConfig());
    }

}
