using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace TM.FECentralizada.Tools
{
    public static class Mail
    {
        public static MailMessage htmlMessage { get; set; }
        public static SmtpClient smtpCliente { get; set; }


        public static bool SendMessage(bool isSuccess, List<String> attachment, string subject, string body)
        {
            bool blnResult = false;
            try
            {
                string from = ConfigurationManager.AppSettings("smtpFrom");
                string[] listDestination = ConfigurationManager.AppSettings(isSuccess ? "SMTP_DESTINATIONSEXITO" : "SMTP_DESTINATIONSERROR").ToString().Split('|');
                string[] listCC = ConfigurationManager.AppSettings("SMTP_CC").ToString().Split('|');

                htmlMessage = new MailMessage();
                if (!from.Equals(""))
                {
                    if (IsMailValid(from))
                    {
                        htmlMessage.From = new MailAddress(from.Trim());
                    }

                }
                if (listDestination.Length > 0)
                {
                    foreach (string destino in listDestination)
                    {
                        if (IsMailValid(destino.Trim()))
                        {
                            htmlMessage.To.Add(destino.Trim());
                        }
                    }
                }
                if (listCC.Length > 0)
                {
                    foreach (string cc in listCC)
                    {
                        if (IsMailValid(cc.Trim()))
                        {
                            htmlMessage.CC.Add(cc.Trim());
                        }
                    }
                }

                if (attachment.Count > 0)
                {
                    foreach (string file in attachment)
                    {
                        Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);
                        htmlMessage.Attachments.Add(data);
                    }
                }
                htmlMessage.Subject = subject;
                htmlMessage.IsBodyHtml = true;
                htmlMessage.BodyEncoding = System.Text.Encoding.UTF8;
                htmlMessage.Body = "<meta charset='ISO-8859-1'><body>" + body + "<br/><p> Saludos cordiales.</p> </body>";

                blnResult = SendMail(htmlMessage);
            }
            catch (Exception e)
            {
                Logging.Error("", "", e.Message);
            }
            return blnResult;
        }

        public static bool SendEmail(string From, List<string> To, List<string> CC, List<string> Attachment, string Subject, string Body, string UserSmtp, string PasswordSmtp, string PortSmtp, string ServiceSmtp)
        {
            bool blnResult = false;
            try
            {
                Logging.Info("Inicio : Envío de Email > [ MailMessage ]");
                htmlMessage = new MailMessage();
                if (!From.Equals(""))
                {
                    if (IsMailValid(From))
                    {
                        htmlMessage.From = new MailAddress(From.Trim());
                    }

                }

                Logging.Info("Inicio : Lectura > [ Destinatarios ]");
                if (To.Count > 0)
                {
                    foreach (string destination in To)
                    {
                        string des = destination.Trim();
                        if (IsMailValid(des))
                        {
                            Logging.Info("Destinatario > [ " + des + " ]");
                            htmlMessage.To.Add(des);
                        }
                    }
                }

                Logging.Info("Inicio : Lectura > [ Contactos Copiados ]");
                if (CC.Count > 0)
                {
                    foreach (string cc in CC)
                    {
                        string c = cc.Trim();
                        if (IsMailValid(c))
                        {
                            Logging.Info("Contacto COpiado > [ " + c + " ]");
                            htmlMessage.CC.Add(c);
                        }
                    }
                }

                Logging.Info("Inicio : Lectura > [ Adjuntos ]");
                if (Attachment.Count > 0)
                {
                    foreach (string file in Attachment)
                    {
                        Logging.Info("Adjunto > [ " + file + " ]");
                        Attachment data = new Attachment(file, MediaTypeNames.Application.Octet);
                        htmlMessage.Attachments.Add(data);
                    }
                }

                htmlMessage.Subject = Subject;
                htmlMessage.IsBodyHtml = true;
                htmlMessage.BodyEncoding = System.Text.Encoding.UTF8;
                htmlMessage.Body = "<meta charset='ISO-8859-1'><body>" + Body + "<br/><p> Saludos cordiales.</p> </body>";


                smtpCliente = new SmtpClient()
                {
                    Port = int.Parse(PortSmtp),
                    Credentials = new NetworkCredential(UserSmtp.Trim(), PasswordSmtp.Trim()),
                    Host = ServiceSmtp
                };
                smtpCliente.Send(htmlMessage);

                blnResult = true;
            }
            catch (Exception ex)
            {
                Logging.Error("", "", ex.Message);
            }
            return blnResult;
        }
        public static bool SendEmailBySMTP(string From, List<string> To, List<string> CC, Dictionary<string, byte[]> oAttachment, string Subject, string Body, string User, string Password, string Port, string Host)
        {
            bool blnResult = false;
            try
            {
                Logging.Info("Inicio : Envío de Email > [ MailMessage ]");
                htmlMessage = new MailMessage();
                if (!From.Equals(""))
                {
                    if (IsMailValid(From))
                    {
                        htmlMessage.From = new MailAddress(From.Trim());
                    }

                }

                Logging.Info("Inicio : Lectura > [ Destinatarios ]");
                if (To.Count > 0)
                {
                    foreach (string destination in To)
                    {
                        string des = destination.Trim();
                        if (IsMailValid(des))
                        {
                            Logging.Info("Destinatario > [ " + des + " ]");
                            htmlMessage.To.Add(des);
                        }
                    }
                }

                Logging.Info("Inicio : Lectura > [ Contactos Copiados ]");
                if (CC != null && CC.Count > 0)
                {
                    foreach (string cc in CC)
                    {
                        string c = cc.Trim();
                        if (IsMailValid(c))
                        {
                            Logging.Info("Contacto COpiado > [ " + c + " ]");
                            htmlMessage.CC.Add(c);
                        }
                    }
                }

                Logging.Info("Inicio : Lectura > [ Adjuntos ]");
                if (oAttachment!= null && oAttachment.Count > 0)
                {
                    foreach (var entry in oAttachment)
                    {
                        Logging.Info(string.Format("Inicio Params:  Key:{0}, Value:{1}", entry.Key.ToString(), entry.Value.ToString()));
                        string FileName = entry.Key;
                        byte[] BytesContent = entry.Value;
                        Stream stream = new MemoryStream(BytesContent);
                        Attachment data = new Attachment(stream, FileName);
                        //Attachment data = new Attachment(stream, MediaTypeNames.Application.Octet);
                        htmlMessage.Attachments.Add(data);

                    }
                }

                htmlMessage.Subject = Subject;
                htmlMessage.IsBodyHtml = true;
                htmlMessage.BodyEncoding = System.Text.Encoding.UTF8;
                htmlMessage.Body = "<meta charset='ISO-8859-1'><body>" + Body + "<br/><p> Saludos cordiales.</p> </body>";


                smtpCliente = new SmtpClient();
                smtpCliente.UseDefaultCredentials = false;

                smtpCliente.Port = int.Parse(Port);
                smtpCliente.Credentials = new NetworkCredential(User.Trim(), Password.Trim());
                smtpCliente.Host = Host;
                smtpCliente.EnableSsl = true;
                smtpCliente.Send(htmlMessage);

                blnResult = true;
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Message);
            }
            return blnResult;
        }

        public static bool SendMail(MailMessage htmlMessage)
        {
            bool blnResult = false;
            try
            {
                string smtpUser = ConfigurationManager.AppSettings("smtpUser");
                string smtpPassword = ConfigurationManager.AppSettings("smtpPass");
                string smtpService = ConfigurationManager.AppSettings("smptHost");
                int smtpPuerto = int.Parse(ConfigurationManager.AppSettings("smptPort"));
                bool smtpCredentials = bool.Parse(ConfigurationManager.AppSettings("defaultCredentials"));
                bool smtpEnableSsl = bool.Parse(ConfigurationManager.AppSettings("enableSsl"));

                smtpCliente = new SmtpClient();
                smtpCliente.Port = smtpPuerto;
                smtpCliente.Credentials = new NetworkCredential(smtpUser.Trim(), smtpPassword.Trim());
                smtpCliente.Host = smtpService;
                smtpCliente.Send(htmlMessage);
                blnResult = true;
            }
            catch (Exception ex)
            {
                Logging.Error("", "", ex.Message);
            }

            return blnResult;
        }

        public static bool IsMailValid(string email)
        {
            Logging.Info("", "", String.Concat("Validando correo: ", email));
            bool blnResult = false;
            try
            {
                blnResult = Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
            }
            catch (Exception ex)
            {
                Logging.Error("", "", ex.Message);
            }
            Logging.Info("", "", (blnResult) ? String.Concat("Correo ", email, " valido") : String.Concat("Correo ", email, " invalido"));
            return blnResult;
        }
    }
}
