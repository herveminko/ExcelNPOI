using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace InterestedExcelParser
{
    class TerritoryHelperFileParser
    {

        /**
         * Parse a given excel file containing interested addresses data
         * and extract needfull data (complete address, territory number 
         * and appartment information). A specific row formatting is applied
         * depending on the rows contents.
         * 
         * param name="sourceFile" is the location of the file to parse.
         */
        public void ParseTerritoryHelperInterestedFile(String sourceFile)
        {
            XSSFWorkbook wb;

            // Reading some configuration/initialization data...  
            Console.WriteLine("Reading some configuration/initialization data...");
            string excelFileLocation = System.Configuration.ConfigurationManager.AppSettings["sourceFilePath"];
            string lastDataColumnsCell = System.Configuration.ConfigurationManager.AppSettings["excel-last-data-column"];
            string wantedColumnsListString = System.Configuration.ConfigurationManager.AppSettings["excel-wanted-columns"];


            // If the default source file location should be overwritten...
            if (sourceFile != null)
            {
                // Overwrite the default source file location
                excelFileLocation = sourceFile;
            }

            // Wait until anexcel addresses file is available at the location defined by 'excelFileLocation'
            int count = 0;
            while (!File.Exists(excelFileLocation) || IsFileLocked(excelFileLocation))
            {
                if (count == 0) { Console.WriteLine("Wait until an excel addresses file is available at " + excelFileLocation + "..."); }
                count++;
            }

            using (FileStream file = new FileStream(@excelFileLocation, FileMode.Open, FileAccess.Read))
            {
                wb = new XSSFWorkbook(file);
            }

            XSSFSheet sh = (XSSFSheet)wb.GetSheetAt(0);
            int rowCount = sh.LastRowNum + 1;
            Console.WriteLine("Source file row count = " + rowCount);
            //Console.WriteLine("Source file columns count = " + sh.GetColumnHelper().lastDataColumnsCell);

            SetSheetSpecialConditonalFormatting(sh);

            Console.WriteLine("Setting columns auto filters...");
            // Set the columns auto filters
            sh.SetAutoFilter(CellRangeAddress.ValueOf("A1" + ":" + lastDataColumnsCell + rowCount));

            // Get the list of column which should remain in the resulting output.
            List<int> wantedColumnsList = wantedColumnsListString.Split(',').Select(int.Parse).ToList();
            int columnCount = AlphabetOrdinal(lastDataColumnsCell);

            Console.WriteLine("Hiding unnecessary columns...");
            // Hide all not wanted columns
            for (int i = 0; i < columnCount; i++)
            {
                if (!wantedColumnsList.Contains(i))
                {
                    sh.SetColumnHidden(i, true);
                }
            }

            // Freeze first row ( headers )
            sh.CreateFreezePane(0, 1);


            string fileLocation = System.Configuration.ConfigurationManager.AppSettings["destinationFilePath"];
            //string fileLocation = "C:\\Development\\c-sharp\\ExcelNPOI\\adresses-bielefeld.xlsx";
            Console.WriteLine("Writing Resulting Excel File to disk: " + fileLocation);

            using (var stream = new FileStream(fileLocation, FileMode.Create, FileAccess.Write))
            {
                wb.Write(stream);
            }

            //Console.WriteLine("Opening the resulting processed file...");
            //Process.Start(fileLocation);

        }

        /**
         * Get all territory numbers having interested addresses from the territoty helper dumped file. 
         * 
         */
        public List<int> GetAllTerritoryNumbersHavingLocations()
        {
            List<int> resultList = new List<int>();

            XSSFWorkbook wb = null;
            string @excelFileLocation = System.Configuration.ConfigurationManager.AppSettings["destinationFilePath"];
            using (FileStream file = new FileStream(@excelFileLocation, FileMode.Open, FileAccess.Read))
            {
                wb = new XSSFWorkbook(file);
            }

            XSSFSheet sh = (XSSFSheet)wb.GetSheetAt(0);
            //Loop the records upto filled row 
            for (int row = 1; row < sh.LastRowNum; row++)
            {
                //null is when the row only contains empty cells  
                if (sh.GetRow(row) != null)
                {
                    string value = sh.GetRow(row).GetCell(1).StringCellValue; //Here for sample , I just save the value in "value" field, Here you can write your custom logics...  
                    int intValue = Int32.Parse(value);
                    if (!resultList.Contains(intValue))
                    {
                        resultList.Add(intValue);
                    }
                }
            }

            return resultList;
        }

        /**
         * Create an excel address file for a given territory number.
         * @param name="territoryNumber" is the number of the territory for which 
         * addresses must be extracted.
         */
        public void CreateTerritoryAddressesFile(int territoryNumber)
        {
            XSSFWorkbook wb = null;
            string @excelFileLocation = System.Configuration.ConfigurationManager.AppSettings["destinationFilePath"];
            using (FileStream file = new FileStream(@excelFileLocation, FileMode.Open, FileAccess.Read))
            {
                wb = new XSSFWorkbook(file);
            }

            XSSFSheet sh = (XSSFSheet)wb.GetSheetAt(0);

            XSSFWorkbook wb2 = new XSSFWorkbook();
            XSSFSheet sheet = (XSSFSheet)wb2.CreateSheet();



            //Loop the records upto filled row 
            for (int row = 1; row <= sh.LastRowNum; row++)
            {
                //null is when the row only contains empty cells  
                if (sh.GetRow(row) != null)
                {
                    string value = sh.GetRow(row).GetCell(1).StringCellValue; //Here for sample , I just save the value in "value" field, Here you can write your custom logics...  

                    int intValue = Int32.Parse(value);
                    //Console.WriteLine("current territory value = " + intValue);
                    if (!intValue.Equals(territoryNumber))
                    {
                        DeleteRow(sh, sh.GetRow(row));
                    }
                    else
                    {
                        // If the column "Logement" is empty, delete the whole row
                        string value2 = sh.GetRow(row).GetCell(10).StringCellValue;
                        if (value2 == null || value2.Length == 0)
                        {
                            DeleteRow(sh, sh.GetRow(row));
                        }
                    }
                }
            }
            Directory.CreateDirectory("output");
            using (var stream = new FileStream("output\\TER-" + territoryNumber + "-Adresses.xlsx", FileMode.Create, FileAccess.Write))
            {
                wb.Write(stream);
            }

        }

        /**
         * Delete an excel row from a given Sheet
         */
        public static void DeleteRow(ISheet sheet, IRow row)
        {
            // sheet.RemoveRow(row);   //this only deletes all the cell values
            row.Hidden = true;           

            int rowIndex = row.RowNum;
            int lastRowNum = sheet.LastRowNum;

            if (rowIndex >= 0 && rowIndex < lastRowNum)
            {
                // sheet.ShiftRows(rowIndex + 1, lastRowNum, -1); 
            }
            
        }


        /**
         * Set conditional formats for specific rows contents.
         */
        private void SetSheetSpecialConditonalFormatting(XSSFSheet sh)
        {
            int rowCount = sh.LastRowNum + 1;
            string frechCondition = System.Configuration.ConfigurationManager.AppSettings["condition-french"];
            string dontVisitCondition = System.Configuration.ConfigurationManager.AppSettings["condition-dont-visit"];
            string conditionalRangeStartCell = System.Configuration.ConfigurationManager.AppSettings["condition-data-range-start"];
            string lastDataColumnsCell = System.Configuration.ConfigurationManager.AppSettings["excel-last-data-column"];

            Console.WriteLine("Creating conditional formatting filter...");
            XSSFSheetConditionalFormatting sCF = (XSSFSheetConditionalFormatting)sh.SheetConditionalFormatting;

            //Fill french speaking address
            XSSFConditionalFormattingRule cfFrench =
               (XSSFConditionalFormattingRule)sCF.CreateConditionalFormattingRule(frechCondition);
            XSSFPatternFormatting fillFrench = (XSSFPatternFormatting)cfFrench.CreatePatternFormatting();
            fillFrench.FillBackgroundColor = IndexedColors.LightGreen.Index;
            fillFrench.FillPattern = FillPattern.SolidForeground;

            //Fill Not interested address
            XSSFConditionalFormattingRule cfNotInterested =
               (XSSFConditionalFormattingRule)sCF.CreateConditionalFormattingRule(dontVisitCondition);
            XSSFPatternFormatting fillNotInterested = (XSSFPatternFormatting)cfNotInterested.CreatePatternFormatting();
            fillNotInterested.FillBackgroundColor = IndexedColors.Red.Index;
            fillNotInterested.FillPattern = FillPattern.SolidForeground;


            //Fill address to verify
            XSSFConditionalFormattingRule cfToVerify =
               (XSSFConditionalFormattingRule)sCF.CreateConditionalFormattingRule("true");
            XSSFPatternFormatting fillVerify = (XSSFPatternFormatting)cfToVerify.CreatePatternFormatting();
            fillVerify.FillBackgroundColor = IndexedColors.White.Index;
            fillVerify.FillPattern = FillPattern.SolidForeground;

            // The first forat setting wins if multiple can be applyed!!!
            XSSFConditionalFormattingRule[] cfRules = { cfNotInterested, cfFrench, cfToVerify };

            Console.WriteLine("Setting conditional formatting filter...");
            // Setting the conditional cell range: e.g "A2:U1082"
            CellRangeAddress[] cfRange = { CellRangeAddress.ValueOf(conditionalRangeStartCell + ":" + lastDataColumnsCell + rowCount) };
            sCF.AddConditionalFormatting(cfRange, cfRules);

        }

        /**
         *  Get the alphabeticat positoin of a given letter.
         *  E.g. A --> 1, D --> 4 etc...
         */
        private static int AlphabetOrdinal(string input)
        {
            return (int)input.ToUpper()[0] - 64;
        }

        /**
         * Check whether a given file can be read or not.
         */
        protected virtual bool IsFileLocked(string fileLocation)
        {
            try
            {
                using (Stream stream = new FileStream(fileLocation, FileMode.Open))
                {
                    // File/Stream manipulating code here
                }
            }
            catch
            {
                //check here why it failed and ask user to retry if the file is in use.
                return true;
            }

            return false;
        }

    }
}
