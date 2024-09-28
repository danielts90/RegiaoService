using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RegiaoApi.Context;

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
                    options.UseInMemoryDatabase("InMemoryTestDb");
                });

                //services.AddSingleton<IMessageProducer>(provider => new TestMessageProducer("test.regiao.updated"));
                services.AddSingleton<IMessageProducer>(provider => new Producer("test.regiao.updated"));


            });
        }
    }
}
