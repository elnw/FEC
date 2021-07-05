using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Atis.Read
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main()
        {
            Tools.Logging.Configure();
            /*ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new AtisRead()
            };*/
            //ServiceBase.Run(ServicesToRun);

            AtisRead ob = new AtisRead();

            ob.probar();
        }
    }
}
