using System.Data;
using System.Configuration;

namespace TM.FECentralizada.Data.Configuration
{
    public class FactoriaConexion
    {
        public static IDbConnection GetConnection(DbConnectionId id)
        {
            string strConnection;
            Connection conn;
            switch (id.ToString())
            {
                case "SQL":
                    conn = new ConnectionSQL();
                    strConnection = ConfigurationManager.ConnectionStrings["Url_ConnectionSQL"].ConnectionString;
                    return conn.GetConnection(strConnection);
                case "Oracle":
                    conn = new ConnectionOracle();
                    strConnection = ConfigurationManager.ConnectionStrings["Url_ConnectionORACLE"].ConnectionString;
                    return conn.GetConnection(strConnection);
                case "Informix":
                    conn = new ConnectionOracle();
                    strConnection = ConfigurationManager.ConnectionStrings["Url_ConnectionInformix"].ConnectionString;
                    return conn.GetConnection(strConnection);
                default: return null;
            }
        }
    }
}
