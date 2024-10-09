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


#region longToken
var token = "eyJhbGciOiJSUzI1NiIsImtpZCI6InpGTDB5ZGF2NDgxSHJDT2M0bk9MdTR2cDB2SDZxeTBvUS1SdlBzQW1vNlkiLCJ0eXAiOiJKV1QifQ.eyJhdWQiOiJmMzdlYmYzNy04NDAwLTQyZmYtYjc0MS04NGQ2YjYwOWZkNWMiLCJpc3MiOiJodHRwczovL2xvZ2ljaWRlbnRpdHlwcm9kLmIyY2xvZ2luLmNvbS9hYTBkNmYzYi04ZmIyLTQ3N2QtYWI4OC1iYjJlNjY0NjIwZTgvdjIuMC8iLCJleHAiOjE3Mjg0NzY3MTgsIm5iZiI6MTcyODQ3MzExOCwic3ViIjoiYjU1YjIzNzMtYjY4ZS00MDFhLWI0ZWQtY2YzZjFiMjdmYzkyIiwiaWRwIjoiTG9naWNJZGVudGl0eSIsImF1dGhlbnRpY2F0aW9uU291cmNlIjoiQ2xpZW50IENyZWRlbnRpYWxzIiwibm9uY2UiOiJkZWZhdWx0Tm9uY2UiLCJzY3AiOiJ1c2VyX2ltcGVyc29uYXRpb24iLCJhenAiOiJiNTViMjM3My1iNjhlLTQwMWEtYjRlZC1jZjNmMWIyN2ZjOTIiLCJ2ZXIiOiIxLjAiLCJpYXQiOjE3Mjg0NzMxMTh9.KU-hSJ7PoERHBTcI7IKUv4UH2s7-bV-i6f2TRswW0m8sAxLW_8vaLTf8GDE9dZSfyIke57v_Q7RPkVHqkBX7VKaJQazeyMHA3gPYI1N4SIdYbGhNxmriLaUUdw5585CAHYdvEQ9-Cm3Hf0czTp8WIxhSxIKCY31V85ZjdgW9a4z88ZJqVpcsSUCQ4gDi7c4tn035PMh12_s73nF2d2kPDjlKCjHAV4ZQetmFSp-4OXB-oieDOF9SzQ37Nk8CkS4ieDfUEm-lATbGEzN6V6Grlc9V-PrR7N3aXFBUOU3Ifbqivf0av0MbjsOAStH9j98e3p-zkFZdhPod2bJtnoKNGA";
#endregion
//var serviceCollection = new ServiceCollection();

//serviceCollection.AddSemanticKernelLibrary();

//var serviceProvider = serviceCollection.BuildServiceProvider();
var config = new ConfigurationBuilder()
    .AddJsonFile("applicationsettings.json").Build();


var httpclient = new HttpClient();
httpclient.DefaultRequestHeaders.Authorization =
new
AuthenticationHeaderValue(
"Bearer"
, token);
httpclient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", "6c15fd04d4504f4e89f10403de2f4299");

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
var plugin = await kernel.ImportPluginFromOpenApiAsync(
    "studica",
    new Uri(config["studica:url"]),
    executionParameters: new OpenApiFunctionExecutionParameters()
    {
        HttpClient = httpclient
    }
    );
#pragma warning restore SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.




var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
ChatHistory history = [];
history.AddSystemMessage($"When you use functions on the studica plugin, you will use this bearer token: {token}");
while (true)
{
    Console.Write("User > ");
    var input = Console.ReadLine() ?? "";
    history.AddUserMessage(input);

    var answer = await chatCompletionService.GetChatMessageContentAsync(
        history,
        openAIPromptExecutionSettings,
        kernel);

    history.Add(answer);

    Console.WriteLine($"GEORGE > {answer}");
}