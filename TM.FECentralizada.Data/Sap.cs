using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TM.FECentralizada.Entities.Common;
using TM.FECentralizada.Entities.Sap;

namespace TM.FECentralizada.Data
{
    public static class Sap
    {
        public static Tuple<List<InvoiceHeader>, List<InvoiceDetail>> GetInvoices(List<string> files, FileServer fileServer, ref bool debeRepetir, DateTime timestamp)
        {
            List<InvoiceHeader> invoiceHeaders = new List<InvoiceHeader>();
            List<InvoiceDetail> invoiceDetails = new List<InvoiceDetail>();
            try
            {
                var headerFiles = files.Where(x => x.StartsWith("FACT_2"));
                string lastSerialNumber = "";
                foreach(var file in headerFiles)
                {
                    var headerLines = Tools.FileServer.DownloadFile(fileServer.Host, fileServer.Port, fileServer.User, fileServer.Password, fileServer.Directory, file);

                    foreach(var line in headerLines)
                    {
                        var fields = line.Split(Tools.Constants.FIELD_SEPARATOR);

                        if(fields[0] == "C")
                        {
                            
                            if (fields.Length >= 45)
                            {
                                lastSerialNumber = fields[1];
                                invoiceHeaders.Add(new InvoiceHeader
                                {
                                    serieNumero = fields[1],
                                    fechaEmision = fields[2],
                                    Horadeemision = fields[3],
                                    tipoMoneda = fields[4],
                                    numeroDocumentoEmisor = fields[5],
                                    tipoDocumentoAdquiriente = fields[6],
                                    numeroDocumentoAdquiriente = fields[7],
                                    razonSocialAdquiriente = fields[8],
                                    direccionAdquiriente = fields[9],
                                    tipoReferencia_1 = fields[10],
                                    numeroDocumentoReferencia_1 = fields[11],
                                    tipoReferencia_2 = fields[12],
                                    numeroDocumentoReferencia_2 = fields[13],
                                    totalVVNetoOpGravadas = fields[14],
                                    totalVVNetoOpNoGravada = fields[15],
                                    conceptovvnetoopnogravada = Convert.ToDouble(fields[16]),
                                    totalVVNetoOpExoneradas = fields[17],
                                    conceptovvnetoopexoneradas = Convert.ToDouble(fields[18]),
                                    totalVVNetoOpGratuitas = fields[19],
                                    conceptovvnetoopgratuitas = Convert.ToDouble(fields[20]),
                                    totalVVNetoExportacion = fields[21],
                                    conceptovvexportacion = Convert.ToDouble(fields[22]),
                                    totalDescuentos = fields[23],
                                    totalIgv = fields[24],
                                    totalVenta = fields[25],
                                    tipooperacion = fields[26],
                                    leyendas = fields[27],
                                    textoLeyenda_3 = fields[28],
                                    textoLeyenda_4 = fields[29],
                                    porcentajeDetraccion = fields[30],
                                    totalDetraccion = fields[31],
                                    descripcionDetraccion = fields[32],
                                    ordenCompra = fields[33],
                                    datosAdicionales = fields[34],
                                    codigoestablecimientosunat = fields[35],
                                    montototalimpuestos = fields[36],
                                    cdgcodigomotivo = fields[37],
                                    cdgporcentaje = fields[38],
                                    descuentosGlobales = fields[39],
                                    cdgmontobasecargo = fields[40],
                                    sumimpuestosopgratuitas = fields[41],
                                    totalvalorventa = fields[42],
                                    totalprecioventa = fields[43],
                                    monredimporttotal = fields[44],
                                    codSistema = "04",
                                    codigoCarga = $"FACT_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                    nombreArchivo = file,
                                    origen = "MA",
                                    estado = "PE",
                                    fechaRegistro = timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT),
                                    formaPago = (fields.Length >= 46 ? fields[45] : ""),
                                    montoPendientePago = (fields.Length >= 47 ? fields[46] : ""),
                                    idCuotas = (fields.Length >= 48 ? fields[47] : ""),
                                    montoPagoUnicoCuotas = (fields.Length >= 49 ? Convert.ToSingle(fields[48]) : 0),
                                    fechaPagoUnico = (fields.Length >= 50 ? fields[49] : ""),
                                    rigvcodigo = (fields.Length >= 51 ? (fields[50]) : ""),
                                    porcentajeRetencion = (fields.Length >= 52 ?(fields[51]) : ""),
                                    totalRetencion = (fields.Length >= 53 ? (fields[52]) : ""),
                                    montoBaseRetencion = (fields.Length >= 54 ? (String.IsNullOrEmpty(fields[53]) ? 0 : Convert.ToSingle(fields[53])) : 0)
                                });
                            }

                        }
                        else
                        {
                            if(fields.Length == 26)
                            {
                                invoiceDetails.Add(new InvoiceDetail
                                {
                                    serieNumero = lastSerialNumber,
                                    numeroOrdenItem = fields[1],
                                    unidadMedida = fields[2],
                                    cantidad = fields[3],
                                    codigoProducto = fields[4],
                                    codigoproductosunat = fields[5],
                                    descripcion = fields[6],
                                    montobaseigv = fields[7],
                                    importeIgv = fields[8],
                                    codigoRazonExoneracion = fields[9],
                                    tasaigv = fields[10],
                                    importeDescuento = fields[11],
                                    codigodescuento = fields[12],
                                    factordescuento = fields[13],
                                    montobasedescuento = fields[14],
                                    codigoImporteReferencial = fields[15],
                                    importeReferencial = fields[16],
                                    importeUnitarioSinImpuesto = fields[17],
                                    importeTotalSinImpuesto = fields[18],
                                    montototalimpuestoitem = fields[19],
                                    codigoImpUnitConImpuesto = fields[20],
                                    importeUnitarioConImpuesto = fields[21],
                                    numeroExpediente = fields[22],
                                    codigoUnidadEjecutora = fields[23],
                                    numeroContrato = fields[24],
                                    numeroProcesoSeleccion = fields[25],
                                    codSistema = "04",
                                    codigoCarga = $"FACT_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}"
                                });
                            }
                        }

                  
                    }

                }


            }catch(Exception ex)
            {
                debeRepetir = true;
                Tools.Logging.Error(ex.Message);
            }
            return new Tuple<List<InvoiceHeader>, List<InvoiceDetail>>(invoiceHeaders, invoiceDetails);
        }

