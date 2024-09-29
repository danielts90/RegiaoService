using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RegiaoApi.Context;
using RegiaoApi.Models;
using Prometheus;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RegiaoDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "RegiaoApi";
    config.Title = "RegiaoApi v1";
    config.Version = "v1";
});

builder.Services.AddSingleton<IMessageProducer>(provider => new Producer("regiao.updated"));

var app = builder.Build();

var counter = Metrics.CreateCounter("webapimetric", "Contador de requests",
    new CounterConfiguration
    {
        LabelNames = new[] { "method", "endpoint", "status" }
    });

app.Use((context, next) =>
{
    counter.WithLabels(context.Request.Method, context.Request.Path, context.Response.StatusCode.ToString()).Inc();
    return next();
});

var regiao = app.MapGroup("/regiao");
regiao.MapGet("/", GetAll);
regiao.MapGet("/{id}", GetById);
regiao.MapPost("/", CreateRegiao);
regiao.MapPut("/{id}", UpdateRegiao);
regiao.MapDelete("/{id}", DeleteRegiao);


if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "RegiaoAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RegiaoDb>();
}


static async Task<IResult> GetAll(RegiaoDb db)
{
    var regioes = await db.Regioes.ToArrayAsync();
    return TypedResults.Ok(regioes);
}

static async Task<IResult> GetById(int id, RegiaoDb db)
{
    return await db.Regioes.FindAsync(id)
        is Regiao regiao
            ? TypedResults.Ok(regiao)
            : TypedResults.NotFound();
}

static async Task<IResult> CreateRegiao(Regiao regiao, RegiaoDb db, IMessageProducer producer)
{
    db.Regioes.Add(regiao);
    await db.SaveChangesAsync();

    var message = new Message<Regiao>(EventTypes.CREATE, regiao);
    producer.SendMessageToQueue(message);
    return TypedResults.Created($"/regiao/{regiao.Id}", regiao);
}

static async Task<IResult> UpdateRegiao(int id, Regiao inputRegiao, RegiaoDb db, IMessageProducer producer)
{
    var regiao = await db.Regioes.FindAsync(id);

    if (regiao is null) return TypedResults.NotFound();

    regiao.Name = inputRegiao.Name;
    await db.SaveChangesAsync();

    var message = new Message<Regiao>(EventTypes.UPDATE, regiao);
    producer.SendMessageToQueue(message);

    return TypedResults.Ok(regiao);
}

static async Task<IResult> DeleteRegiao(int id, RegiaoDb db, IMessageProducer producer)
{
    if (await db.Regioes.FindAsync(id) is Regiao regiao)
    {
        db.Regioes.Remove(regiao);
        await db.SaveChangesAsync();

        var message = new Message<Regiao>(EventTypes.DELETE, regiao);
        producer.SendMessageToQueue(message);


        return TypedResults.Ok();
    }
    return TypedResults.NotFound();
}
app.MapMetrics();
app.Run();

public partial class Program { }