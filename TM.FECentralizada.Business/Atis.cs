using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TM.FECentralizada.Entities.Atis;

namespace TM.FECentralizada.Business
{
    public static class Atis
    {
        public static List<InvoiceHeader> GetInvoceHeader(String filename, List<String> data, DateTime timestamp, ref int intentos, int maxIntentos)
        {
            List<InvoiceHeader> ListHeaders = new List<InvoiceHeader>();
            bool debeRepetir = false;
            try
            {
                for (int i = 0; i < maxIntentos; i++)
                {
                    ListHeaders = Data.Atis.ReadInvoiceHeader(filename, data, timestamp, ref debeRepetir);
                    intentos++;
                    if (!debeRepetir) break;

                }

            }

            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListHeaders;
        }

        public static List<InvoiceDetail> GetInvoceDetail(String filename, List<String> data, DateTime timestamp)
        {
            List<InvoiceDetail> ListDetails = new List<InvoiceDetail>();
            try
            {
                ListDetails = Data.Atis.ReadInvoiceDetail(filename, data, timestamp);
            }

            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return ListDetails;
        }


        public static bool ValidateInvoices(List<InvoiceHeader> invoiceHeaders, ref string messageResult)
        {
            bool checkInvoice = true;

            foreach (var invoice in invoiceHeaders.ToList())
            {
                if (!ValidateInvoice(invoice, ref messageResult))
                {
                    invoiceHeaders.Remove(invoice);
                    checkInvoice &= false;
                }

            }

            return checkInvoice;
        }

