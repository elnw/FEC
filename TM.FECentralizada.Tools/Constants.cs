namespace TM.FECentralizada.Tools
{
    public static class Constants
    {
        #region AppSettings

        #region File Server
        public const string PRM_SFTP_SAP = "PRM_FILE_SERVER_SAP";
        public const string PRM_SFTP_LETTER_OSIPTEL = "PRM_FILE_SERVER_LETTER_OSIPTEL";
        public const string PRM_SFTP_VOUCHER = "PRM_FILE_SERVER_VOUCHER";
        public const string PRM_OPTIONS_DETAIL = "PRM_OPTIONS_DETAIL";

        public const char FIELD_SEPARATOR = '|';

        #endregion

        #region Reports 
        public const string RPT_NAME_OSIPTEL = "Osiptel";
        public const string RPT_NAME_CONSULT = "ConsultaImei";
        public const string RPT_NAME_INVALID = "ImeiInvalidos";
        public const string RPT_NAME_TRAY_LN = "ReporteDeBandejaLN";
        public const string RPT_NAME_HISTORY = "ConsultaHistóricoImei";
        public const string RPT_NAME_INOUT_SUCCESS_LN = "ErroresArchivoLNE";
        public const string RPT_NAME_INOUT_ERROR_LN = "ErroresArchivoLNE";
        public const string RPT_COLUMNS_INVALID = @"IMEI|Motivo";

        public const string RPT_COLUMNS_CONSULT = @"Operador|Imei|Estado|Motivo|Fecha Registro|Línea|Tipo Documento Titular|N° Documento Titular|Titular|Tipo Documento Rep|N° Documento Rep|Reportante|Sistema|Usuario|Fecha Auditoria";

        public const string RPT_COLUMNS_HISTORY = @"Fecha Registro|Operador|Imei|Estado|Motivo|Línea|Tipo Documento Titular|N° Documento Titular|Titular|Tipo Documento Rep|N° Documento Rep|Reportante|Sistema|Usuario|Fecha Auditoria";

        public const string RPT_COLUMNS_OSIPTEL = @"ARCHIVO|NUMERO_DE_FILA|CONCESIONARIO|NUMERO|IMSI|IMEI|MARCA|MODELO|NUMERO_REPORTA_HECHO|FUENTE_DEL_REPORTE|MOTIVO_DEL_REPORTE|CODIGO_DEL_REPORTE|FECHA_Y_HORA_DEL_REPOTE|FECHA_Y_HORA_DEL_BLOQUEO_O_DESBLOQUEO|NOMBRES_DEL_ABONADO|APELLIDO_PATERNO_DEL_ABONADO|APELLIDO_MATERNO_DEL_ABONADO|TIPO_DE_DOCUMENTO|NRO_DE_DOCUMENTO|NOMBRE_REPRESENTANTE_LEGAL|APELLIDO_PATERNO_REPRESENTANTE_LEGAL|APELLIDO_MATERNO_REPRESENTANTE_LEGAL|TIPO_DOCUMENTO_REPRESENTANTE_LEGAL|NRO_DE_DOCUMENTO_REPRESENTANTE_LEGAL|ID_ERROR_OSIPTEL|CAMPO_CON_ERROR|COD_ERROR|ERROR|COMENTARIO_OSIPTEL";

        public const string RPT_COLUMNS_INOUTBLACKLIST = @"Tipo Equipo|Imei|Fecha de Registro|Estado|Operador|Tecnología|Motivo|Área|Usuario Registro|Fecha Alta|Fecha Baja|Usuario Baja|Sistema|Marca Equipo|Modelo Equipo|Tipo Documento Titular|N° Documento Titular|Titular|Tipo Documento Rep|N° Documento Rep|Reportante|Teléfono|Order ID|Observaciones|Perfil";

        public const string CHAR_EXCEL_RANGE = @"A|B|C|D|E|F|G|H|I|J|K|L|M|N|O|P|Q|R|S|T|U|V|W|X|Y|Z|AA|AB|AC|AD|AE|AF|AG|AH|AI|AJ|AK|AL|AM|AN|AO|AP|AQ|AR|AS|AT|AU|AV|AW|AX|AY|AZ|BA|BB|BC|BD|BE|BF|BG|BH|BI|BJ|BK|BL|BM|BN|BO|BP|BQ|BR|BS|BT|BU|BV|BW|BX|BY|BZ";

        #endregion

        #region Bulk Isert
        public const string BULK_INPUT_TABLE_NAME = "LNE_OSIPTEL_TEMP";
        public const string BULK_INPUT_COLUMNS = @"FILA,CONCESIONARIO,NUMERO,IMSI,IMEI,MARCA,MODELO,NUMERO_REPORTA_HECHO,FUENTE_DEL_REPORTE,MOTIVO_DEL_REPORTE,CODIGO_DEL_REPORTE,FECHA_REPORTE,FECHA_BLOQUEO_DESBLOQUEO,NOMBRES_DEL_ABONADO,APELLIDO_PATERNO_DEL_ABONADO,APELLIDO_MATERNO_DEL_ABONADO,RAZON_SOCIAL,TIPO_DE_DOCUMENTO_LEGAL,NUMERO_DE_DOCUMENTO_LEGAL,NOMBRES_REPRESENTANTE,APELLIDO_PATERNO_REPRESENTANTE,APELLIDO_MATERNO_REPRESENTANTE,TIPO_DOCUMENTO_REPRESENTANTE,N_DOCUMENTO_REPRESENTANTE,FECHA_REGISTRO";

        public const string BULK_OUTPUT_TABLE_NAME = "LNE_ERRORES_OSIPTEL_TEMP";
        public const string BULK_OUTPUT_COLUMNS = @"FILA,CORRELATIVO,COD_ERROR,DETALLE,FECHA_REGISTRO";

        public const string INPUT_COLUMN_SEPARATOR = "INPUT_COLUMN_SEPARATOR";
        #endregion

        #region Application
        public const string FILE_SERVER = "FILE_SERVER"; 
        public const string SPEC_FILE_OUT = "SPEC_ARCHIVO_OUT"; 
        public const string SPEC_FILE_IN = "SPEC_ARCHIVO_IN";
        public const string MAIL_SERVER = "MAIL_SERVER";

        public const string AFFILIATIONS = "AFILIACIONES";
        public const string CHARGES = "CARGOS";
        public const string PAYMENT = "PAGOS";
        public const string REJECTED = "RECHAZOS";
        public const string RESPONSE_PAYMENT = "RESPUESTA_OPERACIONAL";
        public const string PAYMENT_ALERT = "ALERTA_PAGOS";

        public const string ERROR_SUBJECT_EXCEPTION = "ERROR : Proceso de Reportes - Débito Automático.";
        public const string ERROR_SUBJECT_PARAMETERS = "ERROR : Proceso de Reportes - Débito Automático.";
        public const string ERROR_MSG_PARAMETERS = "Ocurrió un error al Obtener los parámetros de Configuración."; 
        public const string ERROR_SUBJECT_NOT_INFORMATION = "ERROR : Proceso de Reportes - Débito Automático.";
        public const string INFO_SUBJECT_NOT_INFORMATION = "INFO : Proceso de Reportes - Débito Automático.";
        public const string ERROR_MSG_NOT_INFORMATION_BD = "Ocurrió un error al Obtener la Información de la BD.";
        public const string ERROR_MSG_NOT_INFORMATION_FS = "Ocurrió un error al Obtener la Información del File Server.";
        public const string ERROR_MSG_NOT_INFORMATION_AMD = "Ocurrió un error al Realizar el cruce de Información [RI - AMDOCS].";
        public const string INFO_MSG_NOT_INFORMATION = "Se envía adjunto el Reporte de Débito Automático.";
        public const string PRM_RPT_MAIL_CONFIG = "RPT_MAIL_CONFIG"; 
        //public const string PRM_RPT_MAIL_CONFIG = "RPT_MAIL_CONFIG";


        public const string DATETIME_FORMAT_SESSION = "yyyyMMddHHmmss";
        public const string APPLICATION = "APPLICATION";
        public const string APPLICATION_NAME = "LISTA NEGRA DE EQUIPOS";
        public const string PROCESS_NAME_REJECTED = "REPORTE DE IMEIS RECHAZADOS POR OSIPTEL";
        public const string USER_NAME = "USERNAME";
        public const string SESSION = "SESSION";
        public const string TRANSACTION = "TRANSACTION";
        public const string FILE_EXT_LOCAL_INPUT = "FILE_EXT_LOCAL_INPUT";
        public const string PREFIX_NUMBER = "PREFIX_NUMBER";

        #endregion

        #region File Audit
        public const string DATETIME_FORMAT_AUDIT = "yyyyMMddHHmmss";
        public const string DATETIME_FORMAT_SQL = "yyyy-MM-ddTHH:mm:ss";
        public const string STATE_PROCESSING = "PROCESANDO";
        public const string STATE_SUCCESS = "COMPLETADO";
        public const string STATE_ERROR = "ERROR";
        public const string STATE_OK = "OK";
        public const string NULL_EMPTY_INT = "-1";
        public const string NULL_EMPTY = "EL CAMPO ES NULO O VACÍO";
        public const string HASH_ALPH = "HASH_ALPH";
        public const string COUNT_ID = "COUNT_ID";
        #endregion

        #region SapLectura
        public const string SapRead = "Sap_Lectura";
        public const string SapRead_Invoice = "Factura";
        public const string SapRead_Bill = "Boleta";
        public const string SapRead_CreditNote = "NotaCredito";
        public const string SapRead_DebitNote = "DebitNote";
        #endregion

        #region IsisLectura
        public const string IsisRead = "Isis_Lectura";
        public const string IsisRead_Bill = "Factura";
        public const string IsisRead_FileServer_GFiscal = "FileServer_Gfiscal";

        public const string IsisRead_Select_Header = @"Select
                                                            serieNumero,
                                                            fechaEmision,
                                                            horadeEmision,
                                                            tipoMoneda,
                                                            numeroDocumentoEmisor,
                                                            tipoDocumentoAdquiriente,
                                                            numeroDocumentoAdquiriente,
                                                            razonSocialAdquiriente,
                                                            direccionAdquiriente,
                                                            tipoReferencia_1,
                                                            numeroDocumentoReferencia_1,
                                                            tipoReferencia_2,
                                                            numeroDocumentoReferencia_2,
                                                            totalVVNetoOpGravadas,
                                                            totalVVNetoOpNoGravada,
                                                            conceptoVVNetoOpNoGravada,
                                                            totalVVNetoOpExoneradas,
                                                            conceptoVVNetoOpExoneradas,
                                                            totalVVNetoOpGratuitas,
                                                            conceptoVVNetoOpGratuitas,
                                                            totalVVNetoExportacion,
                                                            conceptoVVExportacion,
                                                            totalDescuentos,
                                                            totalIgv,
                                                            totalVenta,
                                                            tipoOperacion,
                                                            leyendas,
                                                            textoleyenda_3,
                                                            textoleyenda_4, 
                                                            porcentajeDetraccion,
                                                            totalDetraccion,
                                                            descripcionDetraccion,
                                                            ordenCompra,
                                                            datosAdicionales,
                                                            codigoEstablecimientoSunat,
                                                            montoTotalImpuestos,
                                                            cdgCodigoMotivo,
                                                            cdgPorcentaje,
                                                            descuentosGlobales,
                                                            cdgMontoBaseCargo,
                                                            sumImpuestosOpGratuitas,
                                                            totalValorVenta,
                                                            totalPrecioVenta,
                                                            monRedImportTotal,
                                                            lugardestino
                                                        FROM fact_fe01_cab
                                                        WHERE (FECHARECOJO IS NULL) OR (TRIM(FECHARECOJO) IS NULL);";

        public const string IsisRead_Select_Detail = @"Select
	                                                        serieNumero,
	                                                        numeroOrdenItem,
	                                                        unidadMedida,
	                                                        cantidad,
	                                                        codigoProducto,
	                                                        codigoProductoSunat,
	                                                        descripcion,
	                                                        montoBaseIGV,
	                                                        importeIGV,
	                                                        codigoRazonExoneracion,
	                                                        tasaIGV,
	                                                        importeDescuento,
	                                                        codigoDescuento,
	                                                        factorDescuento,
	                                                        montoBaseDescuento,
	                                                        codigoImporteReferencial,
	                                                        importeReferencial,
	                                                        importeUnitarioSinImpuesto,
	                                                        importeTotalSinImpuesto,
	                                                        montoTotalImpuestoItem,
	                                                        codigoImpUnitConImpuesto,
	                                                        importeUnitarioConImpuesto,
	                                                        numeroExpediente,
	                                                        codigoUnidadEjecutora,
	                                                        numeroContrato,
	                                                        numeroProcesoSeleccion
                                                        From fact_fe01_det
                                                        Where serieNumero In (
	                                                        Select serieNumero From fact_fe01_cab Where(FECHARECOJO IS NULL) OR (TRIM(FECHARECOJO) IS NULL)
                                                        );";

        #endregion

        #region IsisRespuesta
        public const string IsisResponse = "Isis_Response";
        public const string IsisResponse_Invoice = "Factura";
        public const string IsisResponse_Bill = "Boleta";
        public const string IsisResponse_CreditNote = "NotaCredito";
        public const string IsisResponse_DebitNote = "NotaDebito";

        #endregion

        #region PacyficLectura
        public const string PacyficRead = "Pacyfic_Lectura";
        public const string PacyficRead_Invoice = "Factura";
        public const string PacyficRead_CreditNote = "NotaCredito";
        public const string PacyficRead_DebitNote = "NotaDebito";
        #endregion

        #region PacyficRespuesta
        public const string PacyficResponse = "Pacyfic_Response";
        public const string PacyficResponse_Invoice = "Factura";
        public const string PacyficResponse_CreditNote = "NotaCredito";
        public const string PacyficResponse_DebitNote = "NotaDebito";

        #endregion

        #region States
        public const int NO_LEIDO = 0;
        public const int LEIDO = 1;
        public const int PROCESADO = 2;
        public const int ENVIADO_GFISCAL = 3;
        public const int RETORNO_GFISCAL = 4;
        public const int ENVIADO_LEGADO = 5;
        public const int ERROR_FECENTRALIZADA = 6;
        public const int ERROR_GFISCAL = 7;
        public const int FALLA_VALIDACION = 8;
        #endregion

        #region CmsLectura
        public const string CmsRead = "Cms_Lectura";
        public const string CmsRead_Invoice = "Factura";
        public const string CmsRead_Bill = "Boleta";
        public const string CmsRead_CreditNote = "NotaCredito";
        public const string CmsRead_DebitNote = "NotaDebito";
        #endregion

        #region CmsRespuesta
        public const string CmsResponse = "Cms_Respuesta";
        public const string CmsResponse_Invoice = "Factura";
        public const string CmsResponse_Bill = "Boleta";
        public const string CmsResponse_CreditNote = "NotaCredito";
        public const string CmsResponse_DebitNote = "NotaDebito";
        #endregion

        #region AtisLectura
        public const string AtisRead = "Atis_Lectura";
        public const string AtisRead_Invoice = "Factura";
        public const string AtisRead_Bill = "Boleta";        
        public const string AtisRead_CreditNote = "NotaCredito";
        public const string AtisRead_DebitNote = "NotaDebito";
        #endregion

        #region AtisRespuesta
        public const string AtisResponse = "Atis_Respuesta";
        public const string AtisResponse_Invoice = "Factura";
        public const string AtisResponse_Bill = "Boleta";
        public const string AtisResponse_CreditNote = "NotaCredito";
        public const string AtisResponse_DebitNote = "NotaDebito";
        #endregion

        #region Trazabilidad
        public const string Trazabilidad = "Trazabilidad";
        public const string Cms = "Cms";
        public const string Atis = "Atis";
        public const string Pacyfic = "Pacyfic";
        public const string Sap = "Sap";
        public const string Isis = "Isis";
        public const string Backup = "Backup";
        #endregion

        #region Parameters
        public const string KEY_CONFIG = "config";
        public const string MAIL_CONFIG = "mail";
        public const string FTP_CONFIG = "ftp";
        public const string FTP_CONFIG_INPUT = "ftp_input";
        public const string FTP_CONFIG_OUTPUT = "ftp_output";
        public const string FTP_SPEC_OUT = "spec_out";
        public const string PATH_LOG = "path_log";
        public const string PATH_LOG_READ = "path_log_read";
        public const string PATH_LOG_RESPONSE = "path_log_response";

        #endregion

        #region NORMS
        public const int NORM_193 = 193;
        public const int NORM_340 = 340;
        #endregion

        #endregion
    }
}
