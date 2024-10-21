using SemanticKernelLibrary;
using AIAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddPeter(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Gpt Api");

var peter = app.Services.GetService<IUVAssistant>();

app.MapGroup("/gpt")
    .RouteGpt(peter!)
    .WithTags("Gpt api");

app.Run();
