using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterFiends.Configuration
{
    public class Config
    {
        public bool EnableAggressiveBehavior { get; set; } = true;
        public float FiendProbabilityMultiplier { get; set; } = 1.0f;
        public float NarcProbabilityMultiplier { get; set; } = 1.0f;
    }
}
