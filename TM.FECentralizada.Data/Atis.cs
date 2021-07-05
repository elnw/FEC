using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TM.FECentralizada.Entities.Atis;

namespace TM.FECentralizada.Data
{
    public static class Atis
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
                    {objBillDetail = new InvoiceDetail
                        {
                            serieNumero = linea.Split('|')[0].Trim(),
                            numeroOrdenItem = Convert.ToString((int)Convert.ToDouble(linea.Split('|')[2].Trim())),
                            unidadMedida = linea.Split('|')[3].Trim(),
                            cantidad = Convert.ToString((int)Convert.ToDouble(linea.Split('|')[4].Trim())),
                            codigoProducto = linea.Split('|')[5].Trim(),
                            codigoproductosunat = linea.Split('|')[6].Trim(),
                            descripcion = linea.Split('|')[7].Trim(),
                            montobaseigv = Convert.ToString(Double.Parse( linea.Split('|')[8].Trim())),
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
                        var lineArray = linea.Split('|');
                    objBillHeader = new DebitNoteHeader()
                    {
                        serieNumero = lineArray[1].Trim(),
                        fechaEmision = lineArray[2].Trim(),
                        horadeEmision = lineArray[3].Trim(),
                        codigoSerieNumeroAfectado = lineArray[4].Trim(),
                        tipoMoneda = lineArray[5].Trim(),
                        numeroDocumentoEmisor = lineArray[6].Trim(),
                        tipoDocumentoAdquiriente = lineArray[7].Trim(),
                        numeroDocumentoAdquiriente = lineArray[8].Trim(),
                        razonSocialAdquiriente = lineArray[9].Trim(),
                        direccionAdquiriente = lineArray[10].Trim(),
                        tipoDocRefPrincipal = lineArray[11].Trim(),
                        numeroDocRefPrincipal = lineArray[12].Trim(),
                        tipoReferencia_1 = lineArray[13].Trim(),                        
                        numeroDocumentoReferencia_1 = lineArray[14].Trim(),
                        tipoReferencia_2 = lineArray[15].Trim(),
                        numeroDocumentoReferencia_2 = lineArray[16].Trim(),
                        motivoDocumento = lineArray[17].Trim(),
                        totalVVNetoOpGravadas = Convert.ToString(Double.Parse(lineArray[18].Trim())),
                        totalVVNetoOpNoGravada = Convert.ToString(Double.Parse(lineArray[19].Trim())),
                        conceptovvnetoopnogravada = Convert.ToString(Double.Parse(lineArray[20].Trim())),
                        totalVVNetoOpExoneradas = Convert.ToString(Double.Parse(lineArray[21].Trim())),
                        conceptovvnetoopexoneradas = Convert.ToString(Double.Parse(lineArray[22].Trim())),
                        totalVVNetoOpGratuitas = String.IsNullOrWhiteSpace(lineArray[23].Trim()) ? "0" : Convert.ToString(Double.Parse(lineArray[23].Trim())),
                        conceptoVVNetoOpGratuitas = String.IsNullOrWhiteSpace(lineArray[24].Trim()) ? "0" : Convert.ToString(Double.Parse(lineArray[24].Trim())),
                        totalVVNetoExportacion = Convert.ToString(Double.Parse(lineArray[25].Trim())),
                        conceptoVVExportacion = Convert.ToString(Double.Parse(lineArray[26].Trim())),
                        totalIgv = Convert.ToString(Double.Parse(lineArray[27].Trim())),
                        totalVenta = Convert.ToString(Double.Parse(lineArray[28].Trim())),
                        leyendas = lineArray[29].Trim(),
                        datosAdicionales = lineArray[30].Trim(),
                        codigoEstablecimientoSunat = lineArray[31].Trim(),
                        montoTotalImpuestos = Convert.ToString(Double.Parse(lineArray[32].Trim())),
                        sumImpuestosOpGratuitas = String.IsNullOrWhiteSpace(lineArray[33].Trim()) ? "0" : Convert.ToString(Double.Parse(lineArray[31].Trim())),
                        monRedImportTotal = String.IsNullOrWhiteSpace(lineArray[34].Trim()) ? "0" : Convert.ToString(Double.Parse(lineArray[32].Trim())),
                        codSistema = "03",
                        codigoCarga = $"NDEB_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
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

    public static List<DebitNoteDetail>ReadDebitNoteDetail(String filename, List<string> data, DateTime timestamp)
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

        public static List<CreditNoteHeader> ReadCreditNoteHeader(string filename, List<string> data, DateTime timestamp, ref bool debeRepetir)
        {
            List<CreditNoteHeader> ListCreditNotes = new List<CreditNoteHeader>();

            CreditNoteHeader objBillHeader = new CreditNoteHeader();
            try
            {
                foreach (String linea in data)
                {
                    if (linea.StartsWith("C"))
                    {
                        var lineArray = linea.Split('|');
                        objBillHeader = new CreditNoteHeader()
                        {
                            serieNumero = lineArray[1].Trim(),
                            fechaEmision = lineArray[2].Trim(),
                            horadeEmision = lineArray[3].Trim(),
                            codigoSerieNumeroAfectado = lineArray[4].Trim(),
                            tipoMoneda = lineArray[5].Trim(),
                            numeroDocumentoEmisor = lineArray[6].Trim(),
                            tipoDocumentoAdquiriente = lineArray[7].Trim(),
                            numeroDocumentoAdquiriente = lineArray[8].Trim(),
                            razonSocialAdquiriente = lineArray[9].Trim(),
                            direccionAdquiriente = lineArray[10].Trim(),
                            tipoDocRefPrincipal = lineArray[11].Trim(),
                            numeroDocRefPrincipal = lineArray[12].Trim(),
                            tipoReferencia_1 = lineArray[13].Trim(),
                            numeroDocumentoReferencia_1 = lineArray[14].Trim(),
                            tipoReferencia_2 = lineArray[15].Trim(),
                            numeroDocumentoReferencia_2 = lineArray[16].Trim(),
                            motivoDocumento = lineArray[17].Trim(),
                            totalvalorventanetoopgravadas = String.IsNullOrWhiteSpace(lineArray[18].Trim()) ? "0" : lineArray[18].Trim(),
                            totalVVNetoOpNoGravada = String.IsNullOrWhiteSpace(lineArray[19].Trim())? "0" : lineArray[19].Trim(),
                            conceptoVVNetoOpNoGravada = String.IsNullOrWhiteSpace(lineArray[20].Trim()) ? "0" :lineArray[20].Trim(),
                            totalVVNetoOpExoneradas = String.IsNullOrWhiteSpace(lineArray[21].Trim())? "0": lineArray[21].Trim(),
                            conceptoVVNetoOpExoneradas = String.IsNullOrWhiteSpace(lineArray[22].Trim())? "0": lineArray[22].Trim(),
                            totalVVNetoOpGratuitas = String.IsNullOrWhiteSpace(lineArray[23].Trim()) ? "0" : Convert.ToString(Double.Parse(lineArray[23].Trim())),
                            conceptoVVNetoOpGratuitas = String.IsNullOrWhiteSpace(lineArray[24].Trim()) ? "0" : lineArray[24].Trim(),
                            totalVVNetoExportacion = String.IsNullOrWhiteSpace(lineArray[25].Trim()) ? "0" : lineArray[25].Trim(),
                            conceptoVVExportacion = String.IsNullOrWhiteSpace(lineArray[26].Trim()) ? "0" : lineArray[26].Trim(),
                            totalIgv = String.IsNullOrWhiteSpace(lineArray[27].Trim()) ? "0" : lineArray[27].Trim(),
                            totalVenta = String.IsNullOrWhiteSpace(lineArray[28].Trim()) ? "0" : lineArray[28].Trim(),
                            leyendas = lineArray[29].Trim(),
                            datosAdicionales = lineArray[30].Trim(),
                            codigoEstablecimientoSunat = lineArray[31].Trim(),
                            montoTotalImpuestos = String.IsNullOrWhiteSpace(lineArray[32].Trim()) ? "0" : lineArray[32].Trim(),
                            sumImpuestosOpGratuitas = String.IsNullOrWhiteSpace(lineArray[33].Trim()) ? "0" : lineArray[33].Trim(),
                            monRedImportTotal = String.IsNullOrWhiteSpace(lineArray[34].Trim()) ? "0" : lineArray[34].Trim(),
                            codSistema = "03",
                            codigoCarga = $"NCRE_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                            nombreArchivo = filename,
                            //totalRetencion = dr["TOTALRETENCION"].ToString(),
                            //porcentajeRetencion = dr["PORCENTAJERETENCION"].ToString()*/
                        };
                        ListCreditNotes.Add(objBillHeader);
                    }
                }

            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListCreditNotes;
        }

        public static List<CreditNoteDetail> ReadCreditNoteDetails(String filename, List<string> data, DateTime timestamp){
            List<CreditNoteDetail> ListDetail = new List<CreditNoteDetail>();

            CreditNoteDetail debitNoteDetail;

            try
            {
                foreach (String linea in data)
                {
                    var lineArray = linea.Split('|');
                    if (!linea.StartsWith("C"))
                    {
                        debitNoteDetail = new CreditNoteDetail
                        {
                            serieNumero = lineArray[0].Trim(),
                            numeroOrdenItem = lineArray[1].Trim(),
                            unidadMedida = lineArray[2].Trim(),
                            cantidad = lineArray[3].Trim(),
                            codigoProducto = lineArray[4].Trim(),
                            codigoProductoSunat = lineArray[5].Trim(),
                            descripcion = lineArray[6].Trim(),
                            montoBaseIGV = lineArray[7].Trim(),
                            importeIGV = lineArray[8].Trim(),
                            codigoRazonExoneracion = lineArray[9].Trim(),
                            tasaIGV = lineArray[10].Trim(),
                            codigoImporteReferencial = lineArray[11].Trim(),
                            importeReferencial = lineArray[12].Trim(),
                            importeUnitarioSinImpuesto = lineArray[13].Trim(),
                            importeTotalSinImpuesto = lineArray[14].Trim(),
                            montoTotalImpuestoItem = lineArray[15].Trim(),
                            codigoImpUnitConImpuesto = lineArray[16].Trim(),
                            importeUnitarioConImpuesto = lineArray[17].Trim()
                        };
                        ListDetail.Add(debitNoteDetail);
                    }
                }
            }catch(Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListDetail;
        }


    }
}
