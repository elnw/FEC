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
                        auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 4, Tools.Constants.NO_LEIDO, responseFiles.Count, 1, serviceConfig.Norm);

                        Tools.Logging.Info("Inicio:  Obtener configuración de email - Sap Response");
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

                            Tools.Logging.Info("Inicio: Envio archivo respuesta a Legado - Sap Response");

                            Tools.Logging.Info("Inicio : Obtener Parámetros de la Estructura del Archivo.");
                            Parameters SpecFileOut = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_SPEC_OUT);
                            Tools.Logging.Info("Fin : Obtener Parámetros  de la Estructura del Archivo.");

                            if (SpecFileOut != null)
                            {
                                var SpecBody = SpecFileOut.ValueJson.Split('|');

                                Tools.Logging.Info($"Inicio : Armado de archivo de respuesta a Legado - Sap Response");

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


                                try
                                {
                                    Tools.Logging.Info($"Inicio : Envío de Archivo Respuesta - Sap Response.");
                                    string FileName = $"RPTA_FACT_{timestamp.ToString("yyyyMMdd")}_230000.txt";
                                    if (ftpParameterOut != null)
                                    {
                                        FileServer fileServerConfigOut = Business.Common.GetParameterDeserialized<Entities.Common.FileServer>(ftpParameterOut);

                                        var currentResponseFile = Tools.FileServer.DownloadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, FileName);

                                        if (currentResponseFile != null && currentResponseFile.Count > 0)
                                        {
                                            Tools.FileServer.DeleteFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, FileName);
                                        }

                                        currentResponseFile.AddRange(ListDataFile);
                                        byte[] ResultBytes = Tools.Common.CreateFileText(currentResponseFile);

                                        Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, FileName, ResultBytes);

                                        Tools.Logging.Info("Inicio :  Mover archivos procesados a ruta PROC ");
                                        List<String> inputFilesFTP = Tools.FileServer.ListDirectory(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory);
                                        inputFilesFTP = inputFilesFTP.Where(x => x.StartsWith("RPTA_FACT_04")).ToList();
                                        foreach (string file in inputFilesFTP)
                                        {
                                            Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file, true, System.IO.Path.GetTempPath());
                                            Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory + "/PROC/", file, System.IO.File.ReadAllBytes(System.IO.Path.GetTempPath() + "/" + file));
                                            Tools.FileServer.DeleteFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file);
                                        };
                                        Tools.Logging.Info("Inicio : Mover archivos procesados a ruta PROC ");

                                    }
                                    else
                                    {
                                        Tools.Logging.Error("No se encontró el parámetro de configuracion FTP_OUTPUT - Sap Response");
                                    }
                                    Tools.Logging.Info($"Fin : Envío de Archivo Respuesta - Sap Response.");
                                }
                                catch (Exception ex)
                                {
                                    Tools.Logging.Error($"Error : Envío de Archivo Respuesta - Sap Response = {ex.Message}");
                                }

                            }
                            else
                            {
                                Tools.Logging.Error("No se encontró el parámetro de configuracion SPEC_OUT - Sap Response");
                            }

                            Tools.Logging.Info("Fin: Envio archivo respuesta a Legado - Sap Response");

                            Tools.Logging.Info("Inicio: Actualizar auditoria - Sap Response");
                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_LEGADO, 1);


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
