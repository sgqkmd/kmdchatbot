using AIConsole;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelLibrary;

//var serviceCollection = new ServiceCollection();

//serviceCollection.AddSemanticKernelLibrary();

//var serviceProvider = serviceCollection.BuildServiceProvider();
var config = new ConfigurationBuilder()
    .AddJsonFile("applicationsettings.json").Build();

var kernelBuilder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(
                config["openai:name"],
                config["openai:endpoint"],
                config["openai:apikey"]
                );

kernelBuilder.Plugins.AddFromType<TimeInformationPlugin>();

var kernel = kernelBuilder.Build();

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};


var input = Console.ReadLine() ?? "";
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

ChatHistory history = [];

history.AddUserMessage(input);

var answer = await chatCompletionService.GetChatMessageContentAsync(
    history, 
    openAIPromptExecutionSettings, 
    kernel);

Console.WriteLine(answer);



