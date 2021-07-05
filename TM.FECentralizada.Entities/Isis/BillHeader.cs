using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Entities.Isis
{
    public class BillHeader
    {
        public string codSistema { get; set; }
        public string serieNumero { get; set; }
        public string codigoCarga { get; set; }
        public string numeroDocumentoEmisor { get; set; }
        public string fechaEmision { get; set; }
        public string Horadeemision { get; set; }
        public string tipoMoneda { get; set; }
        public string tipoDocumentoAdquiriente { get; set; }
        public string numeroDocumentoAdquiriente { get; set; }
        public string razonSocialAdquiriente { get; set; }
        public string direccionAdquiriente { get; set; }
        public string tipoReferencia_1 { get; set; }
        public string numeroDocumentoReferencia_1 { get; set; }
        public string tipoReferencia_2 { get; set; }
        public string numeroDocumentoReferencia_2 { get; set; }
        public string totalVVNetoOpGravadas { get; set; }
        public string totalVVNetoOpNoGravada { get; set; }
        public string conceptovvnetoopnogravada { get; set; }
        public string totalVVNetoOpExoneradas { get; set; }
        public string conceptovvnetoopexoneradas { get; set; }
        public string totalVVNetoOpGratuitas { get; set; }
        public string conceptovvnetoopgratuitas { get; set; }
        public string totalVVNetoExportacion { get; set; }
        public string conceptovvexportacion { get; set; }
        public string totalDescuentos { get; set; }
        public string totalIgv { get; set; }
        public string totalVenta { get; set; }
        public string tipooperacion { get; set; }
        public string leyendas { get; set; }
        public string datosAdicionales { get; set; }
        public string codigoestablecimientosunat { get; set; }
        public string montototalimpuestos { get; set; }
        public string cdgcodigomotivo { get; set; }
        public string cdgporcentaje { get; set; }
        public string descuentosGlobales { get; set; }
        public string cdgmontobasecargo { get; set; }
        public string sumimpuestosopgratuitas { get; set; }
        public string totalvalorventa { get; set; }
        public string totalprecioventa { get; set; }
        public string monredimporttotal { get; set; }
        public string estado { get; set; }
        public string fechaEnvio { get; set; }
        public string origen { get; set; }
        public string fechaRegistro { get; set; }
        public string nombreArchivo { get; set; }
        public string codigoRetorno { get; set; }
        public string nombreArchivoAlignet { get; set; }
        public string nombreArchivoRespuestaAlignet { get; set; }
        public string fechaRespuestaAlignet { get; set; }
        public string nombreArchivoRespuestaLegado { get; set; }
        public string fechaRespuestaLegado { get; set; }
        public string estadoSunat { get; set; }
        public string codigoSunat { get; set; }
        public string mensajeSunat { get; set; }
    }
}
