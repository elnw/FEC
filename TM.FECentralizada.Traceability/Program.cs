using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Traceability
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main()
        {
#if DEBUG
            TraceabilityService traceabilityService = new TraceabilityService();
            traceabilityService.TestProject();



#else
             Tools.Logging.Configure();
             ServiceBase[] ServicesToRun;
             ServicesToRun = new ServiceBase[]
             {
                 new CmsResponse()
             };
             ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
