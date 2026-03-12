using System;
using System.Collections.Generic;
using IntegratorAI.Context.Contracts.Models;
using MediatR;

namespace IntegratorAI.Context.Contracts.Commands
{
    public class CreateContextCommand : IRequest<Guid>
    {
        public CreateContextCommand(string name, string systemPrompt, IEnumerable<ContextToolDto> tools = null)
        {
            Name = name;
            SystemPrompt = systemPrompt;
            Tools = tools;
        }

        public string Name { get; set; }
        public string SystemPrompt { get; set; }
        public IEnumerable<ContextToolDto> Tools { get; set; }
    }
}
