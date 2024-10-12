using AIConsole;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using SemanticKernelLibrary;
using System.Net.Http.Headers;


var config = new ConfigurationBuilder()
    .AddJsonFile("applicationsettings.json")
    .Build();

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
kernelBuilder.Plugins.AddFromType<StudicaUiPlugin>();

var kernel = kernelBuilder.Build();

#pragma warning disable SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
await kernel.ImportPluginFromOpenApiAsync(
    "students",
    new Uri(config["studica-students:url"]),
    executionParameters: new OpenApiFunctionExecutionParameters()
    {
        HttpClient = httpclient
    }
    );
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
history.AddSystemMessage("Always assume school code should be A02332");
history.AddSystemMessage("Always include a specific studentid when making requests for absence");
history.AddSystemMessage("Set paramter OnlyAbsenceReports to true when making requests for absence");
history.AddSystemMessage("Set StartDateFrom to 2024-01-01 and StartDateTo to 2024-12-31 when making requests for educational programmes");
history.AddSystemMessage("Set pagesize to 1000 when this property is present in a request");
history.AddSystemMessage("Ignore absence records with the status Not Registered when processing absence registration records");

while (true)
{
    Console.Write("User > ");
    var input = Console.ReadLine() ?? "";
    history.AddUserMessage(input);

    ChatMessageContent? answer;
    try
    {
        answer = await chatCompletionService.GetChatMessageContentAsync(
                history,
                openAIPromptExecutionSettings,
                kernel);
    }
    catch (HttpOperationException e) when (e.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
    {
        Console.WriteLine("Too many request... retrying in 60 seconds");
        await Task.Delay(60000);
        answer = await chatCompletionService.GetChatMessageContentAsync(
            history,
            openAIPromptExecutionSettings,
            kernel);
    }

    history.Add(answer);

    Console.WriteLine($"GEORGE > {answer}");
    Console.WriteLine();
}