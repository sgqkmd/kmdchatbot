using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using SemanticKernelLibrary;

var config = new ConfigurationBuilder()
    .AddJsonFile("applicationsettings.json")
    .Build();

var serviceCollection = new ServiceCollection();

await serviceCollection.AddGeorge(config);

var service = serviceCollection.BuildServiceProvider();

var george = service.GetService<IUVAssistant>();

if (george != null)
{
    while (true)
    {
        Console.Write("User > ");
        var input = Console.ReadLine() ?? "";

        var answer = george.Ask(input);

        Console.WriteLine(answer);
    }
}