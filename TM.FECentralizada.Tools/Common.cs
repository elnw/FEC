using ExcelDataReader;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using DataTable = System.Data.DataTable;

namespace TM.FECentralizada.Tools
{
    public static class Common
    {
        public static byte[] CreateFileExcel(string strSheetName, DataTable ListData, string[] ColumnsName)
        {
            byte[] bytesContent = null;
            try
            {
                using (ExcelPackage excel = new ExcelPackage())
                {
                    excel.Workbook.Worksheets.Add(strSheetName);
                    string[] excelRange = Constants.CHAR_EXCEL_RANGE.Split('|');
                    string finalRange = excelRange[ColumnsName.Length - 1];
                    string headerRange = string.Format("{0}{1}{2}", "A1:", finalRange, "1");

                    var excelWorksheet = excel.Workbook.Worksheets[strSheetName];

                    excelWorksheet.Cells[headerRange].LoadFromArrays(new List<string[]> { ColumnsName });
                    excelWorksheet.Cells[headerRange].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    excelWorksheet.Cells[headerRange].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    excelWorksheet.Cells[headerRange].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    excelWorksheet.Cells[headerRange].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#a6a6a6"));
                    excelWorksheet.Cells[headerRange].Style.Font.Bold = true;
                    excelWorksheet.Cells[headerRange].AutoFitColumns();
                    int i = 2;
                    foreach (DataRow dr in ListData.Rows)
                    {
                        string RowRange = string.Format("A{0}:", i.ToString()) + finalRange + i.ToString();
                        string[] Rows = new string[ColumnsName.Length];
                        int x = 0;
                        foreach (DataColumn dc in ListData.Columns)
                        {
                            Rows[x] = dr[dc].ToString();
                            x++;
                        }
                        excelWorksheet.Cells[RowRange].LoadFromArrays(new List<string[]> { Rows });
                        excelWorksheet.Cells[RowRange].AutoFitColumns();
                        i++;
                    }
                    using (MemoryStream ms = new MemoryStream())
                    {
                        excel.SaveAs(ms);
                        bytesContent = ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Message);
            }
            return bytesContent;
        }

        public static List<string> ReadLineFromPackage(ExcelPackage Package, int LengthColumn)
        {
            List<string> ListLines = new List<string>();
            try
            {
                ExcelWorksheet worksheet = Package.Workbook.Worksheets[1];
                int ColStar = worksheet.Dimension.Start.Column;
                int ColEnd = LengthColumn;
                int RowStar = worksheet.Dimension.Start.Row;
                int RowEnd = worksheet.Dimension.End.Row;

                for (int x = RowStar; x <= RowEnd; x++)
                {
                    string line = string.Empty;
                    for (int y = ColStar; y <= ColEnd; y++)
                    {
                        string cell = worksheet.Cells[x, y].Value == null ? string.Empty : worksheet.Cells[x, y].Value.ToString();
                        line += string.Concat(cell, y == ColEnd ? "" : "|");
                    }
                    ListLines.Add(line);
                }
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Message);
            }
            return ListLines;
        }

        public static byte[] CreateFileText<T>(List<T> DataList, string columnsName="")
        {
            byte[] bytesContent = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (TextWriter tw = new StreamWriter(ms))
                    {
                        if (columnsName != "") { 
                        tw.WriteLine(columnsName);
                    }
                        foreach (var item in DataList)
                        {
                            tw.WriteLine(item);
                        }
                        tw.Flush();
                        ms.Position = 0;
                        bytesContent = ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Message);
            }
            return bytesContent;
        }

