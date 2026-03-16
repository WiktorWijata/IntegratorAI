using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IntegratorAI.Context.Contracts.Models;

namespace IntegratorAI.Context.Contracts
{
    public interface IContextModule
    {
        Task<Guid> CreateContext(string name, string systemRole, string domainContext, string decisionPolicy, string operatingRules, string outputFormat, IEnumerable<ToolDto> tools = null, IEnumerable<ExampleDto> examples = null, CancellationToken cancellationToken = default);
        Task<string> GetContextPrompt(Guid contextId, CancellationToken cancellationToken = default);
    }
}
