using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Odbc;

namespace Prueba
{
    class Program
    {
        static void Main(string[] args)
        {
            TM.FECentralizada.Tools.Logging.Configure();
            TM.FECentralizada.Tools.Logging.Info("Inicio");
            OdbcConnection DbConnection = new OdbcConnection("DSN=Isis");
            TM.FECentralizada.Tools.Logging.Info("OdbcConnection DbConnection ");
            try
            {
                DbConnection.Open();
                OdbcCommand DbCommand = DbConnection.CreateCommand();
                DbCommand.CommandText = @"Select * FROM fact_fe01_cab WHERE(FECHARECOJO IS NULL) OR(TRIM(FECHARECOJO) IS NULL); ";
                TM.FECentralizada.Tools.Logging.Info("Select");
                OdbcDataReader DbReader = DbCommand.ExecuteReader();
                TM.FECentralizada.Tools.Logging.Info("dbcDataReader DbReader = DbCommand.ExecuteReader();");
                int fCount = DbReader.FieldCount;
                TM.FECentralizada.Tools.Logging.Info("primer for------------------------------------------------------------------");
                for (int i = 0; i < fCount; i++)
                {
                    String fName = DbReader.GetName(i);
                    TM.FECentralizada.Tools.Logging.Info(fName + ":");
                }
                Console.WriteLine();
                TM.FECentralizada.Tools.Logging.Info("segundo for----------------------------------------------------------------------");
                while (DbReader.Read())
                {
                    Console.Write(":");
                    for (int i = 0; i < fCount; i++)
                    {
                        String col = String.IsNullOrEmpty(DbReader.GetString(i))?"": DbReader.GetString(i);
                        TM.FECentralizada.Tools.Logging.Info(col + ":");
                    }
                    Console.WriteLine();
                }

                DbReader.Close();
                DbCommand.Dispose();
                DbConnection.Close();
                TM.FECentralizada.Tools.Logging.Info("DbConnection.Close();");
            }
            catch (Exception e) {
                TM.FECentralizada.Tools.Logging.Error(e.Message);
            }
        }
    }
}
