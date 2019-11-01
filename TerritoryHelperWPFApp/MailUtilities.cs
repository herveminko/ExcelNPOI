using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace TerritoryHelperWinClient
{
    class MailUtilities
    {
        NameValueCollection appSettings = ConfigurationSettings.AppSettings;

    public bool sendTerritoryWorkRequestMail(string receiverAddress, List<string> territoriesDescription)
        {
            bool boolResult = true;
            string message;


            try
            {
                MailMessage mail = new MailMessage();
                string territoriesString = "";
                foreach (string desc in territoriesDescription)
                {
                    territoriesString = territoriesString + desc + "\n";
                }

                string mailSender = appSettings["mail-sender"];
                string mailPassword = appSettings["mail-password"];
                string SmtpHost = appSettings["mail-smtp-server"];
                string SmtpPortNumberString = appSettings["mail-smtp-port"];
                int SmtpPortNumber;
                Int32.TryParse(SmtpPortNumberString, out SmtpPortNumber);

                mail.From = new MailAddress(appSettings["mail-sender"]); //Absender 
                mail.To.Add(receiverAddress); //Empfänger 
                mail.Subject = appSettings["mail-subject-territory-work"];
                mail.Body = "Cher proclamateur/Chère proclamatrice,\n\nD'après notre liste tu travailles les territoires:\n\n" +
                        territoriesString +
                        "\n\nPourrais-tu nous dire s'il te plait quand tu les as travaillé (COMPLÈTEMENT!!) la dernière fois?" +
                        "\n\nMerci d'avance pour ta coopération!! Tu es prié(e) de répondre à l'une des adresses Email ci-dessous." +
                        "\n\nN.B: Par défaut, les dernières dates seront utilisées" +
                        "\n\n" + appSettings["signature-intro"] +
                        "\n" + appSettings["signature"];

                if (appSettings["mail-content-add"] != null && appSettings["mail-content-add"].Length > 0)
                {
                    mail.Body = mail.Body + "\n\n" + appSettings["mail-content-add"];
                }
                //mail.IsBodyHtml = true; //Nur wenn Body HTML Quellcode ist 

                SmtpClient client = new SmtpClient(SmtpHost, SmtpPortNumber); //SMTP Server von Hotmail und Outlook. 

                client.Credentials = new System.Net.NetworkCredential(mailSender, mailPassword);//Anmeldedaten für den SMTP Server 

                client.EnableSsl = true; //Die meisten Anbieter verlangen eine SSL-Verschlüsselung 
                //if (!receiverAddress.Contains("@") || receiverAddress.Contains("hcminko"))
                if (receiverAddress.Contains("@"))
                {
                    client.Send(mail); //Senden 
                }
            }
            catch (Exception ex)
            {
                message = appSettings["mail-sending-error-msg"] + ex.Message;
                
                boolResult = false;
            }

            return boolResult;


        }


    public bool sendReturnAllTerritoriesMail(string receiverAddress, List<string> territoriesDescription)
        {
            bool boolResult = true;
            string message;


            try
            {
                MailMessage mail = new MailMessage();
                string territoriesString = "";
                foreach (string desc in territoriesDescription)
                {
                    territoriesString = territoriesString + desc + "\n";
                }

                string mailSender = appSettings["mail-sender"];
                string mailPassword = appSettings["mail-password"];
                string SmtpHost = appSettings["mail-smtp-server"];
                string SmtpPortNumberString = appSettings["mail-smtp-port"];
                int SmtpPortNumber;
                Int32.TryParse(SmtpPortNumberString, out SmtpPortNumber);

                mail.From = new MailAddress(mailSender); //Absender 
                mail.To.Add(receiverAddress); //Empfänger 
                mail.Subject = appSettings["mail-subject-territory-return"];
                mail.Body = "Cher proclamateur/Chère proclamatrice,\n\nD'après notre liste tu travailles les territoires:\n\n" +
                        territoriesString +
                        "\n\nPourrais-tu nous rendre ou rapporter ces cartes de territoires le plus tôt possible s'il te plait? " +
                        "\n\nMerci d'avance pour ta coopération!! En cas de question, tu es prié(e) de répondre à l'une des adresses Email ci-dessous." +
                        "\n\nN.B: Tu peux aussi nous contacter directement à la prochaine occasion" +
                        "\n\n" + appSettings["signature-intro"] +
                        "\n" + appSettings["signature"];

                if (appSettings["mail-content-add"] != null && appSettings["mail-content-add"].Length > 0)
                {
                    mail.Body = mail.Body + "\n\n" + appSettings["mail-content-add"];
                }
                //mail.IsBodyHtml = true; //Nur wenn Body HTML Quellcode ist 


                SmtpClient client = new SmtpClient(SmtpHost, SmtpPortNumber); //SMTP Server von Hotmail und Outlook. 


                client.Credentials = new System.Net.NetworkCredential(mailSender, mailPassword);//Anmeldedaten für den SMTP Server 

                client.EnableSsl = true; //Die meisten Anbieter verlangen eine SSL-Verschlüsselung 
                //if (!receiverAddress.Contains("@") || receiverAddress.Contains("hcminko"))
                if (receiverAddress.Contains("@"))
                {
                    client.Send(mail); //Senden 
                }
            }
            catch (Exception ex)
            {
                message = appSettings["mail-sending-error-msg"] + ex.Message;

                boolResult = false;
            }

            return boolResult;


        }

    }



}
