
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.Data.Informix;
using TM.FECentralizada.Entities.Isis;

namespace TM.FECentralizada.Data
{
    public static class Isis
    {
        public static List<InvoiceHeader> ReadInvoiceHeader(DateTime timestamp, ref bool debeRepetir)
        {
            List<InvoiceHeader> ListHeaders = new List<InvoiceHeader>();
            InvoiceHeader objBillHeader = new InvoiceHeader();
            try
            {
                //using (OracleConnection connection = (OracleConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Oracle))
                using (IfxConnection conn = (IfxConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Informix))
                {
                    //using (OracleCommand cmd = new OracleCommand("PKG_PACIFYC_CONSULTAS.SP_LEER_FACTURA_CAB", connection))
                    using (IfxCommand cmd = new IfxCommand(Tools.Constants.IsisRead_Select_Header, conn))
                    {

                        conn.Open();
                        cmd.CommandType = CommandType.Text;

                        IfxDataReader dr = cmd.ExecuteReader();
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                ListHeaders.Add
                                (
                                    objBillHeader = new InvoiceHeader()
                                    {
                                        serieNumero = dr["serieNumero"].ToString(),
                                        fechaEmision = dr["fechaEmision"].ToString(),
                                        Horadeemision = dr["horadeEmision"].ToString(),
                                        tipoMoneda = dr["tipoMoneda"].ToString(),
                                        numeroDocumentoEmisor = (dr["numeroDocumentoEmisor"].ToString()),
                                        tipoDocumentoAdquiriente = dr["tipoDocumentoAdquiriente"].ToString(),
                                        numeroDocumentoAdquiriente = dr["numeroDocumentoAdquiriente"].ToString(),
                                        razonSocialAdquiriente = dr["razonSocialAdquiriente"].ToString(),
                                        direccionAdquiriente = dr["direccionAdquiriente"].ToString(),
                                        tipoReferencia_1 = dr["tipoReferencia_1"].ToString(),
                                        numeroDocumentoReferencia_1 = dr["numeroDocumentoReferencia_1"].ToString(),
                                        tipoReferencia_2 = dr["tipoReferencia_2"].ToString(),
                                        numeroDocumentoReferencia_2 = dr["numeroDocumentoReferencia_2"].ToString(),
                                        totalVVNetoOpGravadas = (dr["totalVVNetoOpGravadas"].ToString()),
                                        totalVVNetoOpNoGravada = (dr["totalVVNetoOpNoGravada"].ToString()),
                                        conceptovvnetoopnogravada = (dr["conceptoVVNetoOpNoGravada"].ToString()),
                                        totalVVNetoOpExoneradas = (dr["totalVVNetoOpExoneradas"].ToString()),
                                        conceptovvnetoopexoneradas = (dr["conceptoVVNetoOpExoneradas"].ToString()),
                                        totalVVNetoOpGratuitas = (dr["totalVVNetoOpGratuitas"].ToString()),
                                        conceptovvnetoopgratuitas = (dr["conceptoVVNetoOpGratuitas"].ToString()),
                                        totalVVNetoExportacion = (dr["totalVVNetoExportacion"].ToString()),
                                        conceptovvexportacion = (dr["conceptoVVExportacion"].ToString()),
                                        totalDescuentos = (dr["totalDescuentos"].ToString()),
                                        totalIgv = (dr["totalIgv"].ToString()),
                                        totalVenta = (dr["totalVenta"].ToString()),
                                        tipooperacion = dr["tipoOperacion"].ToString(),
                                        leyendas = dr["leyendas"].ToString(),
                                        textoLeyenda_3 = dr["textoleyenda_3"].ToString(),
                                        textoLeyenda_4 = dr["textoleyenda_4"].ToString(),
                                        porcentajeDetraccion = (dr["porcentajeDetraccion"].ToString()),
                                        totalDetraccion = (dr["totalDetraccion"].ToString()),
                                        descripcionDetraccion = dr["descripcionDetraccion"].ToString(),
                                        ordenCompra = dr["ordenCompra"].ToString(),
                                        datosAdicionales = dr["datosAdicionales"].ToString(),
                                        codigoestablecimientosunat = dr["codigoEstablecimientoSunat"].ToString(),
                                        montototalimpuestos = (dr["montoTotalImpuestos"].ToString()),
                                        cdgcodigomotivo = dr["cdgCodigoMotivo"].ToString(),
                                        cdgporcentaje = (dr["cdgPorcentaje"].ToString()),
                                        descuentosGlobales = (dr["descuentosGlobales"].ToString()),
                                        cdgmontobasecargo = (dr["cdgMontoBaseCargo"].ToString()),
                                        sumimpuestosopgratuitas = (dr["sumImpuestosOpGratuitas"].ToString()),
                                        totalvalorventa = (dr["totalValorVenta"].ToString()),
                                        totalprecioventa = (dr["totalPrecioVenta"].ToString()),
                                        monredimporttotal = String.IsNullOrWhiteSpace(dr["monRedImportTotal"].ToString()) ? "0" : dr["monRedImportTotal"].ToString(),
                                        codSistema = "02",
                                        codigoCarga = $"FACT_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                        nombreArchivo = $"FACT_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                        origen = "MA",
                                        estado = "PE",
                                        fechaRegistro = timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)
                                    }
                                    );
                                ListHeaders.Add(objBillHeader);
                                string ResponseCode = cmd.Parameters["PO_CODIGO_RESPUESTA"].Value.ToString();
                                string ResponseMessage = cmd.Parameters["PO_MENSAJE_RESPUESTA"].Value.ToString();
                                Tools.Logging.Info(string.Format("Fin ReadConfiguration = codeResponse:{0}, messageResponse:{1}", ResponseCode, ResponseMessage));
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return ListHeaders;
        }

        public static List<InvoiceDetail> ReadInvoiceDetail(DateTime timestamp)
        {
            List<InvoiceDetail> invoiceDetails = new List<InvoiceDetail>();
            InvoiceDetail billDetail = new InvoiceDetail();
            try
            {
                //using (OracleConnection connection = (OracleConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Oracle))
                using (IfxConnection conn = (IfxConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Informix))
                {
                    //using (OracleCommand cmd = new OracleCommand("PKG_PACIFYC_CONSULTAS.SP_LEER_FACTURA_CAB", connection))
                    using (IfxCommand cmd = new IfxCommand(Tools.Constants.IsisRead_Select_Detail, conn))
                    {

                        conn.Open();
                        cmd.CommandType = CommandType.Text;

                        IfxDataReader dr = cmd.ExecuteReader();
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                invoiceDetails.Add
                                (
                                    billDetail = new InvoiceDetail()
                                    {
                                        serieNumero = dr["serieNumero"].ToString(),
                                        numeroOrdenItem = dr["numeroOrdenItem"].ToString(),
                                        unidadMedida = dr["unidadMedida"].ToString(),
                                        cantidad = (dr["cantidad"].ToString()),
                                        codigoProducto = dr["codigoProducto"].ToString(),
                                        codigoproductosunat = dr["codigoProductoSunat"].ToString(),
                                        descripcion = dr["descripcion"].ToString(),
                                        montobaseigv = dr["montoBaseIGV"].ToString(),
                                        importeIgv = (dr["importeIGV"].ToString()),
                                        codigoRazonExoneracion = dr["codigoRazonExoneracion"].ToString(),
                                        tasaigv = (dr["tasaIGV"].ToString()),
                                        importeDescuento = (dr["importeDescuento"].ToString()),
                                        codigodescuento = dr["codigoDescuento"].ToString(),
                                        factordescuento = (dr["factorDescuento"].ToString()),
                                        montobasedescuento = (dr["montoBaseDescuento"].ToString()),
                                        codigoImporteReferencial = dr["codigoImporteReferencial"].ToString(),
                                        importeReferencial = (String.IsNullOrWhiteSpace(dr["importeReferencial"].ToString()) ? "0" : dr["importeReferencial"].ToString()),
                                        importeUnitarioSinImpuesto = (dr["importeUnitarioSinImpuesto"].ToString()),
                                        importeTotalSinImpuesto = (dr["importeTotalSinImpuesto"].ToString()),
                                        montototalimpuestoitem = (dr["montoTotalImpuestoItem"].ToString()),
                                        codigoImpUnitConImpuesto = dr["codigoImpUnitConImpuesto"].ToString(),
                                        importeUnitarioConImpuesto = (dr["importeUnitarioConImpuesto"].ToString()),
                                        numeroExpediente = dr["numeroExpediente"].ToString(),
                                        codigoUnidadEjecutora = dr["codigoUnidadEjecutora"].ToString(),
                                        numeroContrato = dr["numeroContrato"].ToString(),
                                        numeroProcesoSeleccion = dr["numeroProcesoSeleccion"].ToString(),
                                        codSistema = "02",
                                        codigoCarga = $"FACT_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                        nombreArchivo = $"FACT_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}"
                                    }
                                    );
                                invoiceDetails.Add(billDetail);
                            }
                            string ResponseCode = cmd.Parameters["PO_CODIGO_RESPUESTA"].Value.ToString();
                            string ResponseMessage = cmd.Parameters["PO_MENSAJE_RESPUESTA"].Value.ToString();
                            Tools.Logging.Info(string.Format("Fin ReadConfiguration = codeResponse:{0}, messageResponse:{1}", ResponseCode, ResponseMessage));


                        }
                    }

                }



            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return invoiceDetails;
        }

