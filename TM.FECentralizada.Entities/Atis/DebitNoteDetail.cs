using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Entities.Atis
{
    public class DebitNoteDetail
    {
        public string serieNumero { get; set; }
        public string numeroOrdenItem { get; set; }
        public string unidadMedida { get; set; }
        public string cantidad { get; set; }
        public string codigoProducto { get; set; }
        public string codigoProductoSunat { get; set; }
        public string descripcion { get; set; }
        public string montoBaseIGV { get; set; }
        public string importeIGV { get; set; }
        public string codigoRazonExoneracion { get; set; }
        public string tasaIGV { get; set; }
        public string importeDescuento { get; set; }
        public string importeUnitarioSinImpuesto { get; set; }
        public string importeTotalSinImpuesto { get; set; }
        public string montoTotalImpuestoItem { get; set; }
        public string codigoImpUnitConImpuesto { get; set; }
        public string importeUnitarioConImpuesto { get; set; }
        public string numeroExpediente { get; set; }
        public string codigoUnidadEjecutora { get; set; }
        public string numeroContrato { get; set; }
        public string codSistema { get; set; }
        public string codigoCarga { get; set; }
        public string nombreArchivo { get; set; }
        public string codigoImporteReferencial { get; set; }
        public string importeReferencial { get; set; }
    }
}
