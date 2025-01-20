using DataTransfer.Worker.Model.Entities;
using DataTransfer.Worker.Model.FluentConfigs;
using Microsoft.EntityFrameworkCore;
using System;


namespace DataTransfer.Worker.Model.Context;

public class DestinationDbContext : DbContext
{
    public DbSet<User> User { get; set; }
    public DestinationDbContext(DbContextOptions<DestinationDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new UserFluentConfig());

        // add-migration destinationdb -context DataTransfer.Worker.Model.Context.DestinationDbContext -outputdir ./Model/Migrations
        // update-database -context DataTransfer.Worker.Model.Context.DestinationDbContext
    }

}
