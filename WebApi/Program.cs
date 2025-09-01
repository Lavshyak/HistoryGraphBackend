using System.Text.Json;
using DbThings;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;

Console.WriteLine("Program.cs start");

var builder = WebApplication.CreateBuilder(args);

T GetRequiredConfigurationValue<T>(string key)
{
    return builder.Configuration.GetValue<T?>(key) ?? throw new InvalidOperationException();
}

#region AddServices

{
    var config = builder.Configuration;

    Neo4jConnectionConfig historyConfig;
    {
        var section = config.GetRequiredSection("NEO4J:HISTORY");
        historyConfig = new Neo4jConnectionConfig(
            section["URI"] ?? throw new InvalidOperationException("NEO4J:HISTORY:URI"),
            section["USER"] ?? throw new InvalidOperationException("NEO4J:HISTORY:USER"),
            section["PASSWORD"] ?? throw new InvalidOperationException("NEO4J:HISTORY:PASSWORD")
        );
    }
    Neo4jConnectionConfig migrationsConfig;
    {
        var section = config.GetRequiredSection("NEO4J:MIGRATIONS");
        migrationsConfig = new Neo4jConnectionConfig(
            section["URI"] ?? throw new InvalidOperationException("NEO4J:MIGRATIONS:URI"),
            section["USER"] ?? throw new InvalidOperationException("NEO4J:MIGRATIONS:USER"),
            section["PASSWORD"] ?? throw new InvalidOperationException("NEO4J:MIGRATIONS:PASSWORD")
        );
    }

    builder.Services.AddDbThings(historyConfig, migrationsConfig);
}

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var originsJson = GetRequiredConfigurationValue<string>("Cors:DefaultPolicy:OriginsJsonArray");
        var origins = JsonSerializer.Deserialize<string[]>(originsJson) ?? throw new InvalidOperationException();
        policy.WithOrigins(origins);
    });
});

#endregion AddServices

var app = builder.Build();

#region AppConfiguration

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

//app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

{
    using var scope = app.Services.CreateScope();
    var migrations = scope.ServiceProvider.GetRequiredService<MigrationsService>();
    await migrations.Migrate();
}

#endregion AppConfiguration

app.Run();