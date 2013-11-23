using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostSpecific
{
    internal class PrioritizedEnviornment
    {
        public int Priority { get; set; }
        public SupportedEnvironment Environment { get; set; }

        public PrioritizedEnviornment(int priority, SupportedEnvironment environment)
        {
            this.Priority = priority;
            this.Environment = environment;
        }
    }
}
