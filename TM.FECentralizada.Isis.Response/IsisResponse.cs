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

namespace TM.FECentralizada.Isis.Response
{
    public partial class IsisResponse : ServiceBase
    {
        Timer oTimer = new Timer();
        public IsisResponse()
        {
            InitializeComponent();
        }

        public void TestProject()
        {
            Procedure();
        }

        protected override void OnStart(string[] args)
        {
            oTimer.Enabled = true;
            oTimer.AutoReset = false;
            oTimer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            oTimer.Start();
            oTimer.Interval = 10000;
        }

        protected override void OnStop()
        {
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Procedure();
        }

        private void Procedure()
        {
            Tools.Logging.Info("Inicio del Proceso: Respuesta Isis.");
            Tools.Logging.Info("Inicio : Obtener Parámetros");

            List<Parameters> ParamsResponse = TM.FECentralizada.Business.Common.GetParametersByKey(new Parameters() { Domain = Tools.Constants.IsisResponse, KeyDomain = "", KeyParam = "" });
            Tools.Logging.Info("Fin : Obtener Parámetros");

            if (ParamsResponse != null && ParamsResponse.Any())
            {
                List<Parameters> ParametersInvoce = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.IsisResponse_Invoice.ToUpper())).ToList();
                List<Parameters> ParametersCreditNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.IsisResponse_CreditNote.ToUpper())).ToList();
                List<Parameters> ParametersDebitNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.IsisResponse_DebitNote.ToUpper())).ToList();
                List<Parameters> ParametersBill = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.IsisResponse_Bill.ToUpper())).ToList();


                Tools.Logging.Info("Inicio : Procesar documentos de BD Isis");

                Invoice(ParametersInvoce);
                Parallel.Invoke(
                    ()=>Invoice(ParametersInvoce),
                    ()=>CreditNote(ParametersCreditNote),
                    ()=>Bill(ParametersBill),
                    ()=>DebitNote(ParametersDebitNote)
                    );


            }
            else
            {
                Tools.Logging.Error("Ocurrió un error al obtener la configuración para Isis.");
            }
            Tools.Logging.Info("Fin del Proceso: Lectura Isis.");
        }


        private void Invoice(List<Parameters> oListParameters)
        {
            //
            ServiceConfig serviceConfig;
            Mail mailConfig;
            FileServer fileServerConfig;

            DateTime timestamp = DateTime.Now;
            List<string> messagesResponse;
            List<ResponseFile> responseFiles;
            int auditId;

            Tools.Logging.Info("Inicio: Obtener parámetros para lectura");

            Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
            serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);
            if (configParameter != null)
            {
                Parameters ftpParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG);

                Tools.Logging.Info("Inicio: Descargar archivos de respuesta de gfiscal - Isis Response");
                if (ftpParameter != null)
                {
                    fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameter);

                    messagesResponse = new List<string>();
                    responseFiles = Business.Common.DownloadFileOutput(fileServerConfig, messagesResponse, "RPTA_FACT_06");



                    if (responseFiles != null && responseFiles.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Insertar auditoria - Isis Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Isis Response");
                        Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);
                        mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                        if (mailConfig != null)
                        {
                            if (messagesResponse.Count > 0)
                            {
                                Business.Common.SendFileNotification(mailConfig, messagesResponse);
                            }
                            Business.Common.UpdateAudit(auditId, Tools.Constants.RETORNO_GFISCAL, 1);

                            Tools.Logging.Info("Inicio: Actualizar documentos en FECentralizada - Isis Response");

                            Business.Common.UpdateInvoiceState(responseFiles);

                            Tools.Logging.Info("Inicio: Actualizar auditoria - Isis Response");
                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_LEGADO, 1);


                        }
                        else
                        {
                            Tools.Logging.Error("No se encontró el parámetro de configuracion MAILCONFIG - Isis Response");
                        }




                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Isis Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, 0, 1, 193);
                    }


                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Isis Response");
            }



        }

        private void Bill(List<Parameters> oListParameters)
        {
            //
            ServiceConfig serviceConfig;
            Mail mailConfig;
            FileServer fileServerConfig;

            DateTime timestamp = DateTime.Now;
            List<string> messagesResponse;
            List<ResponseFile> responseFiles;
            int auditId;

            Tools.Logging.Info("Inicio: Obtener parámetros para lectura");

            Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
            serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);
            if (configParameter != null)
            {
                Parameters ftpParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG);

                Tools.Logging.Info("Inicio: Descargar archivos de respuesta de gfiscal - Isis Response");
                if (ftpParameter != null)
                {
                    fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameter);

                    messagesResponse = new List<string>();
                    responseFiles = Business.Common.DownloadFileOutput(fileServerConfig, messagesResponse, "RPTA_BOLE_06");



                    if (responseFiles != null && responseFiles.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Insertar auditoria - Isis Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Isis Response");
                        Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);
                        mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                        if (mailConfig != null)
                        {
                            if (messagesResponse.Count > 0)
                            {
                                Business.Common.SendFileNotification(mailConfig, messagesResponse);
                            }
                            Business.Common.UpdateAudit(auditId, Tools.Constants.RETORNO_GFISCAL, 1);

                            Tools.Logging.Info("Inicio: Actualizar documentos en FECentralizada - Isis Response");

                            Business.Common.UpdateBillState(responseFiles);

                            Tools.Logging.Info("Inicio: Actualizar auditoria - Isis Response");
                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_LEGADO, 1);


                        }
                        else
                        {
                            Tools.Logging.Error("No se encontró el parámetro de configuracion MAILCONFIG - Isis Response");
                        }




                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Isis Response");
                        
                    }


                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Isis Response");
            }



        }

        private void CreditNote(List<Parameters> oListParameters)
        {
            //
            ServiceConfig serviceConfig;
            Mail mailConfig;
            FileServer fileServerConfig;

            DateTime timestamp = DateTime.Now;
            List<string> messagesResponse;
            List<ResponseFile> responseFiles;
            int auditId;

            Tools.Logging.Info("Inicio: Obtener parámetros para lectura");

            Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
            serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);
            if (configParameter != null)
            {
                Parameters ftpParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG);

                Tools.Logging.Info("Inicio: Descargar archivos de respuesta de gfiscal - Isis Response");
                if (ftpParameter != null)
                {
                    fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameter);

                    messagesResponse = new List<string>();
                    responseFiles = Business.Common.DownloadFileOutput(fileServerConfig, messagesResponse, "RPTA_NCRE_06");



                    if (responseFiles != null && responseFiles.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Insertar auditoria - Isis Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Isis Response");
                        Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);
                        mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                        if (mailConfig != null)
                        {
                            if (messagesResponse.Count > 0)
                            {
                                Business.Common.SendFileNotification(mailConfig, messagesResponse);
                            }
                            Business.Common.UpdateAudit(auditId, Tools.Constants.RETORNO_GFISCAL, 1);

                            Tools.Logging.Info("Inicio: Actualizar documentos en FECentralizada - Isis Response");

                            Business.Common.UpdateCreditNoteState(responseFiles);

                            Tools.Logging.Info("Inicio: Actualizar auditoria - Isis Response");
                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_LEGADO, 1);


                        }
                        else
                        {
                            Tools.Logging.Error("No se encontró el parámetro de configuracion MAILCONFIG - Isis Response");
                        }




                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Isis Response");
                        
                    }


                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Isis Response");
            }



        }

        private void DebitNote(List<Parameters> oListParameters)
        {
            //
            ServiceConfig serviceConfig;
            Mail mailConfig;
            FileServer fileServerConfig;

            DateTime timestamp = DateTime.Now;
            List<string> messagesResponse;
            List<ResponseFile> responseFiles;
            int auditId;

            Tools.Logging.Info("Inicio: Obtener parámetros para lectura");

            Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
            serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);
            if (configParameter != null)
            {
                Parameters ftpParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG);

                Tools.Logging.Info("Inicio: Descargar archivos de respuesta de gfiscal - Isis Response");
                if (ftpParameter != null)
                {
                    fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameter);

                    messagesResponse = new List<string>();
                    responseFiles = Business.Common.DownloadFileOutput(fileServerConfig, messagesResponse, "RPTA_NDEB_06");



                    if (responseFiles != null && responseFiles.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Insertar auditoria - Isis Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.ENVIADO_GFISCAL, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Isis Response");
                        Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);
                        mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                        if (mailConfig != null)
                        {
                            if (messagesResponse.Count > 0)
                            {
                                Business.Common.SendFileNotification(mailConfig, messagesResponse);
                            }
                            Business.Common.UpdateAudit(auditId, Tools.Constants.RETORNO_GFISCAL, 1);

                            Tools.Logging.Info("Inicio: Actualizar documentos en FECentralizada - Isis Response");

                            Business.Common.UpdateDebitNoteState(responseFiles);

                            Tools.Logging.Info("Inicio: Actualizar auditoria - Isis Response");
                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_LEGADO, 1);


                        }
                        else
                        {
                            Tools.Logging.Error("No se encontró el parámetro de configuracion MAILCONFIG - Isis Response");
                        }




                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Isis Response");
                       
                    }


                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Isis Response");
            }



        }



    }
}
