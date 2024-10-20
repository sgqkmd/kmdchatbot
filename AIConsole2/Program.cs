using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SemanticKernelLibrary;

var config = new ConfigurationBuilder()
    .AddJsonFile("applicationsettings.json")
    .Build();

var serviceCollection = new ServiceCollection();

await serviceCollection.AddPeter(config);

var service = serviceCollection.BuildServiceProvider();

var peter = service.GetService<IUVAssistant>();

if (peter != null)
{
    while (true)
    {
        Console.Write("User > ");
        var input = Console.ReadLine() ?? "";

        var answer = peter.Ask(input);

        Console.WriteLine(answer);
    }
}