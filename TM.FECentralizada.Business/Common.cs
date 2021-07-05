using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TM.FECentralizada.Entities.Common;
using Newtonsoft;
using System.Net.Mail;
using System.IO;

namespace TM.FECentralizada.Business
{
    public static class Common
    {
        public static List<Entities.Common.Parameters> GetParametersByKey(Entities.Common.Parameters oParametersRequest)
        {
            List<Entities.Common.Parameters> oListParameter = new List<Entities.Common.Parameters>();
            try
            {
                oListParameter = TM.FECentralizada.Data.Common.GetParametersByKey(oParametersRequest);
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return oListParameter;
        }

        public static T GetParameterDeserialized<T>(Parameters parameter)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(parameter.ValueJson);
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message + ex.InnerException);
                throw ex;
            }

        }

        public static int InsertAudit(string idGrupo, int legado, int estado, int cantidadRegistros, int intentos, int procesoSunat)
        {
            try
            {
                return TM.FECentralizada.Data.Common.InsertAudit(idGrupo, legado, estado, cantidadRegistros, intentos, procesoSunat);
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
                return 0;
            }
        }

        public static void SendFileNotification(Entities.Common.Mail mail, List<string> messageResult)
        {
            string body = "<body><ul>";

            foreach (string message in messageResult)
            {
                body += $"<li>{message}</li>";
            }
            body += "</ul>/</body>";


            bool result = Tools.Mail.SendEmailBySMTP(mail.From, mail.To, null, null, mail.Subject, body, mail.User, mail.Password, mail.Port, mail.Host);

            if (!result)
            {
                Tools.Logging.Error("Ocurrió un error al mandar uno o varios emails, revisar logs anteriores");
            }

        }

        public static void SendFileNotification(Entities.Common.Mail mail, string body)
        {

            bool result = Tools.Mail.SendEmailBySMTP(mail.From, mail.To, null, null, mail.Subject, body, mail.User, mail.Password, mail.Port, mail.Host);

            if (!result)
            {
                Tools.Logging.Error("Ocurrió un error al mandar uno o varios emails, revisar logs anteriores");
            }

        }

        public static void UpdateAudit(int auditoriaId, int estadoid, int intentos)
        {
            try
            {
                TM.FECentralizada.Data.Common.UpdateAudit(auditoriaId, estadoid, intentos);
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void BulkInsertListToTable<T>(List<T> list, string tableName)
        {
            try
            {
                TM.FECentralizada.Data.Common.BulkInsertListToTable(list, tableName);
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void UpdateDocumentInvoice(string alignet, string sendDate, string series)
        {
            try
            {
                Data.Common.UpdateDocumentInvoice(alignet, sendDate, series);
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static List<ResponseFile> DownloadFileOutput(FileServer fileServer, List<string> messages, String sufix)
        {
            List<string> gfiscalFiles = null;
            List<ResponseFile> responseFiles = null;
            try
            {
                gfiscalFiles = Tools.FileServer.ListDirectory(fileServer.Host, fileServer.Port, fileServer.User, fileServer.Password, fileServer.Directory);

                if (gfiscalFiles != null)
                {
                    gfiscalFiles = gfiscalFiles.Where(x => x.StartsWith(sufix)).ToList();
                }
                responseFiles = new List<ResponseFile>(gfiscalFiles.Count);
                foreach (string file in gfiscalFiles)
                {
                    var fileLines = Tools.FileServer.DownloadFile(fileServer.Host, fileServer.Port, fileServer.User, fileServer.Password, fileServer.Directory, file);

                    if (fileLines.Count <= 0)
                    {
                        messages.Add($"El archivo de nombre: {file} no puede ser procesado porque se encuentra vacío");
                    }
                    else
                    {
                        int contadorLineas = 0;
                        foreach (string line in fileLines)
                        {
                            var fields = line.Split(Tools.Constants.FIELD_SEPARATOR);

                            if (fields.Length >= 16)
                            {
                                ResponseFile responseFile = new ResponseFile
                                {
                                    estado = fields[0],
                                    numDocEmisor = fields[1],
                                    tipoDocumento = fields[2],
                                    serieNumero = fields[3],
                                    codigoSunat = fields[4],
                                    mensajeSunat = fields[5],
                                    fechaDeclaracion = fields[6],
                                    fechaEmision = fields[7],
                                    firma = fields[8],
                                    resumen = fields[9],
                                    codSistema = fields[10],
                                    adicional1 = fields[11],
                                    adicional2 = fields[12],
                                    adicional3 = fields[13],
                                    adicional4 = fields[14],
                                    adicional5 = fields[15]
                                };

                                responseFiles.Add(responseFile);

                            }
                            else
                            {
                                messages.Add($"La linea {contadorLineas} del archivo: {file} es inválida");
                            }




                        }
                    }



                }

            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
            return responseFiles;
        }

        private static bool ValidateFileStructure(string file)
        {
            return new FileInfo(file).Length == 0;
        }

        public static void ValidateFilesStructure(List<string> files)
        {

            foreach (string file in files.ToList())
            {
                if (!ValidateFileStructure(file))
                {
                    files.Remove(file);
                }
            }

        }

        public static void UpdateInvoiceState(List<ResponseFile> responseFiles)
        {
            try
            {
                Data.Common.BulkInsertListToTable(responseFiles, "Tmp_Factura_respuesta");
                Data.Common.UpdateInvoiceState();
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void UpdateCreditNoteState(List<ResponseFile> responseFiles)
        {
            try
            {
                Data.Common.BulkInsertListToTable(responseFiles, "Tmp_NotaCredito_respuesta");
                Data.Common.UpdateCreditNoteState();
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void UpdateDebitNoteState(List<ResponseFile> responseFiles)
        {
            try
            {
                Data.Common.BulkInsertListToTable(responseFiles, "Tmp_NotaDebito_respuesta");
                Data.Common.UpdateDebitNoteState();
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        public static void UpdateBillState(List<ResponseFile> responseFiles)
        {
            try
            {
                Data.Common.BulkInsertListToTable(responseFiles, "Tmp_Boleta_respuesta");
                Data.Common.UpdateBillState();


            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);

            }
        }

        public static void MoveFilesToProcessedFolder(FileServer fileServerConfig, string targetDirectory, string prefix)
        {
            var inputFilesFTP = Tools.FileServer.ListDirectory(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory);
            inputFilesFTP = inputFilesFTP.Where(x => x.StartsWith(prefix)).ToList();
            foreach (string file in inputFilesFTP)
            {
                Tools.FileServer.DownloadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file, true, System.IO.Path.GetTempPath());
                Tools.FileServer.UploadFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, targetDirectory, file, System.IO.File.ReadAllBytes(System.IO.Path.GetTempPath() + "/" + file));
                Tools.FileServer.DeleteFile(fileServerConfig.Host, fileServerConfig.Port, fileServerConfig.User, fileServerConfig.Password, fileServerConfig.Directory, file);
            }

        }
       

    }
}