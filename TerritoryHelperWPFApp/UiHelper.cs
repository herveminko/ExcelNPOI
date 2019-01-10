using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerritoryHelperWinClient;
using Newtonsoft.Json.Linq;
using System.IO;

namespace TerritoryHelperWPFApp
{
    class UiHelper
    {
        static Hashtable publishersAssignments;
        static DataTable publishers;
        static DataTable assignments;


        public static List<Territory> loadTerritoriesFromDisk()
        {
            List<Territory> territoriesData = new List<Territory>();
            

            string filepath = "all-territories.json";

            if (File.Exists(filepath))
            {
                string readResult = string.Empty;
                string writeResult = string.Empty;
                using (StreamReader r = new StreamReader(filepath))
                {
                    var json = r.ReadToEnd();
                    var jobj = JObject.Parse(json);


                    readResult = jobj.ToString();

                    dynamic x = Newtonsoft.Json.JsonConvert.DeserializeObject(readResult);
                    var features = x.features;
                    foreach (var item in features)
                    {
                        var props = item.properties;
                        string type = props.TerritoryType;
                        if (!type.Equals("P"))
                        {
                            Territory territory = new Territory();
                            string number = props.TerritoryNumber;
                            string description = props.description;

                            territory.type = type;
                            territory.number = number;
                            territory.description = description;

                            territoriesData.Add(territory);
                        }

                    }

                }
                Console.WriteLine(readResult);
            }

            return territoriesData;
        }


        public static DataTable loadPublishersFromDisk()
        {
            publishers = NPOIExcelUtilities.ExcelSheetToDataTable("all-publishers.xlsx", 0);            
            return publishers;
        }

        public static DataTable loadAssignmentsFromDisk()
        {
            try
            {
                assignments = NPOIExcelUtilities.ExcelSheetToDataTable("all-assignments.xlsx", 0);
            }
            catch (Exception ex)
            {
                // throw ex; 
            }
            return assignments;
        }

        public static Hashtable getPublishersAssignments(DataTable assignmentsDatatable)
        {
            Dictionary<string, DataTable> dictionary = new Dictionary<string, DataTable>();
            foreach (DataRow row in assignmentsDatatable.Rows)
            {
                DataTable userTerritoriesTable = new DataTable("Liste de territoires");
                DataRow newRow = userTerritoriesTable.NewRow();

                string cellType = "System.String";
                DataColumn newHeader = new DataColumn("Liste des Territoires du proclamateur", System.Type.GetType(cellType));
                userTerritoriesTable.Columns.Add(newHeader);

                if (row["Restitués"] == null || row["Restitués"].ToString().Length == 0)
                {



                    string territoryDescription = row["Numéro de territoire"].ToString() + " -- " + row["Titre de territoire"].ToString();
                    string email = null;
                    string name;
                    string keyValue;
                    if (row["Email"] != null && row["Email"].ToString().Length > 0)
                    {
                        email = row["Email"].ToString();
                        keyValue = email;
                    }
                    else
                    {
                        name = row["Nom du proclamateur"].ToString();
                        keyValue = name;
                    }


                    if (dictionary.ContainsKey(keyValue))
                    {
                        userTerritoriesTable = dictionary[keyValue];
                    }

                    newRow = userTerritoriesTable.NewRow();
                    newRow[0] = territoryDescription;
                    userTerritoriesTable.Rows.Add(newRow);

                    dictionary.Remove(keyValue);
                    dictionary.Add(keyValue, userTerritoriesTable);
                }
            }

            publishersAssignments = new Hashtable(dictionary);

            return publishersAssignments;


        }

        public static DataTable loadAllAddressesFromDisk()
        {
            DataTable result = null;
            if (File.Exists("all-addresses.xlsx"))
            {
                result =  NPOIExcelUtilities.ExcelSheetToDataTable("all-addresses.xlsx", 0);
            }
            return result;
        }
    }
}
