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

namespace TM.FECentralizada.Atis.Response
{
    public partial class AtisResponse : ServiceBase
    {
        Timer oTimer = new Timer();
        public AtisResponse()
        {
            InitializeComponent();
        }

        public void TestProject()
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
            Tools.Logging.Info("Inicio del Proceso: Respuesta Atis.");

            Tools.Logging.Info("Inicio : Obtener Parámetros");
            List<Parameters> ParamsResponse = TM.FECentralizada.Business.Common.GetParametersByKey(new Parameters() { Domain = Tools.Constants.AtisResponse, KeyDomain = "", KeyParam = "" });
            Tools.Logging.Info("Fin : Obtener Parámetros");

            if (ParamsResponse != null && ParamsResponse.Any())
            {
                List<Parameters> ParametersInvoce = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.AtisResponse_Invoice.ToUpper())).ToList();
                List<Parameters> ParametersBill = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.AtisResponse_Bill.ToUpper())).ToList();
                List<Parameters> ParametersCreditNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.AtisResponse_CreditNote.ToUpper())).ToList();
                List<Parameters> ParametersDebitNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.AtisResponse_DebitNote.ToUpper())).ToList();

                Tools.Logging.Info("Inicio : Procesar documentos de BD Atis");

                Tuple<List<string>, int> tpInvoice = null, tpBill = null, tpCreditNote = null, tpDebitNote = null;

                //Invoice(ParametersInvoce);
                /*Bill(ParametersBill);
                CreditNote(ParametersCreditNote);
                DebitNote(ParametersDebitNote);*/
                //parallel invoke

                Parallel.Invoke(
                    ()=> tpInvoice = Invoice(ParametersInvoce),
                    () => tpBill = Bill(ParametersBill),
                    () => tpCreditNote = CreditNote(ParametersCreditNote),
                    () => tpDebitNote = DebitNote(ParametersDebitNote)
                    );

                Tools.Logging.Info("Fin : Procesar documentos de Atis");

                Tools.Logging.Info("Inicio: Enviar respuesta a Legado - Atis");

                List<string> finalFile = new List<string>();

                if (tpInvoice != null && tpInvoice.Item1 != null)
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

                if (finalFile.Count > 0)
                {
                    Tools.Logging.Info("Fin : Consolidar archivo de respuesta FTP - Atis");

                    Tools.Logging.Info("Inicio: Enviar archivo consolidado - Atis");
                    Parameters ftpParameterOut = ParametersInvoce.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);
                    FileServer ftpOutConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameterOut);

                    bool didSentFile = SendResponseFile(finalFile, ftpOutConfig);
                    Tools.Logging.Info("Fin: Enviar archivo consolidado - Atis");

                    Tools.Logging.Info("Inicio: Actualizar auditoria - Atis");
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
                    Tools.Logging.Info("Fin:  Actualizar auditoria - Atis");

                    Parameters ftpParameterInput = ParametersInvoce.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
                    FileServer ftpInputConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameterInput);
                    MoveProcessedFiles(ftpInputConfig, "RPTA_FACT_03");

                    ftpParameterInput = ParametersBill.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
                    ftpInputConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameterInput);
                    MoveProcessedFiles(ftpInputConfig, "RPTA_BOLE_03");

                    ftpParameterInput = ParametersCreditNote.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
                    ftpInputConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameterInput);
                    MoveProcessedFiles(ftpInputConfig, "RPTA_NCRE_03");

                    ftpParameterInput = ParametersDebitNote.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
                    ftpInputConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameterInput);
                    MoveProcessedFiles(ftpInputConfig, "RPTA_NDEB_03");

                }

                Tools.Logging.Info("Fin: Enviar respuesta a Legado - Atis");

            }
            else
            {
                Tools.Logging.Error("Ocurrió un error al obtener la configuración para atis.");
            }
            Tools.Logging.Info("Fin del Proceso: Respuesta Atis.");
        }

        private void MoveProcessedFiles(FileServer fileServerConfig, string sufix)
        {
            Tools.Logging.Info("Inicio :  Mover archivos procesados a ruta PROC ");
            List<String> inputFilesFTP = Tools.FileServer.ListDirectory(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory);
            inputFilesFTP = inputFilesFTP.Where(x => x.StartsWith(sufix)).ToList();
            foreach (string file in inputFilesFTP)
            {
                Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file, true, System.IO.Path.GetTempPath());
                Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory + "/PROC/", file, System.IO.File.ReadAllBytes(System.IO.Path.GetTempPath() + "/" + file));
                Tools.FileServer.DeleteFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file);
            };
            Tools.Logging.Info("Inicio : Mover archivos procesados a ruta PROC ");
        }

        private bool SendResponseFile(List<string> finalFile, FileServer fileServerOutConfig)
        {
            byte[] finalArray = Tools.Common.CreateFileText(finalFile);
            DateTime currentTime = DateTime.Now;
            if (!fileServerOutConfig.Directory.EndsWith("/")) fileServerOutConfig.Directory += "/";
            return Tools.FileServer.UploadFile(fileServerOutConfig.Host, fileServerOutConfig.Port, fileServerOutConfig.User, fileServerOutConfig.Password, fileServerOutConfig.Directory, $"RPTA_{currentTime.ToString("yyyyMMdd_")}230000.txt", finalArray);
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
                Parameters ftpParameterOut = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);

                Tools.Logging.Info("Inicio: Descargar archivos de respuesta de gfiscal - Atis Response");
                if (ftpParameter != null)
                {
                    fileServerConfig = Business.Common.GetParameterDeserialized<Entities.Common.FileServer>(ftpParameter);

                    messagesResponse = new List<string>();
                    responseFiles = Business.Common.DownloadFileOutput(fileServerConfig, messagesResponse, "RPTA_FACT_03");



                    if (responseFiles != null && responseFiles.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Insertar auditoria - Atis Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Atis Response");
                        Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);
                        mailConfig = Business.Common.GetParameterDeserialized<Entities.Common.Mail>(mailParameter);

                        if (mailConfig != null)
                        {
                            if (messagesResponse.Count > 0)
                            {
                                Business.Common.SendFileNotification(mailConfig, messagesResponse);
                            }
                            Business.Common.UpdateAudit(auditId, Tools.Constants.RETORNO_GFISCAL, 1);

                            Tools.Logging.Info("Inicio: Actualizar documentos en FECentralizada - Atis Response");

                            Business.Common.UpdateInvoiceState(responseFiles);

                            Tools.Logging.Info("Inicio: Envio archivo respuesta a Legado - Atis Response");

                            Tools.Logging.Info("Inicio : Obtener Parámetros de la Estructura del Archivo.");
                            Parameters SpecFileOut = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_SPEC_OUT);
                            Tools.Logging.Info("Fin : Obtener Parámetros  de la Estructura del Archivo.");

                            if (SpecFileOut != null)
                            {
                                var SpecBody = SpecFileOut.ValueJson.Split('|');

                                Tools.Logging.Info($"Inicio : Armado de archivo de respuesta a Legado - Atis Response");

                                List<string> ListDataFile = new List<string>();
                                foreach (var item in responseFiles)
                                {
                                    var row = @"" +
                                    item.estado.PadRight(int.Parse(SpecBody[0]), ' ') + "" +
                                    item.numDocEmisor.PadRight(int.Parse(SpecBody[1]), ' ') + "" +
                                    item.tipoDocumento.PadRight(int.Parse(SpecBody[2]), ' ') + "" +
                                    item.serieNumero.PadRight(int.Parse(SpecBody[3]), ' ') + "" +
                                    item.codigoSunat.PadRight(int.Parse(SpecBody[4]), ' ') + "" +
                                    item.mensajeSunat.PadRight(int.Parse(SpecBody[5]), ' ') + "" +
                                    item.fechaDeclaracion.PadRight(int.Parse(SpecBody[6]), ' ') + "" +
                                    item.fechaEmision.PadRight(int.Parse(SpecBody[7]), ' ') + "" +
                                    item.firma.PadRight(int.Parse(SpecBody[8]), ' ') + "" +
                                    item.resumen.PadRight(int.Parse(SpecBody[9]), ' ') + "" +
                                    item.codSistema.PadRight(int.Parse(SpecBody[10]), ' ') + "" +
                                    item.adicional1.PadRight(int.Parse(SpecBody[11]), ' ') + "" +
                                    item.adicional2.PadRight(int.Parse(SpecBody[12]), ' ') + "" +
                                    item.adicional3.PadRight(int.Parse(SpecBody[13]), ' ') + "" +
                                    item.adicional4.PadRight(int.Parse(SpecBody[14]), ' ') + "" +
                                    item.adicional5.PadLeft(int.Parse(SpecBody[15]), ' ');
                                    

                                    ListDataFile.Add(row);
                                }

                                

                                return new Tuple<List<string>, int>(ListDataFile, auditId);

                            }
                            else {
                                Tools.Logging.Error("No se encontró el parámetro de configuracion SPEC_OUT - Atis Response");
                            }

                            Tools.Logging.Info("Fin: Envio archivo respuesta a Legado - Atis Response");

                            Tools.Logging.Info("Inicio: Actualizar auditoria - Atis Response");
                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_LEGADO, 1);


                        }
                        else
                        {
                            Tools.Logging.Error("No se encontró el parámetro de configuracion MAILCONFIG - Atis Response");
                        }




                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Atis Response");
                        //auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, 0, 1, 193);
                    }


                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Atis Response");
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

                Tools.Logging.Info("Inicio: Descargar archivos de respuesta de gfiscal - Atis Response");
                if (ftpParameter != null)
                {
                    fileServerConfig = Business.Common.GetParameterDeserialized<Entities.Common.FileServer>(ftpParameter);

                    messagesResponse = new List<string>();
                    responseFiles = Business.Common.DownloadFileOutput(fileServerConfig, messagesResponse, "RPTA_BOLE_03");



                    if (responseFiles != null && responseFiles.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Insertar auditoria - Atis Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Atis Response");
                        Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);
                        mailConfig = Business.Common.GetParameterDeserialized<Entities.Common.Mail>(mailParameter);

                        if (mailConfig != null)
                        {
                            if (messagesResponse.Count > 0)
                            {
                                Business.Common.SendFileNotification(mailConfig, messagesResponse);
                            }
                            Business.Common.UpdateAudit(auditId, Tools.Constants.RETORNO_GFISCAL, 1);

                            Tools.Logging.Info("Inicio: Actualizar documentos en FECentralizada - Atis Response");

                            Business.Common.UpdateBillState(responseFiles);

                            Tools.Logging.Info("Inicio: Envio archivo respuesta a Legado - Atis Response");

                            Tools.Logging.Info("Inicio : Obtener Parámetros de la Estructura del Archivo.");
                            Parameters SpecFileOut = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_SPEC_OUT);
                            Tools.Logging.Info("Fin : Obtener Parámetros  de la Estructura del Archivo.");

                            if (SpecFileOut != null)
                            {
                                var SpecBody = SpecFileOut.ValueJson.Split('|');

                                Tools.Logging.Info($"Inicio : Armado de archivo de respuesta a Legado - Atis Response");

                                List<string> ListDataFile = new List<string>();
                                foreach (var item in responseFiles)
                                {
                                    var row = @"" +
                                    item.estado.PadRight(int.Parse(SpecBody[0]), ' ') + "" +
                                    item.numDocEmisor.PadRight(int.Parse(SpecBody[1]), ' ') + "" +
                                    item.tipoDocumento.PadRight(int.Parse(SpecBody[2]), ' ') + "" +
                                    item.serieNumero.PadRight(int.Parse(SpecBody[3]), ' ') + "" +
                                    item.codigoSunat.PadRight(int.Parse(SpecBody[4]), ' ') + "" +
                                    item.mensajeSunat.PadRight(int.Parse(SpecBody[5]), ' ') + "" +
                                    item.fechaDeclaracion.PadRight(int.Parse(SpecBody[6]), ' ') + "" +
                                    item.fechaEmision.PadRight(int.Parse(SpecBody[7]), ' ') + "" +
                                    item.firma.PadRight(int.Parse(SpecBody[8]), ' ') + "" +
                                    item.resumen.PadRight(int.Parse(SpecBody[9]), ' ') + "" +
                                    item.codSistema.PadRight(int.Parse(SpecBody[10]), ' ') + "" +
                                    item.adicional1.PadRight(int.Parse(SpecBody[11]), ' ') + "" +
                                    item.adicional2.PadRight(int.Parse(SpecBody[12]), ' ') + "" +
                                    item.adicional3.PadRight(int.Parse(SpecBody[13]), ' ') + "" +
                                    item.adicional4.PadRight(int.Parse(SpecBody[14]), ' ') + "" +
                                    item.adicional5.PadLeft(int.Parse(SpecBody[15]), ' ');


                                    ListDataFile.Add(row);
                                }
                                //byte[] ResultBytes = Tools.Common.CreateFileText(ListDataFile);

                                return new Tuple<List<string>, int>(ListDataFile, auditId);

                            }
                            else
                            {
                                Tools.Logging.Error("No se encontró el parámetro de configuracion SPEC_OUT - Atis Response");
                            }

                            Tools.Logging.Info("Fin: Envio archivo respuesta a Legado - Atis Response");

                            Tools.Logging.Info("Inicio: Actualizar auditoria - Atis Response");
                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_LEGADO, 1);


                        }
                        else
                        {
                            Tools.Logging.Error("No se encontró el parámetro de configuracion MAILCONFIG - Atis Response");
                        }




                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Atis Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, 0, 1, 193);
                    }


                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Atis Response");
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

                Tools.Logging.Info("Inicio: Descargar archivos de respuesta de gfiscal - Atis Response");
                if (ftpParameter != null)
                {
                    fileServerConfig = Business.Common.GetParameterDeserialized<Entities.Common.FileServer>(ftpParameter);

                    messagesResponse = new List<string>();
                    responseFiles = Business.Common.DownloadFileOutput(fileServerConfig, messagesResponse, "RPTA_NCRE_03");



                    if (responseFiles != null && responseFiles.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Insertar auditoria - Atis Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Atis Response");
                        Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);
                        mailConfig = Business.Common.GetParameterDeserialized<Entities.Common.Mail>(mailParameter);

                        if (mailConfig != null)
                        {
                            if (messagesResponse.Count > 0)
                            {
                                Business.Common.SendFileNotification(mailConfig, messagesResponse);
                            }
                            Business.Common.UpdateAudit(auditId, Tools.Constants.RETORNO_GFISCAL, 1);

                            Tools.Logging.Info("Inicio: Actualizar documentos en FECentralizada - Atis Response");

                            Business.Common.UpdateCreditNoteState(responseFiles);

                            Tools.Logging.Info("Inicio: Envio archivo respuesta a Legado - Atis Response");

                            Tools.Logging.Info("Inicio : Obtener Parámetros de la Estructura del Archivo.");
                            Parameters SpecFileOut = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_SPEC_OUT);
                            Tools.Logging.Info("Fin : Obtener Parámetros  de la Estructura del Archivo.");

                            if (SpecFileOut != null)
                            {
                                var SpecBody = SpecFileOut.ValueJson.Split('|');

                                Tools.Logging.Info($"Inicio : Armado de archivo de respuesta a Legado - Atis Response");

                                List<string> ListDataFile = new List<string>();
                                foreach (var item in responseFiles)
                                {
                                    var row = @"" +
                                    item.estado.PadRight(int.Parse(SpecBody[0]), ' ') + "" +
                                    item.numDocEmisor.PadRight(int.Parse(SpecBody[1]), ' ') + "" +
                                    item.tipoDocumento.PadRight(int.Parse(SpecBody[2]), ' ') + "" +
                                    item.serieNumero.PadRight(int.Parse(SpecBody[3]), ' ') + "" +
                                    item.codigoSunat.PadRight(int.Parse(SpecBody[4]), ' ') + "" +
                                    item.mensajeSunat.PadRight(int.Parse(SpecBody[5]), ' ') + "" +
                                    item.fechaDeclaracion.PadRight(int.Parse(SpecBody[6]), ' ') + "" +
                                    item.fechaEmision.PadRight(int.Parse(SpecBody[7]), ' ') + "" +
                                    item.firma.PadRight(int.Parse(SpecBody[8]), ' ') + "" +
                                    item.resumen.PadRight(int.Parse(SpecBody[9]), ' ') + "" +
                                    item.codSistema.PadRight(int.Parse(SpecBody[10]), ' ') + "" +
                                    item.adicional1.PadRight(int.Parse(SpecBody[11]), ' ') + "" +
                                    item.adicional2.PadRight(int.Parse(SpecBody[12]), ' ') + "" +
                                    item.adicional3.PadRight(int.Parse(SpecBody[13]), ' ') + "" +
                                    item.adicional4.PadRight(int.Parse(SpecBody[14]), ' ') + "" +
                                    item.adicional5.PadLeft(int.Parse(SpecBody[15]), ' ');


                                    ListDataFile.Add(row);
                                }
                                //byte[] ResultBytes = Tools.Common.CreateFileText(ListDataFile);

                                return new Tuple<List<string>, int>(ListDataFile, auditId);

                            }
                            else
                            {
                                Tools.Logging.Error("No se encontró el parámetro de configuracion SPEC_OUT - Atis Response");
                            }

                            Tools.Logging.Info("Fin: Envio archivo respuesta a Legado - Atis Response");

                            Tools.Logging.Info("Inicio: Actualizar auditoria - Atis Response");
                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_LEGADO, 1);


                        }
                        else
                        {
                            Tools.Logging.Error("No se encontró el parámetro de configuracion MAILCONFIG - Atis Response");
                        }




                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Atis Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, 0, 1, 193);
                    }


                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Atis Response");
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

                Tools.Logging.Info("Inicio: Descargar archivos de respuesta de gfiscal - Atis Response");
                if (ftpParameter != null)
                {
                    fileServerConfig = Business.Common.GetParameterDeserialized<Entities.Common.FileServer>(ftpParameter);

                    messagesResponse = new List<string>();
                    responseFiles = Business.Common.DownloadFileOutput(fileServerConfig, messagesResponse, "RPTA_NDEB_03");



                    if (responseFiles != null && responseFiles.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Insertar auditoria - Atis Response");
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Atis Response");
                        Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);
                        mailConfig = Business.Common.GetParameterDeserialized<Entities.Common.Mail>(mailParameter);

                        if (mailConfig != null)
                        {
                            if (messagesResponse.Count > 0)
                            {
                                Business.Common.SendFileNotification(mailConfig, messagesResponse);
                            }
                            Business.Common.UpdateAudit(auditId, Tools.Constants.RETORNO_GFISCAL, 1);

                            Tools.Logging.Info("Inicio: Actualizar documentos en FECentralizada - Atis Response");

                            Business.Common.UpdateDebitNoteState(responseFiles);

                            Tools.Logging.Info("Inicio: Envio archivo respuesta a Legado - Atis Response");

                            Tools.Logging.Info("Inicio : Obtener Parámetros de la Estructura del Archivo.");
                            Parameters SpecFileOut = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_SPEC_OUT);
                            Tools.Logging.Info("Fin : Obtener Parámetros  de la Estructura del Archivo.");

                            if (SpecFileOut != null)
                            {
                                var SpecBody = SpecFileOut.ValueJson.Split('|');

                                Tools.Logging.Info($"Inicio : Armado de archivo de respuesta a Legado - Atis Response");

                                List<string> ListDataFile = new List<string>();
                                foreach (var item in responseFiles)
                                {
                                    var row = @"" +
                                    item.estado.PadRight(int.Parse(SpecBody[0]), ' ') + "" +
                                    item.numDocEmisor.PadRight(int.Parse(SpecBody[1]), ' ') + "" +
                                    item.tipoDocumento.PadRight(int.Parse(SpecBody[2]), ' ') + "" +
                                    item.serieNumero.PadRight(int.Parse(SpecBody[3]), ' ') + "" +
                                    item.codigoSunat.PadRight(int.Parse(SpecBody[4]), ' ') + "" +
                                    item.mensajeSunat.PadRight(int.Parse(SpecBody[5]), ' ') + "" +
                                    item.fechaDeclaracion.PadRight(int.Parse(SpecBody[6]), ' ') + "" +
                                    item.fechaEmision.PadRight(int.Parse(SpecBody[7]), ' ') + "" +
                                    item.firma.PadRight(int.Parse(SpecBody[8]), ' ') + "" +
                                    item.resumen.PadRight(int.Parse(SpecBody[9]), ' ') + "" +
                                    item.codSistema.PadRight(int.Parse(SpecBody[10]), ' ') + "" +
                                    item.adicional1.PadRight(int.Parse(SpecBody[11]), ' ') + "" +
                                    item.adicional2.PadRight(int.Parse(SpecBody[12]), ' ') + "" +
                                    item.adicional3.PadRight(int.Parse(SpecBody[13]), ' ') + "" +
                                    item.adicional4.PadRight(int.Parse(SpecBody[14]), ' ') + "" +
                                    item.adicional5.PadLeft(int.Parse(SpecBody[15]), ' ');


                                    ListDataFile.Add(row);
                                }
                                //byte[] ResultBytes = Tools.Common.CreateFileText(ListDataFile);

                                return new Tuple<List<string>, int>(ListDataFile, auditId);

                            }
                            else
                            {
                                Tools.Logging.Error("No se encontró el parámetro de configuracion SPEC_OUT - Atis Response");
                            }

                            Tools.Logging.Info("Fin: Envio archivo respuesta a Legado - Atis Response");

                            Tools.Logging.Info("Inicio: Actualizar auditoria - Atis Response");
                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_LEGADO, 1);


                        }
                        else
                        {
                            Tools.Logging.Error("No se encontró el parámetro de configuracion MAILCONFIG - Atis Response");
                        }




                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Atis Response");
                        //auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, 0, 1, 193);
                    }


                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Atis Response");
            }
            return null;
        }

        protected override void OnStop()
        {
            oTimer.Stop();
        }
    }
}
