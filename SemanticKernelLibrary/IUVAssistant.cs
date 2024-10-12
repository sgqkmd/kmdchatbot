using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticKernelLibrary
{
    public interface IUVAssistant
    {
        IAsyncEnumerable<string> Ask(string message);
    }
}
