using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TM.FECentralizada.Entities.Common;

namespace TM.FECentralizada.BackUp
{
    public partial class BackupService : ServiceBase
    {
        Timer oTimer = new Timer();
        public BackupService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                oTimer.Enabled = true;
                oTimer.AutoReset = false;
                oTimer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
                oTimer.Start();
                oTimer.Interval = 10000;
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Procedure();
        }

        private void Procedure()
        {
            Tools.Logging.Info("Inicio del Proceso: Backup");

            List<Parameters> ParamsResponse = TM.FECentralizada.Business.Common.GetParametersByKey(new Parameters() { Domain = Tools.Constants.BackUp, KeyDomain = "", KeyParam = "" });
            Tools.Logging.Info("Fin : Obtener Parámetros");

            if (ParamsResponse != null && ParamsResponse.Any())
            {
                List<Parameters> Parameters = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.BackUp_Config.ToUpper())).ToList();

                Tools.Logging.Info("Inicio : Procesar documentos de BD Pacyfic");



            }

        }

        private void Invoice(List<Parameters> Parameters)
        {

        }

        protected override void OnStop()
        {
        }
    }
}
