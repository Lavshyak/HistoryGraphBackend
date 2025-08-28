using DbThings;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Neo4j.Driver;

Console.WriteLine("started");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
{
    var config = builder.Configuration;
    var driver = GraphDatabase.Driver(config["NEO4J:URI"] ?? throw new InvalidOperationException("NEO4J__URI"),
        AuthTokens.Basic(config["NEO4J:USER"] ?? throw new InvalidOperationException("NEO4J__USER"),
            config["NEO4J:PASSWORD"] ?? throw new InvalidOperationException("NEO4J__PASSWORD")));
    builder.Services.AddSingleton<IDriver>(driver);
}

builder.Services.AddScoped<IAsyncSession>(sp => sp.GetRequiredService<IDriver>().AsyncSession());
builder.Services.AddScoped<DataBase>();
builder.Services.AddScoped<EventsRepository>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger(c =>
    {
        c.PreSerializeFilters.Add((swagger, httpReq) =>
        {
            var scheme = httpReq.Headers["X-Forwarded-Proto"];
            if (string.IsNullOrWhiteSpace(scheme))
                scheme = httpReq.Scheme;

            swagger.Servers = new List<OpenApiServer>
            {
                new OpenApiServer
                    { Url = $"{scheme}://{httpReq.Host.Value}/{httpReq.Headers["X-Forwarded-Prefix"]}" },
                new OpenApiServer
                    { Url = $"http://{httpReq.Host.Value}/{httpReq.Headers["X-Forwarded-Prefix"]}" },
                new OpenApiServer
                    { Url = $"https://{httpReq.Host.Value}/{httpReq.Headers["X-Forwarded-Prefix"]}" }
            };
        });
    });
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("v1/swagger.json", "My API V1"); });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

{
    using var scope = app.Services.CreateScope();
    var dataBase = scope.ServiceProvider.GetRequiredService<DataBase>();
    await dataBase.Migrate();
}

app.Run();