using System;
using System.Data;
using System.Data.SqlClient;

namespace TM.FECentralizada.Data.Configuration
{
    class ConnectionSQL : Connection
    {
        public ConnectionSQL()
        {
        }
        protected override void SetConnection(String StringConnection)
        {
            idbConecction = new SqlConnection(StringConnection);
        }

        public override IDbConnection GetConnection(String StringConnection)
        {
            this.SetConnection(StringConnection);
            return idbConecction;
        }
    }
}
