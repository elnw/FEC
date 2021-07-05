using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Entities.Pacifyc
{
    public class CreditNoteHeader
    {
        public string serieNumero { get; set; }

        public string fechaEmision { get; set; }
        public string horadeEmision { get; set; }
        public string tipoMoneda { get; set; }
        public string numeroDocumentoEmisor { get; set; }
        public string tipoDocumentoAdquiriente { get; set; }
        public string numeroDocumentoAdquiriente { get; set; }
        public string razonSocialAdquiriente { get; set; }
        public string tipoReferencia_1 { get; set; }
        public string numeroDocumentoReferencia_1 { get; set; }
        public string tipoReferencia_2 { get; set; }
        public string numeroDocumentoReferencia_2 { get; set; }
        public string totalVVNetoOpNoGravada { get; set; }
        public string conceptoVVNetoOpNoGravada { get; set; }
        public string totalVVNetoOpExoneradas { get; set; }
        public string conceptoVVNetoOpExoneradas { get; set; }
        public string totalVVNetoOpGratuitas { get; set; }
        public string conceptoVVNetoOpGratuitas { get; set; }
        public string totalVVNetoExportacion { get; set; }
        public string conceptoVVExportacion { get; set; }
        public string totalDescuentos { get; set; }
        public string totalIgv { get; set; }
        public string totalVenta { get; set; }
        public string leyendas { get; set; }
        public string porcentajeDetraccion { get; set; }
        public string totalDetraccion { get; set; }
        public string descripcionDetraccion { get; set; }
        public string camposAdicionales { get; set; }
        public string codigoEstablecimientoSunat { get; set; }
        public string montoTotalImpuestos { get; set; }
        public string descuentosGlobales { get; set; }
        public string sumImpuestosOpGratuitas { get; set; }
        public string monRedImportTotal { get; set; }
        public string codigoSerieNumeroAfectado { get; set; }
        public string serieNumeroAfectado { get; set; }
        public string correoAdquiriente { get; set; }
        public string motivoDocumento { get; set; }
        public string tipoDocRefPrincipal { get; set; }
        public string numeroDocRefPrincipal { get; set; }
        public string lugarDestino { get; set; }
        public string totalvalorventanetoopgravadas { get; set; }
        public string origen { get; set; }
        public string estado { get; set; }
        public string codSistema { get; set; }
        public string codigoCarga { get; set; }
        public string nombreArchivo { get; set; }
        public string totalRetencion { get; set; }
        public string porcentajeRetencion { get; set; }
        public string fechaRegistro { get; set; }
        public string fechaRespuestaLegado { get; set; }
        public string estadoSunat { get; set; }
        public string codigoSunat { get; set; }
        public string mensajeSunat { get; set; }
    }
}
