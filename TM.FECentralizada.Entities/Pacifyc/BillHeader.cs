using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Entities.Pacifyc
{
    public class BillHeader
    {
        public string serieNumero { get; set; }
        public string fechaEmision { get; set; }
        public string horadeEmision { get; set; }
        public string tipoMoneda { get; set; }
        public double numeroDocumentoEmisor { get; set; }
        public string tipoDocumentoAdquiriente { get; set; }
        public string numeroDocumentoAdquiriente { get; set; }
        public string razonSocialAdquiriente { get; set; }
        public string direccionAdquiriente { get; set; }
        public string tipoReferencia_1 { get; set; }
        public string numeroDocumentoReferencia_1 { get; set; }
        public string tipoReferencia_2 { get; set; }
        public string numeroDocumentoReferencia_2 { get; set; }
        public double totalVVNetoOpGravadas { get; set; }
        public double totalVVNetoOpNoGravada { get; set; }
        public double conceptoVVNetoOpNoGravada { get; set; }
        public double totalVVNetoOpExoneradas { get; set; }
        public double conceptoVVNetoOpExoneradas { get; set; }
        public double totalVVNetoOpGratuitas { get; set; }
        public double conceptoVVNetoOpGratuitas { get; set; }
        public double totalVVNetoExportacion { get; set; }
        public double conceptoVVExportacion { get; set; }
        public double totalDescuentos { get; set; }
        public double totalIgv { get; set; }
        public double totalVenta { get; set; }
        public string tipoOperacion { get; set; }
        public string leyendas { get; set; }
        public string textoleyenda_3 { get; set; }
        public string textoleyenda_4 { get; set; }
        public double porcentajeDetraccion { get; set; }
        public double totalDetraccion { get; set; }
        public string descripcionDetraccion { get; set; }
        public string ordenCompra { get; set; }
        public string datosAdicionales { get; set; }
        public string codigoEstablecimientoSunat { get; set; }
        public double montoTotalImpuestos { get; set; }
        public string cdgCodigoMotivo { get; set; }
        public double cdgPorcentaje { get; set; }
        public double descuentosGlobales { get; set; }
        public double cdgMontoBaseCargo { get; set; }
        public double sumImpuestosOpGratuitas { get; set; }
        public double totalValorVenta { get; set; }
        public double totalPrecioVenta { get; set; }
        public double monRedImportTotal { get; set; }
        // ADICIONALES                                  
        public int codSistema { get; set; }
        public string codigoCarga { get; set; }
        public string estado { get; set; }
        public string nombreArchivo { get; set; }
        public string fechaCarga { get; set; }
        public string fechaEnvio { get; set; }
        public string origen { get; set; }
        public string fechaRegistro { get; set; }
    }
}
