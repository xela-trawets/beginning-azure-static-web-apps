using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

var builder = FunctionsApplication.CreateBuilder(args);

// Add user secrets to the configuration
builder.Configuration.AddUserSecrets<Program>();

builder.ConfigureFunctionsWebApplication();
// Register the Cosmos DB client
var configuration = builder.Services.BuildServiceProvider().GetRequiredService<IConfiguration>();
var cosmosDbConnectionString = configuration["CosmosDbConnectionString"];
var cosmosClient = new CosmosClient(cosmosDbConnectionString);

builder.Services.AddSingleton<CosmosClient>(cosmosClient);

var host = builder.Build();

host.Run();
// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
