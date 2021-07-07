using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Entities.Backup
{
    public class BackupConfig
    {
        public int monthRange { get; set; }
        public int executionRate { get; set; }
        public int maxAttempts { get; set; }
    }
}