        public static Tuple<List<BillHeader>, List<BillDetail>> GetBills(List<string> files, FileServer fileServer, ref bool debeRepetir, DateTime timestamp)
        {
            List<BillHeader> billHeaders = new List<BillHeader>();
            List<BillDetail> billDetails = new List<BillDetail>();
            string lastSerialNumber = "";
            try
            {

                foreach(var file in files)
                {
                    var fileLines = Tools.FileServer.DownloadFile(fileServer.Host, fileServer.Port, fileServer.User, fileServer.Password, fileServer.Directory, file);

                    foreach(var line in fileLines)
                    {
                        var fields = line.Split(Tools.Constants.FIELD_SEPARATOR);

                        if(fields[0] == "C")
                        {
                            lastSerialNumber = fields[1];
                            billHeaders.Add(new BillHeader
                            {
                                serieNumero = fields[1].Trim(),
                                fechaEmision = fields[2].Trim(),
                                Horadeemision = fields[3].Trim(),
                                tipoMoneda = fields[4].Trim(),
                                numeroDocumentoEmisor = fields[5].Trim(),
                                tipoDocumentoAdquiriente = fields[6].Trim(),
                                numeroDocumentoAdquiriente = fields[7].Trim(),
                                razonSocialAdquiriente = fields[8].Trim(),
                                direccionAdquiriente = fields[9].Trim(),
                                tipoReferencia_1 = fields[10].Trim(),
                                numeroDocumentoReferencia_1 = fields[11].Trim(),
                                tipoReferencia_2 = fields[12].Trim(),
                                numeroDocumentoReferencia_2 = fields[13].Trim(),
                                totalVVNetoOpGravadas = Convert.ToString(Double.Parse(fields[14].Trim())),
                                totalVVNetoOpNoGravada = Convert.ToString(Double.Parse(fields[15].Trim())),
                                conceptovvnetoopnogravada = Convert.ToString(Double.Parse(fields[16].Trim())),
                                totalVVNetoOpExoneradas = Convert.ToString(Double.Parse(fields[17].Trim())),
                                conceptovvnetoopexoneradas = Convert.ToString(Double.Parse(fields[18].Trim())),
                                totalVVNetoOpGratuitas = String.IsNullOrWhiteSpace(fields[19].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[19].Trim())),
                                conceptovvnetoopgratuitas = String.IsNullOrWhiteSpace(fields[20].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[20].Trim())),
                                totalVVNetoExportacion = Convert.ToString(Double.Parse(fields[21].Trim())),
                                conceptovvexportacion = Convert.ToString(Double.Parse(fields[22].Trim())),
                                totalDescuentos = String.IsNullOrWhiteSpace(fields[23].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[23].Trim())),
                                totalIgv = Convert.ToString(Double.Parse(fields[24].Trim())),
                                totalVenta = Convert.ToString(Double.Parse(fields[25].Trim())),
                                tipooperacion = fields[26].Trim(),
                                leyendas = fields[27].Trim(),
                                datosAdicionales = fields[28].Trim(),
                                codigoestablecimientosunat = fields[29].Trim(),
                                montototalimpuestos = Convert.ToString(Double.Parse(fields[30].Trim())),
                                cdgcodigomotivo = fields[31].Trim(),
                                cdgporcentaje = String.IsNullOrWhiteSpace(fields[32].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[32].Trim())),
                                descuentosGlobales = String.IsNullOrWhiteSpace(fields[33].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[33].Trim())),
                                cdgmontobasecargo = String.IsNullOrWhiteSpace(fields[34].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[34].Trim())),
                                sumimpuestosopgratuitas = String.IsNullOrWhiteSpace(fields[35].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[35].Trim())),
                                totalvalorventa = String.IsNullOrWhiteSpace(fields[36].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[36].Trim())),
                                totalprecioventa = String.IsNullOrWhiteSpace(fields[37].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[37].Trim())),
                                monredimporttotal = String.IsNullOrWhiteSpace(fields[38].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[38].Trim())),
                                codSistema = "04",
                                codigoCarga = $"BOLE_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                nombreArchivo = file,
                                origen = "MA",
                                estado = "PE",
                                fechaRegistro = timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT),
                            });
                        }
                        else
                        {
                            billDetails.Add(new BillDetail
                            {
                                serieNumero = lastSerialNumber,
                                numeroOrdenItem = Convert.ToString(Convert.ToInt32(fields[1].Trim())),
                                unidadMedida = fields[2].Trim(),
                                cantidad = Convert.ToString(Convert.ToDouble(fields[3].Trim())),
                                codigoProducto = fields[4].Trim(),
                                codigoproductosunat = fields[5].Trim(),
                                descripcion = fields[6].Trim(),
                                montobaseigv = Convert.ToString(Double.Parse(fields[7].Trim())),
                                importeIgv = Convert.ToString(Double.Parse(fields[8].Trim())),
                                codigoRazonExoneracion = fields[9].Trim(),
                                tasaigv = Convert.ToString(Double.Parse(fields[10].Trim())),
                                importeDescuento = String.IsNullOrWhiteSpace(fields[11].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[11].Trim())),
                                codigodescuento = fields[12].Trim(),
                                factordescuento = String.IsNullOrWhiteSpace(fields[13].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[13].Trim())),
                                montobasedescuento = String.IsNullOrWhiteSpace(fields[14].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[14].Trim())),
                                codigoImporteReferencial = fields[15].Trim(),
                                importeReferencial = fields[16].Trim(),
                                importeUnitarioSinImpuesto = Convert.ToString(Double.Parse(fields[17].Trim())),
                                importeTotalSinImpuesto = Convert.ToString(Double.Parse(fields[18].Trim())),
                                montototalimpuestoitem = Convert.ToString(Double.Parse(fields[19].Trim())),
                                codigoImpUnitConImpuesto = fields[20].Trim(),
                                importeUnitarioConImpuesto = Convert.ToString(Double.Parse(fields[21].Trim())),
                                codSistema = "04",
                                codigoCarga = $"BOLE_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                nombreArchivo = file
                            });
                        }
                    }

                }



            }catch(Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return new Tuple<List<BillHeader>, List<BillDetail>>(billHeaders, billDetails);

        }


        public static Tuple<List<CreditNoteHeader>, List<CreditNoteDetail>> GetCreditNotes(List<string> files, FileServer fileServer, ref bool debeRepetir, DateTime timestamp)
        {
            List<CreditNoteHeader> Headers = new List<CreditNoteHeader>();
            List<CreditNoteDetail> Details = new List<CreditNoteDetail>();
            string lastSerialNumber = "";
            try
            {

                foreach (var file in files)
                {
                    var fileLines = Tools.FileServer.DownloadFile(fileServer.Host, fileServer.Port, fileServer.User, fileServer.Password, fileServer.Directory, file);

                    foreach (var line in fileLines)
                    {
                        var fields = line.Split(Tools.Constants.FIELD_SEPARATOR);

                        if (fields[0] == "C")
                        {
                            lastSerialNumber = fields[1];
                            Headers.Add(new CreditNoteHeader
                            {
                                serieNumero = fields[1].Trim(),
                                fechaEmision = fields[2].Trim(),
                                horadeEmision = fields[3].Trim(),
                                codigoSerieNumeroAfectado = fields[4].Trim(),
                                tipoMoneda = fields[5].Trim(),
                                numeroDocumentoEmisor = fields[6].Trim(),
                                tipoDocumentoAdquiriente = fields[7].Trim(),
                                numeroDocumentoAdquiriente = fields[8].Trim(),
                                razonSocialAdquiriente = fields[9].Trim(),
                                direccionAdquiriente = fields[10].Trim(),
                                tipoDocRefPrincipal = fields[11].Trim(),
                                numeroDocRefPrincipal = fields[12].Trim(),
                                tipoReferencia_1 = fields[13].Trim(),
                                numeroDocumentoReferencia_1 = fields[14].Trim(),
                                tipoReferencia_2 = fields[15].Trim(),
                                numeroDocumentoReferencia_2 = fields[16].Trim(),
                                motivoDocumento = fields[17].Trim(),
                                totalvalorventanetoopgravadas = String.IsNullOrWhiteSpace(fields[18].Trim()) ? "0" : fields[18].Trim(),
                                totalVVNetoOpNoGravada = String.IsNullOrWhiteSpace(fields[19].Trim()) ? "0" : fields[19].Trim(),
                                conceptoVVNetoOpNoGravada = String.IsNullOrWhiteSpace(fields[20].Trim()) ? "0" : fields[20].Trim(),
                                totalVVNetoOpExoneradas = String.IsNullOrWhiteSpace(fields[21].Trim()) ? "0" : fields[21].Trim(),
                                conceptoVVNetoOpExoneradas = String.IsNullOrWhiteSpace(fields[22].Trim()) ? "0" : fields[22].Trim(),
                                totalVVNetoOpGratuitas = String.IsNullOrWhiteSpace(fields[23].Trim()) ? "0" : Convert.ToString(Double.Parse(fields[23].Trim())),
                                conceptoVVNetoOpGratuitas = String.IsNullOrWhiteSpace(fields[24].Trim()) ? "0" : fields[24].Trim(),
                                totalVVNetoExportacion = String.IsNullOrWhiteSpace(fields[25].Trim()) ? "0" : fields[25].Trim(),
                                conceptoVVExportacion = String.IsNullOrWhiteSpace(fields[26].Trim()) ? "0" : fields[26].Trim(),
                                totalIgv = String.IsNullOrWhiteSpace(fields[27].Trim()) ? "0" : fields[27].Trim(),
                                totalVenta = String.IsNullOrWhiteSpace(fields[28].Trim()) ? "0" : fields[28].Trim(),
                                leyendas = fields[29].Trim(),
                                datosAdicionales = fields[30].Trim(),
                                codigoEstablecimientoSunat = fields[31].Trim(),
                                montoTotalImpuestos = String.IsNullOrWhiteSpace(fields[32].Trim()) ? "0" : fields[32].Trim(),
                                sumImpuestosOpGratuitas = String.IsNullOrWhiteSpace(fields[33].Trim()) ? "0" : fields[33].Trim(),
                                monRedImportTotal = String.IsNullOrWhiteSpace(fields[34].Trim()) ? "0" : fields[34].Trim(),
                                codSistema = "04",
                                codigoCarga = $"NCRE_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                nombreArchivo = file
                            });
                        }
                        else
                        {
                            Details.Add(new CreditNoteDetail
                            {
                                serieNumero = lastSerialNumber,
                                numeroOrdenItem = fields[1].Trim(),
                                unidadMedida = fields[2].Trim(),
                                cantidad = fields[3].Trim(),
                                codigoProducto = fields[4].Trim(),
                                codigoProductoSunat = fields[5].Trim(),
                                descripcion = fields[6].Trim(),
                                montoBaseIGV = fields[7].Trim(),
                                importeIGV = fields[8].Trim(),
                                codigoRazonExoneracion = fields[9].Trim(),
                                tasaIGV = fields[10].Trim(),
                                codigoImporteReferencial = fields[11].Trim(),
                                importeReferencial = fields[12].Trim(),
                                importeUnitarioSinImpuesto = fields[13].Trim(),
                                importeTotalSinImpuesto = fields[14].Trim(),
                                montoTotalImpuestoItem = fields[15].Trim(),
                                codigoImpUnitConImpuesto = fields[16].Trim(),
                                importeUnitarioConImpuesto = fields[17].Trim()
                            });
                        }
                    }

                }



            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return new Tuple<List<CreditNoteHeader>, List<CreditNoteDetail>>(Headers, Details);
        }

    }
}
