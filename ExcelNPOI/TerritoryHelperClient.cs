using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InterestedExcelParser
{
    class TerritoryHelperClient
    {
       
        public void printTerritoryAddresses()
        {
            // generate a file name as the current date/time in unix timestamp format
            string file = (string)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString();

            // the directory to store the output.
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // initialize PrintDocument object
            PrintDocument doc = new PrintDocument()
            {
                PrinterSettings = new PrinterSettings()
                {
                    // set the printer to 'Microsoft Print to PDF'
                    PrinterName = "Microsoft Print to PDF",

                    // tell the object this document will print to file
                    PrintToFile = true,

                    // set the filename to whatever you like (full path)
                    PrintFileName = Path.Combine(directory, file + ".pdf"),
                }
            };

            doc.Print();
        }
        
        /**
          * Get the territory helper addresses data from the web and store the Excel file to the default location if possible.
          */
        public async void GetTerritoryHelperAddressesAsync()
        {
            Console.WriteLine("Try to get latest addresses data from territory helper website...");
            using (var client = new HttpClient())
            {
                // HTTPS Security credentials and Download service URL 
                // Those data should not be viewed in plain text!!!
                string thWebUrl = System.Configuration.ConfigurationManager.AppSettings["territory-helper-url"];
                string addServicePath = System.Configuration.ConfigurationManager.AppSettings["address-service-path"];

                var values = new Dictionary<string, string>
                    {
                       { "Email", "hcminko@hotmail.com" },
                       { "Password", "moneboulou" },
                       { "PersistLogin", "false" },
                       { "RedirectUrl",  addServicePath}
                    };

                var content = new FormUrlEncodedContent(values);

                // Sending the HTTP POST request to the server.
                HttpResponseMessage response = await client.PostAsync(thWebUrl, content);
                byte[] responseData = await response.Content.ReadAsByteArrayAsync();

                // Get the path of the output file where to write the result.
                string excelSourceFileLocation = System.Configuration.ConfigurationManager.AppSettings["sourceFilePath"];
                File.WriteAllBytes(excelSourceFileLocation, responseData);

                Console.WriteLine("HTTP Response data retrieved and saved!!");

            }
        }

    }
}
