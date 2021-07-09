using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TM.FECentralizada.Entities.Common;

namespace TM.FECentralizada.Traceability
{
    public partial class TraceabilityService : ServiceBase
    {
        Timer oTimer = new Timer();
        public TraceabilityService()
        {
            InitializeComponent();
        }

        public void TestProject()
        {
            Procedure();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                oTimer.Enabled = true;
                oTimer.AutoReset = false;
                oTimer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
                oTimer.Start();
                oTimer.Interval = 10000;
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Procedure();
        }

        private void Procedure()
        {
            try
            {
                Tools.Logging.Info("Inicio del Proceso: Trazabilidad.");

                Tools.Logging.Info("Inicio : Obtener Parámetros");
                //Método que Obtendrá los Parámetros.
                List<Parameters> ParamsResponse = TM.FECentralizada.Business.Common.GetParametersByKey(new Parameters() { Domain = Tools.Constants.Trazabilidad, KeyDomain = "", KeyParam = "" });
                Tools.Logging.Info("Fin : Obtener Parámetros");

                if (ParamsResponse != null && ParamsResponse.Any())
                {
                    Traceability(ParamsResponse);

                }
                else
                {
                    Tools.Logging.Error("Ocurrió un error al obtener la configuración para Trazabilidad.");
                }
                Tools.Logging.Info("Fin del Proceso: Trazabilidad.");
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }

        private void Traceability(List<Parameters> oListParameters)
        {
            TraceabilityConfig traceabilityConfig;
            FileServer fileServerConfig;
            DateTime timestamp = DateTime.Now;

            Tools.Logging.Info("Inicio: Obtener parámetros generales");
            Parameters configParameter = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.KEY_CONFIG);

            if (configParameter != null)
            {
                traceabilityConfig = Business.Common.GetParameterDeserialized<TraceabilityConfig>(configParameter);
                Parameters ftpParameterOut = oListParameters.FirstOrDefault(x => x.KeyParam == Tools.Constants.FTP_CONFIG_OUTPUT);
                if (ftpParameterOut != null)
                {
                    DateTime fechaLog = timestamp.AddDays(-traceabilityConfig.NumbDayAgoInput);
                    DateTime fechaFtp = timestamp.AddDays(-traceabilityConfig.NumbDayAgoInput);
                    FileServer fileServerConfigOut = Business.Common.GetParameterDeserialized<Entities.Common.FileServer>(ftpParameterOut);
                    string[] files;
                    //Revisar que en el FTP solo haya archivos de los ultimos N dias
                    List<String> inputFilesFTP = Tools.FileServer.ListDirectory(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory);
                    foreach (string file in inputFilesFTP)
                    {
                        String lastModifiedFileFTP = Tools.FileServer.getLastModifiedFileFTP(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, file);
                        DateTime dateModified = DateTime.ParseExact(lastModifiedFileFTP, "dd/MM/yyyy", null);
                        int res = DateTime.Compare(dateModified.Date, fechaFtp.Date);

                        if (res < 0)
                        {
                            Tools.FileServer.DeleteFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, file);
                        }

                    }
                    //Fin de Revisar que en el FTP solo haya archivos de los ultimos N dias


                    #region Atis
                    Parameters configParameterAtis = oListParameters.FirstOrDefault(x => x.KeyDomain == Tools.Constants.Atis && x.KeyParam == Tools.Constants.PATH_LOG_READ);
                    if (configParameterAtis != null)
                    {
                        try
                        {
                            fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(configParameterAtis);
                            files = Directory.GetFiles(fileServerConfig.Directory);
                            foreach (string file in files)
                            {
                                String fileName = System.IO.Path.GetFileName(file);
                                String orFile = System.IO.Path.Combine(fileServerConfig.Directory, fileName);

                                FileInfo fi = new FileInfo(orFile);
                                DateTime fechaCreacion = fi.CreationTime;
                                int res = DateTime.Compare(fechaCreacion.Date, fechaLog.Date);

                                if (res == 0)
                                {
                                    byte[] ResultBytes = File.ReadAllBytes(orFile);
                                    Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, fileName, ResultBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(ex.Message);
                        }
                    }
                    configParameterAtis = oListParameters.FirstOrDefault(x => x.KeyDomain == Tools.Constants.Atis && x.KeyParam == Tools.Constants.PATH_LOG_RESPONSE);
                    if (configParameterAtis != null)
                    {
                        try
                        {
                            fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(configParameterAtis);
                            files = Directory.GetFiles(fileServerConfig.Directory);
                            foreach (string file in files)
                            {
                                String fileName = System.IO.Path.GetFileName(file);
                                String orFile = System.IO.Path.Combine(fileServerConfig.Directory, fileName);

                                FileInfo fi = new FileInfo(orFile);
                                DateTime fechaCreacion = fi.CreationTime;
                                int res = DateTime.Compare(fechaCreacion.Date, fechaLog.Date);

                                if (res == 0)
                                {
                                    byte[] ResultBytes = File.ReadAllBytes(orFile);
                                    Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, fileName, ResultBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(ex.Message);
                        }
                    }
                    #endregion

                    #region Cms
                    Parameters configParameterCms = oListParameters.FirstOrDefault(x => x.KeyDomain == Tools.Constants.Cms && x.KeyParam == Tools.Constants.PATH_LOG_READ);
                    if (configParameterCms != null)
                    {
                        try
                        {
                            fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(configParameterCms);
                            files = Directory.GetFiles(fileServerConfig.Directory);
                            foreach (string file in files)
                            {
                                String fileName = System.IO.Path.GetFileName(file);
                                String orFile = System.IO.Path.Combine(fileServerConfig.Directory, fileName);

                                FileInfo fi = new FileInfo(orFile);
                                DateTime fechaCreacion = fi.CreationTime;
                                int res = DateTime.Compare(fechaCreacion.Date, fechaLog.Date);

                                if (res == 0)
                                {
                                    byte[] ResultBytes = File.ReadAllBytes(orFile);
                                    Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, fileName, ResultBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(ex.Message);
                        }
                    }
                    configParameterCms = oListParameters.FirstOrDefault(x => x.KeyDomain == Tools.Constants.Cms && x.KeyParam == Tools.Constants.PATH_LOG_RESPONSE);
                    if (configParameterCms != null)
                    {
                        try
                        {
                            fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(configParameterCms);
                            files = Directory.GetFiles(fileServerConfig.Directory);
                            foreach (string file in files)
                            {
                                String fileName = System.IO.Path.GetFileName(file);
                                String orFile = System.IO.Path.Combine(fileServerConfig.Directory, fileName);

                                FileInfo fi = new FileInfo(orFile);
                                DateTime fechaCreacion = fi.CreationTime;
                                int res = DateTime.Compare(fechaCreacion.Date, fechaLog.Date);

                                if (res == 0)
                                {
                                    byte[] ResultBytes = File.ReadAllBytes(orFile);
                                    Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, fileName, ResultBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(ex.Message);
                        }
                    }
                    #endregion

                    #region Pacifyc
                    Parameters configParameterPacifyc = oListParameters.FirstOrDefault(x => x.KeyDomain == Tools.Constants.Pacyfic && x.KeyParam == Tools.Constants.PATH_LOG_READ);
                    if (configParameterPacifyc != null)
                    {
                        try
                        {
                            fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(configParameterPacifyc);
                            files = Directory.GetFiles(fileServerConfig.Directory);
                            foreach (string file in files)
                            {
                                String fileName = System.IO.Path.GetFileName(file);
                                String orFile = System.IO.Path.Combine(fileServerConfig.Directory, fileName);

                                FileInfo fi = new FileInfo(orFile);
                                DateTime fechaCreacion = fi.CreationTime;
                                int res = DateTime.Compare(fechaCreacion.Date, fechaLog.Date);

                                if (res == 0)
                                {
                                    byte[] ResultBytes = File.ReadAllBytes(orFile);
                                    Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, fileName, ResultBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(ex.Message);
                        }
                    }
                    configParameterPacifyc = oListParameters.FirstOrDefault(x => x.KeyDomain == Tools.Constants.Pacyfic && x.KeyParam == Tools.Constants.PATH_LOG_RESPONSE);
                    if (configParameterPacifyc != null)
                    {
                        try
                        {
                            fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(configParameterPacifyc);
                            files = Directory.GetFiles(fileServerConfig.Directory);
                            foreach (string file in files)
                            {
                                String fileName = System.IO.Path.GetFileName(file);
                                String orFile = System.IO.Path.Combine(fileServerConfig.Directory, fileName);

                                FileInfo fi = new FileInfo(orFile);
                                DateTime fechaCreacion = fi.CreationTime;
                                int res = DateTime.Compare(fechaCreacion.Date, fechaLog.Date);

                                if (res == 0)
                                {
                                    byte[] ResultBytes = File.ReadAllBytes(orFile);
                                    Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, fileName, ResultBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(ex.Message);
                        }
                    }
                    #endregion

                    #region Isis
                    Parameters configParameterIsis = oListParameters.FirstOrDefault(x => x.KeyDomain == Tools.Constants.Isis && x.KeyParam == Tools.Constants.PATH_LOG_READ);
                    if (configParameterIsis != null)
                    {
                        try
                        {
                            fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(configParameterIsis);
                            files = Directory.GetFiles(fileServerConfig.Directory);
                            foreach (string file in files)
                            {
                                String fileName = System.IO.Path.GetFileName(file);
                                String orFile = System.IO.Path.Combine(fileServerConfig.Directory, fileName);

                                FileInfo fi = new FileInfo(orFile);
                                DateTime fechaCreacion = fi.CreationTime;
                                int res = DateTime.Compare(fechaCreacion.Date, fechaLog.Date);

                                if (res == 0)
                                {
                                    byte[] ResultBytes = File.ReadAllBytes(orFile);
                                    Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, fileName, ResultBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(ex.Message);
                        }
                    }
                    configParameterIsis = oListParameters.FirstOrDefault(x => x.KeyDomain == Tools.Constants.Isis && x.KeyParam == Tools.Constants.PATH_LOG_RESPONSE);
                    if (configParameterIsis != null)
                    {
                        try
                        {
                            fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(configParameterIsis);
                            files = Directory.GetFiles(fileServerConfig.Directory);
                            foreach (string file in files)
                            {
                                String fileName = System.IO.Path.GetFileName(file);
                                String orFile = System.IO.Path.Combine(fileServerConfig.Directory, fileName);

                                FileInfo fi = new FileInfo(orFile);
                                DateTime fechaCreacion = fi.CreationTime;
                                int res = DateTime.Compare(fechaCreacion.Date, fechaLog.Date);

                                if (res == 0)
                                {
                                    byte[] ResultBytes = File.ReadAllBytes(orFile);
                                    Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, fileName, ResultBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(ex.Message);
                        }
                    }
                    #endregion

                    #region Sap
                    Parameters configParameterSap = oListParameters.FirstOrDefault(x => x.KeyDomain == Tools.Constants.Sap && x.KeyParam == Tools.Constants.PATH_LOG_READ);
                    if (configParameterSap != null)
                    {
                        try
                        {
                            fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(configParameterSap);
                            files = Directory.GetFiles(fileServerConfig.Directory);
                            foreach (string file in files)
                            {
                                String fileName = System.IO.Path.GetFileName(file);
                                String orFile = System.IO.Path.Combine(fileServerConfig.Directory, fileName);

                                FileInfo fi = new FileInfo(orFile);
                                DateTime fechaCreacion = fi.CreationTime;
                                int res = DateTime.Compare(fechaCreacion.Date, fechaLog.Date);

                                if (res == 0)
                                {
                                    byte[] ResultBytes = File.ReadAllBytes(orFile);
                                    Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, fileName, ResultBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(ex.Message);
                        }
                    }
                    configParameterSap = oListParameters.FirstOrDefault(x => x.KeyDomain == Tools.Constants.Sap && x.KeyParam == Tools.Constants.PATH_LOG_RESPONSE);
                    if (configParameterSap != null)
                    {
                        try
                        {
                            fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(configParameterSap);
                            files = Directory.GetFiles(fileServerConfig.Directory);
                            foreach (string file in files)
                            {
                                String fileName = System.IO.Path.GetFileName(file);
                                String orFile = System.IO.Path.Combine(fileServerConfig.Directory, fileName);

                                FileInfo fi = new FileInfo(orFile);
                                DateTime fechaCreacion = fi.CreationTime;
                                int res = DateTime.Compare(fechaCreacion.Date, fechaLog.Date);

                                if (res == 0)
                                {
                                    byte[] ResultBytes = File.ReadAllBytes(orFile);
                                    Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, fileName, ResultBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(ex.Message);
                        }
                    }
                    #endregion

                    #region Backup
                    Parameters configParameterBk = oListParameters.FirstOrDefault(x => x.KeyDomain == Tools.Constants.Backup && x.KeyParam == Tools.Constants.PATH_LOG);
                    if (configParameterBk != null)
                    {
                        try
                        {
                            fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(configParameterBk);
                            files = Directory.GetFiles(fileServerConfig.Directory);
                            foreach (string file in files)
                            {
                                String fileName = System.IO.Path.GetFileName(file);
                                String orFile = System.IO.Path.Combine(fileServerConfig.Directory, fileName);

                                FileInfo fi = new FileInfo(orFile);
                                DateTime fechaCreacion = fi.CreationTime;
                                int res = DateTime.Compare(fechaCreacion.Date, fechaLog.Date);

                                if (res == 0)
                                {
                                    byte[] ResultBytes = File.ReadAllBytes(orFile);
                                    Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, fileName, ResultBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(ex.Message);
                        }
                    }
                    #endregion

                    #region Trazabilidad
                    Parameters configParameterTrazabilidad = oListParameters.FirstOrDefault(x => x.KeyDomain == Tools.Constants.Trazabilidad && x.KeyParam == Tools.Constants.PATH_LOG);
                    if (configParameterTrazabilidad != null)
                    {
                        try
                        {
                            fileServerConfig = Business.Common.GetParameterDeserialized<FileServer>(configParameterTrazabilidad);
                            files = Directory.GetFiles(fileServerConfig.Directory);
                            foreach (string file in files)
                            {
                                String fileName = System.IO.Path.GetFileName(file);
                                String orFile = System.IO.Path.Combine(fileServerConfig.Directory, fileName);

                                FileInfo fi = new FileInfo(orFile);
                                DateTime fechaCreacion = fi.CreationTime;
                                int res = DateTime.Compare(fechaCreacion.Date, fechaLog.Date);

                                if (res == 0)
                                {
                                    byte[] ResultBytes = File.ReadAllBytes(orFile);
                                    Tools.FileServer.UploadFile(fileServerConfigOut.Host, fileServerConfigOut.Port, fileServerConfigOut.User, fileServerConfigOut.Password, fileServerConfigOut.Directory, fileName, ResultBytes);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(ex.Message);
                        }
                    }
                    #endregion

                }
                else
                {
                    Tools.Logging.Error("No se encontró el parámetro de configuracion FTP_OUTPUT - Trazabilidad");
                }

            }
            else
            {
                Tools.Logging.Error("No se encontró el parámetro de configuracion KEYCONFIG - Trazabilidad");
            }
        }

        protected override void OnStop()
        {
            oTimer.Stop();
        }
    }
}
