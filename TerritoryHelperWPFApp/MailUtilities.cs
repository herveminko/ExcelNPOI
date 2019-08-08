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

                if (appSettings["mail-content-add"].Length >  0)
                {
                    mail.Body = mail.Body + "\n\n" + appSettings["mail-content-add"];
                }
                //mail.IsBodyHtml = true; //Nur wenn Body HTML Quellcode ist 
				string SmtpPortNumberString = appSettings["mail-smtp-port"];
                int SmtpPortNumber;

                Int32.TryParse(SmtpPortNumberString, out SmtpPortNumber);
				string SmtpHost = appSettings["mail-smtp-server"];

                SmtpClient client = new SmtpClient(SmtpHost, SmtpPortNumber); //SMTP Server von Hotmail und Outlook. 

                client.Credentials = new System.Net.NetworkCredential("hcminko@hotmail.com", "minkoabylor29");//Anmeldedaten für den SMTP Server 

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
