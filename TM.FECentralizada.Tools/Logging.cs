using log4net;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace TM.FECentralizada.Tools
{
    public delegate T ExecuteMethodDelegate<out T>(Delegate method);

    public static class Logging
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Logging));

        private static void WriteResponse(object objResult, string strIdSession, string strIdTransaction, string strMethod, string strNameSpace)
        {
            WriteParam(strIdSession, strIdTransaction, strNameSpace, strMethod);

            DataContractJsonSerializer serialize;
            MemoryStream stream;
            byte[]
                propertyNameBuffer;

            serialize = new DataContractJsonSerializer(typeof(object), new DataContractJsonSerializerSettings() { IgnoreExtensionDataObject = true, EmitTypeInformation = EmitTypeInformation.Never });
            stream = new MemoryStream();
            propertyNameBuffer = Encoding.UTF8.GetBytes("\"Parámetros de Salida\": ");
            stream.Write(propertyNameBuffer, 0x00, propertyNameBuffer.Length);

            serialize.WriteObject(stream, objResult);
            string strCadena = Encoding.UTF8.GetString(stream.GetBuffer()).Replace("\0", string.Empty);
            strCadena = strCadena.Replace("},{", "^").Replace("[{", string.Empty).Replace("}]", string.Empty).Replace(":{", "->").Replace("}", string.Empty).Replace("{", string.Empty);
            string[] arrCadena = strCadena.Split('^');
            if (ConfigurationManager.AppSettings("FLAG_LOG") == "1" || arrCadena.Count() < 2)
            {
                foreach (string item in arrCadena)
                {
                    log.Info(item);
                }
            }
            else
            {
                log.Info(string.Format("Datos de Salida -> Total Registros : {0}", arrCadena.Count().ToString()));
            }
        }

        private static void WriteRequest(object objResult, string strIdSession, string strIdTransaction, string strMethod, string strNameSpace)
        {
            WriteParam(strIdSession, strIdTransaction, strNameSpace, strMethod);

            DataContractJsonSerializer serialize;
            MemoryStream stream;
            byte[]
                propertyNameBuffer;

            serialize = new DataContractJsonSerializer(typeof(object), new DataContractJsonSerializerSettings() { IgnoreExtensionDataObject = true, EmitTypeInformation = EmitTypeInformation.Never });
            stream = new MemoryStream();
            propertyNameBuffer = Encoding.UTF8.GetBytes("\"Parámetros de Entrada\": ");
            stream.Write(propertyNameBuffer, 0x00, propertyNameBuffer.Length);
            serialize.WriteObject(stream, objResult);
            string strCadena = Encoding.UTF8.GetString(stream.GetBuffer()).Replace("\0", string.Empty);
            strCadena = strCadena.Replace("},{", "^").Replace("[{", string.Empty).Replace("}]", string.Empty).Replace(":{", "->").Replace("}", string.Empty);
            string[] arrCadena = strCadena.Split('^');
            foreach (string item in arrCadena)
            {
                log.Info(item);
            }
        }

        public static void Configure()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void Info(string strIdSession, string strIdTransaction, string strMessage)
        {
            StackFrame callingframe = new StackTrace(0, true).GetFrames().ElementAt(1);
            MethodBase methodBase = callingframe.GetMethod();
            WriteParam(strIdSession, strIdTransaction, methodBase.DeclaringType.FullName.ToString(), methodBase.Name.ToString());
            log.Info(strMessage);
        }

        public static void Error(string strIdSession, string strIdTransaction, string strMessage)
        {
            StackFrame callingframe = new StackTrace(0, true).GetFrames().ElementAt(1);
            MethodBase methodBase = callingframe.GetMethod();
            WriteParam(strIdSession, strIdTransaction, methodBase.DeclaringType.FullName.ToString(), methodBase.Name.ToString());
            log.Error(strMessage);
        }

        public static void Error(params string[] strParamsMessage)
        {
            StackFrame callingframe = new StackTrace(0, true).GetFrames().ElementAt(1);
            MethodBase methodBase = callingframe.GetMethod();
            string valueParams = string.Empty;
            if (strParamsMessage.Length > 1)
            {
                WriteParam(strParamsMessage[0], strParamsMessage[0], methodBase.DeclaringType.FullName.ToString(), methodBase.Name.ToString());
                log.Error(strParamsMessage[1]);
            }
            else
            {
                log.Error(strParamsMessage[0]);
            }
        }

        public static void Info(params string[] strParamsMessage)
        {
            StackFrame callingframe = new StackTrace(0, true).GetFrames().ElementAt(1);
            MethodBase methodBase = callingframe.GetMethod();
            string valueParams = string.Empty;
            if (strParamsMessage.Length > 1)
            {
                WriteParam(strParamsMessage[0], strParamsMessage[0], methodBase.DeclaringType.FullName.ToString(), methodBase.Name.ToString());
                log.Info(strParamsMessage[1]);
            }
            else
            {
                log.Info(strParamsMessage[0]);
            }
        }
        public static void InfoJson<T>(T ObjectParams)
        {
            var Json = JsonConvert.SerializeObject(ObjectParams);
            log.Info(Json);
        }

        private static void WriteParam(string strIdSession, string strIdTransaction, string strClase, string strMethod)
        {
            ThreadContext.Properties["IDSESSION"] = strIdSession;
            ThreadContext.Properties["IDTRANSACTION"] = strIdTransaction;
            ThreadContext.Properties["CLASS"] = strClase;
            ThreadContext.Properties["METHOD"] = strMethod;
            ThreadContext.Properties["USER"] = Environment.UserName;
        }

        private static List<object> RemoveNull(object objValueList)
        {
            List<object> listRequest = new List<object>();
            foreach (var item in objValueList as IEnumerable) if (item != null) listRequest.Add(item);
            return listRequest;
        }

        public static T ExecuteMethod<T>(string strIdSession, string strTransaction, Func<T> method)
        {
            StackFrame callingframe = new StackTrace(0, true).GetFrames().ElementAt(1);
            MethodBase methodBase = callingframe.GetMethod();

            object target = method.Target;

            if (target != null)
            {
                var fields = target.GetType().GetFields();
                var valueList = fields.Select(field => field.GetValue(target)).ToList();

                if (valueList.Count > 0)
                {
                    var request = RemoveNull(valueList).FindAll(x => x.GetType().Name.Contains("Request")).ToList();
                    if (request.Count > 0)
                    {

                        WriteRequest(request, strIdSession, strTransaction, methodBase.Name.ToString(), methodBase.DeclaringType.FullName.ToString());
                    }
                }

            }
            T d = (T)method.DynamicInvoke();
            WriteResponse(d, strIdSession, strTransaction, methodBase.Name.ToString(), methodBase.DeclaringType.FullName.ToString());
            return d;
        }


        public static T ExecuteMethod<T>(string strIdSession, string strTransaction, object T2, Func<T> method)
        {
            StackFrame callingframe = new StackTrace(0, true).GetFrames().ElementAt(1);
            MethodBase methodBase = callingframe.GetMethod();

            object target = method.Target;

            //validar los parametros que vienen en el metodo y si estan pintando en el log
            if (target != null)
            {
                var fields = target.GetType().GetFields();
                var valueList = fields.Select(field => field.GetValue(target)).ToList();

                if (valueList.Count > 0)
                {
                    WriteRequest(valueList, strIdSession, strTransaction, methodBase.Name.ToString(), methodBase.DeclaringType.FullName.ToString());
                }

            }

            //Get URL Service
            if (T2 != null)
            {
                WriteURL(strIdSession, strTransaction, T2);
            }

            T d = (T)method.DynamicInvoke();
            WriteResponse(d, strIdSession, strTransaction, methodBase.Name.ToString(), methodBase.DeclaringType.FullName.ToString());
            return d;
        }

        public static T ExecuteMethod<T>(object T2, Func<T> method)
        {
            try
            {
                object target = method.Target;
                T d = (T)method.DynamicInvoke();
                return d;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static T ExecuteMethod<T>(string strIdSession, string strTransaction, object T2, Delegate method)
        {
            StackFrame callingframe = new StackTrace(0, true).GetFrames().ElementAt(1);
            MethodBase methodBase = callingframe.GetMethod();

            object target = method.Target;

            //validar los parametros que vienen en el metodo y si estan pintando en el log
            if (target != null)
            {
                var fields = target.GetType().GetFields();
                var valueList = fields.Select(field => field.GetValue(target)).ToList();

                if (valueList.Count > 0)
                {
                    WriteRequest(valueList, strIdSession, strTransaction, methodBase.Name.ToString(), methodBase.DeclaringType.FullName.ToString());
                }

            }

            //Get URL Service
            if (T2 != null)
            {
                WriteURL(strIdSession, strTransaction, T2);
            }

            T d = (T)method.DynamicInvoke();
            WriteResponse(d, strIdSession, strTransaction, methodBase.Name.ToString(), methodBase.DeclaringType.FullName.ToString());
            return d;

        }

        public static void ExecuteMethod(string strIdSession, string strTransaction, object T2, Action method)
        {
            StackFrame callingframe = new StackTrace(0, true).GetFrames().ElementAt(1);
            MethodBase methodBase = callingframe.GetMethod();

            object target = method.Target;

            //validar los parametros que vienen en el metodo y si estan pintando en el log
            if (target != null)
            {
                var fields = target.GetType().GetFields();
                var valueList = fields.Select(field => field.GetValue(target)).ToList();

                if (valueList.Count > 0)
                {
                    WriteRequest(valueList, strIdSession, strTransaction, methodBase.Name.ToString(), methodBase.DeclaringType.FullName.ToString());
                }

            }

            //Get URL Service
            if (T2 != null)
            {
                WriteURL(strIdSession, strTransaction, T2);
            }

            method.DynamicInvoke();

        }

        private static void WriteURL(string strIdSession, string strTransaction, object valueList)
        {
            string strURL = "";
            foreach (PropertyInfo propertyInfo in valueList.GetType().GetProperties())
            {
                if (propertyInfo.Name.ToString().ToUpperInvariant() == "URL")
                {
                    strURL = propertyInfo.GetValue(valueList, null).ToString();
                    break;
                }

                if (propertyInfo.Name.ToString().ToUpperInvariant() == "ENDPOINT")
                {
                    object Endpoint = propertyInfo.GetValue(valueList, null);
                    foreach (PropertyInfo propertyInfoEndpoint in Endpoint.GetType().GetProperties())
                    {
                        if (propertyInfoEndpoint.Name.ToString().ToUpperInvariant() == "ADDRESS")
                        {
                            strURL = propertyInfoEndpoint.GetValue(Endpoint, null).ToString();
                            break;
                        }
                    }
                }
            }
            if (strURL != "")
            {
                log.Info("La URL del Service es: '" + strURL + "'");
            }

        }

        public static void WriteDataBase(string strIdSession, string strTransaction, object result)
        {
            DataContractJsonSerializer serialize;
            MemoryStream stream;
            byte[]
                propertyNameBuffer;

            serialize = new DataContractJsonSerializer(typeof(object), new DataContractJsonSerializerSettings() { IgnoreExtensionDataObject = true, EmitTypeInformation = EmitTypeInformation.Never });
            stream = new MemoryStream();
            propertyNameBuffer = Encoding.UTF8.GetBytes("\"Parámetros de salida\": ");
            stream.Write(propertyNameBuffer, 0x00, propertyNameBuffer.Length);

            serialize.WriteObject(stream, result);
        }
    }
}
