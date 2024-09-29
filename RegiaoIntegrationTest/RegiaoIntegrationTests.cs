using Microsoft.Extensions.DependencyInjection;
using RegiaoApi.Context;
using RegiaoApi.Models;
using System.Text;
using System.Text.Json;

namespace RegiaoIntegrationTest
{
    public class RegiaoIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly RegiaoDb _dbContext;

        public void Seed()
        {
            if (!_dbContext.Regioes.Any())
            {
                _dbContext.Regioes.AddRange(
                    new Regiao { Id = 1, Name = "Região 1" },
                    new Regiao { Id = 2, Name = "Região 2" },
                    new Regiao { Id = 3, Name = "Região 3" }
                );
                _dbContext.SaveChanges();
            }
        }

        public RegiaoIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();

            var scopeFactory = factory.Services.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                _dbContext = scope.ServiceProvider.GetRequiredService<RegiaoDb>();
                Seed(); 
            }
        }

        [Fact]
        public async Task Get_Regioes_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/regiao");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
            Assert.NotNull(content);
        }

        [Fact]
        public async Task Get_RegioesById_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/regiao/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
        }

        [Fact]
        public async Task CreateRegiao_IntegrationTest_ReturnsSuccessStatusCode()
        {
            // Arrange
            var regiao = new Regiao { Name = "Nova Região" };
            var json = JsonSerializer.Serialize(regiao);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/regiao", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Update_RegiaoById_ReturnsSuccessStatusCode()
        {
            // Arrange
            var regiao = new Regiao { Name = "TesteUpdate" };
            var json = JsonSerializer.Serialize(regiao);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            //Act
            var response = await _client.PutAsync("/regiao/1", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Delete_RegiaoById_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.DeleteAsync("/regiao/2");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotNull(content);
        }
    }
}
