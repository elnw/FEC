namespace TM.FECentralizada.Entities.Common
{
    public class HeaderRequest
    {
        public string UserLogin { get; set; }
        public string ServiceChannel { get; set; }
        public string SessionCode { get; set; }
        public string Application { get; set; }
        public string IdMessage { get; set; }
        public string IpAddress { get; set; }
        public string FunctionalityCode { get; set; }
        public string TransactionTimestamp { get; set; }
        public string ServiceName { get; set; }
        public string Version { get; set; }
    }
}
