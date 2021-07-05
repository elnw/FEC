using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.Data.Informix;

namespace TM.FECentralizada.Data.Configuration
{
    class ConnectionInformix : Connection
    {
        public ConnectionInformix()
        {
        }
        protected override void SetConnection(string StringConnection)
        {
            idbConecction = new IfxConnection(StringConnection);
        }

        public override IDbConnection GetConnection(string StringConnection)
        {
            this.SetConnection(StringConnection);
            return idbConecction;
        }
    }
}
