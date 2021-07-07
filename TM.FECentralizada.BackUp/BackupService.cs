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
using TM.FECentralizada.Entities.Backup;
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

        public void Test()
        {
            Procedure();
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

            Tools.Logging.Info("Inicio: Obtener Parámetros");
            List<Parameters> ParamsResponse = TM.FECentralizada.Business.Common.GetParametersByKey(new Parameters() { Domain = Tools.Constants.BackUp, KeyDomain = "", KeyParam = "" });
            

            if (ParamsResponse != null && ParamsResponse.Any())
            {
                List<Parameters> Parameters = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.BackUp_Config.ToUpper())).ToList();

                Parameters pmtBackupConfig = Parameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.Backup_config);
                BackupConfig backupService = Business.Common.GetParameterDeserialized<BackupConfig>(pmtBackupConfig);

                Tools.Logging.Info("Fin : Obtener Parámetros");

                Tools.Logging.Info("Inicio : Generar backup - Backup");
                Business.BackUp.MakeBackup(backupService);
                Tools.Logging.Info("Fin : Generar backup - Backup");

            }
            else
            {
                Tools.Logging.Error("No se configuraron los parámetros para el proceso de Backup");
            }
            Tools.Logging.Info("Fin del Proceso: Backup");
        }

        protected override void OnStop()
        {
        }
    }
}
