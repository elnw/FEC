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
using TM.FECentralizada.Entities.Sap;
namespace TM.FECentralizada.Sap.Read
{
    public partial class SapRead : ServiceBase
    {
        Timer oTimer = new Timer();
        public SapRead()
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

        public void Test()
        {
            Procedure();
        }

        private void Procedure()
        {
            try
            {
                Tools.Logging.Info("Inicio del Proceso: Lectura Sap.");

                Tools.Logging.Info("Inicio : Obtener Parámetros");
                //Método que Obtendrá los Parámetros.
                List<Parameters> ParamsResponse = TM.FECentralizada.Business.Common.GetParametersByKey(new Parameters() { Domain = Tools.Constants.SapRead, KeyDomain = "", KeyParam = "" });
                Tools.Logging.Info("Fin : Obtener Parámetros");

                if (ParamsResponse != null && ParamsResponse.Any())
                {
                    List<Parameters> ParametersInvoce = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.SapRead_Invoice.ToUpper())).ToList();
                    List<Parameters> ParametersBill = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.SapRead_Bill.ToUpper())).ToList();
                    List<Parameters> ParametersCreditNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.SapRead_CreditNote.ToUpper())).ToList();
                    List<Parameters> ParametersDebitNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals(Tools.Constants.SapRead_DebitNote.ToUpper())).ToList();

                    Invoice(ParametersInvoce);
                    //Bill(ParametersBill);
                    Tools.Logging.Info("Inicio : Procesar documentos de Archivos Sap");
                    //Parallel.Invoke(
                    //           () => Invoice(ParametersInvoce),
                    //           () => Bill(ParametersBill)
                    //           //() => CreditNote(ParametersCreditNote),
                    //           //() => DebitNote(ParametersDebitNote)
                    //    );
                    Tools.Logging.Info("Fin : Procesar documentos de Archivos Sap");

                    //Obtengo la Configuración Intervalo de Tiempo -ARREGLAR ESTO
                    var oConfiguration = ParamsResponse.Find(x => x.KeyParam.Equals("TimerInterval"));
                    var Minutes = oConfiguration.Value;//oConfiguration.Key3.Equals("D") ? oConfiguration.Value3 : oConfiguration.Key3.Equals("T") ? oConfiguration.Value2 : oConfiguration.Value1;
                    oTimer.Interval = Tools.Common.ConvertMinutesToMilliseconds(int.Parse(Minutes));
                    oTimer.Start();
                    oTimer.AutoReset = true;
                }
                else
                {
                    Tools.Logging.Error("Ocurrió un error al obtener la configuración para Sap.");
                }
                Tools.Logging.Info("Fin del Proceso: Lectura Sap.");
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
            List<string> validationMessage = new List<string>();
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
                    inputFilesFTP = inputFilesFTP.Where(x => x.StartsWith("FACT_2")).ToList();
                    if (inputFilesFTP.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Obtener norma para las facturas de SAP");
                        Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
                        Tools.Logging.Info("Fin: Obtener norma para las facturas de SAP");

                        if (configParameter != null)
                        {
                            serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);

                            Tools.Logging.Info("Inicio : Obtener documentos de FTP SAP - Facturas");

                            List<InvoiceHeader> ListInvoceHeader = new List<InvoiceHeader>();
                            List<InvoiceDetail> ListInvoceDetail = new List<InvoiceDetail>();

                            var invoices = Business.Sap.GetInvoices(inputFilesFTP,fileServerConfig, ref intentos, serviceConfig.maxAttemps, timestamp);

                            ListInvoceHeader = invoices.Item1;
                            ListInvoceDetail = invoices.Item2;


                            Tools.Logging.Info("Inicio: Obtener configuración de correos electronicos - Facturas Sap");

                            Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);

                            if (configParameter != null)
                            {
                                mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                                Tools.Logging.Info("Inicio : Registrar Auditoria");

                                auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 3, Tools.Constants.NO_LEIDO, ListInvoceHeader.Count + ListInvoceDetail.Count, 1, serviceConfig.Norm);

                                if (auditId > 0)
                                {

                                    Tools.Logging.Info("Inicio : Validar Documentos ");

                                    isValid = Business.Sap.ValidateInvoices(ListInvoceHeader, validationMessage);
                                    isValid &= Business.Sap.ValidateInvoiceDetail(ListInvoceDetail, validationMessage);

                                    ListInvoceDetail.RemoveAll(x => !ListInvoceHeader.Select(y => y.serieNumero).Contains(x.serieNumero));


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
                                            resultPath = Business.Sap.CreateInvoiceFile340(ListInvoceHeader, ListInvoceDetail, System.IO.Path.GetTempPath());

                                        }
                                        else
                                        {
                                            resultPath = Business.Sap.CreateInvoiceFile193(ListInvoceHeader, ListInvoceDetail, System.IO.Path.GetTempPath());
                                        }
                                        Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, System.IO.Path.GetFileName(resultPath), System.IO.File.ReadAllBytes(resultPath));

                                        Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");
                                        Business.Common.SendFileNotification(mailConfig, $"Se envió correctamente el documento: {System.IO.Path.GetFileName(resultPath)} a gfiscal");
                                        Tools.Logging.Info("Inicio : Actualizo Auditoria");

                                        Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_GFISCAL, intentos);

                                        //Tools.Logging.Info("Inicio : Actualizar fecha de envio");
                                        //actualizar documento factura -> agregar el nombre archivo alignet,fechaenvio,
                                        Business.Common.UpdateDocumentInvoice(System.IO.Path.GetFileName(resultPath), DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), $"'{String.Join("','", ListInvoceHeader.Select(x => x.serieNumero))}'");
                                        Business.Sap.UpdatePickUpDate(inputFilesFTP, fileServerConfig);

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
                        Tools.Logging.Info("No se encontraron archivos por procesar - Sap Lectura");
                        return;
                    }
                }
                else
                {
                    Tools.Logging.Info("No se encontraron archivos por procesar - Sap Lectura");
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
                        Tools.Logging.Info("Inicio: Obtener norma para las boletas de Atis");
                        Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
                        Tools.Logging.Info("Fin: Obtener norma para las boletas de Atis");

                        if (configParameter != null)
                        {
                            serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);

                            Tools.Logging.Info("Inicio : Obtener documentos de FTP Sap - Boletas");

                            List<BillHeader> ListBillHeader = new List<BillHeader>();
                            List<BillDetail> ListBillDetail = new List<BillDetail>();


                            var billDocuments = Business.Sap.GetBills(inputFilesFTP, fileServerConfig, ref intentos, serviceConfig.maxAttemps, timestamp);

                            ListBillHeader = billDocuments.Item1;
                            ListBillDetail = billDocuments.Item2;

                            Tools.Logging.Info("Inicio: Obtener configuración de correos electronicos - Boletas Sap");

                            Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);

                            if (configParameter != null)
                            {
                                mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                                Tools.Logging.Info("Inicio : Registrar Auditoria");

                                auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 3, Tools.Constants.NO_LEIDO, ListBillHeader.Count + ListBillDetail.Count, 1, serviceConfig.Norm);

                                if (auditId > 0)
                                {

                                    Tools.Logging.Info("Inicio : Validar Documentos ");
                                    isValid = true;
                                    isValid = Business.Sap.ValidateBills(ListBillHeader, ref validationMessage);
                                    isValid &= Business.Sap.ValidateBillDetails(ListBillDetail, ref validationMessage);


                                    //eliminar
                                    ListBillDetail.RemoveAll(x => !ListBillHeader.Select(y => y.serieNumero).Contains(x.serieNumero));
                                    ListBillHeader.RemoveAll(x => !ListBillDetail.Select(y => y.serieNumero).Contains(x.serieNumero));

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
                                            resultPath = Business.Sap.CreateBillFile340(ListBillHeader, ListBillDetail, System.IO.Path.GetTempPath());

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


                                    }
                                }
                                else
                                {
                                    Tools.Logging.Error($"No se pudo recuperar el id de auditoria - Boletas Sap");
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
                        Tools.Logging.Info("No se encontraron archivos por procesar - Sap Boleta");
                        return;
                    }
                }
                else
                {
                    Tools.Logging.Info("No se encontraron archivos por procesar - Sap Lectura");
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
            List<string> validationMessage = new List<string>();
            int auditId;
            int intentos = 0;
            DateTime timestamp = DateTime.Now;
            List<string> inputFilesFTP;
            List<List<string>> inputFiles = new List<List<string>>();

            Tools.Logging.Info("Inicio: Obtener parámetros para lectura de notas de crédito - Sap");
            Parameters ftpParameterInput = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_INPUT);
            Tools.Logging.Info("Fin: Obtener parámetros para lectura de notas de crédito - Sap");


            if (ftpParameterInput != null)
            {
                fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(ftpParameterInput);

                inputFilesFTP = Tools.FileServer.ListDirectory(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory);

                if (inputFilesFTP.Count > 0)
                {
                    inputFilesFTP = inputFilesFTP.Where(x => x.StartsWith("NCRE_")).ToList();
                    if (inputFilesFTP.Count > 0)
                    {
                        Tools.Logging.Info("Inicio: Obtener norma para las notas de crédito de Sap");
                        Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
                        Tools.Logging.Info("Fin: Obtener norma para las notas de crédito de Sap");

                        if (configParameter != null)
                        {
                            serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);

                            Tools.Logging.Info("Inicio : Obtener documentos de FTP Sap - Notas de crédito");

                            List<CreditNoteHeader> ListCreditNoteHeader = new List<CreditNoteHeader>();
                            List<CreditNoteDetail> ListCreditNoteDetail = new List<CreditNoteDetail>();

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

                                List<CreditNoteHeader> ListInvoceHeader2 = Business.Sap.GetCreditNoteHeader(filename, data, timestamp, ref intentos, serviceConfig.maxAttemps);
                                List<CreditNoteDetail> ListInvoceDetail2 = Business.Sap.GetCreditNoteDetail(filename, data, timestamp);
                                ListCreditNoteHeader.AddRange(ListInvoceHeader2);
                                ListCreditNoteDetail.AddRange(ListInvoceDetail2);
                            }


                            Tools.Logging.Info("Inicio: Obtener configuración de correos electronicos - Facturas Sap");

                            Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);

                            if (configParameter != null)
                            {
                                mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                                Tools.Logging.Info("Inicio : Registrar Auditoria");

                                auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 3, Tools.Constants.NO_LEIDO, ListCreditNoteHeader.Count + ListCreditNoteDetail.Count, 1, serviceConfig.Norm);

                                if (auditId > 0)
                                {

                                    Tools.Logging.Info("Inicio : Validar Documentos ");

                                    isValid = Business.Sap.CheckCreditNoteHeaders(ListCreditNoteHeader, validationMessage);

                                    ListCreditNoteDetail.RemoveAll(x => !ListCreditNoteHeader.Select(y => y.serieNumero).Contains(x.serieNumero));


                                    Tools.Logging.Info("Inicio : Notificación de Validación");

                                    if (!isValid)
                                    {
                                        Business.Common.SendFileNotification(mailConfig, validationMessage);
                                        //Business.Common.UpdateAudit(auditId, Tools.Constants.FALLA_VALIDACION, intentos);
                                    }

                                    Tools.Logging.Info("Inicio : Actualizo Auditoria");
                                    Business.Common.UpdateAudit(auditId, Tools.Constants.LEIDO, intentos);

                                    Tools.Logging.Info("Inicio : Insertar Documentos Validados ");
                                    Business.Common.BulkInsertListToTable(ListCreditNoteDetail, "Nota_Credito_Detalle");
                                    Business.Common.BulkInsertListToTable(ListCreditNoteHeader, "Nota_Credito_Cabecera");

                                    Tools.Logging.Info("Inicio : enviar GFiscal ");

                                    Parameters fileParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);
                                    FileServer fileServerConfigOut = Business.Common.GetParameterDeserialized<FileServer>(fileParameter);

                                    if (fileServerConfig != null)
                                    {
                                        string resultPath = "";
                                        if (serviceConfig.Norm == 340)
                                        {
                                            resultPath = Business.Sap.CreateCreditNoteFile340(ListCreditNoteHeader, ListCreditNoteDetail, System.IO.Path.GetTempPath());

                                        }
                                        else
                                        {
                                            resultPath = Business.Sap.CreateCreditNoteFile193(ListCreditNoteHeader, ListCreditNoteDetail, System.IO.Path.GetTempPath());
                                        }
                                        Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, System.IO.Path.GetFileName(resultPath), System.IO.File.ReadAllBytes(resultPath));

                                        Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");
                                        Business.Common.SendFileNotification(mailConfig, $"Se envió correctamente el documento: {System.IO.Path.GetFileName(resultPath)} a gfiscal");
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


                                    }
                                }
                                else
                                {
                                    Tools.Logging.Error($"No se pudo recuperar el id de auditoria - Facturas Sap");
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
                        Tools.Logging.Info("No se encontraron archivos por procesar - Sap Lectura");
                        return;
                    }
                }
                else
                {
                    Tools.Logging.Info("No se encontraron archivos por procesar - Sap Lectura");
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
            List<string> validationMessages = new List<string>();
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
                        Tools.Logging.Info("Inicio: Obtener norma para las notas de debito de Sap");
                        Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);
                        Tools.Logging.Info("Fin: Obtener norma para las notas de debito de Sap");

                        if (configParameter != null)
                        {
                            serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);

                            Tools.Logging.Info("Inicio : Obtener documentos de FTP Sap - Nota de Debito");

                            List<DebitNoteHeader> ListDebitNoteHeader = new List<DebitNoteHeader>();
                            List<DebitNoteDetail> ListDebitNoteDetail = new List<DebitNoteDetail>();

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

                                List<DebitNoteHeader> ListInvoceHeader2 = Business.Sap.GetDebitNoteHeader(filename, data, timestamp, ref intentos, serviceConfig.maxAttemps);
                                List<DebitNoteDetail> ListInvoceDetail2 = Business.Sap.GetDebitNoteDetail(filename, data, timestamp);
                                ListDebitNoteHeader.AddRange(ListInvoceHeader2);
                                ListDebitNoteDetail.AddRange(ListInvoceDetail2);
                            }


                            Tools.Logging.Info("Inicio: Obtener configuración de correos electronicos - Facturas Sap");

                            Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);

                            if (configParameter != null)
                            {
                                mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                                Tools.Logging.Info("Inicio : Registrar Auditoria");

                                auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 3, Tools.Constants.NO_LEIDO, ListDebitNoteHeader.Count + ListDebitNoteDetail.Count, 1, serviceConfig.Norm);

                                if (auditId > 0)
                                {

                                    Tools.Logging.Info("Inicio : Validar Documentos ");

                                    isValid = Business.Sap.CheckDebitNotes(ListDebitNoteHeader, validationMessages);

                                    ListDebitNoteDetail.RemoveAll(x => !ListDebitNoteHeader.Select(y => y.serieNumero).Contains(x.serieNumero));

                                    Tools.Logging.Info("Inicio : Notificación de Validación");

                                    if (!isValid)
                                    {
                                        Business.Common.SendFileNotification(mailConfig, validationMessage);
                                    }

                                    Tools.Logging.Info("Inicio : Actualizo Auditoria");
                                    Business.Common.UpdateAudit(auditId, Tools.Constants.LEIDO, intentos);

                                    Tools.Logging.Info("Inicio : Insertar Documentos Validados ");
                                    Business.Common.BulkInsertListToTable(ListDebitNoteDetail, "Nota_Debito_Detalle");
                                    Business.Common.BulkInsertListToTable(ListDebitNoteHeader, "Nota_Debito_Cabecera");

                                    Tools.Logging.Info("Inicio : enviar GFiscal ");

                                    Parameters fileParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);
                                    FileServer fileServerConfigOut = Business.Common.GetParameterDeserialized<FileServer>(fileParameter);

                                    if (fileServerConfig != null)
                                    {
                                        string resultPath = "";
                                        if (serviceConfig.Norm == 340)
                                        {
                                            resultPath = Business.Sap.CreateDebitNoteFile340(ListDebitNoteHeader, ListDebitNoteDetail, System.IO.Path.GetTempPath());

                                        }
                                        else
                                        {
                                             resultPath = Business.Sap.CreateDebitNoteFile193(ListDebitNoteHeader, ListDebitNoteDetail, System.IO.Path.GetTempPath());
                                        }

                                        Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, System.IO.Path.GetFileName(resultPath), System.IO.File.ReadAllBytes(resultPath));

                                        Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");
                                        Business.Common.SendFileNotification(mailConfig, $"Se envió correctamente el documento: {System.IO.Path.GetFileName(resultPath)} a gfiscal");
                                        Tools.Logging.Info("Inicio : Actualizo Auditoria");

                                        Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_GFISCAL, intentos);

                                        Tools.Logging.Info("Inicio :  Mover archivos procesados a ruta PROC ");
                                        foreach (string file in inputFilesFTP)
                                        {
                                            Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file, true, System.IO.Path.GetTempPath());
                                            Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory + "/PROC/", file, System.IO.File.ReadAllBytes(System.IO.Path.GetTempPath() + "/" + file));
                                            Tools.FileServer.DeleteFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file);
                                        }
                                        Tools.Logging.Info("Inicio : Mover archivos procesados a ruta PROC ");

                                    }
                                }
                                else
                                {
                                    Tools.Logging.Error($"No se pudo recuperar el id de auditoria - Facturas Sap");
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
                        Tools.Logging.Info("No se encontraron archivos por procesar - Sap Lectura");
                        return;
                    }
                }
                else
                {
                    Tools.Logging.Info("No se encontraron archivos por procesar - Sap Lectura");
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
            oTimer.Stop();
        }
    }
}
