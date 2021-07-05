using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Entities.Common
{
    public class ServiceConfig
    {
        public int Norm { get; set; }
        public int ExecutionRate { get; set; }
        public int maxAttemps { get; set; }
    }
}
