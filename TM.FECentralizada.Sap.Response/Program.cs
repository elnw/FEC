using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Sap.Response
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main()
        {
            #if DEBUG
                        SapResponse sapResponse = new SapResponse();
                        sapResponse.Test();
            #else
            ServiceBase[] ServicesToRun;
                        ServicesToRun = new ServiceBase[]
                        {
                            new SapResponse()
                        };
                        ServiceBase.Run(ServicesToRun);
            #endif
            
        }
    }
}
