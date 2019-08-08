using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TerritoryHelperWinClient
{
    class TerritoryHelperWebSkeleton
    {
        NameValueCollection appSettings = ConfigurationSettings.AppSettings;
        /**
         * Get the territory helper addresses data from the web and store the Excel file to the default location if possible.
         */
        public async Task GetTerritoryHelperAddressesAsync(string serverLoginUrl, string servicePath)
        {
            Console.WriteLine("Try to get latest addresses data from territory helper website...");
            await ExecuteTerritoryHelperPostAsyncRequest(serverLoginUrl, servicePath, "all-addresses.xlsx");
        }

        /**
        * Get the territory helper addresses data from the web and store the Excel file to the default location if possible.
        */
        public async Task GetTerritoryHelperTerritoriesAssignmentsAsync(string serverLoginUrl, string servicePath)
        {
            Console.WriteLine("Try to get latest assignments data from territory helper website...");
            await ExecuteTerritoryHelperPostAsyncRequest(serverLoginUrl, servicePath, "all-assignments.xlsx");
        }

        /**
        * Get the territory helper addresses data from the web and store the Excel file to the default location if possible.
        */
        public async Task GetTerritoryHelperTerritoriesCardsAsync(string serverLoginUrl, string servicePath, string format)
        {
            Console.WriteLine("Try to get latest territories geographic data from territory helper website...");
            await ExecuteTerritoryHelperPostAsyncRequest(serverLoginUrl, servicePath, "all-territories." + format);
        }

        /**
        * Get the territory helper addresses data from the web and store the Excel file to the default location if possible.
        */
        public async Task GetTerritoryHelperPublishersAsync(string serverLoginUrl, string servicePath)
        {
            Console.WriteLine("Try to get latest publishers data from territory helper website...");
            await ExecuteTerritoryHelperPostAsyncRequest(serverLoginUrl, servicePath, "all-publishers.xlsx");
        }

        /**
      * Get the territory helper addresses data from the web and store the Excel file to the default location if possible.
      */
        public async Task ExecuteTerritoryHelperPostAsyncRequest(string serverLoginUrl, string requestServicePath, string savePath)
        {
            Console.WriteLine("Executing POST request to territory helper website...");
            Console.WriteLine("serverLoginUrl = " + serverLoginUrl);
            Console.WriteLine("requestServicePath = " + requestServicePath);
            using (var client = new HttpClient())
            {
                // HTTPS Security credentials and Download service URL 
                // Those data should not be viewed in plain text!!!
                string thWebUrl = serverLoginUrl;
                string addServicePath = requestServicePath;

                var values = new Dictionary<string, string>
                    {
                       { "Email", appSettings["territory-helper-user"] },
                       { "Password", "moneboulou" },
                       { "PersistLogin", "false" },
                       { "RedirectUrl",  addServicePath}
                    };

                var content = new FormUrlEncodedContent(values);

                // Sending the HTTP POST request to the server.
                HttpResponseMessage response = await client.PostAsync(thWebUrl, content);
                byte[] responseData = await response.Content.ReadAsByteArrayAsync();

                // Get the path of the output file where to write the result.
                File.WriteAllBytes(savePath, responseData);

                Console.WriteLine("HTTP Response data retrieved from the server!!");

            }
        }

    }
}