        public static byte[] CreateFileText (string Header, List<string> DataList)
        {
            byte[] bytesContent = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (TextWriter tw = new StreamWriter(ms))
                    {
                        tw.WriteLine(Header);
                        foreach (var item in DataList)
                        {
                            tw.WriteLine(item);
                        }
                        tw.Flush();
                        ms.Position = 0;
                        bytesContent = ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Message);
            }
            return bytesContent;
        }

        public static string ToDelimitedString<T>(this IEnumerable<T> source, Func<T, string> func , string delimiter)
        {
            return ToDelimitedString(source, delimiter , func);
        }

        public static string ToDelimitedString<T>(this IEnumerable<T> source, string delimiter, Func<T, string> func)
        {
            string result = string.Empty;
            try
            {
                result = string.Join(delimiter, source.Select(func).ToArray());
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Message);
            }
            return result;
        }

        public static string GetCommandTextBulkInsert(string TableName, string Columns)
        {
            Logging.Info(string.Format("Nombre de la Tabla : {0} = [ Columnas: {1} ]", TableName, Columns));
            string CommandText = string.Empty;
            try
            {
                CommandText = string.Concat("INSERT INTO ", TableName, " (", Columns, ") VALUES(");
                var SplitColumns = Columns.Split(',');

                for (int i = 1; i <= SplitColumns.Length; i++)
                {
                    CommandText += ":" + i + (i == SplitColumns.Length ? ")" : ", ");
                }
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Message);
            }
            Logging.Info(string.Format("CommandText :[ {0} ]", CommandText));
            return CommandText;
        }
        public static string GetHashString(string inputString)
        {
            string hash = string.Empty;
            try
            {
                string original = DateTime.Now.ToString("yyyyMMddHHmmss");
                string calc = inputString;
                string concat = string.Concat(original, calc);
                long compil = long.Parse(concat);
                hash = DecimalToArbitrarySystem(compil, 64);
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Message);
            }
            return hash;
        }
        public static string GetHashLongString(string inputString)
        {
            string hash = string.Empty;
            try
            {
                string original = DateTime.Now.ToString("yyMMddHHmm");
                string calc = inputString;
                string concat = string.Concat(original, calc);
                long compil = long.Parse(concat);
                hash = DecimalToArbitrarySystem(compil, 64);
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Message);
            }
            return hash;
        }
        public static string GetHashStringMD5(string inputString)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(inputString));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }
            return stringBuilder.ToString();
        }

        public static bool VerifyMd5Hash(string strIdSession, string strIdTransaction, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetHashStringMD5(input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return 0 == comparer.Compare(hashOfInput, hash);

        }

        public static string DecimalToArbitrarySystem(long decimalNumber, int radix)
        {
            const int BitsInLong = 64;
            const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";

            if (radix < 2 || radix > Digits.Length)
                throw new ArgumentException("The radix must be >= 2 and <= " +
                    Digits.Length.ToString());

            if (decimalNumber == 0)
                return "0";

            int index = BitsInLong - 1;
            long currentNumber = Math.Abs(decimalNumber);
            char[] charArray = new char[BitsInLong];

            while (currentNumber != 0)
            {
                int remainder = (int)(currentNumber % radix);
                charArray[index--] = Digits[remainder];
                currentNumber = currentNumber / radix;
            }

            string result = new String(charArray, index + 1, BitsInLong - index - 1);
            if (decimalNumber < 0)
            {
                result = "-" + result;
            }
            return result;
        }
        public static void UploadFtpFile(string strIdSession, string strIdTransaction, FileStream streamWriter, FtpWebRequest ftpRequest)
        {
            try
            {
                //byte[] buffer = ReadAllBytes(streamWriter);
                byte[] bufferRead = new byte[16 * 1024];
                byte[] buffer;
                using (MemoryStream mStream = new MemoryStream())
                {
                    int bit;
                    while ((bit = streamWriter.Read(bufferRead, 0, bufferRead.Length)) > 0)
                    {
                        mStream.Write(bufferRead, 0, bit);
                    }
                    buffer = mStream.ToArray();
                }
                ftpRequest.ContentLength = buffer.Length;
                using (Stream requestStream = ftpRequest.GetRequestStream())
                {
                    requestStream.Write(buffer, 0, buffer.Length);
                    requestStream.Flush();
                    requestStream.Close();
                }
            }
            catch (Exception ex)
            {
                Logging.Error(strIdSession, strIdTransaction, ex.Message);
            }
        }

        private static byte[] ReadAllBytes(string strIdSession, string strIdTransaction, Stream source)
        {
            long originalPosition = source.Position;
            source.Position = 0;
            try
            {
                byte[] readBuffer = new byte[4096];
                int totalBytesRead = 0;
                int bytesRead;
                while ((bytesRead = source.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;
                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = source.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }
                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                source.Position = originalPosition;
            }
        }

        public static List<List<T>> SplitList<T>(List<T> Source, int NumberRecords)
        {
            return Source.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / NumberRecords)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            DataTable table = new DataTable();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static int ConvertMinutesToMilliseconds(int minutes)
        {
            return (int)TimeSpan.FromMinutes(minutes).TotalMilliseconds;
        }

        public static byte[] StreamToByteArray(Stream inputStream)
        {
            byte[] bytesResult = null;
            try
            {
                int length = 900000;
                byte[] bytes = new byte[length];
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    int count;
                    while ((count = inputStream.Read(bytes, 0, bytes.Length)) > 0)
                    {
                        memoryStream.Write(bytes, 0, count);
                    }
                    bytesResult = memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Message);
            }
            return bytesResult;
        }
                
    }
}
