using Azure.AI.OpenAI;
using Azure.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using SemanticKernelLibrary.Plugins;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SemanticKernelLibrary
{
    public static class DependencyInject
    {
        public static async Task<IServiceCollection> AddGeorge(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<TimeInformationPlugin>();
            services.AddSingleton<StudicaUiPlugin>();

            var httpclient = CreateHttpClient(config);

#pragma warning disable SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            Kernel kernel = new();
            KernelPlugin studicaStudentPlugin = await kernel.ImportPluginFromOpenApiAsync(
                "students",
                new Uri(config["studica-students:url"]!),
                executionParameters: new OpenApiFunctionExecutionParameters { HttpClient = httpclient }
                );
            KernelPlugin studicaAbsencePlugin = await kernel.ImportPluginFromOpenApiAsync(
                "absence",
                new Uri(config["studica-programmes:url"]),
                executionParameters: new OpenApiFunctionExecutionParameters { HttpClient = httpclient }
                );
#pragma warning restore SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            services.AddKeyedSingleton("StudicaStudentsPlugin", studicaStudentPlugin);
            services.AddKeyedSingleton("StudicaAbsencePlugin", studicaAbsencePlugin);


            services.AddKeyedTransient<Kernel>("GeorgeKernel", (sp, key) =>
            {
                KernelPluginCollection pluginCollection = [];
                pluginCollection.AddFromObject(sp.GetRequiredService<TimeInformationPlugin>());
                pluginCollection.AddFromObject(sp.GetRequiredService<StudicaUiPlugin>());
                pluginCollection.AddFromObject(sp.GetRequiredKeyedService<KernelPlugin>("StudicaStudentsPlugin"), "students");
                pluginCollection.AddFromObject(sp.GetRequiredKeyedService<KernelPlugin>("StudicaAbsencePlugin"), "absence");

                return new Kernel(sp, pluginCollection);
            });

            services.AddTransient<IUVAssistant, George>();

            return services;
        }

        private static HttpClient CreateHttpClient(IConfiguration config)
        {
            var token = config["studica:bearer-token"];
            var httpclient = new HttpClient();
            httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpclient.DefaultRequestHeaders.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", config["studica:subscription-key"]);

            return httpclient;
        }
    }
}
