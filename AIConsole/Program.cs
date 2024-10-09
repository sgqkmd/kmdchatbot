using AIConsole;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using SemanticKernelLibrary;
using System.Net.Http.Headers;


var config = new ConfigurationBuilder()
    .AddJsonFile("applicationsettings.json").Build();

var token = config["studica:bearer-token"];
var httpclient = new HttpClient();
httpclient.DefaultRequestHeaders.Authorization =
new
AuthenticationHeaderValue(
"Bearer"
, token);
httpclient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", config["studica:subscription-key"]);

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

var kernelBuilder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(
                config["openai:name"],
                config["openai:endpoint"],
                config["openai:apikey"]
                );

kernelBuilder.Plugins.AddFromType<TimeInformationPlugin>();
var kernel = kernelBuilder.Build();
#pragma warning disable SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//await kernel.ImportPluginFromOpenApiAsync(
//    "studica",
//    new Uri(config["studica:url"]),
//    executionParameters: new OpenApiFunctionExecutionParameters()
//    {
//        HttpClient = httpclient
//    }
//    );
await kernel.ImportPluginFromOpenApiAsync(
    "absence",
    new Uri(config["studica-programmes:url"]),
    executionParameters: new OpenApiFunctionExecutionParameters()
    {
        HttpClient = httpclient
    }
    );
#pragma warning restore SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.




var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory history = [];

while (true)
{
    Console.Write("User > ");
    var input = Console.ReadLine() ?? "";
    input = input == "" ? "Give me the top 5 students based on absence for school code A02332" : input;
    history.AddUserMessage(input);

    var answer = await chatCompletionService.GetChatMessageContentAsync(
        history,
        openAIPromptExecutionSettings,
        kernel);

    history.Add(answer);

    Console.WriteLine($"GEORGE > {answer}");
}