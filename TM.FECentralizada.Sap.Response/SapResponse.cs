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
            try
            {
                Tools.Logging.Info("Inicio del Proceso: Respuesta Sap.");

                Tools.Logging.Info("Inicio : Obtener Parámetros");
                //Método que Obtendrá los Parámetros.
                List<Parameters> ParamsResponse = TM.FECentralizada.Business.Common.GetParametersByKey(new Parameters() { Domain = Tools.Constants.SapResponse, KeyDomain = "", KeyParam = "" });
                Tools.Logging.Info("Fin : Obtener Parámetros");

                if (ParamsResponse != null && ParamsResponse.Any())
                {
                    List<Parameters> ParametersInvoce = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.SapResponse_Invoice.ToUpper())).ToList();
                    List<Parameters> ParametersBill = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.SapResponse_Bill.ToUpper())).ToList();
                    List<Parameters> ParametersCreditNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.SapResponse_CreditNote.ToUpper())).ToList();
                    List<Parameters> ParametersDebitNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.SapResponse_DebitNote.ToUpper())).ToList();

                    Tuple<List<string>, int> tpInvoice = null, tpBill = null,tpCreditNote = null, tpDebitNote = null;

                    Parameters pmtServiceConfig = ParametersInvoce.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
                    ServiceConfig serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(pmtServiceConfig);

                    Tools.Logging.Info("Inicio : Procesar documentos de BD Sap");
                    //  tpInvoice = Invoice(ParametersInvoce);

                    Parallel.Invoke(
                               () => tpInvoice = Invoice(ParametersInvoce),
                               () => tpBill = Bill(ParametersBill),
                               () => tpCreditNote = CreditNote(ParametersCreditNote),
                               () => tpDebitNote = DebitNote(ParametersDebitNote)
                        );
                    Tools.Logging.Info("Fin : Procesar documentos de BD Sap");

                    Tools.Logging.Info("Inicio : Consolidar archivo de respuesta FTP - Sap");

                    List<string> finalFile = new List<string>();

                    if(tpInvoice != null && tpInvoice.Item1 != null)
                    {
                        finalFile.AddRange(tpInvoice.Item1);
                    }

                    if (tpBill != null && tpBill.Item1 != null)
                    {
                        finalFile.AddRange(tpBill.Item1);
                    }

                    if (tpCreditNote != null && tpCreditNote.Item1 != null)
                    {
                        finalFile.AddRange(tpCreditNote.Item1);
                    }

                    if (tpDebitNote != null && tpDebitNote.Item1 != null)
                    {
                        finalFile.AddRange(tpDebitNote.Item1);
                    }

                    if(finalFile.Count > 0)
                    {
                        Tools.Logging.Info("Fin : Consolidar archivo de respuesta FTP - Sap");

                        Tools.Logging.Info("Inicio: Enviar archivo consolidado - Sap");
                        Parameters ftpParameterOut = ParametersInvoce.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);
                        FileServer ftpOutConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameterOut);

                        bool didSentFile = SendResponseFile(finalFile, ftpOutConfig);
                        Tools.Logging.Info("Fin: Enviar archivo consolidado - Sap");

                        Tools.Logging.Info("Inicio: Actualizar auditoria - Sap");
                        if (didSentFile)
                        {
                            Business.Common.UpdateAudit(tpInvoice.Item2, Tools.Constants.ENVIADO_LEGADO, 1);
                            Business.Common.UpdateAudit(tpBill.Item2, Tools.Constants.ENVIADO_LEGADO, 1);
                            Business.Common.UpdateAudit(tpCreditNote.Item2, Tools.Constants.ENVIADO_LEGADO, 1);
                            Business.Common.UpdateAudit(tpDebitNote.Item2, Tools.Constants.ENVIADO_LEGADO, 1);
                        }
                        else
                        {
                            Business.Common.UpdateAudit(tpInvoice.Item2, Tools.Constants.ERROR_FECENTRALIZADA, 1);
                            Business.Common.UpdateAudit(tpBill.Item2, Tools.Constants.ERROR_FECENTRALIZADA, 1);
                            Business.Common.UpdateAudit(tpCreditNote.Item2, Tools.Constants.ERROR_FECENTRALIZADA, 1);
                            Business.Common.UpdateAudit(tpDebitNote.Item2, Tools.Constants.ERROR_FECENTRALIZADA, 1);
                        }
                        Tools.Logging.Info("Fin:  Actualizar auditoria - Sap");
                    }

                    
                    var Minutes = serviceConfig.ExecutionRate;
                    oTimer.Interval = Tools.Common.ConvertMinutesToMilliseconds(Minutes);
                    oTimer.Start();
                    oTimer.AutoReset = true;
                }
                else
                {
                    Tools.Logging.Error("Ocurrió un error al obtener la configuración para Sap.");
                }
                Tools.Logging.Info("Fin del Proceso: Respuesta Sap.");
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        private bool SendResponseFile(List<string> finalFile, FileServer fileServerOutConfig)
        {
            byte[] finalArray = Tools.Common.CreateFileText(finalFile);
            DateTime currentTime = DateTime.Now;
            if (!fileServerOutConfig.Directory.EndsWith("/")) fileServerOutConfig.Directory += "/";
            return Tools.FileServer.UploadFile(fileServerOutConfig.Host, fileServerOutConfig.Port, fileServerOutConfig.User, fileServerOutConfig.Password, fileServerOutConfig.Directory, $"RPTA_{currentTime.ToString("yyyyMMdd_HHmmss")}.txt", finalArray);
        }

        private Tuple<List<string>, int> Invoice(List<Parameters> oListParameters)
        {
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
                Parameters ftpParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
                

                Tools.Logging.Info("Inicio: Descargar archivos de respuesta de gfiscal - Sap Response");
                if (ftpParameter != null)
                {
                    fileServerConfig = Business.Common.GetParameterDeserialized<Entities.Common.FileServer>(ftpParameter);

                    messagesResponse = new List<string>();
                    responseFiles = Business.Common.DownloadFileOutput(fileServerConfig, messagesResponse, "RPTA_FACT_04");

                    if (responseFiles != null && responseFiles.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Insertar auditoria - Sap Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 4, Tools.Constants.RETORNO_GFISCAL, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Sap Response");
                        Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);
                        mailConfig = Business.Common.GetParameterDeserialized<Entities.Common.Mail>(mailParameter);

                        if (mailConfig != null)
                        {
                            if (messagesResponse.Count > 0)
                            {
                                Business.Common.SendFileNotification(mailConfig, messagesResponse);
                            }
                            

                            Tools.Logging.Info("Inicio: Actualizar documentos en FECentralizada - Sap Response");

                            Business.Common.UpdateInvoiceState(responseFiles);
                            
                            Tools.Logging.Info($"Inicio : Armado de archivo de respuesta a Legado - Sap Response");

                            var outputFile = Business.Sap.CreateLegacyResponseFile(responseFiles);

                            return new Tuple<List<string>, int>(outputFile, auditId);
                        }
                        else
                        {
                            Tools.Logging.Error("No se encontró el parámetro de configuracion MAILCONFIG - Sap Response");
                        }

                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Sap Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, 0, 1, 193);
                    }


                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Sap Response");
            }
            return null;
        }
        private Tuple<List<string>, int> Bill(List<Parameters> oListParameters)
        {
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
                Parameters ftpParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
                Parameters ftpParameterOut = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);

                Tools.Logging.Info("Inicio: Descargar archivos de respuesta de gfiscal - Sap Response");
                if (ftpParameter != null)
                {
                    fileServerConfig = Business.Common.GetParameterDeserialized<Entities.Common.FileServer>(ftpParameter);

                    messagesResponse = new List<string>();
                    responseFiles = Business.Common.DownloadFileOutput(fileServerConfig, messagesResponse, "RPTA_BOLE_04");

                    if (responseFiles != null && responseFiles.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Insertar auditoria - Sap Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 4, Tools.Constants.RETORNO_GFISCAL, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Sap Response");
                        Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);
                        mailConfig = Business.Common.GetParameterDeserialized<Entities.Common.Mail>(mailParameter);

                        if (mailConfig != null)
                        {
                            if (messagesResponse.Count > 0)
                            {
                                Business.Common.SendFileNotification(mailConfig, messagesResponse);
                            }


                            Tools.Logging.Info("Inicio: Actualizar documentos en FECentralizada - Sap Response");

                            Business.Common.UpdateBillState(responseFiles);

                            Tools.Logging.Info($"Inicio : Armado de archivo de respuesta a Legado - Sap Response");

                            var outputFile = Business.Sap.CreateLegacyResponseFile(responseFiles);

                            return new Tuple<List<string>, int>(outputFile, auditId);
                        }
                        else
                        {
                            Tools.Logging.Error("No se encontró el parámetro de configuracion MAILCONFIG - Sap Response");
                        }

                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Sap Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 4, Tools.Constants.NO_LEIDO, 0, 1, 193);
                    }


                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Sap Response");
            }
            return null;
        }
        private Tuple<List<string>, int> CreditNote(List<Parameters> oListParameters)
        {

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
                Parameters ftpParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
                Parameters ftpParameterOut = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);

                Tools.Logging.Info("Inicio: Descargar archivos de respuesta de gfiscal - Sap Response");
                if (ftpParameter != null)
                {
                    fileServerConfig = Business.Common.GetParameterDeserialized<Entities.Common.FileServer>(ftpParameter);

                    messagesResponse = new List<string>();
                    responseFiles = Business.Common.DownloadFileOutput(fileServerConfig, messagesResponse, "RPTA_NCRE_04");

                    if (responseFiles != null && responseFiles.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Insertar auditoria - Sap Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 4, Tools.Constants.RETORNO_GFISCAL, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Sap Response");
                        Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);
                        mailConfig = Business.Common.GetParameterDeserialized<Entities.Common.Mail>(mailParameter);

                        if (mailConfig != null)
                        {
                            if (messagesResponse.Count > 0)
                            {
                                Business.Common.SendFileNotification(mailConfig, messagesResponse);
                            }


                            Tools.Logging.Info("Inicio: Actualizar documentos en FECentralizada - Sap Response");

                            Business.Common.UpdateCreditNoteState(responseFiles);

                            Tools.Logging.Info($"Inicio : Armado de archivo de respuesta a Legado - Sap Response");

                            var outputFile = Business.Sap.CreateLegacyResponseFile(responseFiles);

                            return new Tuple<List<string>, int>(outputFile, auditId);
                        }
                        else
                        {
                            Tools.Logging.Error("No se encontró el parámetro de configuracion MAILCONFIG - Atis Response");
                        }

                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Sap Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, 0, 1, 193);
                    }


                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Sap Response");
            }
            return null;
        }
        private Tuple<List<string>, int> DebitNote(List<Parameters> oListParameters)
        {

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
                Parameters ftpParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
                Parameters ftpParameterOut = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);

                Tools.Logging.Info("Inicio: Descargar archivos de respuesta de gfiscal - Sap Response");
                if (ftpParameter != null)
                {
                    fileServerConfig = Business.Common.GetParameterDeserialized<Entities.Common.FileServer>(ftpParameter);

                    messagesResponse = new List<string>();
                    responseFiles = Business.Common.DownloadFileOutput(fileServerConfig, messagesResponse, "RPTA_FACT_04");

                    if (responseFiles != null && responseFiles.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Insertar auditoria - Sap Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 4, Tools.Constants.RETORNO_GFISCAL, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Sap Response");
                        Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);
                        mailConfig = Business.Common.GetParameterDeserialized<Entities.Common.Mail>(mailParameter);

                        if (mailConfig != null)
                        {
                            if (messagesResponse.Count > 0)
                            {
                                Business.Common.SendFileNotification(mailConfig, messagesResponse);
                            }


                            Tools.Logging.Info("Inicio: Actualizar documentos en FECentralizada - Atis Response");

                            Business.Common.UpdateDebitNoteState(responseFiles);

                            Tools.Logging.Info($"Inicio : Armado de archivo de respuesta a Legado - Sap Response");

                            var outputFile = Business.Sap.CreateLegacyResponseFile(responseFiles);

                            return new Tuple<List<string>, int>(outputFile, auditId);
                        }
                        else
                        {
                            Tools.Logging.Error("No se encontró el parámetro de configuracion MAILCONFIG - Atis Response");
                        }

                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Sap Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, 0, 1, 193);
                    }


                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Sap Response");
            }
            return null;
        }

        protected override void OnStop()
        {
            oTimer.Stop();
        }
    }
}
