using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernelLibrary
{
    public class George : IUVAssistant
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly Kernel _kernel;
        private readonly OpenAIPromptExecutionSettings _openAIPromptExecutionSettings;

        public ChatHistory History { get; set; } =
            [
                new(AuthorRole.System, "Always assume school code should be A02332"),
                new(AuthorRole.System, "Always include a specific studentid when making requests for absence"),
                new(AuthorRole.System, "Set paramter OnlyAbsenceReports to true when making requests for absence"),
                new(AuthorRole.System, "Set StartDateFrom to 2024-01-01 and StartDateTo to 2024-12-31 when making requests for educational programmes"),
                new(AuthorRole.System, "Set pagesize to 1000 when this property is present in a request"),
                new(AuthorRole.System, "Ignore absence records with the status Not Registered when processing absence registration records")
            ];

        public George([FromKeyedServices("HomeAutomationKernel")] Kernel kernel)
        {
            _kernel = kernel;
            _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            _openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };
        }

        public async IAsyncEnumerable<string> Ask(string message)
        {
            History.AddUserMessage(message);

            ChatMessageContent answer = new();
            bool retry = false;
            try
            {
                answer = await _chatCompletionService.GetChatMessageContentAsync(
                        History,
                        _openAIPromptExecutionSettings,
                        _kernel);
            }
            catch (HttpOperationException e) when (e.StatusCode is System.Net.HttpStatusCode.TooManyRequests)
            {
                retry = true;
            }

            if (retry)
            {
                yield return """
                    Too many request... retrying in 60 seconds

                    """;
                await Task.Delay(60000);
                answer = await _chatCompletionService.GetChatMessageContentAsync(
                    History,
                    _openAIPromptExecutionSettings,
                    _kernel);
            }

            History.Add(answer);
            yield return $"George > {answer}";
        }
    }
}