using System.Net.Http.Headers;
using System.Text;

const string CompaniesHouseClient = "CompaniesHouse";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
}).CreateLogger("Program");

var companiesHouseBaseUri = builder.Configuration["CompaniesHouseApi:BaseUri"];
var companiesHouseApiKey = builder.Configuration["CompaniesHouseApi:ApiKey"];

if (string.IsNullOrEmpty(companiesHouseBaseUri) ||
   string.IsNullOrEmpty(companiesHouseApiKey))
{
    throw new InvalidOperationException("Missing configuration");
}

builder.Services.AddHttpClient(CompaniesHouseClient, client =>
{
    client.BaseAddress = new Uri(companiesHouseBaseUri);

    var apiKey = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{companiesHouseApiKey}:"));
    client.DefaultRequestHeaders.Add("Authorization", $"BASIC {apiKey}");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/CompaniesHouse/companies/{companiesHouseNumber}",
    async (string companiesHouseNumber, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient(CompaniesHouseClient);
    var response = await client.GetAsync($"company/{companiesHouseNumber}");

    logger.LogInformation("Companies House response {StatusCode}", response.StatusCode);

    var json = response.Content != null ? await response.Content.ReadAsStringAsync() : "{}";
    return Results.Text(json, contentType: "application/json", statusCode: (int)response.StatusCode);
})
.WithName("GetCompaniesHouseCompany")
.WithOpenApi();

await app.RunAsync();
