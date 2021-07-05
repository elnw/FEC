using ExcelDataReader;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace TM.FECentralizada.Tools
{
    public static class FileServer
    {
        public static bool UploadFile(string strHost, string strPort, string strUserName, string strPassword, string strDirectory, string strFileName, string FileTextPath)
        {
            bool blnResult = false;
            try
            {
                Logging.Info(string.Format("Inicio UploadFile:  Host:{0}, Port:{1}, UserName:{2}, Password:{3}", strHost, strPort, strUserName, strPassword));
                if (strPort.Equals("21"))
                {
                    FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create("ftp://" + strHost + strDirectory + strFileName);
                    ftpReq.UseBinary = true;
                    ftpReq.Method = WebRequestMethods.Ftp.UploadFile;
                    ftpReq.Credentials = new NetworkCredential(strUserName, strPassword);
                    ftpReq.EnableSsl = false;
                    ftpReq.UsePassive = true;
                    ftpReq.KeepAlive = true;

                    Logging.Info(string.Format("Inicio Lectura Binary: {0} ", strFileName));
                    byte[] binary = File.ReadAllBytes(FileTextPath);
                    ftpReq.ContentLength = binary.Length;
                    using (Stream s = ftpReq.GetRequestStream())
                    {
                        Logging.Info(string.Format("Inicio RequestStream: Lectura: {0}, Escritura: {1} ", s.CanRead, s.CanWrite));
                        s.Write(binary, 0, binary.Length);
                        Logging.Info(string.Format("Fin RequestStream: Lectura: {0}, Escritura: {1} ", s.CanRead, s.CanWrite));
                    }
                    Logging.Info("Fin UploadFile FTP.");
                }
                else if (strPort.Equals("22"))
                {
                    using (Renci.SshNet.SftpClient sftpClient = new Renci.SshNet.SftpClient(strHost, int.Parse(strPort), strUserName, strPassword))
                    {
                        sftpClient.Connect();
                        sftpClient.ChangeDirectory("/");
                        Logging.Info(string.Format("Directorio en Contexto: {0} ", sftpClient.WorkingDirectory));
                        using (Stream fileStream = File.OpenRead(FileTextPath))
                        {
                            Logging.Info(string.Format("Inicio copiar : {0}{1} ", strDirectory, strFileName));
                            sftpClient.UploadFile(fileStream, string.Concat(strDirectory, strFileName));
                            Logging.Info(string.Format("Fin copiar : {0}{1} ", strDirectory, strFileName));
                        }
                    }
                    Logging.Info("Fin UploadFile SFTP.");
                }
                blnResult = true;
            }
            catch (Exception ex)
            {
                Logging.Error(string.Format("Exception: {0}, Context: {1}, Puerto: {2}", ex, "Error de al intentar conectar al File Server", strPort));
            }
            return blnResult;
        }

        public static bool UploadFile(string strHost, string strPort, string strUserName, string strPassword, string strDirectory, string strFileName, byte[] bytesContent)
        {
            bool blnResult = false;
            try
            {
                Logging.Info(string.Format("Inicio UploadFile:  Host:{0}, Port:{1}, UserName:{2}, Password:{3}", strHost, strPort, strUserName, strPassword));
                if (strPort.Equals("21"))
                {
                    FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create("ftp://" + strHost + strDirectory + strFileName);
                    ftpReq.UseBinary = true;
                    ftpReq.Method = WebRequestMethods.Ftp.UploadFile;
                    ftpReq.Credentials = new NetworkCredential(strUserName, strPassword);
                    ftpReq.EnableSsl = false;
                    ftpReq.UsePassive = true;
                    ftpReq.KeepAlive = true;

                    byte[] binary = bytesContent;
                    ftpReq.ContentLength = binary.Length;
                    using (Stream s = ftpReq.GetRequestStream())
                    {
                        Logging.Info(string.Format("Inicio RequestStream: Lectura: {0}, Escritura: {1} ", s.CanRead, s.CanWrite));
                        s.Write(binary, 0, binary.Length);
                        Logging.Info(string.Format("Fin RequestStream: Lectura: {0}, Escritura: {1} ", s.CanRead, s.CanWrite));
                    }
                    Logging.Info("Fin UploadFile FTP.");
                }
                else if (strPort.Equals("22"))
                {
                    using (Renci.SshNet.SftpClient sftpClient = new Renci.SshNet.SftpClient(strHost, int.Parse(strPort), strUserName, strPassword))
                    {
                        sftpClient.Connect();
                        sftpClient.ChangeDirectory("/");
                        Logging.Info(string.Format("Directorio en Contexto: {0} ", sftpClient.WorkingDirectory));
                        using (MemoryStream Stream = new MemoryStream(bytesContent))
                        {
                            Logging.Info(string.Format("Inicio copiar : {0}{1} ", strDirectory, strFileName));
                            sftpClient.UploadFile(Stream, string.Concat(strDirectory, strFileName));
                            Logging.Info(string.Format("Fin copiar : {0}{1} ", strDirectory, strFileName));
                        }
                    }
                    Logging.Info("Fin UploadFile SFTP.");
                }
                blnResult = true;
            }
            catch (Exception ex)
            {
                Logging.Error(string.Format("Exception: {0}, Context: {1}, Puerto: {2}", ex, "Error de al intentar conectar al File Server", strPort));
            }
            return blnResult;
        }

        public static List<string> DownloadFile(string strHost, string strPort, string strUserName, string strPassword, string strDirectory, string strFileName, bool saveLocal = false, string strDirectoryLocal = "")
        {
            List<string> ListLinesFile = new List<string>();
            try
            {
                Logging.Info(string.Format("Inicio DownloadFile:  Host:{0}, Port:{1}, UserName:{2}, Password:{3}, Directory:{4}, FileName:{5}", strHost, strPort, strUserName, strPassword, strDirectory, strFileName));

                if (strPort.Equals("21"))
                {
                    try
                    {
                        FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create(string.Concat("ftp://", strHost, strDirectory, strFileName));
                        ftpReq.UseBinary = true;
                        ftpReq.Method = WebRequestMethods.Ftp.DownloadFile;
                        ftpReq.Credentials = new NetworkCredential(strUserName, strPassword);
                        ftpReq.EnableSsl = false;
                        ftpReq.UsePassive = true;
                        ftpReq.KeepAlive = true;

                        Logging.Info(string.Concat("FTP.  GetResponse()"));
                        using (FtpWebResponse response = (FtpWebResponse)ftpReq.GetResponse())
                        {
                            Logging.Info(string.Concat("FTP. GetResponseStream()"));
                            using (Stream responseStream = response.GetResponseStream())
                            {
                                StreamReader oStreamReader = new StreamReader(responseStream);
                                if (saveLocal)
                                {
                                    using (FileStream writer = new FileStream(strDirectoryLocal + strFileName, FileMode.Create))
                                    {
                                        long length = response.ContentLength;
                                        int bufferSize = 2048;
                                        int readCount;
                                        byte[] buffer = new byte[2048];

                                        readCount = responseStream.Read(buffer, 0, bufferSize);
                                        while (readCount > 0)
                                        {
                                            writer.Write(buffer, 0, readCount);
                                            readCount = responseStream.Read(buffer, 0, bufferSize);
                                        }
                                    }
                                    oStreamReader.Close();
                                    response.Close();
                                    return ListLinesFile;
                                }


                                string line = string.Empty;
                                while ((line = oStreamReader.ReadLine()) != null)
                                {
                                    ListLinesFile.Add(line);
                                }
                                Logging.Info(string.Format("FTP. Archivo = {0} [ Nro Filas = {1} ]", strFileName, ListLinesFile.Count));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Tools.Logging.Error(string.Format("Error FTP: al leer el arrchivo, Exception: {0} ", ex.Message));
                    }
                    Logging.Info("Fin DownloadFile FTP.");
                }
                else if (strPort.Equals("22"))
                {
                    using (Renci.SshNet.SftpClient sftp = new Renci.SshNet.SftpClient(strHost, int.Parse(strPort), strUserName, strPassword))
                    {
                        try
                        {
                            Logging.Info(string.Concat("Inicio SFTP. Connect()"));
                            sftp.Connect();
                            sftp.ChangeDirectory("/");
                            string directorySftpInput = string.Concat(strDirectory, strFileName);
                            Logging.Info(string.Concat("Inicio DownloadFile SFTP. Directorio = { ", directorySftpInput, " }"));
                            StreamReader oStreamReader = sftp.OpenText(directorySftpInput);
                            if (saveLocal)
                            {
                                using (Stream file1 = File.OpenWrite(strDirectoryLocal + strFileName))
                                {
                                    sftp.DownloadFile(strDirectory + strFileName, file1);
                                }
                                return ListLinesFile;
                            }
                            string line = string.Empty;
                            while ((line = oStreamReader.ReadLine()) != null)
                            {
                                ListLinesFile.Add(line);
                            }
                            Logging.Info(string.Format("SFTP. Archivo = {0} [ Nro Filas = {1} ]", strFileName, ListLinesFile.Count));
                            Logging.Info("Fin DownloadFile SFTP.");
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(string.Format("Error SFTP: al leer el arrchivo, Exception: {0} ", ex.Message));
                        }
                    }
                    Logging.Info("Fin DownloadFile SFTP.");
                }
            }
            catch (Exception ex)
            {
                Logging.Error(string.Format("Exception: {0}, Context: {1}, Puerto: {2}", ex, "Error de al intentar conectar al File Server", strPort));
            }
            return ListLinesFile;
        }

        public static List<string> ListDirectory(string strHost, string strPort, string strUserName, string strPassword, string strDirectory)
        {
            List<string> ListFileName = new List<string>();
            try
            {
                Logging.Info(string.Format("Inicio ListDirectory:  Host:{0}, Port:{1}, UserName:{2}, Password:{3}, Directory:{4}", strHost, strPort, strUserName, strPassword, strDirectory));

                if (strPort.Equals("21"))
                {
                    try
                    {
                        Logging.Info(string.Concat("FTP >  FtpWebRequest [WebRequest]"));
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://" + strHost + strDirectory);
                        request.Method = WebRequestMethods.Ftp.ListDirectory;
                        request.Credentials = new NetworkCredential(strUserName, strPassword);

                        Logging.Info(string.Concat("FTP >  GetResponse()"));
                        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                        Logging.Info(string.Concat("FTP >  GetResponseStream()"));
                        Stream responseStream = response.GetResponseStream();
                        Logging.Info(string.Concat("FTP >  StreamReader()"));
                        StreamReader reader = new StreamReader(responseStream);
                        Logging.Info(string.Concat("FTP >  ReadToEnd()"));
                        string names = reader.ReadToEnd();

                        reader.Close();
                        response.Close();

                        ListFileName = names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                    catch (Exception ex)
                    {
                        Tools.Logging.Error(string.Format("Error FTP: al leer los archivos, Exception: {0} ", ex.Message));
                    }
                    Logging.Info("Fin ListDirectory FTP.");
                }
                else if (strPort.Equals("22"))
                {
                    using (Renci.SshNet.SftpClient sftp = new Renci.SshNet.SftpClient(strHost, int.Parse(strPort), strUserName, strPassword))
                    {
                        try
                        {
                            Logging.Info(string.Concat("Inicio SFTP. Connect()"));
                            sftp.Connect();
                            sftp.ChangeDirectory("/");
                            string directorySftp = strDirectory;
                            Logging.Info(string.Concat("Inicio ListDirectory SFTP. Directorio = { ", directorySftp, " }"));
                            var ListFilesSftp = sftp.ListDirectory(directorySftp);
                            if (ListFilesSftp.Count() > 0)
                            {
                                ListFileName = ListFilesSftp.Where(x => !x.IsDirectory && !x.IsSymbolicLink).Select(f => (string)f.Name).ToList();
                            }
                            Logging.Info("Fin ListDirectory SFTP.");
                        }
                        catch (Exception ex)
                        {
                            Tools.Logging.Error(string.Format("Error SFTP: al leer el arrchivo, Exception: {0} ", ex.Message));
                        }
                    }
                    Logging.Info("Fin ListDirectory SFTP.");
                }
            }
            catch (Exception ex)
            {
                Logging.Error(string.Format("Exception: {0}, Context: {1}, Puerto: {2}", ex, "Error de al intentar conectar al File Server", strPort));
            }
            return ListFileName;
        }

        public static List<string> DownloadFileExcel(string strHost, string strPort, string strUserName, string strPassword, string strDirectory, string strFileName, int LengthColumn)
        {
            List<string> ListLinesFile = new List<string>();
            try
            {
                Logging.Info(string.Format("Inicio DownloadFile:  Host:{0}, Port:{1}, UserName:{2}, Password:{3}, Directory:{4}, FileName:{5}", strHost, strPort, strUserName,
                    strPassword, strDirectory, strFileName));
                if (strPort.Equals("21"))
                {
                    try
                    {
                        FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create(string.Concat("ftp://", strHost, strDirectory, strFileName));
                        ftpReq.UseBinary = true;
                        ftpReq.Method = WebRequestMethods.Ftp.DownloadFile;
                        ftpReq.Credentials = new NetworkCredential(strUserName, strPassword);
                        ftpReq.EnableSsl = false;
                        ftpReq.UsePassive = true;
                        ftpReq.KeepAlive = true;

                        using (FtpWebResponse response = (FtpWebResponse)ftpReq.GetResponse())
                        {
                            Logging.Info(string.Concat("FTP.  GetResponse()"));
                            using (Stream responseStream = response.GetResponseStream())
                            {
                                Logging.Info(string.Concat("FTP. GetResponseStream()"));
                                byte[] bytes = Common.StreamToByteArray(responseStream);

                                using (MemoryStream stream = new MemoryStream(bytes))
                                {
                                    using (ExcelPackage package = new ExcelPackage(stream))
                                    {
                                        ListLinesFile = Common.ReadLineFromPackage(package, LengthColumn);
                                    }
                                }
                                Logging.Info(string.Format("FTP. Archivo = {0} [ Nro Filas = {1} ]", strFileName, ListLinesFile.Count));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Tools.Logging.Error(string.Format("Error FTP: al leer el arrchivo, Exception: {0} ", ex.Message));
                    }
                    Logging.Info("Fin DownloadFileExcel FTP.");
                }
                else if (strPort.Equals("22"))
                {
                    using (Renci.SshNet.SftpClient sftp = new Renci.SshNet.SftpClient(strHost, int.Parse(strPort), strUserName, strPassword))
                    {
                        try
                        {
                            Logging.Info(string.Concat("Inicio SFTP. Connect()"));
                            sftp.Connect();
                            sftp.ChangeDirectory("/");
                            string directorySftpInput = string.Concat(strDirectory, strFileName);
                            Logging.Info(string.Concat("Inicio DownloadFile SFTP. Directorio = { ", directorySftpInput, " }"));
                            Stream oStream = sftp.OpenRead(directorySftpInput);
                            byte[] bytes = Common.StreamToByteArray(oStream);
                            using (MemoryStream stream = new MemoryStream(bytes))
                            {
                                using (ExcelPackage package = new ExcelPackage(stream))
                                {
                                    ListLinesFile = Common.ReadLineFromPackage(package, LengthColumn);
                                }
                            }
                            Logging.Info(string.Format("SFTP. Archivo = {0} [ Nro Filas = {1} ]", strFileName, ListLinesFile.Count));
                            Logging.Info("Fin DownloadFile SFTP.");
                        }
                        catch (Exception ex)
                        {
                            Logging.Error(string.Format("Error SFTP: al leer el arrchivo, Exception: {0} ", ex.Message));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error(string.Format("Exception: {0}, Context: {1}, Puerto: {2}", ex, "Error de al intentar conectar al File Server", strPort));
            }
            return ListLinesFile;
        }

        public static bool DeleteFile(string strHost, string strPort, string strUserName, string strPassword, string strDirectory, string strFileName)
        {
            bool res = false;
            try
            {
                FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create(string.Concat("ftp://", strHost, strDirectory, strFileName));
                ftpReq.UseBinary = true;
                ftpReq.Credentials = new NetworkCredential(strUserName, strPassword);
                ftpReq.EnableSsl = false;
                ftpReq.UsePassive = true;
                ftpReq.KeepAlive = true;
                ftpReq.Method = WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse response = (FtpWebResponse)ftpReq.GetResponse();
                response.Close();
                res = true;
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(string.Format("Error FTP: al mover el archivo, Exception: {0} ", ex.Message));
            }
            return res;
        }


        public static List<string> ReadExcelLocal(string filename, string localPath)
        {
            Tools.Logging.Info("Inicio ReadExcelLocal: Leyendo archivo " + filename);
            List<string> ListLinesFile = new List<string>();
            try
            {
                using (var stream = File.Open(localPath + filename, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            if (reader.IsDBNull(0))
                            {
                                break;
                            }

                            string line = string.Empty;
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                line += (reader.GetValue(i) == null) ? (count == 0 ? string.Empty : "|") : reader.GetValue(i).ToString() + "|";
                            }
                            line = line.Substring(0, line.Length - 1);
                            ListLinesFile.Add(line);
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.Logging.Error(string.Format("Error ReadExcelLocal: error al leer archivo. [ Exception = {0}]", ex.Message));
            }
            Tools.Logging.Info("Fin ReadExcelLocal.");
            return ListLinesFile;
        }

    }
}
