using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TM.FECentralizada.Entities.Common;
using TM.FECentralizada.Entities.Sap;

namespace TM.FECentralizada.Business
{
    public static class Sap
    {
        public static Tuple<List<InvoiceHeader>,List<InvoiceDetail>> GetInvoices(List<string> files, Entities.Common.FileServer fileServer, ref int intentos, int maxAttemps,DateTime timestamp)
        {
            List<InvoiceHeader> invoiceHeaders = new List<InvoiceHeader>();
            List<InvoiceDetail> invoiceDetails = new List<InvoiceDetail>();
            try
            {
                
                bool debeRepetir = false;
                Tools.Logging.Info("Iniciando Consulta FTP- Factura Cabecera Sap");

                for(int i = 0; i < maxAttemps; i++)
                {
                    var invoices = Data.Sap.GetInvoices(files, fileServer, ref debeRepetir, timestamp);

                    invoiceHeaders = invoices.Item1;
                    invoiceDetails = invoices.Item2;

                    intentos++;
                    if (!debeRepetir) break;
                }


            }
            catch(Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return new Tuple<List<InvoiceHeader>, List<InvoiceDetail>>(invoiceHeaders, invoiceDetails);
        }

        public static bool ValidateInvoices(List<InvoiceHeader> listInvoceHeader, List<string> validationMessage)
        {
            bool checkInvoice = true;

            foreach (var invoice in listInvoceHeader.ToList())
            {
                if (!ValidateInvoice(invoice, validationMessage))
                {
                    listInvoceHeader.Remove(invoice);
                    checkInvoice &= false;
                }

            }

            return checkInvoice;
        }

        private static bool ValidateInvoice(InvoiceHeader invoiceHeader, List<string> messageResult)
        {
            bool isValid = true;
            if (String.IsNullOrEmpty(invoiceHeader.serieNumero) || invoiceHeader.serieNumero.Length < 13 || !invoiceHeader.serieNumero.StartsWith("F"))
            {
                messageResult.Add("La serie y número de la factura: " + invoiceHeader.serieNumero + " tiene una longitud invalida o no cumple con el formato correcto");
                isValid &= false;
            }

            if (String.IsNullOrEmpty(invoiceHeader.fechaEmision))
            {
                messageResult.Add("La fecha de emision de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.tipoDocumentoAdquiriente))
            {
                messageResult.Add("El tipo de documento adquiriente de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacío.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.numeroDocumentoAdquiriente))
            {
                messageResult.Add("El número de documento adquiriente de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacío.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.razonSocialAdquiriente))
            {
                messageResult.Add("La razon social del adquiriente de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.tipoMoneda))
            {
                messageResult.Add("El tipo de moneda de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.tipooperacion))
            {
                messageResult.Add("El tipo de operación de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.codigoestablecimientosunat))
            {
                messageResult.Add("El codigo de establecimiento sunat de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrWhiteSpace(invoiceHeader.totalvalorventa))
            {
                messageResult.Add("El total del valor de venta de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacío.");
                isValid &= false;
            }
            return isValid;
        }

        public static bool ValidateInvoiceDetail(List<InvoiceDetail> listInvoceDetail, List<string> validationMessage)
        {
            bool isValid = true;

            foreach (var detail in listInvoceDetail.ToList())
            {
                if (ShouldDeleteInvoiceDetail(detail, validationMessage))
                {
                    listInvoceDetail.Remove(detail);
                    isValid &= false;
                }

            }
            return isValid;
        }

        private static bool ShouldDeleteInvoiceDetail(InvoiceDetail detail, List<string> messageResult)
        {
            bool isValid = true;
            if (String.IsNullOrEmpty(detail.serieNumero) || detail.serieNumero.Length < 13 || !detail.serieNumero.StartsWith("F"))
            {
                messageResult.Add("La serie y número de la factura: " + detail.serieNumero + " tiene una longitud invalida o no cumple con el formato correcto");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(detail.descripcion))
            {
                messageResult.Add("La descripcion del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacia.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(detail.unidadMedida))
            {
                messageResult.Add("La unidad de medida del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(detail.codigoImpUnitConImpuesto))
            {
                messageResult.Add("El codigo de imp. unitario del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(detail.codigoRazonExoneracion))
            {
                messageResult.Add("El codigo de razon de exgoneracion del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacía.");
                isValid &= false;
            }
            return !isValid;
        }

        public static List<string> CreateLegacyResponseFile(List<ResponseFile> files)
        {
            List<string> lines = new List<string>();
            try
            {
                foreach(var item in files)
                {
                    lines.Add($"{item.estado}|{item.numDocEmisor}|{item.tipoDocumento}|{item.serieNumero}|{item.codigoSunat}|" +
                        $"{item.mensajeSunat}||{item.fechaDeclaracion}|{item.fechaEmision}|{item.firma}|{item.resumen}|" +
                        $"{item.codSistema}|{item.adicional1}|{item.adicional2}|{item.adicional3}|{item.adicional4}|" +
                        $"{item.adicional5}|{item.numDocEmisor.Substring(0,11)}|");
                }

               

            }catch(Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return lines;
        }

        public static List<BillHeader> GetBillHeader(string filename, List<string> data, DateTime timestamp, ref int intentos, int maxAttemps)
        {
            throw new NotImplementedException();
        }

        public static List<BillDetail> GetBillDetail(string filename, List<string> data, DateTime timestamp)
        {
            throw new NotImplementedException();
        }

        public static Tuple<List<BillHeader>, List<BillDetail>> GetBills(List<string> files, Entities.Common.FileServer fileServer, ref int intentos, int maxAttemps, DateTime timestamp)
        {
            List<BillHeader> billHeaders = new List<BillHeader>();
            List<BillDetail> billDetails = new List<BillDetail>();

            try
            {

                bool debeRepetir = false;
                Tools.Logging.Info("Iniciando Consulta FTP- Factura Cabecera Sap");

                for (int i = 0; i < maxAttemps; i++)
                {
                    var bills = Data.Sap.GetBills(files, fileServer, ref debeRepetir, timestamp);

                    billHeaders = bills.Item1;
                    billDetails = bills.Item2;

                    intentos++;
                    if (!debeRepetir) break;
                }


            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return new Tuple<List<BillHeader>, List<BillDetail>>(billHeaders, billDetails);

        }

        public static string CreateBillFile340(List<BillHeader> listBillHeader, List<BillDetail> listBillDetail, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "BOLE_04_" + current.ToString("yyyyMMddHHmmss") + ".txt";

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (BillHeader bill in listBillHeader)
                {
                    writer.WriteLine($"C|{bill.serieNumero}|{bill.fechaEmision}|{bill.Horadeemision}|" +
                        $"{bill.tipoMoneda}|{bill.numeroDocumentoEmisor}|{bill.tipoDocumentoAdquiriente}|{bill.numeroDocumentoAdquiriente}|" +
                        $"{bill.razonSocialAdquiriente}|{bill.direccionAdquiriente}|{bill.tipoReferencia_1}|{bill.numeroDocumentoReferencia_1}|" +
                        $"{bill.tipoReferencia_2}|{bill.numeroDocumentoReferencia_2}|{bill.totalVVNetoOpGravadas}|{bill.totalVVNetoOpNoGravada}|{bill.conceptovvnetoopnogravada}|" +
                        $"{bill.totalVVNetoOpExoneradas}|{bill.conceptovvnetoopexoneradas}|{bill.totalVVNetoOpGratuitas}|" +
                        $"{bill.conceptovvnetoopgratuitas}|{bill.totalVVNetoExportacion}|{bill.conceptovvexportacion}|{bill.totalDescuentos}|{bill.totalIgv}|" +
                        $"{bill.totalVenta}|{bill.tipooperacion}|{bill.leyendas}|{bill.datosAdicionales}|{bill.codigoestablecimientosunat}|{bill.montototalimpuestos}|{bill.cdgcodigomotivo}|{bill.cdgporcentaje}|" +
                        $"{bill.descuentosGlobales}|{bill.cdgmontobasecargo}|{bill.sumimpuestosopgratuitas}|{bill.totalvalorventa}|{bill.totalprecioventa}|" +
                        $"{bill.monredimporttotal}||||");

                    var currentDetails = listBillDetail.Where(x => x.serieNumero == bill.serieNumero).ToList();

                    foreach (BillDetail billDetail in currentDetails)
                    {

                        writer.WriteLine($"D|{billDetail.numeroOrdenItem}|{billDetail.unidadMedida}|{billDetail.cantidad}|" +
                            $"{billDetail.codigoProducto}|{billDetail.codigoproductosunat}|{billDetail.descripcion}|" +
                            $"{billDetail.montobaseigv}|{billDetail.importeIgv}|{billDetail.codigoRazonExoneracion}|{billDetail.tasaigv}|" +
                            $"{billDetail.importeDescuento}|{billDetail.codigodescuento}|{billDetail.factordescuento}|" +
                            $"{billDetail.montobasedescuento}|" +
                            $"{billDetail.codigoImporteReferencial}|{billDetail.importeReferencial}|" +
                            $"{billDetail.importeUnitarioSinImpuesto}|{billDetail.importeTotalSinImpuesto}|{billDetail.montototalimpuestoitem}|" +
                            $"{billDetail.codigoImpUnitConImpuesto}|{billDetail.importeUnitarioConImpuesto}");
                    }


                }



            }
            return Path.Combine(path, fileName);
        }

        public static string CreateInvoiceFile193(List<InvoiceHeader> invoiceHeaders, List<InvoiceDetail> invoiceDetails, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "FACT_04" + current.ToString("_yyyyMMddHHmmss") + ".txt";

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (InvoiceHeader invoice in invoiceHeaders)
                {
                    string additionalData = $"{invoice.formaPago}::{invoice.montoPendientePago}::{invoice.idCuotas}::{invoice.montoPagoUnicoCuotas}::{invoice.fechaPagoUnico}";

                    writer.WriteLine($"C|{invoice.serieNumero}|{invoice.fechaEmision}|{invoice.Horadeemision}|" +
                         $"{invoice.tipoMoneda}|{invoice.numeroDocumentoEmisor}|{invoice.tipoDocumentoAdquiriente}|{invoice.numeroDocumentoAdquiriente}|" +
                         $"{invoice.razonSocialAdquiriente}|{invoice.direccionAdquiriente}|{invoice.tipoReferencia_1}|{invoice.numeroDocumentoReferencia_1}|" +
                         $"{invoice.tipoReferencia_2}|{invoice.numeroDocumentoReferencia_2}|{invoice.totalVVNetoOpGravadas}|{invoice.totalVVNetoOpNoGravada}|{invoice.conceptovvnetoopnogravada}|" +
                         $"{invoice.totalVVNetoOpExoneradas}|{invoice.conceptovvnetoopexoneradas}|{invoice.totalVVNetoOpGratuitas}|" +
                         $"{invoice.conceptovvnetoopgratuitas}|{invoice.totalVVNetoExportacion}|{invoice.conceptovvexportacion}|{invoice.totalDescuentos}|{invoice.totalIgv}|" +
                         $"{invoice.totalVenta}|{invoice.tipooperacion}|{invoice.leyendas}||||{invoice.porcentajeDetraccion}|{invoice.totalDetraccion}|{invoice.descripcionDetraccion}|" +
                         $"{invoice.ordenCompra}|{invoice.datosAdicionales}|{invoice.codigoestablecimientosunat}|{invoice.montototalimpuestos}|{invoice.cdgcodigomotivo}|{invoice.cdgporcentaje}|" +
                         $"{invoice.descuentosGlobales}|{invoice.cdgmontobasecargo}|{invoice.sumimpuestosopgratuitas}|{invoice.totalvalorventa}|{invoice.totalprecioventa}|" +
                         $"{invoice.monredimporttotal}|{additionalData}|{invoice.rigvcodigo}|{invoice.porcentajeDetraccion}|{invoice.totalRetencion}|{invoice.montoBaseRetencion}||||");

                    var currentDetails = invoiceDetails.Where(x => x.serieNumero == invoice.serieNumero).ToList();

                    foreach (InvoiceDetail invoiceDetail in currentDetails)
                    {

                        writer.WriteLine($"D|{invoiceDetail.numeroOrdenItem}|{invoiceDetail.unidadMedida}|{invoiceDetail.cantidad}|" +
                            $"{invoiceDetail.codigoProducto}|{invoiceDetail.codigoproductosunat}|{invoiceDetail.descripcion}|" +
                            $"{invoiceDetail.montobaseigv}|{invoiceDetail.importeIgv}|{invoiceDetail.codigoRazonExoneracion}|{invoiceDetail.tasaigv}|" +
                            $"{invoiceDetail.importeDescuento}|{invoiceDetail.codigodescuento}|{invoiceDetail.factordescuento}|" +
                            $"{invoiceDetail.montobasedescuento}|{invoiceDetail.codigoImporteReferencial}|{invoiceDetail.importeReferencial}|" +
                            $"{invoiceDetail.importeUnitarioSinImpuesto}|{invoiceDetail.importeTotalSinImpuesto}|{invoiceDetail.montototalimpuestoitem}|" +
                            $"||{invoiceDetail.codigoImpUnitConImpuesto}|{invoiceDetail.importeUnitarioConImpuesto}|{invoiceDetail.numeroExpediente}|{invoiceDetail.codigoUnidadEjecutora}|" +
                            $"{invoiceDetail.numeroContrato}|{invoiceDetail.numeroProcesoSeleccion}");
                    }


                }
            }

            return Path.Combine(path, fileName);
        }

        public static bool ValidateBills(List<BillHeader> listBillHeader, List<string> validationMessage)
        {
            bool checkInvoice = true;

            foreach (var invoice in listBillHeader.ToList())
            {
                if (!ValidateBill(invoice, validationMessage))
                {
                    listBillHeader.Remove(invoice);
                    checkInvoice &= false;
                }

            }

            return checkInvoice;
        }

        public static bool ValidateBill(BillHeader billHeader, List<string> validationMessages)
        {
            bool isValid = true;
            if (String.IsNullOrEmpty(billHeader.serieNumero) || billHeader.serieNumero.Length < 13 || !billHeader.serieNumero.StartsWith("B"))
            {
                validationMessages.Add("La serie y número de la factura: " + billHeader.serieNumero + " tiene una longitud invalida o no cumple con el formato correcto");
                isValid &= false;
            }

            if (String.IsNullOrEmpty(billHeader.fechaEmision))
            {
                validationMessages.Add("La fecha de emision de la factura con número de serie: " + billHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(billHeader.tipoDocumentoAdquiriente))
            {
                validationMessages.Add("El tipo de documento adquiriente de la factura con número de serie: " + billHeader.serieNumero + " está vacío.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(billHeader.numeroDocumentoAdquiriente))
            {
                validationMessages.Add("El número de documento adquiriente de la factura con número de serie: " + billHeader.serieNumero + " está vacío.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(billHeader.razonSocialAdquiriente))
            {
                validationMessages.Add("La razon social del adquiriente de la factura con número de serie: " + billHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(billHeader.tipoMoneda))
            {
                validationMessages.Add("El tipo de moneda de la factura con número de serie: " + billHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(billHeader.tipooperacion))
            {
                validationMessages.Add("El tipo de operación de la factura con número de serie: " + billHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(billHeader.codigoestablecimientosunat))
            {
                validationMessages.Add("El codigo de establecimiento sunat de la factura con número de serie: " + billHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrWhiteSpace(billHeader.totalvalorventa))
            {
                validationMessages.Add("El total del valor de venta de la factura con número de serie: " + billHeader.serieNumero + " está vacío.");
                isValid &= false;
            }
            return isValid;
        }

        public static bool ValidateBillDetails(List<BillDetail> listBillDetail, List<string> validationMessage)
        {
            bool isValid = true;

            foreach (var detail in listBillDetail.ToList())
            {
                if (ShouldDeleteBill(detail, validationMessage))
                {
                    listBillDetail.Remove(detail);
                    isValid &= false;
                }

            }
            return isValid;
        }

        private static bool ShouldDeleteBill(BillDetail detail, List<string> validationMessages)
        {
            bool isValid = true;
            if (String.IsNullOrEmpty(detail.serieNumero) || detail.serieNumero.Length < 13 || !detail.serieNumero.StartsWith("F"))
            {
                validationMessages.Add("La serie y número de la factura: " + detail.serieNumero + " tiene una longitud invalida o no cumple con el formato correcto");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(detail.descripcion))
            {
                validationMessages.Add("La descripcion del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacia.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(detail.unidadMedida))
            {
                validationMessages.Add("La unidad de medida del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(detail.codigoImpUnitConImpuesto))
            {
                validationMessages.Add("El codigo de imp. unitario del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(detail.codigoRazonExoneracion))
            {
                validationMessages.Add("El codigo de razon de exgoneracion del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacía.");
                isValid &= false;
            }
            return !isValid;
        }

        public static Tuple<List<CreditNoteHeader>, List<CreditNoteDetail>> GetCreditNotes(List<string> files, Entities.Common.FileServer fileServer, ref int intentos, int maxAttemps, DateTime timestamp)
        {
            List<CreditNoteHeader> creditNoteHeaders = new List<CreditNoteHeader>();
            List<CreditNoteDetail> creditNoteDetails = new List<CreditNoteDetail>();

            try
            {

                bool debeRepetir = false;
                Tools.Logging.Info("Iniciando Consulta FTP- Nota de crédito Sap");

                for (int i = 0; i < maxAttemps; i++)
                {
                    var bills = Data.Sap.GetCreditNotes(files, fileServer, ref debeRepetir, timestamp);

                    creditNoteHeaders = bills.Item1;
                    creditNoteDetails = bills.Item2;

                    intentos++;
                    if (!debeRepetir) break;
                }


            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return new Tuple<List<CreditNoteHeader>, List<CreditNoteDetail>>(creditNoteHeaders, creditNoteDetails);

        }


        public static void UpdatePickUpDate(List<string> inputFilesFTP, Entities.Common.FileServer fileServerConfig)
        {
            Tools.Logging.Info("Inicio :  Mover archivos procesados a ruta PROC ");
            foreach (string file in inputFilesFTP)
            {
                Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file, true, System.IO.Path.GetTempPath());
                Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory + "/PROC/", file, System.IO.File.ReadAllBytes(System.IO.Path.GetTempPath() + "/" + file));
                Tools.FileServer.DeleteFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file);
            };
            Tools.Logging.Info("Inicio : Mover archivos procesados a ruta PROC ");

        }

        public static bool CheckCreditNoteHeaders(List<CreditNoteHeader> listCreditNoteHeader, List<string> validationMessage)
        {
            bool isValid = true;

            foreach (var header in listCreditNoteHeader.ToList())
            {
                if (ShouldDeleteCreditNote(header, validationMessage))
                {
                    listCreditNoteHeader.Remove(header);
                    isValid &= false;
                }
            }
            return isValid;
        }

        private static bool ShouldDeleteCreditNote(CreditNoteHeader creditNoteHeader, List<string> messages)
        {
            bool checkCN = true;

            if (String.IsNullOrEmpty(creditNoteHeader.fechaEmision))
            {
                checkCN &= false;
                messages.Add($"La cabecera de la nota de crédito con serie: {creditNoteHeader.serieNumero} tiene la fecha de emisión vacía");
            }

            if (String.IsNullOrEmpty(creditNoteHeader.codigoSerieNumeroAfectado))
            {
                checkCN &= false;
                messages.Add($"La cabecera de la nota de crédito con serie: {creditNoteHeader.serieNumero} tiene el codigo serie numero afectado vacío");
            }

            if (String.IsNullOrEmpty(creditNoteHeader.serieNumeroAfectado))
            {
                checkCN &= false;
                messages.Add($"La cabecera de la nota de crédito con serie: {creditNoteHeader.serieNumero} tiene la serie de numero afectado vacío");
            }
            if (String.IsNullOrEmpty(creditNoteHeader.razonSocialAdquiriente))
            {
                checkCN &= false;
                messages.Add($"La cabecera de la nota de crédito con serie: {creditNoteHeader.serieNumero} tiene la razón social adquiriente vacía");
            }
            if (String.IsNullOrEmpty(creditNoteHeader.correoAdquiriente))
            {
                checkCN &= false;
                messages.Add($"La cabecera de la nota de crédito con serie: {creditNoteHeader.serieNumero} tiene el correo adquiriente vacío");
            }
            if (String.IsNullOrEmpty(creditNoteHeader.motivoDocumento))
            {
                checkCN &= false;
                messages.Add($"La cabecera de la nota de crédito con serie: {creditNoteHeader.serieNumero} tiene el motivo del documento vacío");
            }
            if (String.IsNullOrEmpty(creditNoteHeader.tipoMoneda))
            {
                checkCN &= false;
                messages.Add($"La cabecera de la nota de crédito con serie: {creditNoteHeader.serieNumero} tiene el tipo de moneda vacío");
            }
            if (String.IsNullOrEmpty(creditNoteHeader.tipoDocRefPrincipal))
            {
                checkCN &= false;
                messages.Add($"La cabecera de la nota de crédito con serie: {creditNoteHeader.serieNumero} tiene el tipo de doc ref principal vacío");
            }
            if (String.IsNullOrEmpty(creditNoteHeader.numeroDocRefPrincipal))
            {
                checkCN &= false;
                messages.Add($"La cabecera de la nota de crédito con serie: {creditNoteHeader.serieNumero} tiene el numero de doc ref principal vacío");
            }
            return checkCN;
        }

        public static string CreateCreditNoteFile340(List<CreditNoteHeader> listCreditNoteHeader, List<CreditNoteDetail> listCreditNoteDetail, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "NCRE_04" + current.ToString("_yyyyMMddHHmmss") + ".txt";
            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (CreditNoteHeader creditNoteHeader in listCreditNoteHeader)
                {
                    writer.WriteLine($"C|{creditNoteHeader.serieNumero}|{creditNoteHeader.fechaEmision}|{creditNoteHeader.horadeEmision}|{creditNoteHeader.codigoSerieNumeroAfectado}|" +
                        $"{creditNoteHeader.tipoMoneda}|{creditNoteHeader.numeroDocumentoEmisor}|{creditNoteHeader.tipoDocumentoAdquiriente}|{creditNoteHeader.numeroDocumentoAdquiriente}|" +
                        $"{creditNoteHeader.razonSocialAdquiriente}|{creditNoteHeader.lugarDestino}|{creditNoteHeader.tipoDocRefPrincipal}|{creditNoteHeader.tipoReferencia_1}|{creditNoteHeader.numeroDocumentoReferencia_1}|" +
                        $"{creditNoteHeader.tipoReferencia_2}|{creditNoteHeader.numeroDocumentoReferencia_2}|{creditNoteHeader.motivoDocumento}|{creditNoteHeader.totalvalorventanetoopgravadas}|{creditNoteHeader.totalVVNetoOpNoGravada}|" +
                        $"{creditNoteHeader.conceptoVVNetoOpNoGravada}|{creditNoteHeader.totalVVNetoOpExoneradas}|{creditNoteHeader.conceptoVVNetoOpExoneradas}|{creditNoteHeader.totalVVNetoOpGratuitas}|" +
                        $"{creditNoteHeader.conceptoVVNetoOpGratuitas}|{creditNoteHeader.totalVVNetoExportacion}|{creditNoteHeader.conceptoVVExportacion}|{creditNoteHeader.totalIgv}|{creditNoteHeader.totalVenta}|" +
                        $"{creditNoteHeader.leyendas}||{creditNoteHeader.codigoEstablecimientoSunat}|{creditNoteHeader.montoTotalImpuestos}|{creditNoteHeader.sumImpuestosOpGratuitas}|{creditNoteHeader.monRedImportTotal}|" +
                        $"||||");

                    var currentDetails = listCreditNoteDetail.Where(x => x.serieNumero == creditNoteHeader.serieNumero).ToList();

                    foreach (CreditNoteDetail cnDetail in currentDetails)
                    {

                        writer.WriteLine($"D|{cnDetail.numeroOrdenItem}|{cnDetail.unidadMedida}|{cnDetail.cantidad}|" +
                            $"{cnDetail.codigoProducto}|{cnDetail.codigoProductoSunat}|{cnDetail.descripcion}|" +
                            $"{cnDetail.montoBaseIGV}|{cnDetail.importeIGV}|{cnDetail.codigoRazonExoneracion}|{cnDetail.tasaIGV}|" +
                            $"{cnDetail.codigoImporteReferencial}|{cnDetail.importeReferencial}|{cnDetail.importeUnitarioSinImpuesto}|" +
                            $"{cnDetail.importeTotalSinImpuesto}|{cnDetail.montoTotalImpuestoItem}|{cnDetail.codigoImpUnitConImpuesto}|" +
                            $"{cnDetail.importeUnitarioConImpuesto}");
                    }
                }
            }
            return Path.Combine(path, fileName);
        }

        public static string CreateCreditNoteFile193(List<CreditNoteHeader> listCreditNoteHeader, List<CreditNoteDetail> listCreditNoteDetail, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "NCRE_04" + current.ToString("_yyyyMMddHHmmss") + ".txt"; ;

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (CreditNoteHeader creditNoteHeader in listCreditNoteHeader)
                {
                    writer.WriteLine($"C|{creditNoteHeader.serieNumero}|{creditNoteHeader.fechaEmision}|{creditNoteHeader.horadeEmision}|{creditNoteHeader.codigoSerieNumeroAfectado}|" +
                        $"{creditNoteHeader.tipoMoneda}|{creditNoteHeader.numeroDocumentoEmisor}|{creditNoteHeader.tipoDocumentoAdquiriente}|{creditNoteHeader.numeroDocumentoAdquiriente}|" +
                        $"{creditNoteHeader.razonSocialAdquiriente}|{creditNoteHeader.lugarDestino}|{creditNoteHeader.tipoDocRefPrincipal}|{creditNoteHeader.tipoReferencia_1}|{creditNoteHeader.numeroDocumentoReferencia_1}|" +
                        $"{creditNoteHeader.tipoReferencia_2}|{creditNoteHeader.numeroDocumentoReferencia_2}|{creditNoteHeader.motivoDocumento}|{creditNoteHeader.totalvalorventanetoopgravadas}|{creditNoteHeader.totalVVNetoOpNoGravada}|" +
                        $"{creditNoteHeader.conceptoVVNetoOpNoGravada}|{creditNoteHeader.totalVVNetoOpExoneradas}|{creditNoteHeader.conceptoVVNetoOpExoneradas}|{creditNoteHeader.totalVVNetoOpGratuitas}|" +
                        $"{creditNoteHeader.conceptoVVNetoOpGratuitas}|{creditNoteHeader.totalVVNetoExportacion}|{creditNoteHeader.conceptoVVExportacion}|{creditNoteHeader.totalIgv}|{creditNoteHeader.totalVenta}|" +
                        $"{creditNoteHeader.leyendas}||{creditNoteHeader.codigoEstablecimientoSunat}|{creditNoteHeader.montoTotalImpuestos}|{creditNoteHeader.sumImpuestosOpGratuitas}|{creditNoteHeader.monRedImportTotal}|" +
                        $"|||||");

                    var currentDetails = listCreditNoteDetail.Where(x => x.serieNumero == creditNoteHeader.serieNumero).ToList();

                    foreach (CreditNoteDetail cnDetail in currentDetails)
                    {

                        writer.WriteLine($"D|{cnDetail.numeroOrdenItem}|{cnDetail.unidadMedida}|{cnDetail.cantidad}|" +
                            $"{cnDetail.codigoProducto}|{cnDetail.codigoProductoSunat}|{cnDetail.descripcion}|" +
                            $"{cnDetail.montoBaseIGV}|{cnDetail.importeIGV}|{cnDetail.codigoRazonExoneracion}|{cnDetail.tasaIGV}|" +
                            $"{cnDetail.codigoImporteReferencial}|{cnDetail.importeReferencial}|{cnDetail.importeUnitarioSinImpuesto}|" +
                            $"{cnDetail.importeTotalSinImpuesto}|{cnDetail.montoTotalImpuestoItem}|{cnDetail.codigoImpUnitConImpuesto}|" +
                            $"{cnDetail.importeUnitarioConImpuesto}");
                    }

                }
            }
            return Path.Combine(path, fileName);
        }

        public static Tuple<List<DebitNoteHeader>, List<DebitNoteDetail>> GetDebitNotes(List<string> files, Entities.Common.FileServer fileServer, ref int intentos, int maxAttemps, DateTime timestamp)
        {
            List<DebitNoteHeader> creditNoteHeaders = new List<DebitNoteHeader>();
            List<DebitNoteDetail> creditNoteDetails = new List<DebitNoteDetail>();

            try
            {

                bool debeRepetir = false;
                Tools.Logging.Info("Iniciando Consulta FTP- Nota de crédito Sap");

                for (int i = 0; i < maxAttemps; i++)
                {
                    var bills = Data.Sap.GetDebitNotes(files, fileServer, ref debeRepetir, timestamp);

                    creditNoteHeaders = bills.Item1;
                    creditNoteDetails = bills.Item2;

                    intentos++;
                    if (!debeRepetir) break;
                }


            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return new Tuple<List<DebitNoteHeader>, List<DebitNoteDetail>>(creditNoteHeaders, creditNoteDetails);
        }

        public static string CreateInvoiceFile340(List<InvoiceHeader> listInvoceHeader, List<InvoiceDetail> listInvoceDetail, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "FACT_04" + current.ToString("_yyyyMMddHHmmss") + ".txt";

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (InvoiceHeader invoice in listInvoceHeader)
                {
                    writer.WriteLine($"C|{invoice.serieNumero}|{invoice.fechaEmision}|{invoice.Horadeemision}|" +
                        $"{invoice.tipoMoneda}|{invoice.numeroDocumentoEmisor}|{invoice.tipoDocumentoAdquiriente}|{invoice.numeroDocumentoAdquiriente}|" +
                        $"{invoice.razonSocialAdquiriente}|{invoice.direccionAdquiriente}|{invoice.tipoReferencia_1}|{invoice.numeroDocumentoReferencia_1}|" +
                        $"{invoice.tipoReferencia_2}|{invoice.numeroDocumentoReferencia_2}|{invoice.totalVVNetoOpGravadas}|{invoice.totalVVNetoOpNoGravada}|{invoice.conceptovvnetoopnogravada}|" +
                        $"{invoice.totalVVNetoOpExoneradas}|{invoice.conceptovvnetoopexoneradas}|{invoice.totalVVNetoOpGratuitas}|" +
                        $"{invoice.conceptovvnetoopgratuitas}|{invoice.totalVVNetoExportacion}|{invoice.conceptovvexportacion}|{invoice.totalDescuentos}|{invoice.totalIgv}|" +
                        $"{invoice.totalVenta}|{invoice.tipooperacion}|{invoice.leyendas}||||{invoice.porcentajeDetraccion}|{invoice.totalDetraccion}|{invoice.descripcionDetraccion}|" +
                        $"{invoice.ordenCompra}|{invoice.datosAdicionales}|{invoice.codigoestablecimientosunat}|{invoice.montototalimpuestos}|{invoice.cdgcodigomotivo}|{invoice.cdgporcentaje}|" +
                        $"{invoice.descuentosGlobales}|{invoice.cdgmontobasecargo}|{invoice.sumimpuestosopgratuitas}|{invoice.totalvalorventa}|{invoice.totalprecioventa}|" +
                        $"{invoice.monredimporttotal}||||");

                    var currentDetails = listInvoceDetail.Where(x => x.serieNumero == invoice.serieNumero).ToList();

                    foreach (InvoiceDetail invoiceDetail in currentDetails)
                    {

                        writer.WriteLine($"D|{invoiceDetail.numeroOrdenItem}|{invoiceDetail.unidadMedida}|{invoiceDetail.cantidad}|" +
                            $"{invoiceDetail.codigoProducto}|{invoiceDetail.codigoproductosunat}|{invoiceDetail.descripcion}|" +
                            $"{invoiceDetail.montobaseigv}|{invoiceDetail.importeIgv}|{invoiceDetail.codigoRazonExoneracion}|{invoiceDetail.tasaigv}|" +
                            $"{invoiceDetail.importeDescuento}|{invoiceDetail.codigodescuento}|{invoiceDetail.factordescuento}|" +
                            $"{invoiceDetail.montobasedescuento}|{invoiceDetail.codigoImporteReferencial}|{invoiceDetail.importeReferencial}|" +
                            $"{invoiceDetail.importeUnitarioSinImpuesto}|{invoiceDetail.importeTotalSinImpuesto}|{invoiceDetail.montototalimpuestoitem}|" +
                            $"{invoiceDetail.codigoImpUnitConImpuesto}|{invoiceDetail.importeUnitarioConImpuesto}|{invoiceDetail.numeroExpediente}|{invoiceDetail.codigoUnidadEjecutora}|" +
                            $"{invoiceDetail.numeroContrato}|{invoiceDetail.numeroProcesoSeleccion}");
                    }
                }
            }
            return Path.Combine(path, fileName);
        }

        public static bool CheckDebitNotes(List<DebitNoteHeader> listDebitNoteHeader, List<string> validationMessages)
        {
            bool isValid = true;

            foreach (DebitNoteHeader debitNoteHeader in listDebitNoteHeader.ToList())
            {
                if (ShouldDeleteDebitNote(debitNoteHeader, validationMessages))
                {
                    listDebitNoteHeader.Remove(debitNoteHeader);
                    isValid &= false;
                }
            }
            return isValid;
        }

        private static bool ShouldDeleteDebitNote(DebitNoteHeader debitNoteHeader, List<string> messages)
        {
            bool checkDN = false;
            if (String.IsNullOrEmpty(debitNoteHeader.fechaEmision))
            {
                checkDN &= true;
                messages.Add($"La nota de débito con número de serie: {debitNoteHeader.serieNumero} tiene la fecha de emisión vacía");
            }
            if (String.IsNullOrEmpty(debitNoteHeader.codigoSerieNumeroAfectado))
            {
                checkDN &= true;
                messages.Add($"La nota de débito con número de serie: {debitNoteHeader.serieNumero} tiene el codigo de serie número afectado vacío");
            }
            if (String.IsNullOrEmpty(debitNoteHeader.serieNumeroAfectado))
            {
                checkDN &= true;
                messages.Add($"La nota de débito con número de serie: {debitNoteHeader.serieNumero} tiene el serie número afectado vacío");
            }
            if (String.IsNullOrEmpty(debitNoteHeader.tipoDocumentoAdquiriente))
            {
                checkDN &= true;
                messages.Add($"La nota de débito con número de serie: {debitNoteHeader.serieNumero} tiene el tipo de documento adquiriente vacío");
            }
            if (String.IsNullOrEmpty(debitNoteHeader.numeroDocumentoAdquiriente))
            {
                checkDN &= true;
                messages.Add($"La nota de débito con número de serie: {debitNoteHeader.serieNumero} tiene el número de documento adquiriente vacío");
            }
            if (String.IsNullOrEmpty(debitNoteHeader.razonSocialAdquiriente))
            {
                checkDN &= true;
                messages.Add($"La nota de débito con número de serie: {debitNoteHeader.serieNumero} tiene la razón social del adquiriente vacío");
            }
            if (String.IsNullOrEmpty(debitNoteHeader.correoAdquiriente))
            {
                checkDN &= true;
                messages.Add($"La nota de débito con número de serie: {debitNoteHeader.serieNumero} tiene el correo del adquiriente vacío");
            }
            if (String.IsNullOrEmpty(debitNoteHeader.motivoDocumento))
            {
                checkDN &= true;
                messages.Add($"La nota de débito con número de serie: {debitNoteHeader.serieNumero} tiene el motivo del documento vacío");
            }
            if (String.IsNullOrEmpty(debitNoteHeader.tipoMoneda))
            {
                checkDN &= true;
                messages.Add($"La nota de débito con número de serie: {debitNoteHeader.serieNumero} tiene el tipo de moneda vacío");
            }
            if (String.IsNullOrEmpty(debitNoteHeader.tipoDocRefPrincipal))
            {
                checkDN &= true;
                messages.Add($"La nota de débito con número de serie: {debitNoteHeader.serieNumero} tiene el tipo de doc ref principal vacío");
            }
            if (String.IsNullOrEmpty(debitNoteHeader.numeroDocRefPrincipal))
            {
                checkDN &= true;
                messages.Add($"La nota de débito con número de serie: {debitNoteHeader.serieNumero} tiene el número de doc ref principal vacío");
            }
            return checkDN;
        }

        public static string CreateDebitNoteFile340(List<DebitNoteHeader> listDebitNoteHeader, List<DebitNoteDetail> listDebitNoteDetail, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "NDEB_04" + current.ToString("_yyyyMMddHHmmss") + ".txt";

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (DebitNoteHeader dnHeader in listDebitNoteHeader)
                {
                    writer.WriteLine($"C|{dnHeader.serieNumero}|{dnHeader.fechaEmision}|{dnHeader.horadeEmision}|" +
                        $"{dnHeader.codigoSerieNumeroAfectado}|{dnHeader.tipoMoneda}|{dnHeader.numeroDocumentoEmisor}|{dnHeader.tipoDocumentoAdquiriente}|" +
                        $"{dnHeader.numeroDocumentoAdquiriente}|{dnHeader.razonSocialAdquiriente}||{dnHeader.tipoDocRefPrincipal}|" +
                        $"{dnHeader.numeroDocRefPrincipal}|{dnHeader.tipoReferencia_1}|{dnHeader.numeroDocumentoReferencia_1}|{dnHeader.tipoReferencia_2}|" +
                        $"{dnHeader.numeroDocumentoReferencia_2}|{dnHeader.motivoDocumento}|{dnHeader.totalvalorventanetoopgravadas}|{dnHeader.totalVVNetoOpNoGravada}|" +
                        $"{dnHeader.conceptoVVNetoOpNoGravada}|{dnHeader.totalVVNetoOpExoneradas}|{dnHeader.conceptoVVNetoOpExoneradas}|{dnHeader.totalVVNetoOpGratuitas}|{dnHeader.conceptoVVNetoOpGratuitas}|" +
                        $"{dnHeader.totalVVNetoExportacion}|{dnHeader.conceptoVVExportacion}|{dnHeader.totalIgv}|{dnHeader.totalVenta}|{dnHeader.leyendas}|{dnHeader.datosAdicionales}|{dnHeader.codigoEstablecimientoSunat}|" +
                        $"{dnHeader.montoTotalImpuestos}|{dnHeader.sumImpuestosOpGratuitas}|{dnHeader.monRedImportTotal}||||");

                    var currentDetails = listDebitNoteDetail.Where(x => x.serieNumero == dnHeader.serieNumero).ToList();


                    foreach (DebitNoteDetail dnDetail in currentDetails)
                    {

                        writer.WriteLine($"D|{dnDetail.numeroOrdenItem}|{dnDetail.unidadMedida}|{dnDetail.cantidad}|" +
                            $"{dnDetail.codigoProducto}|{dnDetail.codigoProductoSunat}|{dnDetail.descripcion}|" +
                            $"{dnDetail.montoBaseIGV}|{dnDetail.importeIGV}|{dnDetail.codigoRazonExoneracion}|{dnDetail.tasaIGV}|" +
                            $"{dnDetail.importeUnitarioSinImpuesto}|{dnDetail.importeTotalSinImpuesto}|{dnDetail.montoTotalImpuestoItem}|" +
                            $"{dnDetail.codigoImpUnitConImpuesto}|{dnDetail.importeUnitarioConImpuesto}");
                    }
                }
            }
            return Path.Combine(path, fileName);
        }

        public static string CreateDebitNoteFile193(List<DebitNoteHeader> listDebitNoteHeader, List<DebitNoteDetail> listDebitNoteDetail, string v)
        {
            throw new NotImplementedException();
        }
    }
}
