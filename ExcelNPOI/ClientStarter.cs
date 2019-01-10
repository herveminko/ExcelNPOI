using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace InterestedExcelParser
{
    /**
     * Utility class used to parse territory helper interested addresses 
     * and transform it into a reduced Excel form.
     * 
     */
    class TerritoryHelperUtilClient
    {
        // HTTP client used to communicate with territory helper web site.
        public static TerritoryHelperClient territoryHelperWebClient = new TerritoryHelperClient();

        // Helper class used to read territory helper excel files and edit excel documents.
        public static TerritoryHelperFileParser territoryHelperFileParser = new TerritoryHelperFileParser();

        /**
         * Entry point of the program executable.
         */
        static void Main(string[] args)
        {
            //ExcelNPOIParser starter = new ExcelNPOIParser();
            string territoryHelperExportFile = null;

            if (args.Length > 0)
            {
                // If an existing territory helper address file is passed as parameter
                territoryHelperExportFile = args[0];
                Console.WriteLine("Using interested Excel file " + territoryHelperExportFile);
            }
            else
            {
                // Try to get latest addresses data from territory helper website
                Console.WriteLine("Using default interested Excel file location from configuration...");
                territoryHelperWebClient.GetTerritoryHelperAddressesAsync();
            }
            territoryHelperFileParser.ParseTerritoryHelperInterestedFile(territoryHelperExportFile);
            List<int> territories = territoryHelperFileParser.GetAllTerritoryNumbersHavingLocations();
            for (int territoryNumber = 0; territoryNumber < territories.Count; territoryNumber++)
            {
               territoryHelperFileParser.CreateTerritoryAddressesFile(territories[territoryNumber]);
            }
            //territoryHelperWebClient.printTerritoryAddresses();
            //Console.ReadKey();
        }


    }
}
