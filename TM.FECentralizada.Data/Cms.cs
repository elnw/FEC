using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TM.FECentralizada.Entities.Cms;

namespace TM.FECentralizada.Data
{
    public static class Cms
    {
        public static List<InvoiceHeader> ReadInvoiceHeader(string filename, List<string> data, DateTime timestamp, ref bool debeRepetir)
        {
            List<InvoiceHeader> ListHeaders = new List<InvoiceHeader>();

            InvoiceHeader objBillHeader = new InvoiceHeader();
            try
            {
                foreach (String linea in data)
                {
                    if (linea.StartsWith("C"))
                    {
                        objBillHeader = new InvoiceHeader()
                        {
                            serieNumero = linea.Split('|')[1].Trim(),
                            fechaEmision = linea.Split('|')[2].Trim(),
                            Horadeemision = linea.Split('|')[3].Trim(),
                            tipoMoneda = linea.Split('|')[4].Trim(),
                            numeroDocumentoEmisor = linea.Split('|')[5].Trim(),
                            tipoDocumentoAdquiriente = linea.Split('|')[6].Trim(),
                            numeroDocumentoAdquiriente = linea.Split('|')[7].Trim(),
                            razonSocialAdquiriente = linea.Split('|')[8].Trim(),
                            direccionAdquiriente = linea.Split('|')[9].Trim(),
                            tipoReferencia_1 = linea.Split('|')[10].Trim(),
                            numeroDocumentoReferencia_1 = linea.Split('|')[11].Trim(),
                            tipoReferencia_2 = linea.Split('|')[12].Trim(),
                            numeroDocumentoReferencia_2 = linea.Split('|')[13].Trim(),
                            totalVVNetoOpGravadas = Convert.ToString(Double.Parse(linea.Split('|')[14].Trim())),
                            totalVVNetoOpNoGravada = Convert.ToString(Double.Parse(linea.Split('|')[15].Trim())),
                            conceptovvnetoopnogravada = Convert.ToString(Double.Parse(linea.Split('|')[16].Trim())),
                            totalVVNetoOpExoneradas = Convert.ToString(Double.Parse(linea.Split('|')[17].Trim())),
                            conceptovvnetoopexoneradas = Convert.ToString(Double.Parse(linea.Split('|')[18].Trim())),
                            totalVVNetoOpGratuitas = String.IsNullOrWhiteSpace(linea.Split('|')[19].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[19].Trim())),
                            conceptovvnetoopgratuitas = String.IsNullOrWhiteSpace(linea.Split('|')[20].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[20].Trim())),
                            totalVVNetoExportacion = Convert.ToString(Double.Parse(linea.Split('|')[21].Trim())),
                            conceptovvexportacion = Convert.ToString(Double.Parse(linea.Split('|')[22].Trim())),
                            totalDescuentos = String.IsNullOrWhiteSpace(linea.Split('|')[23].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[23].Trim())),
                            totalIgv = Convert.ToString(Double.Parse(linea.Split('|')[24].Trim())),
                            totalVenta = Convert.ToString(Double.Parse(linea.Split('|')[25].Trim())),
                            tipooperacion = linea.Split('|')[26].Trim(),
                            leyendas = linea.Split('|')[27].Trim(),
                            textoLeyenda_3 = linea.Split('|')[28].Trim(),
                            textoLeyenda_4 = linea.Split('|')[29].Trim(),
                            porcentajeDetraccion = String.IsNullOrWhiteSpace(linea.Split('|')[30].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[30].Trim())),
                            totalDetraccion = String.IsNullOrWhiteSpace(linea.Split('|')[31].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[31].Trim())),
                            descripcionDetraccion = linea.Split('|')[32].Trim(),
                            ordenCompra = linea.Split('|')[33].Trim(),
                            datosAdicionales = linea.Split('|')[34].Trim(),
                            codigoestablecimientosunat = linea.Split('|')[35].Trim(),
                            montototalimpuestos = Convert.ToString(Double.Parse(linea.Split('|')[36].Trim())),
                            cdgcodigomotivo = linea.Split('|')[37].Trim(),
                            cdgporcentaje = String.IsNullOrWhiteSpace(linea.Split('|')[38].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[38].Trim())),
                            descuentosGlobales = String.IsNullOrWhiteSpace(linea.Split('|')[39].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[39].Trim())),
                            cdgmontobasecargo = String.IsNullOrWhiteSpace(linea.Split('|')[40].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[40].Trim())),
                            sumimpuestosopgratuitas = String.IsNullOrWhiteSpace(linea.Split('|')[41].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[41].Trim())),
                            totalvalorventa = String.IsNullOrWhiteSpace(linea.Split('|')[42].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[42].Trim())),
                            totalprecioventa = String.IsNullOrWhiteSpace(linea.Split('|')[43].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[43].Trim())),
                            monredimporttotal = String.IsNullOrWhiteSpace(linea.Split('|')[44].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[44].Trim())),
                            codSistema = "03",
                            codigoCarga = $"FACT_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                            nombreArchivo = filename,
                            origen = "MA",
                            estado = "PE",
                            fechaRegistro = timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT),
                            //totalRetencion = dr["TOTALRETENCION"].ToString(),
                            //porcentajeRetencion = dr["PORCENTAJERETENCION"].ToString()*/
                        };
                        ListHeaders.Add(objBillHeader);
                    }
                }

            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListHeaders;
        }


        public static List<InvoiceDetail> ReadInvoiceDetail(String filename, List<string> data, DateTime timestamp)
        {
            List<InvoiceDetail> ListDetail = new List<InvoiceDetail>();

            InvoiceDetail objBillDetail = new InvoiceDetail();
            try
            {
                foreach (String linea in data)
                {
                    if (!linea.StartsWith("C"))
                    {
                        objBillDetail = new InvoiceDetail
                        {
                            serieNumero = linea.Split('|')[0].Trim(),
                            numeroOrdenItem = Convert.ToString((int)Convert.ToDouble(linea.Split('|')[2].Trim())),
                            unidadMedida = linea.Split('|')[3].Trim(),
                            cantidad = Convert.ToString((int)Convert.ToDouble(linea.Split('|')[4].Trim())),
                            codigoProducto = linea.Split('|')[5].Trim(),
                            codigoproductosunat = linea.Split('|')[6].Trim(),
                            descripcion = linea.Split('|')[7].Trim(),
                            montobaseigv = Convert.ToString(Double.Parse(linea.Split('|')[8].Trim())),
                            importeIgv = Convert.ToString(Double.Parse(linea.Split('|')[9].Trim())),
                            codigoRazonExoneracion = linea.Split('|')[10].Trim(),
                            tasaigv = Convert.ToString(Double.Parse(linea.Split('|')[11].Trim())),
                            importeDescuento = String.IsNullOrWhiteSpace(linea.Split('|')[12].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[12].Trim())),
                            codigodescuento = linea.Split('|')[13].Trim(),
                            factordescuento = String.IsNullOrWhiteSpace(linea.Split('|')[14].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[14].Trim())),
                            montobasedescuento = String.IsNullOrWhiteSpace(linea.Split('|')[15].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[15].Trim())),
                            codigoImporteReferencial = linea.Split('|')[16].Trim(),
                            importeReferencial = linea.Split('|')[17].Trim(),
                            importeUnitarioSinImpuesto = Convert.ToString(Double.Parse(linea.Split('|')[18].Trim())),
                            importeTotalSinImpuesto = Convert.ToString(Double.Parse(linea.Split('|')[19].Trim())),
                            montototalimpuestoitem = Convert.ToString(Double.Parse(linea.Split('|')[20].Trim())),
                            codigoImpUnitConImpuesto = linea.Split('|')[21].Trim(),
                            importeUnitarioConImpuesto = Convert.ToString(Double.Parse(linea.Split('|')[22].Trim())),
                            numeroExpediente = linea.Split('|')[23].Trim(),
                            codigoUnidadEjecutora = linea.Split('|')[24].Trim(),
                            numeroContrato = linea.Split('|')[25].Trim(),
                            numeroProcesoSeleccion = linea.Split('|')[26].Trim(),
                            codSistema = "03",
                            codigoCarga = $"FACT_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                            nombreArchivo = filename
                        };
                        ListDetail.Add(objBillDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return ListDetail;
        }
        public static List<BillHeader> ReadBillHeader(string filename, List<string> data, DateTime timestamp, ref bool debeRepetir)
        {
            List<BillHeader> ListHeaders = new List<BillHeader>();

            BillHeader objBillHeader = new BillHeader();
            try
            {
                foreach (String linea in data)
                {
                    if (linea.StartsWith("C"))
                    {
                        objBillHeader = new BillHeader()
                        {
                            serieNumero = linea.Split('|')[1].Trim(),
                            fechaEmision = linea.Split('|')[2].Trim(),
                            Horadeemision = linea.Split('|')[3].Trim(),
                            tipoMoneda = linea.Split('|')[4].Trim(),
                            numeroDocumentoEmisor = linea.Split('|')[5].Trim(),
                            tipoDocumentoAdquiriente = linea.Split('|')[6].Trim(),
                            numeroDocumentoAdquiriente = linea.Split('|')[7].Trim(),
                            razonSocialAdquiriente = linea.Split('|')[8].Trim(),
                            direccionAdquiriente = linea.Split('|')[9].Trim(),
                            tipoReferencia_1 = linea.Split('|')[10].Trim(),
                            numeroDocumentoReferencia_1 = linea.Split('|')[11].Trim(),
                            tipoReferencia_2 = linea.Split('|')[12].Trim(),
                            numeroDocumentoReferencia_2 = linea.Split('|')[13].Trim(),
                            totalVVNetoOpGravadas = Convert.ToString(Double.Parse(linea.Split('|')[14].Trim())),
                            totalVVNetoOpNoGravada = Convert.ToString(Double.Parse(linea.Split('|')[15].Trim())),
                            conceptovvnetoopnogravada = Convert.ToString(Double.Parse(linea.Split('|')[16].Trim())),
                            totalVVNetoOpExoneradas = Convert.ToString(Double.Parse(linea.Split('|')[17].Trim())),
                            conceptovvnetoopexoneradas = Convert.ToString(Double.Parse(linea.Split('|')[18].Trim())),
                            totalVVNetoOpGratuitas = String.IsNullOrWhiteSpace(linea.Split('|')[19].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[19].Trim())),
                            conceptovvnetoopgratuitas = String.IsNullOrWhiteSpace(linea.Split('|')[20].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[20].Trim())),
                            totalVVNetoExportacion = Convert.ToString(Double.Parse(linea.Split('|')[21].Trim())),
                            conceptovvexportacion = Convert.ToString(Double.Parse(linea.Split('|')[22].Trim())),
                            totalDescuentos = String.IsNullOrWhiteSpace(linea.Split('|')[23].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[23].Trim())),
                            totalIgv = Convert.ToString(Double.Parse(linea.Split('|')[24].Trim())),
                            totalVenta = Convert.ToString(Double.Parse(linea.Split('|')[25].Trim())),
                            tipooperacion = linea.Split('|')[26].Trim(),
                            leyendas = linea.Split('|')[27].Trim(),
                            datosAdicionales = linea.Split('|')[28].Trim(),
                            codigoestablecimientosunat = linea.Split('|')[29].Trim(),
                            montototalimpuestos = Convert.ToString(Double.Parse(linea.Split('|')[30].Trim())),
                            cdgcodigomotivo = linea.Split('|')[31].Trim(),
                            cdgporcentaje = String.IsNullOrWhiteSpace(linea.Split('|')[32].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[32].Trim())),
                            descuentosGlobales = String.IsNullOrWhiteSpace(linea.Split('|')[33].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[33].Trim())),
                            cdgmontobasecargo = String.IsNullOrWhiteSpace(linea.Split('|')[34].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[34].Trim())),
                            sumimpuestosopgratuitas = String.IsNullOrWhiteSpace(linea.Split('|')[35].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[35].Trim())),
                            totalvalorventa = String.IsNullOrWhiteSpace(linea.Split('|')[36].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[36].Trim())),
                            totalprecioventa = String.IsNullOrWhiteSpace(linea.Split('|')[37].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[37].Trim())),
                            monredimporttotal = String.IsNullOrWhiteSpace(linea.Split('|')[38].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[38].Trim())),
                            codSistema = "03",
                            codigoCarga = $"BOLE_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                            nombreArchivo = filename,
                            origen = "MA",
                            estado = "PE",
                            fechaRegistro = timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT),
                        };
                        ListHeaders.Add(objBillHeader);
                    }
                }

            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListHeaders;
        }

        public static List<BillDetail> ReadBillDetail(String filename, List<string> data, DateTime timestamp)
        {
            List<BillDetail> ListDetail = new List<BillDetail>();

            BillDetail objBillDetail = new BillDetail();
            try
            {
                foreach (String linea in data)
                {
                    if (!linea.StartsWith("C"))
                    {
                        objBillDetail = new BillDetail
                        {
                            serieNumero = linea.Split('|')[0].Trim(),
                            numeroOrdenItem = Convert.ToString((int)Convert.ToDouble(linea.Split('|')[2].Trim())),
                            unidadMedida = linea.Split('|')[3].Trim(),
                            cantidad = Convert.ToString((int)Convert.ToDouble(linea.Split('|')[4].Trim())),
                            codigoProducto = linea.Split('|')[5].Trim(),
                            codigoproductosunat = linea.Split('|')[6].Trim(),
                            descripcion = linea.Split('|')[7].Trim(),
                            montobaseigv = Convert.ToString(Double.Parse(linea.Split('|')[8].Trim())),
                            importeIgv = Convert.ToString(Double.Parse(linea.Split('|')[9].Trim())),
                            codigoRazonExoneracion = linea.Split('|')[10].Trim(),
                            tasaigv = Convert.ToString(Double.Parse(linea.Split('|')[11].Trim())),
                            importeDescuento = String.IsNullOrWhiteSpace(linea.Split('|')[12].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[12].Trim())),
                            codigodescuento = linea.Split('|')[13].Trim(),
                            factordescuento = String.IsNullOrWhiteSpace(linea.Split('|')[14].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[14].Trim())),
                            montobasedescuento = String.IsNullOrWhiteSpace(linea.Split('|')[15].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[15].Trim())),
                            codigoImporteReferencial = linea.Split('|')[16].Trim(),
                            importeReferencial = linea.Split('|')[17].Trim(),
                            importeUnitarioSinImpuesto = Convert.ToString(Double.Parse(linea.Split('|')[18].Trim())),
                            importeTotalSinImpuesto = Convert.ToString(Double.Parse(linea.Split('|')[19].Trim())),
                            montototalimpuestoitem = Convert.ToString(Double.Parse(linea.Split('|')[20].Trim())),
                            codigoImpUnitConImpuesto = linea.Split('|')[21].Trim(),
                            importeUnitarioConImpuesto = Convert.ToString(Double.Parse(linea.Split('|')[22].Trim())),
                            codSistema = "03",
                            codigoCarga = $"BOLE_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                            nombreArchivo = filename
                        };
                        ListDetail.Add(objBillDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return ListDetail;
        }

        public static List<CreditNoteHeader> ReadCreditNoteHeader(string filename, List<string> data, DateTime timestamp, ref bool debeRepetir)
        {
            List<CreditNoteHeader> ListHeaders = new List<CreditNoteHeader>();

            CreditNoteHeader objBillHeader = new CreditNoteHeader();
            try
            {
                foreach (String linea in data)
                {
                    if (linea.StartsWith("C"))
                    {
                        objBillHeader = new CreditNoteHeader()
                        {
                            serieNumero = linea.Split('|')[1].Trim(),
                            fechaEmision = linea.Split('|')[2].Trim(),
                            horadeEmision = linea.Split('|')[3].Trim(),
                            codigoSerieNumeroAfectado = linea.Split('|')[4].Trim(),
                            tipoMoneda = linea.Split('|')[4].Trim(),
                            numeroDocumentoEmisor = linea.Split('|')[5].Trim(),
                            tipoDocumentoAdquiriente = linea.Split('|')[6].Trim(),
                            numeroDocumentoAdquiriente = linea.Split('|')[7].Trim(),
                            razonSocialAdquiriente = linea.Split('|')[8].Trim(),
                            tipoDocRefPrincipal = linea.Split('|')[9].Trim(),
                            numeroDocRefPrincipal = linea.Split('|')[9].Trim(),
                            tipoReferencia_1 = linea.Split('|')[10].Trim(),
                            numeroDocumentoReferencia_1 = linea.Split('|')[11].Trim(),
                            tipoReferencia_2 = linea.Split('|')[12].Trim(),
                            numeroDocumentoReferencia_2 = linea.Split('|')[13].Trim(),
                            motivoDocumento = linea.Split('|')[9].Trim(),
                            totalVVNetoOpNoGravada = Convert.ToString(Double.Parse(linea.Split('|')[15].Trim())),
                            totalVVNetoOpExoneradas = Convert.ToString(Double.Parse(linea.Split('|')[17].Trim())),
                            totalVVNetoOpGratuitas = String.IsNullOrWhiteSpace(linea.Split('|')[19].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[19].Trim())),
                            totalVVNetoExportacion = Convert.ToString(Double.Parse(linea.Split('|')[21].Trim())),
                            totalIgv = Convert.ToString(Double.Parse(linea.Split('|')[24].Trim())),
                            totalVenta = Convert.ToString(Double.Parse(linea.Split('|')[25].Trim())),
                            leyendas = linea.Split('|')[27].Trim(),
                            codSistema = "03",
                            codigoCarga = $"BOLE_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                            nombreArchivo = filename,
                            origen = "MA",
                            estado = "PE",
                            fechaRegistro = timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT),
                        };
                        ListHeaders.Add(objBillHeader);
                    }
                }

            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListHeaders;
        }

        public static List<CreditNoteDetail> ReadCreditNoteDetail(String filename, List<string> data, DateTime timestamp)
        {
            List<CreditNoteDetail> ListDetail = new List<CreditNoteDetail>();

            CreditNoteDetail objBillDetail = new CreditNoteDetail();
            try
            {
                foreach (String linea in data)
                {
                    if (!linea.StartsWith("C"))
                    {
                        objBillDetail = new CreditNoteDetail
                        {
                            serieNumero = linea.Split('|')[0].Trim(),
                            numeroOrdenItem = Convert.ToString((int)Convert.ToDouble(linea.Split('|')[2].Trim())),
                            unidadMedida = linea.Split('|')[3].Trim(),
                            cantidad = Convert.ToString((int)Convert.ToDouble(linea.Split('|')[4].Trim())),
                            codigoProducto = linea.Split('|')[5].Trim(),
                            codigoProductoSunat = linea.Split('|')[6].Trim(),
                            descripcion = linea.Split('|')[7].Trim(),
                            montoBaseIGV = Convert.ToString(Double.Parse(linea.Split('|')[8].Trim())),
                            importeIGV = Convert.ToString(Double.Parse(linea.Split('|')[9].Trim())),
                            codigoRazonExoneracion = linea.Split('|')[10].Trim(),
                            tasaIGV = Convert.ToString(Double.Parse(linea.Split('|')[11].Trim())),
                            importeUnitarioSinImpuesto = String.IsNullOrWhiteSpace(linea.Split('|')[12].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[12].Trim())),
                            importeTotalSinImpuesto = Convert.ToString(Double.Parse(linea.Split('|')[13].Trim())),
                            montoTotalImpuestoItem = Convert.ToString(Double.Parse(linea.Split('|')[14].Trim())),
                            codigoImpUnitConImpuesto = linea.Split('|')[15].Trim(),
                            importeUnitarioConImpuesto = Convert.ToString(Double.Parse(linea.Split('|')[16].Trim())),
                            codSistema = "03",
                            codigoCarga = $"NDEB_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                            nombreArchivo = filename
                        };
                        ListDetail.Add(objBillDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return ListDetail;
        }


        public static List<DebitNoteHeader> ReadDebitNoteHeader(string filename, List<string> data, DateTime timestamp, ref bool debeRepetir)
        {
            List<DebitNoteHeader> ListHeaders = new List<DebitNoteHeader>();

            DebitNoteHeader objBillHeader = new DebitNoteHeader();
            try
            {
                foreach (String linea in data)
                {
                    if (linea.StartsWith("C"))
                    {
                        objBillHeader = new DebitNoteHeader()
                        {
                            serieNumero = linea.Split('|')[1].Trim(),
                            fechaEmision = linea.Split('|')[2].Trim(),
                            horadeEmision = linea.Split('|')[3].Trim(),
                            codigoSerieNumeroAfectado = linea.Split('|')[4].Trim(),
                            tipoMoneda = linea.Split('|')[4].Trim(),
                            numeroDocumentoEmisor = linea.Split('|')[5].Trim(),
                            tipoDocumentoAdquiriente = linea.Split('|')[6].Trim(),
                            numeroDocumentoAdquiriente = linea.Split('|')[7].Trim(),
                            razonSocialAdquiriente = linea.Split('|')[8].Trim(),
                            tipoDocRefPrincipal = linea.Split('|')[9].Trim(),
                            numeroDocRefPrincipal = linea.Split('|')[9].Trim(),
                            tipoReferencia_1 = linea.Split('|')[10].Trim(),
                            numeroDocumentoReferencia_1 = linea.Split('|')[11].Trim(),
                            tipoReferencia_2 = linea.Split('|')[12].Trim(),
                            numeroDocumentoReferencia_2 = linea.Split('|')[13].Trim(),
                            motivoDocumento = linea.Split('|')[9].Trim(),
                            totalVVNetoOpNoGravada = Convert.ToString(Double.Parse(linea.Split('|')[15].Trim())),
                            totalVVNetoOpExoneradas = Convert.ToString(Double.Parse(linea.Split('|')[17].Trim())),
                            totalVVNetoOpGratuitas = String.IsNullOrWhiteSpace(linea.Split('|')[19].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[19].Trim())),
                           totalVVNetoExportacion = Convert.ToString(Double.Parse(linea.Split('|')[21].Trim())),
                            totalIgv = Convert.ToString(Double.Parse(linea.Split('|')[24].Trim())),
                            totalVenta = Convert.ToString(Double.Parse(linea.Split('|')[25].Trim())),
                            leyendas = linea.Split('|')[27].Trim(),
                            datosAdicionales = linea.Split('|')[28].Trim(),
                            codSistema = "03",
                            codigoCarga = $"BOLE_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                            nombreArchivo = filename,
                            origen = "MA",
                            estado = "PE",
                            fechaRegistro = timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT),
                        };
                        ListHeaders.Add(objBillHeader);
                    }
                }

            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListHeaders;
        }

        public static List<DebitNoteDetail> ReadDebitNoteDetail(String filename, List<string> data, DateTime timestamp)
        {
            List<DebitNoteDetail> ListDetail = new List<DebitNoteDetail>();

            DebitNoteDetail objBillDetail = new DebitNoteDetail();
            try
            {
                foreach (String linea in data)
                {
                    if (!linea.StartsWith("C"))
                    {
                        objBillDetail = new DebitNoteDetail
                        {
                            serieNumero = linea.Split('|')[0].Trim(),
                            numeroOrdenItem = Convert.ToString((int)Convert.ToDouble(linea.Split('|')[2].Trim())),
                            unidadMedida = linea.Split('|')[3].Trim(),
                            cantidad = Convert.ToString((int)Convert.ToDouble(linea.Split('|')[4].Trim())),
                            codigoProducto = linea.Split('|')[5].Trim(),
                            codigoProductoSunat = linea.Split('|')[6].Trim(),
                            descripcion = linea.Split('|')[7].Trim(),
                            montoBaseIGV = Convert.ToString(Double.Parse(linea.Split('|')[8].Trim())),
                            importeIGV = Convert.ToString(Double.Parse(linea.Split('|')[9].Trim())),
                            codigoRazonExoneracion = linea.Split('|')[10].Trim(),
                            tasaIGV = Convert.ToString(Double.Parse(linea.Split('|')[11].Trim())),
                            importeUnitarioSinImpuesto = String.IsNullOrWhiteSpace(linea.Split('|')[12].Trim()) ? "0" : Convert.ToString(Double.Parse(linea.Split('|')[12].Trim())),
                            importeTotalSinImpuesto = Convert.ToString(Double.Parse(linea.Split('|')[13].Trim())),
                            montoTotalImpuestoItem = Convert.ToString(Double.Parse(linea.Split('|')[14].Trim())),
                            codigoImpUnitConImpuesto = linea.Split('|')[15].Trim(),
                            importeUnitarioConImpuesto = Convert.ToString(Double.Parse(linea.Split('|')[16].Trim())),
                            codSistema = "03",
                            codigoCarga = $"NDEB_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                            nombreArchivo = filename
                        };
                        ListDetail.Add(objBillDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return ListDetail;
        }
    }
}
