using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Entities.Isis
{
    public class InvoiceDetail
    {
        public string serieNumero { get; set; }
        public string numeroOrdenItem { get; set; }
        public string unidadMedida { get; set; }
        public int cantidad { get; set; }
        public string CodigoProducto { get; set; }
        public string codigoProductoSunat { get; set; }
        public string descripcion { get; set; }
        public string montoBaseIGV { get; set; }
        public double importeIGV { get; set; }
        public string codigoRazonExoneracion { get; set; }
        public double tasaIGV { get; set; }
        public double importeDescuento { get; set; }
        public string codigoDescuento { get; set; }
        public double factorDescuento { get; set; }
        public double montoBaseDescuento { get; set; }
        public string codigoImporteReferencial { get; set; }
        public double importeReferencial { get; set; }
        public double importeUnitarioSinImpuesto { get; set; }
        public double importeTotalSinImpuesto { get; set; }
        public double montoTotalImpuestoItem { get; set; }
        public string codigoImpUnitConImpuesto { get; set; }
        public double importeUnitarioConImpuesto { get; set; }
        public string numeroExpediente { get; set; }
        public string codigoUnidadEjecutora { get; set; }
        public string numeroContrato { get; set; }
        public string numeroProcesoSeleccion { get; set; }

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
