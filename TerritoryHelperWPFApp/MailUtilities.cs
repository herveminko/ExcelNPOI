using System;
using System.Collections.Generic;
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

                mail.From = new MailAddress("hcminko@hotmail.com"); //Absender 
            mail.To.Add(receiverAddress); //Empfänger 
            mail.Subject = "Travail de tes territoires";
            mail.Body = "Cher proclamateur,\nD'après notre liste tu travalles les territoires:\n\n" +
                    territoriesString +
                    "\n\nPourrais-tu nous dire s'il te plait quand tu les as travaillé complètement la dernière fois?" +
                    "\n\nMerci d'avance pour ta coopération!!" +
                    "\n\nN.B: Par défaut, les dernières dates seront utilisées" +
                    "\n\nTes frères" +
                    "\nHervé Minko & Hervé Ngassop";
            //mail.IsBodyHtml = true; //Nur wenn Body HTML Quellcode ist 

            SmtpClient client = new SmtpClient("smtp.live.com", 25); //SMTP Server von Hotmail und Outlook. 

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
                message = "Erreur durant l'envoi du courriel\n\n" + ex.Message;
                
                boolResult = false;
            }

            return boolResult;


        }

    }
}