        private static bool ValidateInvoice(InvoiceHeader invoiceHeader, ref string messageResult)
        {
            bool isValid = true;
            if (String.IsNullOrEmpty(invoiceHeader.serieNumero) || invoiceHeader.serieNumero.Length < 13 || !invoiceHeader.serieNumero.StartsWith("F"))
            {
                messageResult += "La serie y número de la factura: " + invoiceHeader.serieNumero + " tiene una longitud invalida o no cumple con el formato correcto";
                isValid &= false;
            }

            if (String.IsNullOrEmpty(invoiceHeader.fechaEmision))
            {
                messageResult += "La fecha de emision de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacía.";
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.tipoDocumentoAdquiriente))
            {
                messageResult += "El tipo de documento adquiriente de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacío.";
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.numeroDocumentoAdquiriente))
            {
                messageResult += "El número de documento adquiriente de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacío.";
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.razonSocialAdquiriente))
            {
                messageResult += "La razon social del adquiriente de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacía.";
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.tipoMoneda))
            {
                messageResult += "El tipo de moneda de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacía.";
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.tipooperacion))
            {
                messageResult += "El tipo de operación de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacía.";
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.codigoestablecimientosunat))
            {
                messageResult += "El codigo de establecimiento sunat de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacía.";
                isValid &= false;
            }
            if (String.IsNullOrWhiteSpace(invoiceHeader.totalvalorventa))
            {
                messageResult += "El total del valor de venta de la factura con número de serie: " + invoiceHeader.serieNumero + " está vacío.";
                isValid &= false;
            }
            return isValid;
        }

        private static bool ShouldDeleteInvoice(InvoiceDetail detail, ref string messageResult)
        {
            bool isValid = true;
            /*if (String.IsNullOrEmpty(detail.serieNumero) || detail.serieNumero.Length < 13 || !detail.serieNumero.StartsWith("F"))
            {
                messageResult += "La serie y número de la factura: " + detail.serieNumero + " tiene una longitud invalida o no cumple con el formato correcto";
                isValid &= false;
            }
            if (String.IsNullOrEmpty(detail.descripcion))
            {
                messageResult += "La descripcion del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacia.";
                isValid &= false;
            }
            if (String.IsNullOrEmpty(detail.unidadMedida))
            {
                messageResult += "La unidad de medida del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacía.";
                isValid &= false;
            }
            if (String.IsNullOrEmpty(detail.codigoImpUnitConImpuesto))
            {
                messageResult += "El codigo de imp. unitario del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacía.";
                isValid &= false;
            }
            if (String.IsNullOrEmpty(detail.codigoRazonExoneracion))
            {
                messageResult += "El codigo de razon de exgoneracion del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacía.";
                isValid &= false;
            }*/
            return !isValid;
        }

        public static bool ValidateInvoiceDetail(List<InvoiceDetail> invoiceDetail, ref string messageResult)
        {
            bool isValid = true;

            foreach (var detail in invoiceDetail.ToList())
            {
                if (ShouldDeleteInvoice(detail, ref messageResult))
                {
                    invoiceDetail.Remove(detail);
                    isValid &= false;
                }

            }
            return isValid;
        }


        public static void InsertInvoices(List<InvoiceHeader> invoiceHeaders)
        {
            try
            {
                //Data.Atis.InsertInvoices(invoiceHeaders);
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void InsertInvoicesDetails(List<InvoiceDetail> invoiceDetails)
        {
            try
            {
                //Data.Atis.InsertInvoiceDetails(invoiceDetails);
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static string CreateInvoiceFile340(List<InvoiceHeader> invoices, List<InvoiceDetail> invoiceDetails, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "FACT_03_" + current.ToString("yyyyMMddHHmmss") + ".txt";

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (InvoiceHeader invoice in invoices)
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

                    var currentDetails = invoiceDetails.Where(x => x.serieNumero == invoice.serieNumero).ToList();

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

        public static string CreateInvoiceFile193(List<InvoiceHeader> invoices, List<InvoiceDetail> invoiceDetails, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "FACT_03_" + current.ToString("yyyyMMddHHmmss") + ".txt";

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (InvoiceHeader invoice in invoices)
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

        public static void UpdatePickUpDate(List<InvoiceHeader> invoiceHeaders)
        {

            //Data.Atis.UpdatePickupDate(invoiceHeaders.Select(x => x.serieNumero).ToList());
            // Data.Atis.InvokeInvoiceUpdate();

        }

        public static List<BillHeader> GetBillHeader(String filename, List<String> data, DateTime timestamp, ref int intentos, int maxIntentos)
        {
            List<BillHeader> ListHeaders = new List<BillHeader>();
            bool debeRepetir = false;
            try
            {
                for (int i = 0; i < maxIntentos; i++)
                {
                    ListHeaders = Data.Atis.ReadBillHeader(filename, data, timestamp, ref debeRepetir);
                    intentos++;
                    if (!debeRepetir) break;

                }

            }

            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListHeaders;
        }

        public static List<BillDetail> GetBillDetail(String filename, List<String> data, DateTime timestamp)
        {
            List<BillDetail> ListDetails = new List<BillDetail>();
            try
            {
                ListDetails = Data.Atis.ReadBillDetail(filename, data, timestamp);
            }

            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return ListDetails;
        }

        public static List<CreditNoteHeader> GetCreditNoteHeader(String filename, List<String> data, DateTime timestamp, ref int intentos, int maxIntentos)
        {
            List<CreditNoteHeader> ListHeaders = new List<CreditNoteHeader>();
            bool debeRepetir = false;
            try
            {
                for (int i = 0; i < maxIntentos; i++)
                {
                    ListHeaders = Data.Atis.ReadCreditNoteHeader(filename, data, timestamp, ref debeRepetir);
                    intentos++;
                    if (!debeRepetir) break;

                }

            }

            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListHeaders;
        }

        public static List<CreditNoteDetail> GetCreditNoteDetail(String filename, List<String> data, DateTime timestamp)
        {
            List<CreditNoteDetail> ListDetails = new List<CreditNoteDetail>();
            try
            {
                ListDetails = Data.Atis.ReadCreditNoteDetails(filename, data, timestamp);
            }

            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return ListDetails;
        }

        public static bool ValidateBills(List<BillHeader> invoiceHeaders, List<string> messageResult)
        {
            bool checkInvoice = true;

            foreach (var invoice in invoiceHeaders.ToList())
            {
                if (!ValidateBill(invoice, messageResult))
                {
                    invoiceHeaders.Remove(invoice);
                    checkInvoice &= false;
                }

            }

            return checkInvoice;
        }

        private static bool ValidateBill(BillHeader invoiceHeader, List<string> messageResult)
        {
            bool isValid = true;
            if (String.IsNullOrEmpty(invoiceHeader.serieNumero) || invoiceHeader.serieNumero.Length < 13 || !invoiceHeader.serieNumero.StartsWith("B"))
            {
                messageResult.Add("La serie y número de la boleta: " + invoiceHeader.serieNumero + " tiene una longitud invalida o no cumple con el formato correcto");
                isValid &= false;
            }

            if (String.IsNullOrEmpty(invoiceHeader.fechaEmision))
            {
                messageResult.Add("La fecha de emision de la boleta con número de serie: " + invoiceHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.tipoDocumentoAdquiriente))
            {
                messageResult.Add("El tipo de documento adquiriente de la boleta con número de serie: " + invoiceHeader.serieNumero + " está vacío.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.numeroDocumentoAdquiriente))
            {
                messageResult.Add("El número de documento adquiriente de la boleta con número de serie: " + invoiceHeader.serieNumero + " está vacío.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.razonSocialAdquiriente))
            {
                messageResult.Add("La razon social del adquiriente de la boleta con número de serie: " + invoiceHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.tipoMoneda))
            {
                messageResult.Add("El tipo de moneda de la boleta con número de serie: " + invoiceHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.tipooperacion))
            {
                messageResult.Add("El tipo de operación de la boleta con número de serie: " + invoiceHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrEmpty(invoiceHeader.codigoestablecimientosunat))
            {
                messageResult.Add("El codigo de establecimiento sunat de la boleta con número de serie: " + invoiceHeader.serieNumero + " está vacía.");
                isValid &= false;
            }
            if (String.IsNullOrWhiteSpace(invoiceHeader.totalvalorventa))
            {
                messageResult.Add("El total del valor de venta de la boleta con número de serie: " + invoiceHeader.serieNumero + " está vacío.");
                isValid &= false;
            }
            return isValid;
        }

        public static bool ValidateBillDetails(List<BillDetail> invoiceDetail, List<string> messageResult)
        {
            bool isValid = true;

            foreach (var detail in invoiceDetail.ToList())
            {
                if (ShouldDeleteBill(detail, messageResult))
                {
                    invoiceDetail.Remove(detail);
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

        public static bool CheckCreditNoteHeaders(List<CreditNoteHeader> creditNoteHeaders, List<string> messageResult)
        {
            bool isValid = true;

            foreach(var header in creditNoteHeaders.ToList())
            {
                if(ShouldDeleteCreditNote(header, messageResult))
                {
                    creditNoteHeaders.Remove(header);
                    isValid &= false;
                }
            }
            return isValid;
        }

        private static bool ShouldDeleteBill(BillDetail detail, List<string> messageResult)
        {
            bool isValid = true;
            if (String.IsNullOrEmpty(detail.serieNumero) || detail.serieNumero.Length < 13 || !detail.serieNumero.StartsWith("B"))
            {
                messageResult.Add("La serie y número de la boleta: " + detail.serieNumero + " tiene una longitud invalida o no cumple con el formato correcto");
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
                messageResult.Add("El codigo de razon de exoneracion del detalle con número de orden: " + detail.numeroOrdenItem + " esta vacía.");
                isValid &= false;
            }
            return !isValid;
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

        public static bool CheckDebitNotes(List<DebitNoteHeader> debitNoteHeaders, List<string> messages)
        {
            bool isValid = true;

            foreach(DebitNoteHeader debitNoteHeader in debitNoteHeaders.ToList())
            {
                if(ShouldDeleteDebitNote(debitNoteHeader, messages))
                {
                    debitNoteHeaders.Remove(debitNoteHeader);
                    isValid &= false;
                }
            }
            return isValid;
        }

        public static string CreateBillFile340(List<BillHeader> bills, List<BillDetail> currentBills, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "BOLE_03_" + current.ToString("yyyyMMddHHmmss") + ".txt";

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (BillHeader invoice in bills)
                {
                    writer.WriteLine($"C|{invoice.serieNumero}|{invoice.fechaEmision}|{invoice.Horadeemision}|" +
                        $"{invoice.tipoMoneda}|{invoice.numeroDocumentoEmisor}|{invoice.tipoDocumentoAdquiriente}|{invoice.numeroDocumentoAdquiriente}|" +
                        $"{invoice.razonSocialAdquiriente}|{invoice.direccionAdquiriente}|{invoice.tipoReferencia_1}|{invoice.numeroDocumentoReferencia_1}|" +
                        $"{invoice.tipoReferencia_2}|{invoice.numeroDocumentoReferencia_2}|{invoice.totalVVNetoOpGravadas}|{invoice.totalVVNetoOpNoGravada}|{invoice.conceptovvnetoopnogravada}|" +
                        $"{invoice.totalVVNetoOpExoneradas}|{invoice.conceptovvnetoopexoneradas}|{invoice.totalVVNetoOpGratuitas}|" +
                        $"{invoice.conceptovvnetoopgratuitas}|{invoice.totalVVNetoExportacion}|{invoice.conceptovvexportacion}|{invoice.totalDescuentos}|{invoice.totalIgv}|" +
                        $"{invoice.totalVenta}|{invoice.tipooperacion}|{invoice.leyendas}|{invoice.datosAdicionales}|{invoice.codigoestablecimientosunat}|{invoice.montototalimpuestos}|{invoice.cdgcodigomotivo}|{invoice.cdgporcentaje}|" +
                        $"{invoice.descuentosGlobales}|{invoice.cdgmontobasecargo}|{invoice.sumimpuestosopgratuitas}|{invoice.totalvalorventa}|{invoice.totalprecioventa}|" +
                        $"{invoice.monredimporttotal}||||");

                    var currentDetails = currentBills.Where(x => x.serieNumero == invoice.serieNumero).ToList();

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

        public static string CreateCreditNoteFile340(List<CreditNoteHeader> creditNoteHeaders, List<CreditNoteDetail> creditNoteDetails, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "NCRE_03" + current.ToString("_yyyyMMddHHmmss") + ".txt";
            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (CreditNoteHeader creditNoteHeader in creditNoteHeaders)
                {
                    writer.WriteLine($"C|{creditNoteHeader.serieNumero}|{creditNoteHeader.fechaEmision}|{creditNoteHeader.horadeEmision}|{creditNoteHeader.codigoSerieNumeroAfectado}|" +
                        $"{creditNoteHeader.tipoMoneda}|{creditNoteHeader.numeroDocumentoEmisor}|{creditNoteHeader.tipoDocumentoAdquiriente}|{creditNoteHeader.numeroDocumentoAdquiriente}|" +
                        $"{creditNoteHeader.razonSocialAdquiriente}|{creditNoteHeader.lugarDestino}|{creditNoteHeader.tipoDocRefPrincipal}|{creditNoteHeader.tipoReferencia_1}|{creditNoteHeader.numeroDocumentoReferencia_1}|" +
                        $"{creditNoteHeader.tipoReferencia_2}|{creditNoteHeader.numeroDocumentoReferencia_2}|{creditNoteHeader.motivoDocumento}|{creditNoteHeader.totalvalorventanetoopgravadas}|{creditNoteHeader.totalVVNetoOpNoGravada}|" +
                        $"{creditNoteHeader.conceptoVVNetoOpNoGravada}|{creditNoteHeader.totalVVNetoOpExoneradas}|{creditNoteHeader.conceptoVVNetoOpExoneradas}|{creditNoteHeader.totalVVNetoOpGratuitas}|" +
                        $"{creditNoteHeader.conceptoVVNetoOpGratuitas}|{creditNoteHeader.totalVVNetoExportacion}|{creditNoteHeader.conceptoVVExportacion}|{creditNoteHeader.totalIgv}|{creditNoteHeader.totalVenta}|" +
                        $"{creditNoteHeader.leyendas}||{creditNoteHeader.codigoEstablecimientoSunat}|{creditNoteHeader.montoTotalImpuestos}|{creditNoteHeader.sumImpuestosOpGratuitas}|{creditNoteHeader.monRedImportTotal}|" +
                        $"||||");

                    var currentDetails = creditNoteDetails.Where(x => x.serieNumero == creditNoteHeader.serieNumero).ToList();

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

        public static string CreateCreditNoteFile193(List<CreditNoteHeader> creditNoteHeaders, List<CreditNoteDetail> creditNoteDetails, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "NCRE_03" + current.ToString("_yyyyMMddHHmmss") + ".txt"; ;

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (CreditNoteHeader creditNoteHeader in creditNoteHeaders)
                {
                    writer.WriteLine($"C|{creditNoteHeader.serieNumero}|{creditNoteHeader.fechaEmision}|{creditNoteHeader.horadeEmision}|{creditNoteHeader.codigoSerieNumeroAfectado}|" +
                        $"{creditNoteHeader.tipoMoneda}|{creditNoteHeader.numeroDocumentoEmisor}|{creditNoteHeader.tipoDocumentoAdquiriente}|{creditNoteHeader.numeroDocumentoAdquiriente}|" +
                        $"{creditNoteHeader.razonSocialAdquiriente}|{creditNoteHeader.lugarDestino}|{creditNoteHeader.tipoDocRefPrincipal}|{creditNoteHeader.tipoReferencia_1}|{creditNoteHeader.numeroDocumentoReferencia_1}|" +
                        $"{creditNoteHeader.tipoReferencia_2}|{creditNoteHeader.numeroDocumentoReferencia_2}|{creditNoteHeader.motivoDocumento}|{creditNoteHeader.totalvalorventanetoopgravadas}|{creditNoteHeader.totalVVNetoOpNoGravada}|" +
                        $"{creditNoteHeader.conceptoVVNetoOpNoGravada}|{creditNoteHeader.totalVVNetoOpExoneradas}|{creditNoteHeader.conceptoVVNetoOpExoneradas}|{creditNoteHeader.totalVVNetoOpGratuitas}|" +
                        $"{creditNoteHeader.conceptoVVNetoOpGratuitas}|{creditNoteHeader.totalVVNetoExportacion}|{creditNoteHeader.conceptoVVExportacion}|{creditNoteHeader.totalIgv}|{creditNoteHeader.totalVenta}|" +
                        $"{creditNoteHeader.leyendas}||{creditNoteHeader.codigoEstablecimientoSunat}|{creditNoteHeader.montoTotalImpuestos}|{creditNoteHeader.sumImpuestosOpGratuitas}|{creditNoteHeader.monRedImportTotal}|" +
                        $"|||||");

                    var currentDetails = creditNoteDetails.Where(x => x.serieNumero == creditNoteHeader.serieNumero).ToList();

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


        public static string CreateDebitNoteFile340(List<DebitNoteHeader> debitNoteHeaders, List<DebitNoteDetail> debitNoteDetails, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "NDEB_03" + current.ToString("_yyyyMMddHHmmss") + ".txt";

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (DebitNoteHeader dnHeader in debitNoteHeaders)
                {
                    writer.WriteLine($"C|{dnHeader.serieNumero}|{dnHeader.fechaEmision}|{dnHeader.horadeEmision}|" +
                        $"{dnHeader.codigoSerieNumeroAfectado}|{dnHeader.tipoMoneda}|{dnHeader.numeroDocumentoEmisor}|{dnHeader.tipoDocumentoAdquiriente}|" +
                        $"{dnHeader.numeroDocumentoAdquiriente}|{dnHeader.razonSocialAdquiriente}||{dnHeader.tipoDocRefPrincipal}|" +
                        $"{dnHeader.numeroDocRefPrincipal}|{dnHeader.tipoReferencia_1}|{dnHeader.numeroDocumentoReferencia_1}|{dnHeader.tipoReferencia_2}|" +
                        $"{dnHeader.numeroDocumentoReferencia_2}|{dnHeader.motivoDocumento}|{dnHeader.totalvalorventanetoopgravadas}|{dnHeader.totalVVNetoOpNoGravada}|" +
                        $"{dnHeader.conceptoVVNetoOpNoGravada}|{dnHeader.totalVVNetoOpExoneradas}|{dnHeader.conceptoVVNetoOpExoneradas}|{dnHeader.totalVVNetoOpGratuitas}|{dnHeader.conceptoVVNetoOpGratuitas}|" +
                        $"{dnHeader.totalVVNetoExportacion}|{dnHeader.conceptoVVExportacion}|{dnHeader.totalIgv}|{dnHeader.totalVenta}|{dnHeader.leyendas}|{dnHeader.datosAdicionales}|{dnHeader.codigoEstablecimientoSunat}|" +
                        $"{dnHeader.montoTotalImpuestos}|{dnHeader.sumImpuestosOpGratuitas}|{dnHeader.monRedImportTotal}||||");

                    var currentDetails = debitNoteDetails.Where(x => x.serieNumero == dnHeader.serieNumero).ToList();


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

        public static List<DebitNoteHeader> GetDebitNoteHeader(String filename, List<String> data, DateTime timestamp, ref int intentos, int maxIntentos)
        {
            List<DebitNoteHeader> ListHeaders = new List<DebitNoteHeader>();
            bool debeRepetir = false;
            try
            {
                for (int i = 0; i < maxIntentos; i++)
                {
                    ListHeaders = Data.Atis.ReadDebitNoteHeader(filename, data, timestamp, ref debeRepetir);
                    intentos++;
                    if (!debeRepetir) break;

                }

            }

            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListHeaders;
        }

        public static List<DebitNoteDetail> GetDebitNoteDetail(String filename, List<String> data, DateTime timestamp)
        {
            List<DebitNoteDetail> ListDetails = new List<DebitNoteDetail>();
            try
            {
                ListDetails = Data.Atis.ReadDebitNoteDetail(filename, data, timestamp);
            }

            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return ListDetails;
        }

    }
}
