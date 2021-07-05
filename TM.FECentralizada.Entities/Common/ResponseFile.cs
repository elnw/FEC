using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Entities.Common
{
    public class ResponseFile
    {
        public string estado{get;set;}
        public string numDocEmisor{get;set;}
        public string tipoDocumento{get;set;}
        public string serieNumero{get;set;}
        public string codigoSunat{get;set;}
        public string mensajeSunat{get;set;}
        public string fechaDeclaracion{get;set;}
        public string fechaEmision{get;set;}
        public string firma{get;set;}
        public string resumen{get;set;}
        public string adicional1{get;set;}
        public string adicional2{get;set;}
        public string adicional3{get;set;}
        public string adicional4{get;set;}
        public string adicional5{get;set;}
        public string codSistema{get;set;}
    }
}
