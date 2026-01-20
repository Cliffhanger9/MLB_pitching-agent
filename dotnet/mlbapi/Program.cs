using mlbapi.Data;
using mlbapi.Models;
using Dapper;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DB>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();
app.MapGet("/leaders/era", async (DB db, int season, int take=10, int outsPitched = 90) =>
{
    const string sql = @"
        SELECT TOP (@take)
            p.mlbamId AS mlbamId,
            p.fullName AS fullName,
            ps.era AS era,
            ps.season AS season
        FROM PitchingStats ps
        JOIN Players p ON p.mlbamId = ps.mlbamId
        WHERE ps.season=@season AND ps.outsPitched>=@outsPitched
        ORDER BY ps.era ASC;";
    using var conn = db.CreateConnection();
    var rows = await conn.QueryAsync<EraLeader>(sql, new {season, take, outsPitched});
    return Results.Ok(rows);
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
