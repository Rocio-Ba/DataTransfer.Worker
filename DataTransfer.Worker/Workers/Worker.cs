using DataTransfer.Worker.Model.Context;
using DataTransfer.Worker.Model.Entities;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DataTransfer.Worker.Workers
{
    public class Worker : BackgroundService
    {
        private readonly IDbContextFactory<SourceDbContext> _sourceDbContextFactory;
        private readonly IDbContextFactory<DestinationDbContext> _destinationDbContextFactory;
        public Worker(IDbContextFactory<SourceDbContext> sourceDbContextFactory,IDbContextFactory<DestinationDbContext> destinationDbContextFactory)
        {
            _sourceDbContextFactory = sourceDbContextFactory;
            _destinationDbContextFactory = destinationDbContextFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("DataTransferWorker started.");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await TransferDataAsync(stoppingToken);
                    Console.WriteLine("Data transfer completed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in DataTransferWorker: {ex.Message}");
                }
                break;
            }

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine($"Process took {elapsedTime}");
        }

        private async Task TransferDataAsync(CancellationToken stoppingToken)
        {

            int totalUsersCount = await _sourceDbContextFactory.CreateDbContext().User.AsNoTracking().CountAsync(); // 1,000,000         
            int pageSize = 1000;
            int totalPages = (int)Math.Ceiling(totalUsersCount / (double)pageSize);

            int maxDegreeOfParallelism = Environment.ProcessorCount / 2;

            var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            var tasks = new List<Task>();

            for (int pageNumber = 1; pageNumber <= totalPages; pageNumber++)
            {
                var currentPage = pageNumber;

                await semaphore.WaitAsync(stoppingToken);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        using var sourceDbContext = _sourceDbContextFactory.CreateDbContext();
                        using var destinationDbContext = _destinationDbContextFactory.CreateDbContext();

                        var users = await GetUsersAsync(sourceDbContext, currentPage, pageSize, stoppingToken);
                        await BulkInsertUsersAsync(destinationDbContext, users, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _ = ex.Message;
                    }
                    finally
                    {
                        semaphore.Release();
                    }

                }, stoppingToken);

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async Task<List<User>> GetUsersAsync(SourceDbContext context, int pageNumber, int pageSize, CancellationToken stoppingToken)
        {
            return await context.User.OrderBy(u => u.Id)
                                     .Skip((pageNumber - 1) * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync(stoppingToken);
        }

        private async Task BulkInsertUsersAsync(DestinationDbContext context, List<User> users, CancellationToken stoppingToken)
        {
            try
            {
                await context.BulkInsertAsync(users, cancellationToken:stoppingToken);           
                Console.WriteLine($"Inserted {users.Count} users into DestinationDb.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to insert users: {ex.Message}");
            }
        }
    }
}