using AIConsole;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using SemanticKernelLibrary;

//var serviceCollection = new ServiceCollection();

//serviceCollection.AddSemanticKernelLibrary();

//var serviceProvider = serviceCollection.BuildServiceProvider();
var config = new ConfigurationBuilder()
    .AddJsonFile("applicationsettings.json").Build();

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
await kernel.ImportPluginFromOpenApiAsync(
    "studica",
    new Uri(config["studica:url"])
    );
#pragma warning restore SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.




var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory history = [];

while (true)
{
    var input = Console.ReadLine() ?? "";
    history.AddUserMessage(input);

    var answer = await chatCompletionService.GetChatMessageContentAsync(
        history,
        openAIPromptExecutionSettings,
        kernel);

    history.Add(answer);

    Console.WriteLine(answer);
}