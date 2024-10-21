using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernelLibrary
{
    public class Peter : IUVAssistant
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly Kernel _kernel;
        private readonly OpenAIPromptExecutionSettings _openAIPromptExecutionSettings;

        public ChatHistory History { get; set; } =
            [
                new(AuthorRole.System, "You always end your sentences with: And that's all she wrote...!"),
                new(AuthorRole.User, "Hello there"),
                new(AuthorRole.Assistant, "Hi how can i help you? And that's all she wrote...!")
            ];

        public Peter([FromKeyedServices("PeterKernel")] Kernel kernel)
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
            yield return $"""
                            Peter > {answer}
                            """;
        }
    }
}