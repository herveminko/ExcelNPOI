using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace TerritoryHelperWinClient
{
    class NPOIExcelUtilities
    {


        public static DataTable ExcelSheetToDataTable(string excelFileLocation, int excelSheetIndex)
        {
            // --------------------------------- //
            /* REFERENCES:
             * NPOI.dll
             * NPOI.OOXML.dll
             * NPOI.OpenXml4Net.dll */
            // --------------------------------- //
            /* USING:
             * using NPOI.SS.UserModel;
             * using NPOI.HSSF.UserModel;
             * using NPOI.XSSF.UserModel; */
            // --------------------------------- //
            DataTable Tabla = null;
            try
            {
                if (System.IO.File.Exists(excelFileLocation))
                {

                    IWorkbook workbook = null;  //IWorkbook xls or xlsx              
                    ISheet worksheet = null;
                    string sheetName = "";

                    using (FileStream FS = new FileStream(excelFileLocation, FileMode.Open, FileAccess.Read))
                    {
                        workbook = WorkbookFactory.Create(FS); //Can be XLS or XLSX
                        worksheet = workbook.GetSheetAt(excelSheetIndex
                            ); // Get the desired sheet at the appropriate index
                        sheetName = worksheet.SheetName;  //Get the sheet name

                        Tabla = new DataTable(sheetName);
                        Tabla.Rows.Clear();
                        Tabla.Columns.Clear();

                        // Leer Fila por fila desde la primera
                        for (int rowIndex = 0; rowIndex <= worksheet.LastRowNum; rowIndex++)
                        {
                            DataRow NewReg = null;
                            IRow row = worksheet.GetRow(rowIndex);
                            IRow row2 = null;

                            if (row != null) //null is when the row only contains empty cells 
                            {
                                if (rowIndex > 0) NewReg = Tabla.NewRow();

                                // Loop over all row cells
                                foreach (ICell cell in row.Cells)
                                {
                                    object valorCell = null;
                                    string cellType = "";

                                    if (rowIndex == 0) //The first row contains headers
                                    {
                                        // The concrete column types are to retrieved from the first data row (e.g. #1)
                                        row2 = worksheet.GetRow(rowIndex + 1); 
                                        ICell cell2 = row2.GetCell(cell.ColumnIndex);
                                        switch (cell2.CellType)
                                        {
                                            case CellType.Boolean: cellType = "System.Boolean"; break;
                                            case CellType.String: cellType = "System.String"; break;
                                            case CellType.Numeric:
                                                if (HSSFDateUtil.IsCellDateFormatted(cell2)) { cellType = "System.DateTime"; }
                                                else { cellType = "System.Double"; }
                                                break;
                                            case CellType.Formula:
                                                switch (cell2.CachedFormulaResultType)
                                                {
                                                    case CellType.Boolean: cellType = "System.Boolean"; break;
                                                    case CellType.String: cellType = "System.String"; break;
                                                    case CellType.Numeric:
                                                        if (HSSFDateUtil.IsCellDateFormatted(cell2)) {
                                                            cellType = "System.DateTime";
                                                        }
                                                        else { cellType = "System.Double"; }
                                                        break;
                                                }
                                                break;
                                            default:
                                                cellType = "System.String"; break;
                                        }

                                        // Create an appropriate UI DataTable column with the appropriate type
                                        DataColumn codigo = new DataColumn(cell.StringCellValue, System.Type.GetType(cellType));
                                        Tabla.Columns.Add(codigo);
                                    } 
                                    else
                                    {
                                        // For the concrete data rows...
                                        switch (cell.CellType)
                                        {
                                            case CellType.Blank: valorCell = DBNull.Value; break;
                                            case CellType.Boolean: valorCell = cell.BooleanCellValue; break;
                                            case CellType.String: valorCell = cell.StringCellValue; break;
                                            case CellType.Numeric:
                                                if (HSSFDateUtil.IsCellDateFormatted(cell))
                                                {
                                                    valorCell = DateTime.FromOADate(cell.NumericCellValue);
                                                    /*cell.DateCellValue; */
                                                }
                                                else { valorCell = cell.NumericCellValue; }
                                                break;
                                            case CellType.Formula:
                                                switch (cell.CachedFormulaResultType)
                                                {
                                                    case CellType.Blank: valorCell = DBNull.Value; break;
                                                    case CellType.String: valorCell = cell.StringCellValue; break;
                                                    case CellType.Boolean: valorCell = cell.BooleanCellValue; break;
                                                    case CellType.Numeric:
                                                        if (HSSFDateUtil.IsCellDateFormatted(cell)) { valorCell = cell.DateCellValue; }
                                                        else { valorCell = cell.NumericCellValue; }
                                                        break;
                                                }
                                                break;
                                            default: valorCell = cell.StringCellValue; break;
                                        }
                                        try
                                        {
                                            NewReg[cell.ColumnIndex] = valorCell;
                                        } catch(Exception e)
                                        {

                                        }
                                    }
                                }
                            }
                            if (rowIndex > 0) Tabla.Rows.Add(NewReg);
                        }
                        Tabla.AcceptChanges();
                    }
                }
                else
                {
                    string message = "ERROR: The file " + excelFileLocation + " doesn't exist!!! ";
                    //if (result == DialogResult.Yes)
                    //{
                    //    this.Close();
                    //}
                    //else
                    //{
                    //    // Do something  
                    //}
                    throw new Exception(message);         
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Tabla;
        }

    }
}