        public static List<BillHeader> ReadBillHeader(DateTime timestamp, ref bool debeRepetir)
        {
            List<BillHeader> BillHeaders = new List<BillHeader>();
            BillHeader objBillHeader = new BillHeader();
            try
            {
                //using (OracleConnection connection = (OracleConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Oracle))
                using (IfxConnection conn = (IfxConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Informix))
                {
                    //using (OracleCommand cmd = new OracleCommand("PKG_PACIFYC_CONSULTAS.SP_LEER_FACTURA_CAB", connection))
                    using (IfxCommand cmd = new IfxCommand(Tools.Constants.IsisRead_Select_Header, conn))
                    {

                        conn.Open();
                        cmd.CommandType = CommandType.Text;

                        IfxDataReader dr = cmd.ExecuteReader();
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                BillHeaders.Add
                                (
                                    objBillHeader = new BillHeader()
                                    {
                                        serieNumero = dr["serieNumero"].ToString(),
                                        fechaEmision = dr["fechaEmision"].ToString(),
                                        Horadeemision = dr["horadeEmision"].ToString(),
                                        tipoMoneda = dr["tipoMoneda"].ToString(),
                                        numeroDocumentoEmisor = (dr["numeroDocumentoEmisor"].ToString()),
                                        tipoDocumentoAdquiriente = dr["tipoDocumentoAdquiriente"].ToString(),
                                        numeroDocumentoAdquiriente = dr["numeroDocumentoAdquiriente"].ToString(),
                                        razonSocialAdquiriente = dr["razonSocialAdquiriente"].ToString(),
                                        direccionAdquiriente = dr["direccionAdquiriente"].ToString(),
                                        tipoReferencia_1 = dr["tipoReferencia_1"].ToString(),
                                        numeroDocumentoReferencia_1 = dr["numeroDocumentoReferencia_1"].ToString(),
                                        tipoReferencia_2 = dr["tipoReferencia_2"].ToString(),
                                        numeroDocumentoReferencia_2 = dr["numeroDocumentoReferencia_2"].ToString(),
                                        totalVVNetoOpGravadas = (dr["totalVVNetoOpGravadas"].ToString()),
                                        totalVVNetoOpNoGravada = (dr["totalVVNetoOpNoGravada"].ToString()),
                                        conceptovvnetoopnogravada = (dr["conceptoVVNetoOpNoGravada"].ToString()),
                                        totalVVNetoOpExoneradas = (dr["totalVVNetoOpExoneradas"].ToString()),
                                        conceptovvnetoopexoneradas = (dr["conceptoVVNetoOpExoneradas"].ToString()),
                                        totalVVNetoOpGratuitas = (dr["totalVVNetoOpGratuitas"].ToString()),
                                        conceptovvnetoopgratuitas = (dr["conceptoVVNetoOpGratuitas"].ToString()),
                                        totalVVNetoExportacion = (dr["totalVVNetoExportacion"].ToString()),
                                        conceptovvexportacion = (dr["conceptoVVExportacion"].ToString()),
                                        totalDescuentos = (dr["totalDescuentos"].ToString()),
                                        totalIgv = (dr["totalIgv"].ToString()),
                                        totalVenta = (dr["totalVenta"].ToString()),
                                        tipooperacion = dr["tipoOperacion"].ToString(),
                                        leyendas = dr["leyendas"].ToString(),
                                        datosAdicionales = dr["datosAdicionales"].ToString(),
                                        codigoestablecimientosunat = dr["codigoEstablecimientoSunat"].ToString(),
                                        montototalimpuestos = (dr["montoTotalImpuestos"].ToString()),
                                        cdgcodigomotivo = dr["cdgCodigoMotivo"].ToString(),
                                        cdgporcentaje = (dr["cdgPorcentaje"].ToString()),
                                        descuentosGlobales = (dr["descuentosGlobales"].ToString()),
                                        cdgmontobasecargo = (dr["cdgMontoBaseCargo"].ToString()),
                                        sumimpuestosopgratuitas = (dr["sumImpuestosOpGratuitas"].ToString()),
                                        totalvalorventa = (dr["totalValorVenta"].ToString()),
                                        totalprecioventa = (dr["totalPrecioVenta"].ToString()),
                                        monredimporttotal = String.IsNullOrWhiteSpace(dr["monRedImportTotal"].ToString()) ? "0" : dr["monRedImportTotal"].ToString(),
                                        codSistema = "02",
                                        codigoCarga = $"FACT_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                        nombreArchivo = $"FACT_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                        origen = "MA",
                                        estado = "PE",
                                        fechaRegistro = timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)
                                    }
                                    );
                                BillHeaders.Add(objBillHeader);
                                string ResponseCode = cmd.Parameters["PO_CODIGO_RESPUESTA"].Value.ToString();
                                string ResponseMessage = cmd.Parameters["PO_MENSAJE_RESPUESTA"].Value.ToString();
                                Tools.Logging.Info(string.Format("Fin ReadConfiguration = codeResponse:{0}, messageResponse:{1}", ResponseCode, ResponseMessage));
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return BillHeaders;
        }

        public static List<BillDetail> ReadBillDetail(DateTime timestamp)
        {
            List<BillDetail> BillDetails = new List<BillDetail>();
            BillDetail billDetail = new BillDetail();
            try
            {
                //using (OracleConnection connection = (OracleConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Oracle))
                using (IfxConnection conn = (IfxConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Informix))
                {
                    //using (OracleCommand cmd = new OracleCommand("PKG_PACIFYC_CONSULTAS.SP_LEER_FACTURA_CAB", connection))
                    using (IfxCommand cmd = new IfxCommand(Tools.Constants.IsisRead_Select_Header, conn))
                    {

                        conn.Open();
                        cmd.CommandType = CommandType.Text;

                        IfxDataReader dr = cmd.ExecuteReader();
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                BillDetails.Add
                                (
                                    billDetail = new BillDetail()
                                    {
                                        serieNumero = dr["serieNumero"].ToString(),
                                        numeroOrdenItem = dr["numeroOrdenItem"].ToString(),
                                        unidadMedida = dr["unidadMedida"].ToString(),
                                        cantidad = (dr["cantidad"].ToString()),
                                        codigoProducto = dr["codigoProducto"].ToString(),
                                        codigoproductosunat = dr["codigoProductoSunat"].ToString(),
                                        descripcion = dr["descripcion"].ToString(),
                                        montobaseigv = dr["montoBaseIGV"].ToString(),
                                        importeIgv = (dr["importeIGV"].ToString()),
                                        codigoRazonExoneracion = dr["codigoRazonExoneracion"].ToString(),
                                        tasaigv = (dr["tasaIGV"].ToString()),
                                        importeDescuento = (dr["importeDescuento"].ToString()),
                                        codigodescuento = dr["codigoDescuento"].ToString(),
                                        factordescuento = (dr["factorDescuento"].ToString()),
                                        montobasedescuento = (dr["montoBaseDescuento"].ToString()),
                                        codigoImporteReferencial = dr["codigoImporteReferencial"].ToString(),
                                        importeReferencial = (String.IsNullOrWhiteSpace(dr["importeReferencial"].ToString()) ? "0" : dr["importeReferencial"].ToString()),
                                        importeUnitarioSinImpuesto = (dr["importeUnitarioSinImpuesto"].ToString()),
                                        importeTotalSinImpuesto = (dr["importeTotalSinImpuesto"].ToString()),
                                        montototalimpuestoitem = (dr["montoTotalImpuestoItem"].ToString()),
                                        codigoImpUnitConImpuesto = dr["codigoImpUnitConImpuesto"].ToString(),
                                        importeUnitarioConImpuesto = (dr["importeUnitarioConImpuesto"].ToString()),
                                        codSistema = "02",
                                        codigoCarga = $"FACT_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                        nombreArchivo = $"FACT_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}"
                                    }
                                    );
                                BillDetails.Add(billDetail);
                            }
                            string ResponseCode = cmd.Parameters["PO_CODIGO_RESPUESTA"].Value.ToString();
                            string ResponseMessage = cmd.Parameters["PO_MENSAJE_RESPUESTA"].Value.ToString();
                            Tools.Logging.Info(string.Format("Fin ReadConfiguration = codeResponse:{0}, messageResponse:{1}", ResponseCode, ResponseMessage));


                        }
                    }

                }



            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

            return BillDetails;
        }


