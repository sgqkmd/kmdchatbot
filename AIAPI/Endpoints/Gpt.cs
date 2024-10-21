using Microsoft.AspNetCore.Mvc;
using SemanticKernelLibrary;

namespace AIAPI.Endpoints
{
    public static class GptRoute
    {
        public static RouteGroupBuilder RouteGpt(this RouteGroupBuilder group, IUVAssistant assistant)
        {
            var gptEndpoint = new GptEndpoint(assistant);

            group.MapPost("/ask", async ([FromBody] GptQuestion question) => await gptEndpoint.Ask(question.Q));

            return group;
        }
    }

    public record GptQuestion(string Q);

    public class GptEndpoint(IUVAssistant assistant)
    {
        public async Task<string> Ask(string question)
        {
            await foreach (var answer in assistant.Ask(question))
            {
                if (!answer.StartsWith("Too many request"))
                {
                    return answer;
                }
            }

            return "no answer";
        }
    }
}
