using System.Net;
using System.Net.Http.Headers;
using System.Text;

const string CompaniesHouseClient = "CompaniesHouse";

var random = new Random();

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

app.MapGet("/company/{companiesHouseNumber}",
    async (string companiesHouseNumber, IHttpClientFactory httpClientFactory) =>
{
    // Inject some chaos
    //var chaos = random.Next(100);
    //if (chaos % 5 == 0)
    //{
    //    var delay = chaos * 250;
    //    logger.LogInformation("Delaying by {Delay}ms for company {Company}", delay, companiesHouseNumber);
    //    await Task.Delay(delay);
    //}
    //if (chaos % 7 == 0 || chaos % 3 == 0)
    //{
    //    logger.LogInformation("Returning 429 for company {Company}", companiesHouseNumber);
    //    return Results.Text(null, contentType: "application/json", statusCode: (int)HttpStatusCode.TooManyRequests);
    //}

    var client = httpClientFactory.CreateClient(CompaniesHouseClient);
    var response = await client.GetAsync($"company/{companiesHouseNumber}");

    logger.LogInformation("Companies House response {StatusCode}", response.StatusCode);

    var json = response.Content != null ? await response.Content.ReadAsStringAsync() : "{}";
    return Results.Text(json, contentType: "application/json", statusCode: (int)response.StatusCode);
})
.WithName("GetCompaniesHouseCompany")
.WithOpenApi();

await app.RunAsync();
