using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM.FECentralizada.Entities.Common
{
    public class FileServer
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string LocalPath { get; set; }
        public string Directory { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string DateFormat { get; set; }
        public string NumbDayAgoInput { get; set; }
    }
}
