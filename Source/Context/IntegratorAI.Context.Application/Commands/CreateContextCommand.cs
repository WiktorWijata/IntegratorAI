using System;
using System.Collections.Generic;
using IntegratorAI.Context.Contracts.Models;
using MediatR;

namespace IntegratorAI.Context.Application.Commands
{
    public class CreateContextCommand : IRequest<Guid>
    {
        public CreateContextCommand(
            string name,
            string systemRole,
            string domainContext,
            string decisionPolicy,
            string operatingRules,
            string outputFormat,
            IEnumerable<ToolDto> tools = null,
            IEnumerable<ExampleDto> examples = null)
        {
            Name = name;
            SystemRole = systemRole;
            DomainContext = domainContext;
            DecisionPolicy = decisionPolicy;
            OperatingRules = operatingRules;
            OutputFormat = outputFormat;
            Tools = tools;
            Examples = examples;
        }

        public string Name { get; }
        public string SystemRole { get; }
        public string DomainContext { get; }
        public string DecisionPolicy { get; }
        public string OperatingRules { get; }
        public string OutputFormat { get; }
        public IEnumerable<ToolDto> Tools { get; }
        public IEnumerable<ExampleDto> Examples { get; }
    }
}
