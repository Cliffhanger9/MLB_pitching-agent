using mlbapi.Data;
using mlbapi.Models;
using Dapper;
using System;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Responses;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OpenAI.Chat;
using mlbapi.Tools;
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





var apiKey = builder.Configuration["OpenAI:ApiKey"];
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine("Missing OpenAI:ApiKey");
    return;
}

/*
var client = new OpenAIClient(apiKey);

ResponsesClient client = new(
    model: "gpt-4o-mini",
    apiKey: apiKey
);
var resp = await client.Responses.CreateAsync(new ResponseCreateRequest
{
    Model = "gpt-5.2",
    Input = "How tall is the CN Tower?"
});

Console.WriteLine(resp.OutputText);
*/

app.MapGet("/ask", async (string q, IConfiguration config) =>
{
    var apiKey = config["OpenAI:ApiKey"];
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    var body = new
    {
        model = "gpt-4o-mini",
        input = q
    };

    var json = JsonSerializer.Serialize(body);
    var resp = await http.PostAsync(
        "https://api.openai.com/v1/responses",
        new StringContent(json, Encoding.UTF8, "application/json")
    );

    var text = await resp.Content.ReadAsStringAsync();
    return Results.Ok(text);
});
//openAIResponseClient client = new(model:"gpt-4o-mini", apiKey: apiKey);
//ResponsesClient client = new(model:"gpt-4o-mini", apiKey: apiKey);
app.MapGet("/ask1", async(string q1, IConfiguration config) =>
{
    var apiKey = config["OpenAI:ApiKey"];
    var client = new OpenAIClient(apiKey);
    ChatCompletionOptions options = new()
    {
        Tools = {EraLeadersTool.Create()}
    };
    ChatClient chat = client.GetChatClient("gpt-4o");
    var messages = new List<ChatMessage>
    {
        new SystemChatMessage("If the user asks for the top pitchers by ERA, call get_era_leaders. If the season is missing, ask a short follow up question. After tool results, answer with a ranked list."),
        new UserChatMessage(q1)
    };
    ChatCompletion completion = await chat.CompleteChatAsync(messages, options);
    if (completion.FinishReason == ChatFinishReason.ToolCalls)
    {
        Console.WriteLine("yessirrrr");
    }
});

app.Run();
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
