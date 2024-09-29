using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RegiaoApi.Context;
using RegiaoApi.Models;

namespace RegiaoIntegrationTest
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<RegiaoDb>();

                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                if (!dbContext.Regioes.Any())
                {
                    dbContext.Regioes.AddRange(
                        new Regiao { Name = "Regiao 1" },
                        new Regiao { Name = "Regiao 2" },
                        new Regiao { Name = "Regiao 3" }
                    );
                    dbContext.SaveChanges();
                }
            }


            return host;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RegiaoDb>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<RegiaoDb>(options =>
                {
                    var connectionString = "Host=localhost;Database=RegiaoTestDb;Username=techuser;Password=techpassword";
                    options.UseNpgsql(connectionString);
                });
                services.AddSingleton<IMessageProducer>(provider => new Producer("test.regiao.updated"));
            });
        }
    }
}
