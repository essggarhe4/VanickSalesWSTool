using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SalesWSTool.Data
{
    public class ReadExcelOpenXML
    {
        public string GetIndividualCellValue(Stream ExcelStream, string sheetName, string addressName)
        {
            string value = null;
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(ExcelStream, false))
            {
                WorkbookPart wbPart = document.WorkbookPart;
                Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().
                  Where(s => s.Name == sheetName).FirstOrDefault();
                if (theSheet == null)
                {
                    throw new ArgumentException("sheetName");
                }
                WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));
                value = GetValueCell(ref wbPart, ref wsPart, addressName);
            }
            return value;
        }

        public string GetValueCell(ref WorkbookPart wbPart, ref WorksheetPart wsPart, string addressName)
        {
            string value = null;
            Cell theCell = wsPart.Worksheet.Descendants<Cell>().
                Where(c => c.CellReference == addressName).FirstOrDefault();
            value = GetCleanValueCell(theCell, ref wbPart);
            return value;
        }

        public List<string> GetSheetsName(Stream ExcelStream)
        {
            List<string> ResultSheetNames = new List<string>();
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(ExcelStream, false))
            {
                WorkbookPart wbPart = document.WorkbookPart;
                Sheets isheets = wbPart.Workbook.Sheets;
                foreach (Sheet currentsheet in isheets)
                {
                    IList<DocumentFormat.OpenXml.OpenXmlAttribute> attr = currentsheet.GetAttributes();
                    string SheetName = attr.Single(s => s.LocalName == "name").Value;
                    ResultSheetNames.Add(SheetName);
                    //foreach (var atttttr in currentsheet.GetAttributes())
                    //{
                    //    //Console.WriteLine("{0}: {1}", attr.LocalName, attr.Value);
                    //    ResultSheetNames.Add(atttttr.Value);
                    //}
                }
            }
            return ResultSheetNames;
        }

        /// <summary>
        /// Obtiene los valores de una Columna
        /// </summary>
        /// <param name="ExcelStream">Stream del excel</param>
        /// <param name="sheetName">Nombre de la hoja</param>
        /// <param name="columnName">Nombre de la columna</param>
        /// <returns></returns>
        public string GetValuesColumnName(Stream ExcelStream, string sheetName, string columnName)
        {
            StringBuilder ValuesColumn = new StringBuilder();
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(ExcelStream, false))
            {
                WorkbookPart wbPart = document.WorkbookPart;
                Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().
                  Where(s => s.Name == sheetName).FirstOrDefault();

                if (theSheet == null)
                {
                    throw new ArgumentException("sheetName");
                }

                WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));
                IEnumerable<Cell> cells = wsPart.Worksheet.Descendants<Cell>().Where(c => string.Compare(GetColumnName(c.CellReference.Value),
                    columnName, true) == 0).OrderBy(R => R.CellReference.Value);
                foreach (Cell currentcell in cells)
                {
                    //ValuesColumn.Append(" Value: " + GetValueCell(ref wsPart, currentcell.CellReference.Value));
                }
            }

            return ValuesColumn.ToString();
        }

        public List<PricingIndexExcel> GetValuesIndexExcel(Stream ExcelStream, string sheetName, string columnName)
        {
            List<PricingIndexExcel> ListIndexExcel = new List<PricingIndexExcel>();
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(ExcelStream, false))
            {
                WorkbookPart wbPart = document.WorkbookPart;
                Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().
                  Where(s => s.Name == sheetName).FirstOrDefault();

                if (theSheet == null)
                {
                    throw new ArgumentException("sheetName");
                }

                WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));
                IEnumerable<Cell> cells = wsPart.Worksheet.Descendants<Cell>().Where(c => string.Compare(GetColumnName(c.CellReference.Value),
                    columnName, true) == 0).OrderBy(R => R.CellReference.Value);
                foreach (Cell currentcell in cells)
                {
                    string cValue = GetCleanValueCell(currentcell, ref wbPart);
                    int cresultint;
                    if (int.TryParse(cValue, out cresultint))
                    {
                        ListIndexExcel.Add(new PricingIndexExcel
                        {
                            Index = cValue,
                            Position = int.Parse(GetRowIndex(currentcell.CellReference.Value))
                        });
                        //ValuesColumn.Append(" Value: " + GetValueCell(ref wbPart, ref wsPart, currentcell.CellReference.Value));
                    }
                }
            }

            ListIndexExcel.OrderBy(k => k.Position);

            return ListIndexExcel.OrderBy(x => x.Position).ToList();
        }

        //Temporal hide
        /*
        public List<SCNNiveldeGestion> GetValuesIndexNiveldeGestion(Stream ExcelStream, string sheetName, string columnName, List<string> GestionListValues, List<string> EstatalValues)
        {
            List<SCNNiveldeGestion> ListNiveldeGestion = new List<SCNNiveldeGestion>();
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(ExcelStream, false))
            {
                WorkbookPart wbPart = document.WorkbookPart;
                Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().
                  Where(s => s.Name == sheetName).FirstOrDefault();

                if (theSheet == null)
                {
                    throw new ArgumentException("sheetName");
                }

                WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(theSheet.Id));
                IEnumerable<Cell> cells = wsPart.Worksheet.Descendants<Cell>().Where(c => string.Compare(GetColumnName(c.CellReference.Value),
                    columnName, true) == 0).OrderBy(R => R.CellReference.Value);
                foreach (Cell currentcell in cells)
                {
                    string cValue = GetCleanValueCell(currentcell, ref wbPart);
                    if (!string.IsNullOrEmpty(cValue))
                    {
                        if (GestionListValues.IndexOf(cValue) != -1)
                        {
                            ListNiveldeGestion.Add(new SCNNiveldeGestion
                            {
                                NiveldeGestionIndex = cValue,
                                Position = GetRowIndex(currentcell.CellReference.Value)
                            });
                        }
                        else
                        {
                            if (EstatalValues.IndexOf(cValue) != -1)
                            {
                                ListNiveldeGestion.Add(new SCNNiveldeGestion
                                {
                                    NiveldeGestionIndex = "Estatal",
                                    Position = GetRowIndex(currentcell.CellReference.Value)
                                });
                            }
                            else
                            {
                                if (!string.Equals(cValue, "Programa de Cumplimiento Grupo Modelo") && !string.Equals(cValue, "Indicar el órgano que emite el ordenamiento (sin abreviaciones)"))
                                {
                                    ListNiveldeGestion.Add(new SCNNiveldeGestion
                                    {
                                        NiveldeGestionIndex = cValue,
                                        Position = "-1"
                                    });
                                }
                            }
                        }
                        //ValuesColumn.Append(" Value: " + GetValueCell(ref wbPart, ref wsPart, currentcell.CellReference.Value));
                    }
                }
            }

            return ListNiveldeGestion;
        }

        */
        //Final Temporal Hide

        private string GetCleanValueCell(Cell theCell, ref WorkbookPart wbPart)
        {
            string value = null;
            if (theCell != null)
            {
                value = theCell.InnerText;
                if (theCell.DataType != null)
                {
                    switch (theCell.DataType.Value)
                    {
                        case CellValues.SharedString:
                            // For shared strings, look up the value in the shared strings table.
                            var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                            // If the shared string table is missing, something's wrong.
                            // Just return the index that you found in the cell.
                            // Otherwise, look up the correct text in the table.
                            if (stringTable != null)
                            {
                                value = stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
                            }
                            break;
                        case CellValues.Boolean:
                            switch (value)
                            {
                                case "0":
                                    value = "FALSE";
                                    break;
                                default:
                                    value = "TRUE";
                                    break;
                            }
                            break;
                    }
                }
            }
            return value;
        }

        private string GetColumnName(string Value)
        {
            StringBuilder ColumnName = new StringBuilder();
            foreach (var alpha in Value.Where(c => Char.IsLetter(c)))
            {
                ColumnName.Append(alpha);
            }
            return ColumnName.ToString();
        }

        private string GetRowIndex(string Value)
        {
            StringBuilder RowIndex = new StringBuilder();
            foreach (var lt in Value.Where(r => char.IsNumber(r)))
            {
                RowIndex.Append(lt);
            }
            return RowIndex.ToString();
        }
    }
}
