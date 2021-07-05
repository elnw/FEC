using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Common = TM.FECentralizada.Entities.Common;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace TM.FECentralizada.Pacifyc.Read
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();

            Tools.Logging.Configure();
            try
            {
                Tools.Logging.Info("Inicio del Proceso: Lectura (BD)");

               

                //    {
                
                //    Tools.Logging.Info("Inicio : UpFront.Data.UploadFileOutput.");
                //    Tools.Logging.InfoJson(oFileServerOutput);
                //    bool blnResult = Data.Transform.UploadFileOutput(oFileServerOutput, blnListResult);
                //    Tools.Logging.InfoJson(blnResult);
                //    Tools.Logging.Info("Fin : UpFront.Data.UploadFileOutput.");
                //}
                //else
                //{
                //    Tools.Logging.Error("Ocurrió un error al leer el archivo Excel.");
                //}
                //Tools.Logging.Info("Fin del Proceso: Tranformacion de Archivo.");
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }
        public void prueba()
        {
            TM.FECentralizada.Business.Pacifyc objBPacifyc = new TM.FECentralizada.Business.Pacifyc();
            objBPacifyc.ReadHeader();
        }
        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
