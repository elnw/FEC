namespace TM.FECentralizada.Tools
{
    public static class ConfigurationManager
    {
        public static string AppSettings(string strKey)
        {
            string strResponse;
            if (System.Configuration.ConfigurationManager.AppSettings[strKey] == null || System.Configuration.ConfigurationManager.AppSettings[strKey] == "")
            {
               Logging.Info("La llave " + strKey + " no esta configurada o esta vacia ");
                throw new System.Exception("La llave " + strKey + " no esta configurada o esta vacia ");
            }
            else
            {
                strResponse = System.Configuration.ConfigurationManager.AppSettings[strKey].ToString();
            }
            return strResponse;
        }

        public static string AppSettings(string strKey, string strDefaultValue)
        {
            string strResponse;
            if (System.Configuration.ConfigurationManager.AppSettings[strKey] == null || System.Configuration.ConfigurationManager.AppSettings[strKey] == "")
            {
                strResponse = strDefaultValue;
            }
            else
            {
                strResponse = System.Configuration.ConfigurationManager.AppSettings[strKey].ToString();
            }
            return strResponse;
        }
    }

}
