using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace TM.FECentralizada.Data.Configuration
{
    class ConnectionOracle : Connection
    {
        public ConnectionOracle()
        {
        }
        protected override void SetConnection(string StringConnection)
        {
            idbConecction = new OracleConnection(StringConnection);
        }

        public override IDbConnection GetConnection(string StringConnection)
        {
            this.SetConnection(StringConnection);
            return idbConecction;
        }
    }
}
