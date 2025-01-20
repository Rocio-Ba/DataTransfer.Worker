using DataTransfer.Worker.Model.Context;
using DataTransfer.Worker.Workers;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContextFactory<SourceDbContext>(p => p.UseSqlServer(builder.Configuration.GetConnectionString("SourceDB"),
                                                      q => 
                                                      { 
                                                          q.MigrationsHistoryTable("Migrations", "Common");
                                                          q.CommandTimeout(60); 

                                                          q.EnableRetryOnFailure(maxRetryCount: 5,
                                                                                 maxRetryDelay: TimeSpan.FromSeconds(10),
                                                                                 errorNumbersToAdd: null);
                                                      }));


builder.Services.AddDbContextFactory<DestinationDbContext>(p => p.UseSqlServer(builder.Configuration.GetConnectionString("DestinationDB"),
                                                           q =>
                                                           {
                                                               q.MigrationsHistoryTable("Migrations", "Common");
                                                               q.CommandTimeout(60); 

                                                               q.EnableRetryOnFailure(maxRetryCount: 5, 
                                                                                      maxRetryDelay: TimeSpan.FromSeconds(10),
                                                                                      errorNumbersToAdd: null);
                                                           }));


builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
  