        public static List<CreditNoteHeader> ReadCreditNoteHeaders(DateTime timestamp, ref bool debeRepetir)
        {
            List<CreditNoteHeader> creditNoteHeaders = new List<CreditNoteHeader>();
            CreditNoteHeader objcreditNoteHeaders = new CreditNoteHeader();
            try
            {
                //using (OracleConnection connection = (OracleConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Oracle))
                using (IfxConnection conn = (IfxConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Informix))
                {
                    //using (OracleCommand cmd = new OracleCommand("PKG_PACIFYC_CONSULTAS.SP_LEER_FACTURA_CAB", connection))
                    using (IfxCommand cmd = new IfxCommand(Tools.Constants.IsisRead_Select_Header, conn))
                    {

                        conn.Open();
                        cmd.CommandType = CommandType.Text;

                        IfxDataReader dr = cmd.ExecuteReader();
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                creditNoteHeaders.Add
                                (
                                    objcreditNoteHeaders = new CreditNoteHeader()
                                    {
                                        serieNumero = dr["serieNumero"].ToString(),
                                        fechaEmision = dr["fechaEmision"].ToString(),
                                        horadeEmision = dr["horadeEmision"].ToString(),
                                        tipoMoneda = dr["tipoMoneda"].ToString(),
                                        numeroDocumentoEmisor = dr["numeroDocumentoEmisor"].ToString(),
                                        tipoDocumentoAdquiriente = dr["tipoDocumentoAdquiriente"].ToString(),
                                        numeroDocumentoAdquiriente = dr["numeroDocumentoAdquiriente"].ToString(),
                                        razonSocialAdquiriente = dr["razonSocialAdquiriente"].ToString(),
                                        tipoReferencia_1 = dr["tipoReferencia_1"].ToString(),
                                        numeroDocumentoReferencia_1 = dr["numeroDocumentoReferencia_1"].ToString(),
                                        tipoReferencia_2 = dr["tipoReferencia_2"].ToString(),
                                        numeroDocumentoReferencia_2 = dr["numeroDocumentoReferencia_2"].ToString(),
                                        totalVVNetoOpNoGravada = (dr["totalVVNetoOpNoGravada"].ToString()),
                                        conceptoVVNetoOpNoGravada = (dr["conceptoVVNetoOpNoGravada"].ToString()),
                                        totalVVNetoOpExoneradas = (dr["totalVVNetoOpExoneradas"].ToString()),
                                        conceptoVVNetoOpExoneradas = (dr["conceptoVVNetoOpExoneradas"].ToString()),
                                        totalVVNetoOpGratuitas = (dr["totalVVNetoOpGratuitas"].ToString()),
                                        conceptoVVNetoOpGratuitas = (dr["conceptoVVNetoOpGratuitas"].ToString()),
                                        totalVVNetoExportacion = (dr["totalVVNetoExportacion"].ToString()),
                                        conceptoVVExportacion = (dr["conceptoVVExportacion"].ToString()),
                                        totalDescuentos = (dr["totalDescuentos"].ToString()),
                                        totalIgv = (dr["totalIgv"].ToString()),
                                        totalVenta = (dr["totalVenta"].ToString()),
                                        leyendas = dr["leyendas"].ToString(),
                                        porcentajeDetraccion = (dr["porcentajeDetraccion"].ToString()),
                                        totalDetraccion = (dr["totalDetraccion"].ToString()),
                                        descripcionDetraccion = dr["descripcionDetraccion"].ToString(),
                                        codigoEstablecimientoSunat = dr["codigoEstablecimientoSunat"].ToString(),
                                        montoTotalImpuestos = (dr["montoTotalImpuestos"].ToString()),
                                        descuentosGlobales = (dr["descuentosGlobales"].ToString()),
                                        sumImpuestosOpGratuitas = (dr["sumImpuestosOpGratuitas"].ToString()),
                                        monRedImportTotal = (dr["monRedImportTotal"].ToString()),
                                        codigoSerieNumeroAfectado = dr["codigoSerieNumeroAfectado"].ToString(),
                                        serieNumeroAfectado = dr["serieNumeroAfectado"].ToString(),
                                        correoAdquiriente = dr["correoAdquiriente"].ToString(),
                                        motivoDocumento = dr["motivoDocumento"].ToString(),
                                        tipoDocRefPrincipal = dr["tipoDocRefPrincipal"].ToString(),
                                        numeroDocRefPrincipal = dr["numeroDocRefPrincipal"].ToString(),
                                        lugarDestino = dr["lugardestino"].ToString(),
                                        totalvalorventanetoopgravadas = dr["TOTALVALORVENTANETOOPGRAVADAS"].ToString(),
                                        codSistema = "02",
                                        codigoCarga = $"NCRE_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                        nombreArchivo = $"NCRE_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                        origen = "MA",
                                        estado = "PE",
                                        fechaRegistro = timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT),
                                        totalRetencion = dr["TOTALRETENCION"].ToString(),
                                        porcentajeRetencion = dr["PORCENTAJERETENCION"].ToString()
                                    }
                                    );
                                creditNoteHeaders.Add(objcreditNoteHeaders);
                                string ResponseCode = cmd.Parameters["PO_CODIGO_RESPUESTA"].Value.ToString();
                                string ResponseMessage = cmd.Parameters["PO_MENSAJE_RESPUESTA"].Value.ToString();
                                Tools.Logging.Info(string.Format("Fin ReadConfiguration = codeResponse:{0}, messageResponse:{1}", ResponseCode, ResponseMessage));

                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return creditNoteHeaders;
        }

        public static List<CreditNoteDetail> ReadCreditNoteDetails(DateTime timestamp)
        {
            List<CreditNoteDetail> creditNoteDetails = new List<CreditNoteDetail>();
            CreditNoteDetail objcreditNoteDetails = new CreditNoteDetail();
            try
            {
                //using (OracleConnection connection = (OracleConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Oracle))
                using (IfxConnection conn = (IfxConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Informix))
                {
                    //using (OracleCommand cmd = new OracleCommand("PKG_PACIFYC_CONSULTAS.SP_LEER_FACTURA_CAB", connection))
                    using (IfxCommand cmd = new IfxCommand(Tools.Constants.IsisRead_Select_Header, conn))
                    {

                        conn.Open();
                        cmd.CommandType = CommandType.Text;

                        IfxDataReader dr = cmd.ExecuteReader();
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                creditNoteDetails.Add
                                (
                                    objcreditNoteDetails = new CreditNoteDetail()
                                    {
                                        serieNumero = dr["serieNumero"].ToString(),
                                        numeroOrdenItem = dr["numeroOrdenItem"].ToString(),
                                        unidadMedida = dr["unidadMedida"].ToString(),
                                        cantidad = dr["cantidad"].ToString(),
                                        codigoProducto = dr["codigoProducto"].ToString(),
                                        codigoProductoSunat = dr["codigoProductoSunat"].ToString(),
                                        descripcion = dr["descripcion"].ToString(),
                                        montoBaseIGV = dr["montoBaseIGV"].ToString(),
                                        importeIGV = dr["importeIGV"].ToString(),
                                        codigoRazonExoneracion = dr["codigoRazonExoneracion"].ToString(),
                                        tasaIGV = dr["tasaIGV"].ToString(),
                                        importeDescuento = dr["importeDescuento"].ToString(),
                                        codigoImporteReferencial = dr["codigoImporteReferencial"].ToString(),
                                        importeReferencial = dr["importeReferencial"].ToString(),
                                        importeUnitarioSinImpuesto = dr["importeUnitarioSinImpuesto"].ToString(),
                                        importeTotalSinImpuesto = dr["importeTotalSinImpuesto"].ToString(),
                                        montoTotalImpuestoItem = dr["montoTotalImpuestoItem"].ToString(),
                                        codigoImpUnitConImpuesto = dr["codigoImpUnitConImpuesto"].ToString(),
                                        importeUnitarioConImpuesto = dr["importeUnitarioConImpuesto"].ToString(),
                                        numeroExpediente = dr["numeroExpediente"].ToString(),
                                        codigoUnidadEjecutora = dr["codigoUnidadEjecutora"].ToString(),
                                        numeroContrato = dr["numeroContrato"].ToString(),
                                        numeroProcesoSeleccion = dr["numeroProcesoSeleccion"].ToString(),
                                        codSistema = "02"
                                    }
                                    );
                                creditNoteDetails.Add(objcreditNoteDetails);
                                string ResponseCode = cmd.Parameters["PO_CODIGO_RESPUESTA"].Value.ToString();
                                string ResponseMessage = cmd.Parameters["PO_MENSAJE_RESPUESTA"].Value.ToString();
                                Tools.Logging.Info(string.Format("Fin ReadConfiguration = codeResponse:{0}, messageResponse:{1}", ResponseCode, ResponseMessage));


                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return creditNoteDetails;
        }

        public static List<DebitNoteHeader> ReadDebitNoteHeader(DateTime timestamp, ref bool debeRepetir)
        {
            List<DebitNoteHeader> debitNoteHeader = new List<DebitNoteHeader>();
            DebitNoteHeader objdebitNoteHeader = new DebitNoteHeader();
            try
            {
                //using (OracleConnection connection = (OracleConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Oracle))
                using (IfxConnection conn = (IfxConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Informix))
                {
                    //using (OracleCommand cmd = new OracleCommand("PKG_PACIFYC_CONSULTAS.SP_LEER_FACTURA_CAB", connection))
                    using (IfxCommand cmd = new IfxCommand(Tools.Constants.IsisRead_Select_Header, conn))
                    {

                        conn.Open();
                        cmd.CommandType = CommandType.Text;

                        IfxDataReader dr = cmd.ExecuteReader();
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                debitNoteHeader.Add
                                (
                                    objdebitNoteHeader = new DebitNoteHeader()
                                    {
                                        serieNumero = dr["serieNumero"].ToString(),
                                        fechaEmision = dr["fechaEmision"].ToString(),
                                        horadeEmision = dr["horadeEmision"].ToString(),
                                        tipoMoneda = dr["tipoMoneda"].ToString(),
                                        numeroDocumentoEmisor = dr["numeroDocumentoEmisor"].ToString(),
                                        tipoDocumentoAdquiriente = dr["tipoDocumentoAdquiriente"].ToString(),
                                        numeroDocumentoAdquiriente = dr["numeroDocumentoAdquiriente"].ToString(),
                                        razonSocialAdquiriente = dr["razonSocialAdquiriente"].ToString(),
                                        tipoReferencia_1 = dr["tipoReferencia_1"].ToString().Trim(),
                                        numeroDocumentoReferencia_1 = dr["numeroDocumentoReferencia_1"].ToString(),
                                        tipoReferencia_2 = dr["tipoReferencia_2"].ToString(),
                                        numeroDocumentoReferencia_2 = dr["numeroDocumentoReferencia_2"].ToString(),
                                        totalVVNetoOpNoGravada = dr["totalVVNetoOpNoGravada"].ToString(),
                                        conceptoVVNetoOpNoGravada = dr["conceptoVVNetoOpNoGravada"].ToString(),
                                        totalVVNetoOpExoneradas = dr["totalVVNetoOpExoneradas"].ToString(),
                                        conceptoVVNetoOpExoneradas = dr["conceptoVVNetoOpExoneradas"].ToString(),
                                        totalVVNetoOpGratuitas = dr["totalVVNetoOpGratuitas"].ToString(),
                                        conceptoVVNetoOpGratuitas = dr["conceptoVVNetoOpGratuitas"].ToString(),
                                        totalVVNetoExportacion = dr["totalVVNetoExportacion"].ToString(),
                                        conceptoVVExportacion = dr["conceptoVVExportacion"].ToString(),
                                        totalDescuentos = dr["totalDescuentos"].ToString(),
                                        totalIgv = dr["totalIgv"].ToString(),
                                        totalVenta = dr["totalVenta"].ToString(),
                                        porcentajeDetraccion = dr["porcentajeDetraccion"].ToString(),
                                        totalDetraccion = dr["totalDetraccion"].ToString(),
                                        descripcionDetraccion = dr["descripcionDetraccion"].ToString(),
                                        codigoEstablecimientoSunat = dr["codigoEstablecimientoSunat"].ToString(),
                                        montoTotalImpuestos = dr["montoTotalImpuestos"].ToString(),
                                        descuentosGlobales = dr["descuentosGlobales"].ToString(),
                                        sumImpuestosOpGratuitas = dr["sumImpuestosOpGratuitas"].ToString(),
                                        monRedImportTotal = dr["monRedImportTotal"].ToString(),
                                        codigoSerieNumeroAfectado = dr["codigoSerieNumeroAfectado"].ToString(),
                                        serieNumeroAfectado = dr["serieNumeroAfectado"].ToString(),
                                        correoAdquiriente = dr["correoAdquiriente"].ToString(),
                                        motivoDocumento = dr["motivoDocumento"].ToString(),
                                        tipoDocRefPrincipal = dr["tipoDocRefPrincipal"].ToString(),
                                        numeroDocRefPrincipal = dr["numeroDocRefPrincipal"].ToString(),
                                        totalvalorventanetoopgravadas = dr["TOTALVALORVENTANETOOPGRAVADAS"].ToString(),
                                        codSistema = "02",
                                        codigoCarga = $"NDEB_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                        nombreArchivo = $"NDEB_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                        origen = "MA",
                                        estado = "PE",
                                        fechaRegistro = timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT),
                                        totalRetencion = dr["TOTALRETENCION"].ToString(),
                                        porcentajeRetencion = dr["PORCENTAJERETENCION"].ToString()
                                    }
                                    );
                                debitNoteHeader.Add(objdebitNoteHeader);
                                string ResponseCode = cmd.Parameters["PO_CODIGO_RESPUESTA"].Value.ToString();
                                string ResponseMessage = cmd.Parameters["PO_MENSAJE_RESPUESTA"].Value.ToString();
                                Tools.Logging.Info(string.Format("Fin ReadConfiguration = codeResponse:{0}, messageResponse:{1}", ResponseCode, ResponseMessage));


                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return debitNoteHeader;
        }

        public static List<DebitNoteDetail> ReadDebitNoteDetails(DateTime timestamp)
        {
            List<DebitNoteDetail> debitNoteDetails = new List<DebitNoteDetail>();
            DebitNoteDetail objdebitNoteDetails = new DebitNoteDetail();
            try
            {
                //using (OracleConnection connection = (OracleConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Oracle))
                using (IfxConnection conn = (IfxConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Informix))
                {
                    //using (OracleCommand cmd = new OracleCommand("PKG_PACIFYC_CONSULTAS.SP_LEER_FACTURA_CAB", connection))
                    using (IfxCommand cmd = new IfxCommand(Tools.Constants.IsisRead_Select_Header, conn))
                    {

                        conn.Open();
                        cmd.CommandType = CommandType.Text;

                        IfxDataReader dr = cmd.ExecuteReader();
                        if (dr != null && dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                debitNoteDetails.Add
                                (
                                    objdebitNoteDetails = new DebitNoteDetail()
                                    {
                                        serieNumero = dr["serieNumero"].ToString(),
                                        numeroOrdenItem = dr["numeroOrdenItem"].ToString(),
                                        unidadMedida = dr["unidadMedida"].ToString(),
                                        cantidad = dr["cantidad"].ToString(),
                                        codigoProducto = dr["codigoProducto"].ToString(),
                                        codigoProductoSunat = dr["codigoProductoSunat"].ToString(),
                                        descripcion = dr["descripcion"].ToString(),
                                        montoBaseIGV = dr["montoBaseIGV"].ToString(),
                                        importeIGV = dr["importeIGV"].ToString(),
                                        codigoRazonExoneracion = dr["codigoRazonExoneracion"].ToString(),
                                        tasaIGV = dr["tasaIGV"].ToString(),
                                        importeDescuento = dr["importeDescuento"].ToString(),
                                        importeUnitarioSinImpuesto = dr["importeUnitarioSinImpuesto"].ToString(),
                                        importeTotalSinImpuesto = dr["importeTotalSinImpuesto"].ToString(),
                                        montoTotalImpuestoItem = dr["montoTotalImpuestoItem"].ToString(),
                                        codigoImpUnitConImpuesto = dr["codigoImpUnitConImpuesto"].ToString(),
                                        importeUnitarioConImpuesto = dr["importeUnitarioConImpuesto"].ToString(),
                                        numeroExpediente = dr["numeroExpediente"].ToString(),
                                        codigoUnidadEjecutora = dr["codigoUnidadEjecutora"].ToString(),
                                        numeroContrato = dr["numeroContrato"].ToString(),
                                        codSistema = "02",
                                        codigoCarga = $"NDEB_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}",
                                        nombreArchivo = $"NDEB_{timestamp.ToString(Tools.Constants.DATETIME_FORMAT_AUDIT)}"


                                    }
                                    );
                                debitNoteDetails.Add(objdebitNoteDetails);
                                string ResponseCode = cmd.Parameters["PO_CODIGO_RESPUESTA"].Value.ToString();
                                string ResponseMessage = cmd.Parameters["PO_MENSAJE_RESPUESTA"].Value.ToString();
                                Tools.Logging.Info(string.Format("Fin ReadConfiguration = codeResponse:{0}, messageResponse:{1}", ResponseCode, ResponseMessage));

                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return debitNoteDetails;

        }

        public static void InsertInvoices(List<InvoiceHeader> invoiceHeaders)
        {
            try
            {
                using (SqlConnection connection = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    using (SqlBulkCopy objbulk = new SqlBulkCopy(connection))
                    {

                        //assign Destination table name  
                        objbulk.DestinationTableName = "Factura_Cabecera_Temporal";


                        foreach (var property in typeof(InvoiceHeader).GetMembers().Where(x => x.MemberType == System.Reflection.MemberTypes.Property).ToList())
                        {
                            objbulk.ColumnMappings.Add(property.Name, property.Name);
                        }

                        connection.Open();
                        //insert bulk Records into DataBase.  
                        objbulk.WriteToServer(Tools.Common.ConvertToDataTable(invoiceHeaders));
                    }
                }



            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void InsertInvoiceDetails(List<InvoiceDetail> invoiceDetails)
        {
            try
            {
                using (SqlConnection connection = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    using (SqlBulkCopy objbulk = new SqlBulkCopy(connection))
                    {

                        //assign Destination table name  
                        objbulk.DestinationTableName = "Factura_Detalle_Temporal";

                        // int xx = 0;
                        foreach (var property in typeof(InvoiceDetail).GetMembers().Where(x => x.MemberType == System.Reflection.MemberTypes.Property).ToList())
                        {
                            //if (xx > 0) break;
                            objbulk.ColumnMappings.Add(property.Name, property.Name);
                            // xx++;
                        }

                        connection.Open();
                        //insert bulk Records into DataBase.  
                        objbulk.WriteToServer(Tools.Common.ConvertToDataTable(invoiceDetails));
                    }
                }



            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void UpdatePickupDate(List<string> listSerialNumbers, String tableName)
        {
            try
            {
                //using (OracleConnection connection = (OracleConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Oracle))
                using (IfxConnection conn = (IfxConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Informix))
                {

                    //using (OracleCommand cmd = connection.CreateCommand())
                    using (IfxCommand cmd = conn.CreateCommand())
                    {
                        conn.Open();

                        cmd.CommandText = Tools.Common.GetCommandTextBulkInsert("TEMP_SERIES", "serie");

                        //cmd.ArrayBindCount = listSerialNumbers.Count;
                        

                        //cmd.Parameters.Add(new OracleParameter() { OracleDbType = OracleDbType.Varchar2, Value = listSerialNumbers.ToArray() });
                        cmd.Parameters.Add(new IfxParameter() { IfxType = IfxType.VarChar, Value = listSerialNumbers.ToArray() });

                        cmd.ExecuteNonQuery();
                        String ser = String.Join("','", listSerialNumbers);

                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            
        }
        
        public static void InvokeUpdate(Int32 ar)
        {
            try
            {
                //using (OracleConnection connection = (OracleConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Oracle))
                using (IfxConnection conn = (IfxConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.Informix))
                {
                    if (ar ==1) { 
                    //using (OracleCommand cmd = new OracleCommand("PKG_PACIFYC_TRANSACCIONES.SP_ACTUALIZAR_FECH_RECOJO_FACT", connection))
                    using (IfxCommand cmd = new IfxCommand("update fact_fe01_cab set fecharecojo= systdate() where serienumero in (ser)", conn))
                    {
                        conn.Open();
                        //cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandType = CommandType.Text;
                        IfxParameter pmtDate = new IfxParameter("PI_FECHA", IfxType.VarChar, 50);
                        IfxParameter pmtAffectedRows = new IfxParameter("PO_FILAS_AFECTADAS", IfxType.Integer);
                        IfxParameter pmtMsgResponse = new IfxParameter("PO_MENSAJE_RESPUESTA", IfxType.VarChar, 3000);
                        IfxParameter pmtResponseCode = new IfxParameter("PO_CODIGO_RESPUESTA", IfxType.VarChar, 50);


                        pmtDate.Value = DateTime.Now.ToString("yyyy-MM-dd");
                        cmd.Parameters.Add(pmtDate);
                        cmd.Parameters.Add(pmtAffectedRows);
                        cmd.Parameters.Add(pmtMsgResponse);
                        cmd.Parameters.Add(pmtResponseCode);
                        cmd.ExecuteNonQuery();

                        string messageResponse = cmd.Parameters["PO_MENSAJE_RESPUESTA"].ToString();
                        string updatesInvoices = cmd.Parameters["PO_FILAS_AFECTADAS"].ToString();

                        Tools.Logging.Info($"Actualización masiva de la fecha de recojo Isis - Facturas: {messageResponse} - filas afectadas:{updatesInvoices}");

                    }
                    }
                    if (ar == 2)
                    {
                        //using (OracleCommand cmd = new OracleCommand("PKG_PACIFYC_TRANSACCIONES.SP_ACTUALIZAR_FECH_RECOJO_FACT", connection))
                        using (IfxCommand cmd = new IfxCommand("update fact_fe07_cab set fecharecojo= systdate() where serienumero in (ser)", conn))
                        {
                            conn.Open();
                            //cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandType = CommandType.Text;
                            IfxParameter pmtDate = new IfxParameter("PI_FECHA", IfxType.VarChar, 50);
                            IfxParameter pmtAffectedRows = new IfxParameter("PO_FILAS_AFECTADAS", IfxType.Integer);
                            IfxParameter pmtMsgResponse = new IfxParameter("PO_MENSAJE_RESPUESTA", IfxType.VarChar, 3000);
                            IfxParameter pmtResponseCode = new IfxParameter("PO_CODIGO_RESPUESTA", IfxType.VarChar, 50);


                            pmtDate.Value = DateTime.Now.ToString("yyyy-MM-dd");
                            cmd.Parameters.Add(pmtDate);
                            cmd.Parameters.Add(pmtAffectedRows);
                            cmd.Parameters.Add(pmtMsgResponse);
                            cmd.Parameters.Add(pmtResponseCode);
                            cmd.ExecuteNonQuery();

                            string messageResponse = cmd.Parameters["PO_MENSAJE_RESPUESTA"].ToString();
                            string updatesInvoices = cmd.Parameters["PO_FILAS_AFECTADAS"].ToString();

                            Tools.Logging.Info($"Actualización masiva de la fecha de recojo Isis - Nota de Credito: {messageResponse} - filas afectadas:{updatesInvoices}");

                        }
                    }
                    if (ar == 3)
                    {
                        //using (OracleCommand cmd = new OracleCommand("PKG_PACIFYC_TRANSACCIONES.SP_ACTUALIZAR_FECH_RECOJO_FACT", connection))
                        using (IfxCommand cmd = new IfxCommand("update fact_fe08_cab set fecharecojo= systdate() where serienumero in (ser)", conn))
                        {
                            conn.Open();
                            //cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandType = CommandType.Text;
                            IfxParameter pmtDate = new IfxParameter("PI_FECHA", IfxType.VarChar, 50);
                            IfxParameter pmtAffectedRows = new IfxParameter("PO_FILAS_AFECTADAS", IfxType.Integer);
                            IfxParameter pmtMsgResponse = new IfxParameter("PO_MENSAJE_RESPUESTA", IfxType.VarChar, 3000);
                            IfxParameter pmtResponseCode = new IfxParameter("PO_CODIGO_RESPUESTA", IfxType.VarChar, 50);


                            pmtDate.Value = DateTime.Now.ToString("yyyy-MM-dd");
                            cmd.Parameters.Add(pmtDate);
                            cmd.Parameters.Add(pmtAffectedRows);
                            cmd.Parameters.Add(pmtMsgResponse);
                            cmd.Parameters.Add(pmtResponseCode);
                            cmd.ExecuteNonQuery();

                            string messageResponse = cmd.Parameters["PO_MENSAJE_RESPUESTA"].ToString();
                            string updatesInvoices = cmd.Parameters["PO_FILAS_AFECTADAS"].ToString();

                            Tools.Logging.Info($"Actualización masiva de la fecha de recojo Isis - Nota de Debito: {messageResponse} - filas afectadas:{updatesInvoices}");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }

        }
        





    }
}
