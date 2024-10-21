namespace SemanticKernelLibrary
{
    public interface IUVAssistant
    {
        IAsyncEnumerable<string> Ask(string message);
    }
}
