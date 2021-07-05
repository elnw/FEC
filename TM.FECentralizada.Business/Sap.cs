using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static bool ValidateBills(List<BillHeader> listBillHeader, ref string validationMessage)
        {
            throw new NotImplementedException();
        }

        public static bool ValidateBillDetails(List<BillDetail> listBillDetail, ref string validationMessage)
        {
            throw new NotImplementedException();
        }

        public static List<CreditNoteHeader> GetCreditNoteHeader(string filename, List<string> data, DateTime timestamp, ref int intentos, int maxAttemps)
        {
            throw new NotImplementedException();
        }

        public static List<CreditNoteDetail> GetCreditNoteDetail(string filename, List<string> data, DateTime timestamp)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public static string CreateCreditNoteFile340(List<CreditNoteHeader> listCreditNoteHeader, List<CreditNoteDetail> listCreditNoteDetail, string v)
        {
            throw new NotImplementedException();
        }

        public static string CreateCreditNoteFile193(List<CreditNoteHeader> listCreditNoteHeader, List<CreditNoteDetail> listCreditNoteDetail, string v)
        {
            throw new NotImplementedException();
        }

        public static List<DebitNoteHeader> GetDebitNoteHeader(string filename, List<string> data, DateTime timestamp, ref int intentos, int maxAttemps)
        {
            throw new NotImplementedException();
        }

        public static List<DebitNoteDetail> GetDebitNoteDetail(string filename, List<string> data, DateTime timestamp)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public static string CreateDebitNoteFile340(List<DebitNoteHeader> listDebitNoteHeader, List<DebitNoteDetail> listDebitNoteDetail, string v)
        {
            throw new NotImplementedException();
        }

        public static string CreateDebitNoteFile193(List<DebitNoteHeader> listDebitNoteHeader, List<DebitNoteDetail> listDebitNoteDetail, string v)
        {
            throw new NotImplementedException();
        }
    }
}
