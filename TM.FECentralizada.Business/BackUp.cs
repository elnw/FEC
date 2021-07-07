using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TM.FECentralizada.Data;
using TM.FECentralizada.Entities.Backup;

namespace TM.FECentralizada.Business
{
    public static class BackUp
    {
        public static void MakeBackup(BackupConfig backup)
        {
            bool shouldRepeat = true;
            try
            {
                for(int i = 0; i < backup.maxAttempts; i++)
                {
                    Data.BackUp.MakeBackup(ref shouldRepeat, backup);
                    if (!shouldRepeat) break;
                }


            }catch(Exception ex)
            {
                Tools.Logging.Error(ex.Message);
            }
        }
    }
}
