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
using TM.FECentralizada.Entities.Isis;
using Common = TM.FECentralizada.Entities.Common;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace TM.FECentralizada.Isis.Read
{
    public partial class IsisRead : ServiceBase
    {
        Timer oTimer = new Timer();
        public IsisRead()
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
                Tools.Logging.Info("Inicio del Proceso: Lectura Isis.");

                Tools.Logging.Info("Inicio : Obtener Parámetros");
                //Método que Obtendrá los Parámetros.
                List<Parameters> ParamsResponse = TM.FECentralizada.Business.Common.GetParametersByKey(new Parameters() { Domain = Tools.Constants.IsisRead, KeyDomain = "", KeyParam = "" });
                Tools.Logging.Info("Fin : Obtener Parámetros");

                if (ParamsResponse != null && ParamsResponse.Any())
                {
                    List<Parameters> ParametersInvoce = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals( Tools.Constants.IsisRead_Bill.ToUpper())).ToList();
                    List<Parameters> ParametersBill = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals( Tools.Constants.IsisRead_Bill.ToUpper())).ToList();
                    List<Parameters> ParametersCreditNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals( Tools.Constants.IsisRead_Bill.ToUpper())).ToList();
                    List<Parameters> ParametersDebitNote = ParamsResponse.FindAll(x => x.KeyDomain.ToUpper().Equals( Tools.Constants.IsisRead_Bill.ToUpper())).ToList();

                    Tools.Logging.Info("Inicio : Procesar documentos de BD Isis");
                    //Invoice(ParametersInvoce);
                    Parallel.Invoke(
                               () => Invoice(ParametersInvoce),
                               () => Bill(ParametersBill),
                               () => CreditNote(ParametersCreditNote),
                               () => DebitNote(ParametersDebitNote)
                        );
                    Tools.Logging.Info("Fin : Procesar documentos de BD Isis");

                    //Obtengo la Configuración Intervalo de Tiempo
                    var oConfiguration = Business.Common.GetParameterDeserialized<ServiceConfig>(ParamsResponse.Find(x => x.KeyParam == Tools.Constants.KEY_CONFIG));

                    var Minutes = oConfiguration.ExecutionRate == 0 ? 10 : oConfiguration.ExecutionRate;//oConfiguration.Key3.Equals("D") ? oConfiguration.Value3 : oConfiguration.Key3.Equals("T") ? oConfiguration.Value2 : oConfiguration.Value1;
                    oTimer.Interval = Tools.Common.ConvertMinutesToMilliseconds(Minutes);
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

        private void Invoice(List<Parameters> oListParameters) {
            //
            ServiceConfig serviceConfig;
            Mail mailConfig;
            FileServer fileServerConfig;
            bool isValid;
            List<string> validationMessage = new List<string>();
            int auditId;
            int intentos = 0;
            DateTime timestamp = DateTime.Now;



            Tools.Logging.Info("Inicio: Obtener norma para las facturas de Isis");

            Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);

            if (configParameter != null)
            {
                serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);

                Tools.Logging.Info("Inicio : Obtener documentos de BD Isis - Facturas");
                List<InvoiceHeader> ListInvoceHeader = Business.Isis.GetInvoceHeader(timestamp, ref intentos, serviceConfig.maxAttemps);
                List<InvoiceDetail> ListInvoceDetail = Business.Isis.GetInvoceDetail(timestamp);


                Tools.Logging.Info("Inicio: Obtener configuración de correos electronicos - Facturas Isis");

                Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);

                if (mailParameter != null)
                {


                    Tools.Logging.Info("Inicio : Registrar Auditoria");

                    auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, ListInvoceHeader.Count * ListInvoceDetail.Count, 1, serviceConfig.Norm);

                    if (auditId > 0)
                    {

                        Tools.Logging.Info("Inicio : Validar Documentos ");

                        isValid = Business.Isis.ValidateInvoices(ListInvoceHeader, validationMessage);
                        isValid &= Business.Isis.ValidateInvoiceDetail(ListInvoceDetail, validationMessage);

                        ListInvoceDetail.RemoveAll(x => !ListInvoceHeader.Select(y => y.serieNumero).Contains(x.serieNumero));


                        Tools.Logging.Info("Inicio : Notificación de Validación");

                        if (!isValid)
                        {
                            if (mailParameter != null)
                            {
                                mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);
                                Business.Common.SendFileNotification(mailConfig, validationMessage);
                                //Business.Common.UpdateAudit(auditId, Tools.Constants.FALLA_VALIDACION, intentos);
                            }
                        }

                        Tools.Logging.Info("Inicio : Actualizo Auditoria");
                        Business.Common.UpdateAudit(auditId, Tools.Constants.LEIDO, intentos);

                        if (ListInvoceHeader.Count() > 0)
                        {
                            Tools.Logging.Info("Inicio : Insertar Documentos Validados ");
                            Business.Common.BulkInsertListToTable(ListInvoceDetail, "Factura_Detalle");
                            Business.Common.BulkInsertListToTable(ListInvoceHeader, "Factura_Cabecera");
                        }

                        Tools.Logging.Info("Inicio : enviar GFiscal ");

                        Parameters fileParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG);
                        fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(fileParameter);

                        if (fileServerConfig != null && ListInvoceHeader.Count() > 0)
                        {
                            string resultPath = "";
                            if (serviceConfig.Norm == 340)
                            {
                                resultPath = Business.Isis.CreateInvoiceFile340(ListInvoceHeader, ListInvoceDetail, System.IO.Path.GetTempPath());

                            }
                            else
                            {
                                resultPath = Business.Isis.CreateInvoiceFile193(ListInvoceHeader, ListInvoceDetail, System.IO.Path.GetTempPath());
                            }
                            Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, System.IO.Path.GetFileName(resultPath), System.IO.File.ReadAllBytes(resultPath));

                            if (mailParameter != null)
                            {
                                mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);
                                Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");
                                Business.Common.SendFileNotification(mailConfig, $"Se envió correctamente el documento: {System.IO.Path.GetFileName(resultPath)} a gfiscal");
                                Tools.Logging.Info("Inicio : Actualizo Auditoria");
                            }

                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_GFISCAL, intentos);
                            Tools.Logging.Info("Inicio : Actualizar fecha de envio");
                            //actualizar documento factura -> agregar el nombre archivo alignet,fechaenvio,

                            /*Business.Common.UpdateDocumentDebitNote(System.IO.Path.GetFileName(resultPath), DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT));
                            Business.Isis.UpdatePickUpDate(ListInvoceHeader);*/

                            Business.Common.UpdateDocumentInvoice(System.IO.Path.GetFileName(resultPath), DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), "'"+String.Join("','", ListInvoceHeader.Select(x=>x.serieNumero))+ "'");
                            Business.Isis.UpdateInvoicePickUpDate(ListInvoceHeader);


                        }





                    }
                    else
                    {
                        Tools.Logging.Error($"No se pudo recuperar el id de auditoria - Facturas Isis");
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
        private void Bill(List<Parameters> oListParameters)
        {
            //
            ServiceConfig serviceConfig;
            Mail mailConfig;
            FileServer fileServerConfig;
            bool isValid;
            List<string> validationMessage = new List<string>();
            int auditId;
            int intentos = 0;
            DateTime timestamp = DateTime.Now;



            Tools.Logging.Info("Inicio: Obtener norma para las Boletas de Isis");

            Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);

            if (configParameter != null)
            {
                serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);

                Tools.Logging.Info("Inicio : Obtener documentos de BD Isis - Boletas");
                List<BillHeader> ListBillHeader = Business.Isis.GetBillHeader(timestamp, ref intentos, serviceConfig.maxAttemps);
                List<BillDetail> ListBillDetail = Business.Isis.GetBillDetail(timestamp);


                Tools.Logging.Info("Inicio: Obtener configuración de correos electronicos - Boletas Isis");

                Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);

                if (mailParameter != null)
                {


                    Tools.Logging.Info("Inicio : Registrar Auditoria");

                    auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, ListBillHeader.Count * ListBillDetail.Count, 1, serviceConfig.Norm);

                    if (auditId > 0)
                    {

                        Tools.Logging.Info("Inicio : Validar Documentos ");

                        isValid = Business.Isis.ValidateBills(ListBillHeader, validationMessage);
                        isValid &= Business.Isis.ValidateBillDetail(ListBillDetail, validationMessage);

                        ListBillDetail.RemoveAll(x => !ListBillHeader.Select(y => y.serieNumero).Contains(x.serieNumero));


                        Tools.Logging.Info("Inicio : Notificación de Validación");

                        if (!isValid)
                        {
                            if (mailParameter != null)
                            {
                                mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);
                                Business.Common.SendFileNotification(mailConfig, validationMessage);
                                //Business.Common.UpdateAudit(auditId, Tools.Constants.FALLA_VALIDACION, intentos);
                            }
                        }

                        Tools.Logging.Info("Inicio : Actualizo Auditoria");
                        Business.Common.UpdateAudit(auditId, Tools.Constants.LEIDO, intentos);

                        if (ListBillHeader.Count() > 0)
                        {
                            Tools.Logging.Info("Inicio : Insertar Documentos Validados ");
                            Business.Common.BulkInsertListToTable(ListBillDetail, "Boleta_Detalle");
                            Business.Common.BulkInsertListToTable(ListBillHeader, "Boleta_Cabecera");
                        }

                        Tools.Logging.Info("Inicio : enviar GFiscal ");

                        Parameters fileParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG);
                        fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(fileParameter);

                        if (fileServerConfig != null && ListBillHeader.Count() > 0)
                        {
                            string resultPath = "";
                            if (serviceConfig.Norm == 340)
                            {
                                resultPath = Business.Isis.CreateBillFile340(ListBillHeader, ListBillDetail, System.IO.Path.GetTempPath());

                            }
                            else
                            {
                                resultPath = Business.Isis.CreateBillFile193(ListBillHeader, ListBillDetail, System.IO.Path.GetTempPath());
                            }
                            Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, System.IO.Path.GetFileName(resultPath), System.IO.File.ReadAllBytes(resultPath));

                            if (mailParameter != null)
                            {
                                mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);
                                Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");
                                Business.Common.SendFileNotification(mailConfig, $"Se envió correctamente el documento: {System.IO.Path.GetFileName(resultPath)} a gfiscal");
                                Tools.Logging.Info("Inicio : Actualizo Auditoria");
                            }

                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_GFISCAL, intentos);
                            Tools.Logging.Info("Inicio : Actualizar fecha de envio");
                            //actualizar documento factura -> agregar el nombre archivo alignet,fechaenvio,
                            Business.Common.UpdateDocumentInvoice(System.IO.Path.GetFileName(resultPath), DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), String.Join("','", "'" + ListBillHeader.Select(x=>x.serieNumero) + "'"));
                            Business.Isis.UpdateBillPickUpDate(ListBillHeader);

                        }





                    }
                    else
                    {
                        Tools.Logging.Error($"No se pudo recuperar el id de auditoria - Facturas Isis");
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

            Tools.Logging.Info("Inicio: Obtener norma para las Notas de Credito de Isis");
            Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);

            if (configParameter != null)
            {
                serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);
                Tools.Logging.Info("Inicio : Obtener documentos de BD Isisc - Notas de crédito");

                List<CreditNoteHeader> ListCreditNoteHeader = Business.Isis.GetCreditNoteHeaders(timestamp, ref intentos, serviceConfig.maxAttemps);
                List<CreditNoteDetail> ListCreditNoteDetail = Business.Isis.GetCreditNoteDetails(timestamp);

                Tools.Logging.Info("Inicio: Obtener configuración de correos electronicos - Notas de Credito Isis");
                Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);

                if (mailParameter != null)
                {
                    mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                    Tools.Logging.Info("Inicio : Registrar Auditoria");
                    auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, ListCreditNoteHeader.Count + ListCreditNoteDetail.Count, 1, serviceConfig.Norm);

                    if (auditId > 0)
                    {
                        List<string> messages = new List<string>();
                        Tools.Logging.Info("Inicio : Validar Documentos ");

                        isValid = Business.Isis.ValidateCreditNoteHeaders(ListCreditNoteHeader, messages);

                        ListCreditNoteDetail.RemoveAll(x => !ListCreditNoteHeader.Select(y => y.serieNumero).Contains(x.serieNumero));

                        Tools.Logging.Info("Inicio : Notificación de Validación");

                        if (!isValid)
                        {
                            Business.Common.SendFileNotification(mailConfig, validationMessage);
                            //Business.Common.UpdateAudit(auditId, Tools.Constants.FALLA_VALIDACION, intentos);
                        }

                        Tools.Logging.Info("Inicio : Actualizo Auditoria");
                        Business.Common.UpdateAudit(auditId, Tools.Constants.LEIDO, intentos);

                        Tools.Logging.Info("Inicio : Insertar Documentos Validados");
                        Business.Common.BulkInsertListToTable(ListCreditNoteDetail, "Nota_Credito_Detalle");
                        Business.Common.BulkInsertListToTable(ListCreditNoteHeader, "Nota_Credito_Cabecera");


                        Tools.Logging.Info("Inicio : enviar GFiscal ");
                        Parameters fileParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG);
                        fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(fileParameter);

                        if (fileServerConfig != null)
                        {
                            string resultPath = "";
                            if (serviceConfig.Norm == 340)
                            {
                                resultPath = Business.Isis.CreateCreditNoteFile340(ListCreditNoteHeader, ListCreditNoteDetail, System.IO.Path.GetTempPath());

                            }
                            else
                            {
                                resultPath = Business.Isis.CreateCreditNoteFile193(ListCreditNoteHeader, ListCreditNoteDetail, System.IO.Path.GetTempPath());
                            }

                            Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");
                            Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, System.IO.Path.GetFileName(resultPath), System.IO.File.ReadAllBytes(resultPath));

                            Tools.Logging.Info("Inicio : Actualizo Auditoria");
                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_GFISCAL, intentos);
                            Tools.Logging.Info("Inicio : Actualizar fecha de envio");
                            //actualizar documento factura -> agregar el nombre archivo alignet,fechaenvio,
                            //Business.Common.UpdateDocumentInvoice(System.IO.Path.GetFileName(resultPath), DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT));
                            Business.Isis.UpdateCreditNotePickUpDate(ListCreditNoteHeader);

                        }


                    }


                }



            }

        }
        private void DebitNote(List<Parameters> oListParameters)
        {
            ServiceConfig serviceConfig;
            Mail mailConfig;
            FileServer fileServerConfig;
            bool isValid;
            List<string> validationMessage = new List<string>();
            int auditId;
            int intentos = 0;
            DateTime timestamp = DateTime.Now;

            Tools.Logging.Info("Inicio: Obtener norma para las Notas de Debito de Isis");

            Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);

            if (configParameter != null)
            {
                serviceConfig = Business.Common.GetParameterDeserialized<ServiceConfig>(configParameter);

                Tools.Logging.Info("Inicio : Obtener documentos de BD Pacyfic - Notas de débito");
                List<DebitNoteHeader> debitNoteHeaders = Business.Isis.GetDebitNoteHeaders(timestamp, ref intentos, serviceConfig.maxAttemps);
                List<DebitNoteDetail> debitNoteDetails = Business.Isis.GetDebitNoteDetails(timestamp);


                Tools.Logging.Info("Inicio: Obtener configuración de correos electronicos - Notas de Debito Isis");

                Parameters mailParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.MAIL_CONFIG);

                if (mailParameter != null)
                {
                    mailConfig = Business.Common.GetParameterDeserialized<Mail>(mailParameter);

                    Tools.Logging.Info("Inicio : Registrar Auditoria");

                    auditId = TM.FECentralizada.Business.Common.InsertAudit(DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), 2, Tools.Constants.NO_LEIDO, debitNoteHeaders.Count + debitNoteDetails.Count, 1, serviceConfig.Norm);

                    if (auditId > 0)
                    {

                        Tools.Logging.Info("Inicio : Validar Documentos ");

                        isValid = Business.Isis.CheckDebitNotes(debitNoteHeaders, validationMessage);

                        debitNoteHeaders.RemoveAll(x => !debitNoteHeaders.Select(y => y.serieNumero).Contains(x.serieNumero));


                        Tools.Logging.Info("Inicio : Notificación de Validación");

                        if (!isValid)
                        {
                            Business.Common.SendFileNotification(mailConfig, validationMessage);
                            //Business.Common.UpdateAudit(auditId, Tools.Constants.FALLA_VALIDACION, intentos);
                        }

                        Tools.Logging.Info("Inicio : Actualizo Auditoria");
                        Business.Common.UpdateAudit(auditId, Tools.Constants.LEIDO, intentos);

                        Tools.Logging.Info("Inicio : Insertar Documentos Validados");
                        Business.Common.BulkInsertListToTable(debitNoteDetails, "Nota_Debito_Detalle");
                        Business.Common.BulkInsertListToTable(debitNoteHeaders, "Nota_Debito_Cabecera");

                        Tools.Logging.Info("Inicio : enviar GFiscal ");

                        Parameters fileParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG);
                        fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(fileParameter);

                        if (fileServerConfig != null)
                        {
                            string resultPath = "";

                            resultPath = Business.Isis.CreateDebitNoteFile340(debitNoteHeaders, debitNoteDetails, System.IO.Path.GetTempPath());

                            Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, System.IO.Path.GetFileName(resultPath), System.IO.File.ReadAllBytes(resultPath));

                            Tools.Logging.Info("Inicio :  Notificación de envio  GFiscal ");
                            Business.Common.SendFileNotification(mailConfig, $"Se envió correctamente el documento: {System.IO.Path.GetFileName(resultPath)} a gfiscal");
                            Tools.Logging.Info("Inicio : Actualizo Auditoria");

                            Business.Common.UpdateAudit(auditId, Tools.Constants.ENVIADO_GFISCAL, intentos);
                            Tools.Logging.Info("Inicio : Actualizar fecha de envio");
                            //actualizar documento factura -> agregar el nombre archivo alignet,fechaenvio,
                            Business.Common.UpdateDocumentInvoice(System.IO.Path.GetFileName(resultPath), DateTime.Now.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT), String.Join("','","'"+ debitNoteHeaders.Select(x=>x.serieNumero)+ "'"));
                            Business.Isis.UpdateDebitNotePickUpDate(debitNoteHeaders);

                        }





                    }
                    else
                    {
                        Tools.Logging.Error($"No se pudo recuperar el id de auditoria - Notas de Debito Isis");
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


        protected override void OnStop()
        {
        }
    }
}
