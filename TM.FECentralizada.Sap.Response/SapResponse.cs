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

namespace TM.FECentralizada.Sap.Response
{
    public partial class SapResponse : ServiceBase
    {
        Timer oTimer = new Timer();
        public SapResponse()
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
            try
            {
                Tools.Logging.Info("Inicio del Proceso: Respuesta Sap.");

                Tools.Logging.Info("Inicio : Obtener Parámetros");
                //Método que Obtendrá los Parámetros.
                List<Parameters> ParamsResponse = TM.FECentralizada.Business.Common.GetParametersByKey(new Parameters() { Domain = Tools.Constants.IsisRead });
                Tools.Logging.Info("Fin : Obtener Parámetros");

                if (ParamsResponse != null && ParamsResponse.Any())
                {
                    List<Parameters> ParametersInvoce = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.IsisRead_Bill.ToUpper())).ToList();
                    List<Parameters> ParametersBill = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.IsisRead_Bill.ToUpper())).ToList();
                    List<Parameters> ParametersCreditNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.IsisRead_Bill.ToUpper())).ToList();
                    List<Parameters> ParametersDebitNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.IsisRead_Bill.ToUpper())).ToList();

                    Tools.Logging.Info("Inicio : Procesar documentos de BD Isis");
                    Parallel.Invoke(
                               () => Invoice(ParametersInvoce),
                               () => Bill(ParametersBill),
                               () => CreditNote(ParametersCreditNote),
                               () => DebitNote(ParametersDebitNote)
                        );
                    Tools.Logging.Info("Fin : Procesar documentos de BD Isis");

                    //Obtengo la Configuración Intervalo de Tiempo
                    var oConfiguration = ParamsResponse.Find(x => x.KeyParam.Equals("TimerInterval"));
                    var Minutes = oConfiguration.Value;//oConfiguration.Key3.Equals("D") ? oConfiguration.Value3 : oConfiguration.Key3.Equals("T") ? oConfiguration.Value2 : oConfiguration.Value1;
                    oTimer.Interval = Tools.Common.ConvertMinutesToMilliseconds(int.Parse(Minutes));
                    oTimer.Start();
                    oTimer.AutoReset = true;
                }
                else
                {
                    Tools.Logging.Error("Ocurrió un error al obtener la configuración para Isis.");
                }
                Tools.Logging.Info("Fin del Proceso: Lectura Isis.");
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        private void Invoice(List<Parameters> oListParameters)
        {
            //
            Tools.Logging.Info("Inicio : Obtener documentos de BD Isis - Facturas");
            //var ListInvoceHeader = Business.Isis.GetInvoceHeader();
            //var ListInvoceDetail = Business.Isis.GetInvoceDetail();


            Tools.Logging.Info("Inicio : Registrar Auditoria");


            Tools.Logging.Info("Inicio : Validar Documentos ");

            Tools.Logging.Info("Inicio : Notificación de Validación");

            Tools.Logging.Info("Inicio : Actualizo Auditoria");

            Tools.Logging.Info("Inicio : Insertar Documentos Validados ");

            Tools.Logging.Info("Inicio : Valido Documentos insertados ");

            Tools.Logging.Info("Inicio : Lees  Documentos insertados ");

            Tools.Logging.Info("Inicio : enviar GFiscal ");

            Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");

            Tools.Logging.Info("Inicio : Actualizo Auditoria");

        }
        private void Bill(List<Parameters> oListParameters)
        {
            Tools.Logging.Info("Inicio : Obtener documentos de BD Isis - Boletas");


            Tools.Logging.Info("Inicio : Registrar Auditoria");


            Tools.Logging.Info("Inicio : Validar Documentos ");

            Tools.Logging.Info("Inicio : Notificación de Validación");

            Tools.Logging.Info("Inicio : Actualizo Auditoria");

            Tools.Logging.Info("Inicio : Insertar Documentos Validados ");

            Tools.Logging.Info("Inicio : Valido Documentos insertados ");

            Tools.Logging.Info("Inicio : Lees  Documentos insertados ");

            Tools.Logging.Info("Inicio : enviar GFiscal ");

            Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");

            Tools.Logging.Info("Inicio : Actualizo Auditoria");

        }
        private void CreditNote(List<Parameters> oListParameters)
        {

            Tools.Logging.Info("Inicio : Obtener documentos de BD Isis - Boletas");


            Tools.Logging.Info("Inicio : Registrar Auditoria");


            Tools.Logging.Info("Inicio : Validar Documentos ");

            Tools.Logging.Info("Inicio : Notificación de Validación");

            Tools.Logging.Info("Inicio : Actualizo Auditoria");

            Tools.Logging.Info("Inicio : Insertar Documentos Validados ");

            Tools.Logging.Info("Inicio : Valido Documentos insertados ");

            Tools.Logging.Info("Inicio : Lees  Documentos insertados ");

            Tools.Logging.Info("Inicio : enviar GFiscal ");

            Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");

            Tools.Logging.Info("Inicio : Actualizo Auditoria");
        }
        private void DebitNote(List<Parameters> oListParameters)
        {

            Tools.Logging.Info("Inicio : Obtener documentos de BD Isis - Boletas");


            Tools.Logging.Info("Inicio : Registrar Auditoria");


            Tools.Logging.Info("Inicio : Validar Documentos ");

            Tools.Logging.Info("Inicio : Notificación de Validación");

            Tools.Logging.Info("Inicio : Actualizo Auditoria");

            Tools.Logging.Info("Inicio : Insertar Documentos Validados ");

            Tools.Logging.Info("Inicio : Valido Documentos insertados ");

            Tools.Logging.Info("Inicio : Lees  Documentos insertados ");

            Tools.Logging.Info("Inicio : enviar GFiscal ");

            Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");

            Tools.Logging.Info("Inicio : Actualizo Auditoria");
        }

        protected override void OnStop()
        {
            oTimer.Stop();
        }
    }
}
