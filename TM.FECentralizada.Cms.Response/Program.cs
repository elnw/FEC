using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Cms.Response
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main()
        {
#if DEBUG
            CmsResponse cmsResponse = new CmsResponse();
            cmsResponse.TestProject();



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
