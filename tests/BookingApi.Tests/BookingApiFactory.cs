using BookingApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookingApi.Tests;

public sealed class BookingApiFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static string TestConnectionString =>
        Environment.GetEnvironmentVariable("BOOKING_API_TEST_CONNECTION")
        ?? throw new InvalidOperationException(
            "Set BOOKING_API_TEST_CONNECTION before running tests.");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:BookingDatabase"] =
                        TestConnectionString
                });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<BookingDbContext>>();
            services.RemoveAll<BookingDbContext>();

            services.AddDbContext<BookingDbContext>(options =>
                options.UseNpgsql(TestConnectionString));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();

        var database = scope.ServiceProvider
            .GetRequiredService<BookingDbContext>();

        await database.Database.MigrateAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}