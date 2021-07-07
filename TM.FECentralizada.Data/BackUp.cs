using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TM.FECentralizada.Entities.Backup;

namespace TM.FECentralizada.Data
{
    public class BackUp
    {
        public static void MakeBackup(ref bool shouldRepeat, BackupConfig backup)
        {
            string messageResponse = "";
            try
            {
                using(SqlConnection conn = (SqlConnection)Configuration.FactoriaConexion.GetConnection(Configuration.DbConnectionId.SQL))
                {
                    using (SqlCommand cmd = new SqlCommand("Sp_Crear_Backup", conn))
                    {
                        conn.Open();

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@mes_rango", SqlDbType.VarChar) { Value = backup.monthRange, Direction = ParameterDirection.Input });
                        cmd.Parameters.Add(new SqlParameter("@mensaje_respuesta", SqlDbType.VarChar) { Value = "", Direction = ParameterDirection.Output, Size = 3000 });

                        cmd.ExecuteNonQuery();

                        messageResponse = cmd.Parameters["@mensaje_respuesta"].Value.ToString();
                        Tools.Logging.Info($"Sp_Crear_Backup: {messageResponse}");
                        shouldRepeat = false;
                    }
                }


            }catch(Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }


    }
}
