using System.ComponentModel.DataAnnotations;
using IntegratorAI.Api.Contracts.Context.Models;

namespace IntegratorAI.Api.Contracts.Context
{
    public class ContextRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string SystemRole { get; set; }
        public string DomainContext { get; set; }
        public string DecisionPolicy { get; set; }
        public string OperatingRules { get; set; }
        public string OutputFormat { get; set; }
        public Tool[] Tools { get; set; }
        public Example[] Examples { get; set; }
    }
}
