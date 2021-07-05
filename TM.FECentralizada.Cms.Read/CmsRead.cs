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
using TM.FECentralizada.Entities.Cms;
using TM.FECentralizada.Entities.Common;

namespace TM.FECentralizada.Cms.Read
{
    public partial class CmsRead : ServiceBase
    {
        Timer oTimer = new Timer();
        public CmsRead()
        {
            InitializeComponent();

            Tools.Logging.Configure();
        }

        public void probar()
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
                Tools.Logging.Info("Inicio del Proceso: Lectura Cms.");

                Tools.Logging.Info("Inicio : Obtener Parámetros");
                //Método que Obtendrá los Parámetros.
                List<Parameters> ParamsResponse = TM.FECentralizada.Business.Common.GetParametersByKey(new Parameters() { Domain = Tools.Constants.CmsRead, KeyDomain = "", KeyParam = "" });
                Tools.Logging.Info("Fin : Obtener Parámetros");

                if (ParamsResponse != null && ParamsResponse.Any())
                {
                    List<Parameters> ParametersInvoce = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.CmsRead_Invoice.ToUpper())).ToList();
                    List<Parameters> ParametersBill = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.CmsRead_Bill.ToUpper())).ToList();
                    List<Parameters> ParametersCreditNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.CmsRead_CreditNote.ToUpper())).ToList();
                    List<Parameters> ParametersDebitNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.CmsRead_DebitNote.ToUpper())).ToList();

                    Tools.Logging.Info("Inicio : Procesar documentos de FTP Cms");
                    Invoice(ParametersInvoce);
                    Bill(ParametersBill);
                    CreditNote(ParametersCreditNote);
                    DebitNote(ParametersDebitNote);
                    /*Parallel.Invoke(
                               () => Invoice(ParametersInvoce),
                               () => CreditNote(ParametersCreditNote),
                               () => DebitNote(ParametersDebitNote)
                        );*/
                    Tools.Logging.Info("Fin : Procesar documentos de FTP Cms");

                    //Obtengo la Configuración Intervalo de Tiempo
                    var oConfiguration = Business.Common.GetParameterDeserialized<ServiceConfig>(ParamsResponse.Find(x => x.KeyParam == Tools.Constants.KEY_CONFIG));

                    var Minutes = oConfiguration.ExecutionRate;//oConfiguration.Key3.Equals("D") ? oConfiguration.Value3 : oConfiguration.Key3.Equals("T") ? oConfiguration.Value2 : oConfiguration.Value1;
                    oTimer.Interval = Tools.Common.ConvertMinutesToMilliseconds(Minutes);
                    oTimer.Start();
                    oTimer.AutoReset = true;
                }
                else
                {
                    Tools.Logging.Error("Ocurrió un error al obtener la configuración para pacyfic.");
                }
                Tools.Logging.Info("Fin del Proceso: Lectura Pacyfic.");
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
            bool isValid;
            string validationMessage = "";
            int auditId;
            int intentos = 0;
            DateTime timestamp = DateTime.Now;
            List<string> inputFilesFTP;
            List<List<string>> inputFiles = new List<List<string>>();

            Tools.Logging.Info("Inicio: Obtener parámetros para lectura");
            Parameters ftpParameterInput = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
            Tools.Logging.Info("Fin: Obtener parámetros para lectura");


            if (ftpParameterInput != null)
            {
                fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameterInput);

                inputFilesFTP = Tools.FileServer.ListDirectory(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory);

                if (inputFilesFTP.Count > 0)
                {
                    inputFilesFTP = inputFilesFTP.Where(x => x.StartsWith("FACT_")).ToList();
                    if (inputFilesFTP.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Obtener norma para las facturas de Cms");
                        Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
                        Tools.Logging.Info("Fin: Obtener norma para las facturas de Cms");

                        if (configParameter != null)
                        {
                            serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);

                            Tools.Logging.Info("Inicio : Obtener documentos de FTP Cms - Facturas");

                            List<InvoiceHeader> ListInvoceHeader = new List<InvoiceHeader>();
                            List<InvoiceDetail> ListInvoceDetail = new List<InvoiceDetail>();

                            List<string> data;

                            foreach (string filename in inputFilesFTP)
                            {
                                data = Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, filename);
                                string serie = "";
                                for (int i = 0; i < data.Count(); i++)
                                {
                                    if (data[i].StartsWith("C"))
                                    {
                                        serie = data[i].Split('|')[1].Trim();
                                    }
                                    if (data[i].StartsWith("D"))
                                    {
                                        data[i] = serie + "|" + data[i];
                                    }
                                }

                                List<InvoiceHeader> ListInvoceHeader2 = Business.Cms.GetInvoceHeader(filename, data, timestamp, ref intentos, serviceConfig.maxAttemps);
                                List<InvoiceDetail> ListInvoceDetail2 = Business.Cms.GetInvoceDetail(filename, data, timestamp);
                                ListInvoceHeader.AddRange(ListInvoceHeader2);
                                ListInvoceDetail.AddRange(ListInvoceDetail2);
                            }


                            Tools.Logging.Info("Inicio: Obtener configuración de correos electronicos - Facturas Cms");

                            Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);

                            if (configParameter != null)
                            {
                                mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                                Tools.Logging.Info("Inicio : Registrar Auditoria");

                                auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 5, Tools.Constants.NO_LEIDO, ListInvoceHeader.Count + ListInvoceDetail.Count, 1, serviceConfig.Norm);

                                if (auditId > 0)
                                {

                                    Tools.Logging.Info("Inicio : Validar Documentos ");

                                    isValid = Business.Cms.ValidateInvoices(ListInvoceHeader, ref validationMessage);
                                    isValid &= Business.Cms.ValidateInvoiceDetail(ListInvoceDetail, ref validationMessage);


                                    /*for(int i = 0; i < ListInvoceDetail.Count; i++)
                                    {
                                        if(!ListInvoceHeader.Exists(x => x.serieNumero == ListInvoceDetail[i].serieNumero))
                                        {
                                            ListInvoceDetail.RemoveAt(i);
                                        }
                                    }*/

                                    Tools.Logging.Info("Inicio : Notificación de Validación");

                                    if (!isValid)
                                    {
                                        Business.Common.SendFileNotification(mailConfig, validationMessage);
                                        //Business.Common.UpdateAudit(auditId, Tools.Constants.FALLA_VALIDACION, intentos);
                                    }

                                    Tools.Logging.Info("Inicio : Actualizo Auditoria");
                                    Business.Common.UpdateAudit(auditId, Tools.Constants.LEIDO, intentos);

                                    Tools.Logging.Info("Inicio : Insertar Documentos Validados ");
                                    Business.Common.BulkInsertListToTable(ListInvoceDetail, "Factura_Detalle");
                                    Business.Common.BulkInsertListToTable(ListInvoceHeader, "Factura_Cabecera");

                                    Tools.Logging.Info("Inicio : enviar GFiscal ");

                                    Parameters fileParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);
                                    FileServer fileServerConfigOut = Business.Common.GetParameterDeserialized<FileServer>(fileParameter);

                                    if (fileServerConfig != null)
                                    {
                                        string resultPath = "";
                                        if (serviceConfig.Norm == 340)
                                        {
                                            resultPath = Business.Cms.CreateInvoiceFile340(ListInvoceHeader, ListInvoceDetail, System.IO.Path.GetTempPath());

                                        }
                                        else
                                        {
                                            resultPath = Business.Cms.CreateInvoiceFile193(ListInvoceHeader, ListInvoceDetail, System.IO.Path.GetTempPath());
                                        }
                                        Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, System.IO.Path.GetFileName(resultPath), System.IO.File.ReadAllBytes(resultPath));

                                        Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");
                                        Business.Common.SendFileNotification(mailConfig, $"Se envió correctamenteel documento: {System.IO.Path.GetFileName(resultPath)} a gfiscal");
                                        Tools.Logging.Info("Inicio : Actualizo Auditoria");

                                        Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_GFISCAL, intentos);

                                        Tools.Logging.Info("Inicio :  Mover archivos procesados a ruta PROC ");
                                        foreach (string file in inputFilesFTP)
                                        {
                                            Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file, true, System.IO.Path.GetTempPath());
                                            Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory + "/PROC/", file, System.IO.File.ReadAllBytes(System.IO.Path.GetTempPath() + "/" + file));
                                            Tools.FileServer.DeleteFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file);
                                        };
                                        Tools.Logging.Info("Inicio : Mover archivos procesados a ruta PROC ");


                                        //Tools.Logging.Info("Inicio : Actualizar fecha de envio");
                                        //actualizar documento factura -> agregar el nombre archivo alignet,fechaenvio,
                                        //Business.Common.UpdateDocumentInvoice(System.IO.Path.GetFileName(resultPath), DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT));
                                        //Business.Pacifyc.UpdatePickUpDate(ListInvoceHeader);

                                    }
                                }
                                else
                                {
                                    Tools.Logging.Error($"No se pudo recuperar el id de auditoria - Facturas pacyfic");
                                    Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                                }
                            }
                            else
                            {
                                Tools.Logging.Error($"No se insertó en base de datos el parámetro con llave: {Tools.Constants.MAIL_CONFIG}");
                                //Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                                return;
                            }
                        }
                        else
                        {
                            Tools.Logging.Error($"No se insertó en base de datos el parámetro con llave: {Tools.Constants.KEY_CONFIG}");
                            //Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                            return;
                        }
                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Cms Lectura");
                        return;
                    }
                }
                else
                {
                    Tools.Logging.Info("No se encontraron archivos por procesar - Cms Lectura");
                    return;
                }
            }
            else
            {
                Tools.Logging.Error($"No se insertó en base de datos el parámetro con llave: {Tools.Constants.FTP_CONFIG_INPUT}");
                //Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                return;
            }
        }
        private void Bill(List<Parameters> oListParameters)
        {
            ServiceConfig serviceConfig;
            Mail mailConfig;
            FileServer fileServerConfig;
            bool isValid;
            string validationMessage = "";
            int auditId;
            int intentos = 0;
            DateTime timestamp = DateTime.Now;
            List<string> inputFilesFTP;
            List<List<string>> inputFiles = new List<List<string>>();

            Tools.Logging.Info("Inicio: Obtener parámetros para lectura de boletas");
            Parameters ftpParameterInput = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
            Tools.Logging.Info("Fin: Obtener parámetros para lectura de boletas");


            if (ftpParameterInput != null)
            {
                fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameterInput);

                inputFilesFTP = Tools.FileServer.ListDirectory(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory);

                if (inputFilesFTP.Count > 0)
                {
                    inputFilesFTP = inputFilesFTP.Where(x => x.StartsWith("BOLE_")).ToList();
                    if (inputFilesFTP.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Obtener norma para las boletas de Cms");
                        Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
                        Tools.Logging.Info("Fin: Obtener norma para las boletas de Cms");

                        if (configParameter != null)
                        {
                            serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);

                            Tools.Logging.Info("Inicio : Obtener documentos de FTP Cms - Boletas");

                            List<BillHeader> ListBillHeader = new List<BillHeader>();
                            List<BillDetail> ListBillDetail = new List<BillDetail>();

                            List<string> data;

                            foreach (string filename in inputFilesFTP)
                            {
                                data = Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, filename);
                                string serie = "";
                                for (int i = 0; i < data.Count(); i++)
                                {
                                    if (data[i].StartsWith("C"))
                                    {
                                        serie = data[i].Split('|')[1].Trim();
                                    }
                                    if (data[i].StartsWith("D"))
                                    {
                                        data[i] = serie + "|" + data[i];
                                    }
                                }

                                List<BillHeader> ListBillHeader2 = Business.Cms.GetBillHeader(filename, data, timestamp, ref intentos, serviceConfig.maxAttemps);
                                List<BillDetail> ListBillDetail2 = Business.Cms.GetBillDetail(filename, data, timestamp);
                                ListBillHeader.AddRange(ListBillHeader2);
                                ListBillDetail.AddRange(ListBillDetail2);
                            }


                            Tools.Logging.Info("Inicio: Obtener configuración de correos electronicos - Facturas Cms");

                            Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);

                            if (configParameter != null)
                            {
                                mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                                Tools.Logging.Info("Inicio : Registrar Auditoria");

                                auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 5, Tools.Constants.NO_LEIDO, ListBillHeader.Count + ListBillDetail.Count, 1, serviceConfig.Norm);

                                if (auditId > 0)
                                {

                                    Tools.Logging.Info("Inicio : Validar Documentos ");

                                    isValid = Business.Cms.ValidateBills(ListBillHeader, ref validationMessage);
                                    isValid &= Business.Cms.ValidateBillDetails(ListBillDetail, ref validationMessage);


                                    /*for(int i = 0; i < ListInvoceDetail.Count; i++)
                                    {
                                        if(!ListInvoceHeader.Exists(x => x.serieNumero == ListInvoceDetail[i].serieNumero))
                                        {
                                            ListInvoceDetail.RemoveAt(i);
                                        }
                                    }*/

                                    Tools.Logging.Info("Inicio : Notificación de Validación");

                                    if (!isValid)
                                    {
                                        Business.Common.SendFileNotification(mailConfig, validationMessage);
                                        //Business.Common.UpdateAudit(auditId, Tools.Constants.FALLA_VALIDACION, intentos);
                                    }

                                    Tools.Logging.Info("Inicio : Actualizo Auditoria");
                                    Business.Common.UpdateAudit(auditId, Tools.Constants.LEIDO, intentos);

                                    Tools.Logging.Info("Inicio : Insertar Documentos Validados ");
                                    Business.Common.BulkInsertListToTable(ListBillDetail, "Boleta_Detalle");
                                    Business.Common.BulkInsertListToTable(ListBillHeader, "Boleta_Cabecera");

                                    Tools.Logging.Info("Inicio : enviar GFiscal ");

                                    Parameters fileParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);
                                    FileServer fileServerConfigOut = Business.Common.GetParameterDeserialized<FileServer>(fileParameter);

                                    if (fileServerConfig != null)
                                    {
                                        string resultPath = "";
                                        if (serviceConfig.Norm == 340)
                                        {
                                            resultPath = Business.Cms.CreateBillFile340(ListBillHeader, ListBillDetail, System.IO.Path.GetTempPath());

                                        }
                                        else
                                        {
                                            //resultPath = Business.Pacifyc.CreateInvoiceFile193(ListInvoceHeader, ListInvoceDetail, System.IO.Path.GetTempPath());
                                        }
                                        Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, System.IO.Path.GetFileName(resultPath), System.IO.File.ReadAllBytes(resultPath));

                                        Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");
                                        Business.Common.SendFileNotification(mailConfig, $"Se envió correctamenteel documento: {System.IO.Path.GetFileName(resultPath)} a gfiscal");
                                        Tools.Logging.Info("Inicio : Actualizo Auditoria");

                                        Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_GFISCAL, intentos);

                                        Tools.Logging.Info("Inicio :  Mover archivos procesados a ruta PROC ");
                                        foreach (string file in inputFilesFTP)
                                        {
                                            Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file, true, System.IO.Path.GetTempPath());
                                            Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory + "/PROC/", file, System.IO.File.ReadAllBytes(System.IO.Path.GetTempPath() + "/" + file));
                                            Tools.FileServer.DeleteFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file);
                                        };
                                        Tools.Logging.Info("Inicio : Mover archivos procesados a ruta PROC ");


                                        //Tools.Logging.Info("Inicio : Actualizar fecha de envio");
                                        //actualizar documento factura -> agregar el nombre archivo alignet,fechaenvio,
                                        //Business.Common.UpdateDocumentInvoice(System.IO.Path.GetFileName(resultPath), DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT));
                                        //Business.Pacifyc.UpdatePickUpDate(ListInvoceHeader);

                                    }
                                }
                                else
                                {
                                    Tools.Logging.Error($"No se pudo recuperar el id de auditoria - Boletas Cms");
                                    Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                                }
                            }
                            else
                            {
                                Tools.Logging.Error($"No se insertó en base de datos el parámetro con llave: {Tools.Constants.MAIL_CONFIG}");
                                //Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                                return;
                            }
                        }
                        else
                        {
                            Tools.Logging.Error($"No se insertó en base de datos el parámetro con llave: {Tools.Constants.KEY_CONFIG}");
                            //Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                            return;
                        }
                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Cms Lectura");
                        return;
                    }
                }
                else
                {
                    Tools.Logging.Info("No se encontraron archivos por procesar - Cms Lectura");
                    return;
                }
            }
            else
            {
                Tools.Logging.Error($"No se insertó en base de datos el parámetro con llave: {Tools.Constants.FTP_CONFIG_INPUT}");
                //Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                return;
            }

        }
        private void CreditNote(List<Parameters> oListParameters)
        {
            ServiceConfig serviceConfig;
            Mail mailConfig;
            FileServer fileServerConfig;
            bool isValid;
            string validationMessage = "";
            int auditId;
            int intentos = 0;
            DateTime timestamp = DateTime.Now;
            List<string> inputFilesFTP;
            List<List<string>> inputFiles = new List<List<string>>();

            Tools.Logging.Info("Inicio: Obtener parámetros para lectura de notas de credito");
            Parameters ftpParameterInput = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
            Tools.Logging.Info("Fin: Obtener parámetros para lectura de notas de credito");


            if (ftpParameterInput != null)
            {
                fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameterInput);

                inputFilesFTP = Tools.FileServer.ListDirectory(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory);

                if (inputFilesFTP.Count > 0)
                {
                    inputFilesFTP = inputFilesFTP.Where(x => x.StartsWith("NDEB_")).ToList();
                    if (inputFilesFTP.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Obtener norma para las notas de credito de Cms");
                        Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
                        Tools.Logging.Info("Fin: Obtener norma para las notas de credito de Cms");

                        if (configParameter != null)
                        {
                            serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);

                            Tools.Logging.Info("Inicio : Obtener documentos de FTP Cms - Nota de credito");

                            List<CreditNoteHeader> ListInvoceHeader = new List<CreditNoteHeader>();
                            List<CreditNoteDetail> ListInvoceDetail = new List<CreditNoteDetail>();

                            List<string> data;

                            foreach (string filename in inputFilesFTP)
                            {
                                data = Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, filename);
                                string serie = "";
                                for (int i = 0; i < data.Count(); i++)
                                {
                                    if (data[i].StartsWith("C"))
                                    {
                                        serie = data[i].Split('|')[1].Trim();
                                    }
                                    if (data[i].StartsWith("D"))
                                    {
                                        data[i] = serie + "|" + data[i];
                                    }
                                }

                                List<CreditNoteHeader> ListInvoceHeader2 = Business.Cms.GetCreditNoteHeader(filename, data, timestamp, ref intentos, serviceConfig.maxAttemps);
                                List<CreditNoteDetail> ListInvoceDetail2 = Business.Cms.GetCreditNoteDetail(filename, data, timestamp);
                                ListInvoceHeader.AddRange(ListInvoceHeader2);
                                ListInvoceDetail.AddRange(ListInvoceDetail2);
                            }


                            Tools.Logging.Info("Inicio: Obtener configuración de correos electronicos - Nota credito Cms");

                            Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);

                            if (configParameter != null)
                            {
                                mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                                Tools.Logging.Info("Inicio : Registrar Auditoria");

                                auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 5, Tools.Constants.NO_LEIDO, ListInvoceHeader.Count + ListInvoceDetail.Count, 1, serviceConfig.Norm);

                                if (auditId > 0)
                                {

                                    Tools.Logging.Info("Inicio : Validar Documentos ");

                                    isValid = true;// Business.Cms.ValidateBills(ListInvoceHeader, ref validationMessage);
                                                   // isValid &= Business.Cms.ValidateBillDetails(ListInvoceDetail, ref validationMessage);


                                    /*for(int i = 0; i < ListInvoceDetail.Count; i++)
                                    {
                                        if(!ListInvoceHeader.Exists(x => x.serieNumero == ListInvoceDetail[i].serieNumero))
                                        {
                                            ListInvoceDetail.RemoveAt(i);
                                        }
                                    }*/

                                    Tools.Logging.Info("Inicio : Notificación de Validación");

                                    if (!isValid)
                                    {
                                        Business.Common.SendFileNotification(mailConfig, validationMessage);
                                        //Business.Common.UpdateAudit(auditId, Tools.Constants.FALLA_VALIDACION, intentos);
                                    }

                                    Tools.Logging.Info("Inicio : Actualizo Auditoria");
                                    Business.Common.UpdateAudit(auditId, Tools.Constants.LEIDO, intentos);

                                    Tools.Logging.Info("Inicio : Insertar Documentos Validados ");
                                    Business.Common.BulkInsertListToTable(ListInvoceDetail, "Notacredito_Detalle");
                                    Business.Common.BulkInsertListToTable(ListInvoceHeader, "Notacredito_Cabecera");

                                    Tools.Logging.Info("Inicio : enviar GFiscal ");

                                    Parameters fileParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);
                                    FileServer fileServerConfigOut = Business.Common.GetParameterDeserialized<FileServer>(fileParameter);

                                    if (fileServerConfig != null)
                                    {
                                        string resultPath = "";
                                        if (serviceConfig.Norm == 340)
                                        {
                                            resultPath = Business.Cms.CreateCreditNoteFile340(ListInvoceHeader, ListInvoceDetail, System.IO.Path.GetTempPath());

                                        }
                                        else
                                        {
                                            resultPath = Business.Cms.CreateCreditNoteFile193(ListInvoceHeader, ListInvoceDetail, System.IO.Path.GetTempPath());
                                        }
                                        Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, System.IO.Path.GetFileName(resultPath), System.IO.File.ReadAllBytes(resultPath));

                                        Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");
                                        Business.Common.SendFileNotification(mailConfig, $"Se envió correctamenteel documento: {System.IO.Path.GetFileName(resultPath)} a gfiscal");
                                        Tools.Logging.Info("Inicio : Actualizo Auditoria");

                                        Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_GFISCAL, intentos);

                                        Tools.Logging.Info("Inicio :  Mover archivos procesados a ruta PROC ");
                                        foreach (string file in inputFilesFTP)
                                        {
                                            Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file, true, System.IO.Path.GetTempPath());
                                            Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory + "/PROC/", file, System.IO.File.ReadAllBytes(System.IO.Path.GetTempPath() + "/" + file));
                                            Tools.FileServer.DeleteFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file);
                                        };
                                        Tools.Logging.Info("Inicio : Mover archivos procesados a ruta PROC ");


                                        //Tools.Logging.Info("Inicio : Actualizar fecha de envio");
                                        //actualizar documento factura -> agregar el nombre archivo alignet,fechaenvio,
                                        //Business.Common.UpdateDocumentInvoice(System.IO.Path.GetFileName(resultPath), DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT));
                                        //Business.Pacifyc.UpdatePickUpDate(ListInvoceHeader);

                                    }
                                }
                                else
                                {
                                    Tools.Logging.Error($"No se pudo recuperar el id de auditoria - notacredito Cms");
                                    Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                                }
                            }
                            else
                            {
                                Tools.Logging.Error($"No se insertó en base de datos el parámetro con llave: {Tools.Constants.MAIL_CONFIG}");
                                //Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                                return;
                            }
                        }
                        else
                        {
                            Tools.Logging.Error($"No se insertó en base de datos el parámetro con llave: {Tools.Constants.KEY_CONFIG}");
                            //Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                            return;
                        }
                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Cms Lectura");
                        return;
                    }
                }
                else
                {
                    Tools.Logging.Info("No se encontraron archivos por procesar - Cms Lectura");
                    return;
                }
            }
            else
            {
                Tools.Logging.Error($"No se insertó en base de datos el parámetro con llave: {Tools.Constants.FTP_CONFIG_INPUT}");
                //Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                return;
            }
        }
        private void DebitNote(List<Parameters> oListParameters)
        {
            ServiceConfig serviceConfig;
            Mail mailConfig;
            FileServer fileServerConfig;
            bool isValid;
            string validationMessage = "";
            int auditId;
            int intentos = 0;
            DateTime timestamp = DateTime.Now;
            List<string> inputFilesFTP;
            List<List<string>> inputFiles = new List<List<string>>();

            Tools.Logging.Info("Inicio: Obtener parámetros para lectura de notas de debito");
            Parameters ftpParameterInput = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
            Tools.Logging.Info("Fin: Obtener parámetros para lectura de notas de debito");


            if (ftpParameterInput != null)
            {
                fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameterInput);

                inputFilesFTP = Tools.FileServer.ListDirectory(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory);

                if (inputFilesFTP.Count > 0)
                {
                    inputFilesFTP = inputFilesFTP.Where(x => x.StartsWith("NDEB_")).ToList();
                    if (inputFilesFTP.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Obtener norma para las notas de debito de Cms");
                        Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
                        Tools.Logging.Info("Fin: Obtener norma para las notas de debito de Cms");

                        if (configParameter != null)
                        {
                            serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);

                            Tools.Logging.Info("Inicio : Obtener documentos de FTP Cms - Nota de Debito");

                            List<DebitNoteHeader> ListInvoceHeader = new List<DebitNoteHeader>();
                            List<DebitNoteDetail> ListInvoceDetail = new List<DebitNoteDetail>();

                            List<string> data;

                            foreach (string filename in inputFilesFTP)
                            {
                                data = Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, filename);
                                string serie = "";
                                for (int i = 0; i < data.Count(); i++)
                                {
                                    if (data[i].StartsWith("C"))
                                    {
                                        serie = data[i].Split('|')[1].Trim();
                                    }
                                    if (data[i].StartsWith("D"))
                                    {
                                        data[i] = serie + "|" + data[i];
                                    }
                                }

                                List<DebitNoteHeader> ListInvoceHeader2 = Business.Cms.GetDebitNoteHeader(filename, data, timestamp, ref intentos, serviceConfig.maxAttemps);
                                List<DebitNoteDetail> ListInvoceDetail2 = Business.Cms.GetDebitNoteDetail(filename, data, timestamp);
                                ListInvoceHeader.AddRange(ListInvoceHeader2);
                                ListInvoceDetail.AddRange(ListInvoceDetail2);
                            }


                            Tools.Logging.Info("Inicio: Obtener configuración de correos electronicos - Nota Debito Cms");

                            Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);

                            if (configParameter != null)
                            {
                                mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                                Tools.Logging.Info("Inicio : Registrar Auditoria");

                                auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 5, Tools.Constants.NO_LEIDO, ListInvoceHeader.Count + ListInvoceDetail.Count, 1, serviceConfig.Norm);

                                if (auditId > 0)
                                {

                                    Tools.Logging.Info("Inicio : Validar Documentos ");

                                    isValid = true;// Business.Cms.ValidateBills(ListInvoceHeader, ref validationMessage);
                                                   // isValid &= Business.Cms.ValidateBillDetails(ListInvoceDetail, ref validationMessage);


                                    /*for(int i = 0; i < ListInvoceDetail.Count; i++)
                                    {
                                        if(!ListInvoceHeader.Exists(x => x.serieNumero == ListInvoceDetail[i].serieNumero))
                                        {
                                            ListInvoceDetail.RemoveAt(i);
                                        }
                                    }*/

                                    Tools.Logging.Info("Inicio : Notificación de Validación");

                                    if (!isValid)
                                    {
                                        Business.Common.SendFileNotification(mailConfig, validationMessage);
                                        //Business.Common.UpdateAudit(auditId, Tools.Constants.FALLA_VALIDACION, intentos);
                                    }

                                    Tools.Logging.Info("Inicio : Actualizo Auditoria");
                                    Business.Common.UpdateAudit(auditId, Tools.Constants.LEIDO, intentos);

                                    Tools.Logging.Info("Inicio : Insertar Documentos Validados ");
                                    Business.Common.BulkInsertListToTable(ListInvoceDetail, "NotaDebito_Detalle");
                                    Business.Common.BulkInsertListToTable(ListInvoceHeader, "NotaDebito_Cabecera");

                                    Tools.Logging.Info("Inicio : enviar GFiscal ");

                                    Parameters fileParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);
                                    FileServer fileServerConfigOut = Business.Common.GetParameterDeserialized<FileServer>(fileParameter);

                                    if (fileServerConfig != null)
                                    {
                                        string resultPath = "";
                                        if (serviceConfig.Norm == 340)
                                        {
                                            resultPath = Business.Cms.CreateDebitNoteFile340(ListInvoceHeader, ListInvoceDetail, System.IO.Path.GetTempPath());

                                        }
                                        else
                                        {
                                            //resultPath = Business.Pacifyc.CreateInvoiceFile193(ListInvoceHeader, ListInvoceDetail, System.IO.Path.GetTempPath());
                                        }
                                        Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, System.IO.Path.GetFileName(resultPath), System.IO.File.ReadAllBytes(resultPath));

                                        Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");
                                        Business.Common.SendFileNotification(mailConfig, $"Se envió correctamenteel documento: {System.IO.Path.GetFileName(resultPath)} a gfiscal");
                                        Tools.Logging.Info("Inicio : Actualizo Auditoria");

                                        Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_GFISCAL, intentos);

                                        Tools.Logging.Info("Inicio :  Mover archivos procesados a ruta PROC ");
                                        foreach (string file in inputFilesFTP)
                                        {
                                            Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file, true, System.IO.Path.GetTempPath());
                                            Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory + "/PROC/", file, System.IO.File.ReadAllBytes(System.IO.Path.GetTempPath() + "/" + file));
                                            Tools.FileServer.DeleteFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file);
                                        };
                                        Tools.Logging.Info("Inicio : Mover archivos procesados a ruta PROC ");


                                        //Tools.Logging.Info("Inicio : Actualizar fecha de envio");
                                        //actualizar documento factura -> agregar el nombre archivo alignet,fechaenvio,
                                        //Business.Common.UpdateDocumentInvoice(System.IO.Path.GetFileName(resultPath), DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT));
                                        //Business.Pacifyc.UpdatePickUpDate(ListInvoceHeader);

                                    }
                                }
                                else
                                {
                                    Tools.Logging.Error($"No se pudo recuperar el id de auditoria - Facturas Cms");
                                    Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                                }
                            }
                            else
                            {
                                Tools.Logging.Error($"No se insertó en base de datos el parámetro con llave: {Tools.Constants.MAIL_CONFIG}");
                                //Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                                return;
                            }
                        }
                        else
                        {
                            Tools.Logging.Error($"No se insertó en base de datos el parámetro con llave: {Tools.Constants.KEY_CONFIG}");
                            //Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                            return;
                        }
                    }
                    else
                    {
                        Tools.Logging.Info("No se encontraron archivos por procesar - Cms Lectura");
                        return;
                    }
                }
                else
                {
                    Tools.Logging.Info("No se encontraron archivos por procesar - Cms Lectura");
                    return;
                }
            }
            else
            {
                Tools.Logging.Error($"No se insertó en base de datos el parámetro con llave: {Tools.Constants.FTP_CONFIG_INPUT}");
                //Business.Common.UpdateAudit(auditId, Tools.Constants.ERROR_FECENTRALIZADA, intentos);
                return;
            }
        }

        protected override void OnStop()
        {
        }
    }
}
