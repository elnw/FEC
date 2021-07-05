using System.Data;

namespace TM.FECentralizada.Data.Configuration
{
    public abstract class Connection
    {
        protected static IDbConnection idbConecction;

        public Connection()
        {
        }
        protected abstract void SetConnection(string StringConnection);

        public abstract IDbConnection GetConnection(string StringConnection);
    }
}
