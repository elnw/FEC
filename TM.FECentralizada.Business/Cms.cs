using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TM.FECentralizada.Entities.Cms;
using TM.FECentralizada.Data;


namespace TM.FECentralizada.Business
{
    public static class Cms
    {
        public static List<InvoiceHeader> GetInvoceHeader(String filename, List<String> data, DateTime timestamp, ref int intentos, int maxIntentos)
        {
            List<InvoiceHeader> ListHeaders = new List<InvoiceHeader>();
            bool debeRepetir = false;
            try
            {
                for (int i = 0; i < maxIntentos; i++)
                {
                    ListHeaders = Data.Cms.ReadInvoiceHeader(filename, data, timestamp, ref debeRepetir);
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
                ListDetails = Data.Cms.ReadInvoiceDetail(filename, data, timestamp);
            }

            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return ListDetails;
        }

        public static List<BillHeader> GetBillHeader(String filename, List<String> data, DateTime timestamp, ref int intentos, int maxIntentos)
        {
            List<BillHeader> ListHeaders = new List<BillHeader>();
            bool debeRepetir = false;
            Tools.Logging.Info("Iniciando Consulta BD- Boleta Cabecera");
            try
            {
                for (int i = 0; i < maxIntentos; i++)
                {
                    ListHeaders = Data.Cms.ReadBillHeader(filename, data, timestamp, ref debeRepetir);
                    intentos++;
                    Tools.Logging.Info("Fin Consulta BD-Boleta Cabecera");
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
            BillDetail objBillHeader = new BillDetail();

            Tools.Logging.Info("Iniciando Consulta BD- Cabecera");
            try
            {
                ListDetails = Data.Cms.ReadBillDetail(filename, data, timestamp);
                Tools.Logging.Info("Fin Consulta BD- Cabecera");
            }

            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListDetails;
        }

        //public static List<CreditNoteHeader> GetCreditNoteHeader(DateTime timestamp, ref int intentos, int maxIntentos)
        //{
        //    List<CreditNoteHeader> ListHeaders = new List<CreditNoteHeader>();
        //    bool debeRepetir = false;

        //    Tools.Logging.Info("Iniciando Consulta BD- Cabecera");
        //    try
        //    {
        //        for (int i = 0; i < maxIntentos; i++)
        //        {
        //            ListHeaders = Data.Cms.ReadCreditNoteHeader(timestamp, ref debeRepetir);
        //            intentos++;
        //            Tools.Logging.Info("Fin Consulta BD-Factura Cabecera");
        //            if (!debeRepetir) break;
        //        }


        //        Tools.Logging.Info("Fin Consulta BD- Cabecera");

        //    }

        //    catch (Exception ex)
        //    {
        //        Tools.Logging.Error(ex.Message);
        //    }
        //    return ListHeaders;
        //}

        //public static List<CreditNoteDetail> GetCreditNoteDetail(DateTime timestamp)
        //{
        //    List<CreditNoteDetail> ListHeaders = new List<CreditNoteDetail>();
        //    CreditNoteDetail objBillHeader = new CreditNoteDetail();

        //    Tools.Logging.Info("Iniciando Consulta BD- Cabecera");
        //    try
        //    {

        //        // ListHeaders = Data.Pacifyc.ReadCreditNoteDetails();

        //        ListHeaders = Data.Cms.ReadCreditNoteDetail(timestamp);
        //        //      Stashed changes
        //        Tools.Logging.Info("Fin Consulta BD- Cabecera");
        //        Tools.Logging.Info("Iniciando registro en BD - Cabecera");
        //    }

        //    catch (Exception ex)
        //    {
        //        Tools.Logging.Error(ex.Message);
        //    }
        //    return ListHeaders;
        //}

        public static List<DebitNoteHeader> GetDebitNoteHeaders(String filename, List<String> data, DateTime timestamp, ref int intentos, int maxIntentos)
        {
            List<DebitNoteHeader> ListHeaders = new List<DebitNoteHeader>();

            bool debeRepetir = false;
            Tools.Logging.Info("Iniciando Consulta BD- Cabecera");
            try
            {
                for (int i = 0; i < maxIntentos; i++)
                {
                    ListHeaders = Data.Cms.ReadDebitNoteHeader(filename, data, timestamp, ref debeRepetir);
                    intentos++;
                    Tools.Logging.Info("Fin Consulta BD-Factura Cabecera");
                    if (!debeRepetir) break;
                }

                Tools.Logging.Info("Fin Consulta BD- Cabecera");

            }

            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListHeaders;
        }

        public static List<DebitNoteDetail> GetDebitNoteDetails(String filename, List<String> data, DateTime timestamp)
        {
            List<DebitNoteDetail> ListDetails = new List<DebitNoteDetail>();
            DebitNoteDetail objBillHeader = new DebitNoteDetail();

            Tools.Logging.Info("Iniciando Consulta BD- Cabecera");
            try
            {
                ListDetails = Data.Cms.ReadDebitNoteDetail(filename, data, timestamp);
                Tools.Logging.Info("Fin Consulta BD- Cabecera");
                Tools.Logging.Info("Iniciando registro en BD - Cabecera");
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
            //string fileName = $"FACT_{current.Year}{current.Month}{current.Day}_{current.Hour}_{current.Year}{current.Month}{current.Day}{current.Hour}{current.Minute}{current.Second}.txt";
            string fileName = "FACT_03_" + current.ToString("yyyyMMddHHmmss") + ".txt";

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (InvoiceHeader invoice in invoices)
                {
                    writer.WriteLine($"C|{invoice.serieNumero}|{invoice.fechaEmision}|{invoice.Horadeemision}|" +
                        $"{invoice.tipoMoneda}|{invoice.numeroDocumentoEmisor}|{invoice.tipoDocumentoAdquiriente}|{invoice.numeroDocumentoAdquiriente}|" +
                        $"{invoice.razonSocialAdquiriente}|{invoice.direccionAdquiriente}|{invoice.tipoReferencia_1}|{invoice.numeroDocumentoReferencia_1}|" +
                        $"{invoice.tipoReferencia_2}|{invoice.numeroDocumentoReferencia_2}|{invoice.totalVVNetoOpGravadas}|{invoice.conceptovvnetoopnogravada}|" +
                        $"{invoice.totalVVNetoOpExoneradas}|{invoice.conceptovvnetoopexoneradas}|{invoice.totalVVNetoOpGratuitas}|" +
                        $"{invoice.conceptovvnetoopgratuitas}|{invoice.totalVVNetoExportacion}|{invoice.conceptovvexportacion}|{invoice.totalDescuentos}|{invoice.totalIgv}|" +
                        $"{invoice.totalVenta}|{invoice.tipooperacion}|{invoice.leyendas}||||{invoice.porcentajeDetraccion}|{invoice.totalDetraccion}|{invoice.descripcionDetraccion}|" +
                        $"{invoice.ordenCompra}|{invoice.datosAdicionales}|{invoice.codigoestablecimientosunat}|{invoice.montototalimpuestos}|{invoice.cdgcodigomotivo}|{invoice.cdgporcentaje}|" +
                        $"{invoice.descuentosGlobales}|{invoice.cdgmontobasecargo}|{invoice.sumimpuestosopgratuitas}|{invoice.totalvalorventa}|{invoice.totalprecioventa}|" +
                        $"{invoice.monredimporttotal}||||||{invoice.estado}||{invoice.origen}|");

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

        public static string CreateInvoiceFile193(List<InvoiceHeader> invoices, List<InvoiceDetail> invoiceDetails, string path)
        {
            DateTime current = DateTime.Now;
            string fileName = "FACT_" + current.ToString("yyyyMMdd_HH_yyyyMMddHHmmss") + ".txt";

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (InvoiceHeader invoice in invoices)
                {
                    string codigoMotivo = String.IsNullOrWhiteSpace(invoice.totalRetencion) || invoice.totalRetencion == "0" ? "" : "62";
                    double montoBaseRetencion = 0;

                    if (!String.IsNullOrWhiteSpace(invoice.porcentajeRetencion) && !String.IsNullOrWhiteSpace(invoice.totalRetencion))
                    {
                        montoBaseRetencion = Convert.ToDouble(invoice.totalRetencion) / (1 + Convert.ToDouble(invoice.porcentajeRetencion));
                    }

                    writer.WriteLine($"C|{invoice.serieNumero}|{invoice.fechaEmision}|{invoice.Horadeemision}|" +
                         $"{invoice.tipoMoneda}|{invoice.numeroDocumentoEmisor}|{invoice.tipoDocumentoAdquiriente}|{invoice.numeroDocumentoAdquiriente}|" +
                         $"{invoice.razonSocialAdquiriente}|{invoice.direccionAdquiriente}|{invoice.tipoReferencia_1}|{invoice.numeroDocumentoReferencia_1}|" +
                         $"{invoice.tipoReferencia_2}|{invoice.numeroDocumentoReferencia_2}|{invoice.totalVVNetoOpGravadas}|{invoice.conceptovvnetoopnogravada}|" +
                         $"{invoice.totalVVNetoOpExoneradas}|{invoice.conceptovvnetoopexoneradas}|{invoice.totalVVNetoOpGratuitas}|" +
                         $"{invoice.conceptovvnetoopgratuitas}|{invoice.totalVVNetoExportacion}|{invoice.conceptovvexportacion}|{invoice.totalDescuentos}|{invoice.totalIgv}|" +
                         $"{invoice.totalVenta}|{invoice.tipooperacion}|{invoice.leyendas}||||{invoice.porcentajeDetraccion}|{invoice.totalDetraccion}|{invoice.descripcionDetraccion}|" +
                         $"{invoice.ordenCompra}|{invoice.datosAdicionales}|{invoice.codigoestablecimientosunat}|{invoice.montototalimpuestos}|{invoice.cdgcodigomotivo}|{invoice.cdgporcentaje}|" +
                         $"{invoice.descuentosGlobales}|{invoice.cdgmontobasecargo}|{invoice.sumimpuestosopgratuitas}|{invoice.totalvalorventa}|{invoice.totalprecioventa}|" +
                         $"{invoice.monredimporttotal}|Contado::-::-::-::-|{codigoMotivo}|{invoice.porcentajeDetraccion}|{invoice.totalRetencion}|{montoBaseRetencion}|{invoice.estado}||{invoice.origen}|");

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
        //public static List<BillHeader> GetBillHeader(String filename, List<String> data, DateTime timestamp, ref int intentos, int maxIntentos)
        //{
        //    List<BillHeader> ListHeaders = new List<BillHeader>();
        //    bool debeRepetir = false;
        //    try
        //    {
        //        for (int i = 0; i < maxIntentos; i++)
        //        {
        //            ListHeaders = Data.Cms.ReadBillHeader(filename, data, timestamp, ref debeRepetir);
        //            intentos++;
        //            if (!debeRepetir) break;

        //        }

        //    }

        //    catch (Exception ex)
        //    {
        //        Tools.Logging.Error(ex.Message);
        //    }
        //    return ListHeaders;
        //}

        //public static List<BillDetail> GetBillDetail(String filename, List<String> data, DateTime timestamp)
        //{
        //    List<BillDetail> ListDetails = new List<BillDetail>();
        //    try
        //    {
        //        ListDetails = Data.Cms.ReadBillDetail(filename, data, timestamp);
        //    }

        //    catch (Exception ex)
        //    {
        //        Tools.Logging.Error(ex.Message);
        //    }

        //    return ListDetails;
        //}

        public static bool ValidateBills(List<BillHeader> invoiceHeaders, ref string messageResult)
        {
            bool checkInvoice = true;

            foreach (var invoice in invoiceHeaders.ToList())
            {
                if (!ValidateBills(invoice, ref messageResult))
                {
                    invoiceHeaders.Remove(invoice);
                    checkInvoice &= false;
                }

            }

            return checkInvoice;
        }

        private static bool ValidateBills(BillHeader invoiceHeader, ref string messageResult)
        {
            bool isValid = true;
            if (String.IsNullOrEmpty(invoiceHeader.serieNumero) || invoiceHeader.serieNumero.Length < 13 || !invoiceHeader.serieNumero.StartsWith("B"))
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

        public static bool ValidateBillDetails(List<BillDetail> invoiceDetail, ref string messageResult)
        {
            bool isValid = true;

            foreach (var detail in invoiceDetail.ToList())
            {
                if (ShouldDeleteBill(detail, ref messageResult))
                {
                    invoiceDetail.Remove(detail);
                    isValid &= false;
                }

            }
            return isValid;
        }
        private static bool ShouldDeleteBill(BillDetail detail, ref string messageResult)
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

        public static string CreateBillFile340(List<BillHeader> bills, List<BillDetail> currentBills, string path)
        {
            DateTime current = DateTime.Now;
            //string fileName = $"FACT_{current.Year}{current.Month}{current.Day}_{current.Hour}_{current.Year}{current.Month}{current.Day}{current.Hour}{current.Minute}{current.Second}.txt";
            string fileName = "BOLE_03_" + current.ToString("yyyyMMddHHmmss") + ".txt";

            using (StreamWriter writer = new StreamWriter(Path.Combine(path, fileName)))
            {
                foreach (BillHeader invoice in bills)
                {
                    writer.WriteLine($"C|{invoice.serieNumero}|{invoice.fechaEmision}|{invoice.Horadeemision}|" +
                        $"{invoice.tipoMoneda}|{invoice.numeroDocumentoEmisor}|{invoice.tipoDocumentoAdquiriente}|{invoice.numeroDocumentoAdquiriente}|" +
                        $"{invoice.razonSocialAdquiriente}|{invoice.direccionAdquiriente}|{invoice.tipoReferencia_1}|{invoice.numeroDocumentoReferencia_1}|" +
                        $"{invoice.tipoReferencia_2}|{invoice.numeroDocumentoReferencia_2}|{invoice.totalVVNetoOpGravadas}|{invoice.conceptovvnetoopnogravada}|" +
                        $"{invoice.totalVVNetoOpExoneradas}|{invoice.conceptovvnetoopexoneradas}|{invoice.totalVVNetoOpGratuitas}|" +
                        $"{invoice.conceptovvnetoopgratuitas}|{invoice.totalVVNetoExportacion}|{invoice.conceptovvexportacion}|{invoice.totalDescuentos}|{invoice.totalIgv}|" +
                        $"{invoice.totalVenta}|{invoice.tipooperacion}|{invoice.leyendas}|{invoice.datosAdicionales}|{invoice.codigoestablecimientosunat}|{invoice.montototalimpuestos}|{invoice.cdgcodigomotivo}|{invoice.cdgporcentaje}|" +
                        $"{invoice.descuentosGlobales}|{invoice.cdgmontobasecargo}|{invoice.sumimpuestosopgratuitas}|{invoice.totalvalorventa}|{invoice.totalprecioventa}|" +
                        $"{invoice.monredimporttotal}|{invoice.estado}||{invoice.origen}|{invoice.fechaRegistro}");

                    var currentDetails = currentBills.Where(x => x.serieNumero == invoice.serieNumero).ToList();

                    foreach (BillDetail billDetail in currentBills)
                    {

                        writer.WriteLine($"D|{billDetail.numeroOrdenItem}|{billDetail.unidadMedida}|{billDetail.cantidad}|" +
                            $"{billDetail.codigoProducto}|{billDetail.codigoproductosunat}|{billDetail.descripcion}|" +
                            $"{billDetail.montobaseigv}|{billDetail.importeIgv}|{billDetail.codigoRazonExoneracion}|{billDetail.tasaigv}|" +
                            $"{billDetail.importeDescuento}|{billDetail.codigodescuento}|{billDetail.factordescuento}|" +
                            $"{billDetail.montobasedescuento}|" +
                            $"{billDetail.codigoImporteReferencial}|{billDetail.importeReferencial}|" +
                            $"{billDetail.importeUnitarioSinImpuesto}|{billDetail.importeTotalSinImpuesto}|{billDetail.montototalimpuestoitem}|" +
                            $"||{billDetail.codigoImpUnitConImpuesto}|{billDetail.importeUnitarioConImpuesto}");
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
                    ListHeaders = Data.Cms.ReadDebitNoteHeader(filename, data, timestamp, ref debeRepetir);
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
                ListDetails = Data.Cms.ReadDebitNoteDetail(filename, data, timestamp);
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
                    ListHeaders = Data.Cms.ReadCreditNoteHeader(filename, data, timestamp, ref debeRepetir);
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
                ListDetails = Data.Cms.ReadCreditNoteDetail(filename, data, timestamp);
            }

            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return ListDetails;
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

                        writer.WriteLine($"{cnDetail.numeroOrdenItem}|{cnDetail.unidadMedida}|{cnDetail.cantidad}|" +
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

                        writer.WriteLine($"{cnDetail.numeroOrdenItem}|{cnDetail.unidadMedida}|{cnDetail.cantidad}|" +
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

    }
}

