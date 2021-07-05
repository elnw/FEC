using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Sap.Read
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main()
        {
            #if DEBUG
                        SapRead sapRead = new SapRead();
                        sapRead.Test();
            #else
            ServiceBase[] ServicesToRun;
                        ServicesToRun = new ServiceBase[]
                        {
                            new SapRead()
                        };
                        ServiceBase.Run(ServicesToRun);
            #endif



        }
    }
}
