using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.SharePoint;
using SalesWSTool.Data;
using System.IO;
using System.Data;
using System.Web.Script.Serialization;
using SalesWSTool.ServiceSalesWS;

namespace SalesWSTool.Services
{
    class PricingUploadEXCEL : IHttpHandler
    {
        PricingConfigurations pricingConfig;

        string GlobalFileName = string.Empty;
        Stream PricingExcelStream;
        string PricingMessageError = string.Empty;

        List<SQFT> PricingFinalHeaders = new List<SQFT>();
        List<SQFT> PricingExtraHeaders = new List<SQFT>();

        string PricingSheetName = string.Empty;

        ReadExcelOpenXML pricingReadExcel = new ReadExcelOpenXML();

        int ErrorCounter = 0;
        PricingResult PricingResults = new PricingResult();
        List<string> PricingLogErrors = new List<string>();

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                AnalyzeExcelFile(context);                
            }
            catch(Exception ex)
            {
                PricingResults.IsSuccess = false;
                AddError(ex.Message);
            }
            PricingResults.NumberofErrors = ErrorCounter.ToString();
            PricingResults.MessageErrors = PricingLogErrors;

            JavaScriptSerializer lSerializer = new JavaScriptSerializer();
            context.Response.Write(lSerializer.Serialize(PricingResults));
            context.Response.End();
        }

        private void AnalyzeExcelFile(HttpContext context)
        {
            string ProductsId = context.Request.Params["productsid"];
            string PricingAction = context.Request.Params["pricingaction"];
            if (!string.IsNullOrEmpty(ProductsId) && !string.IsNullOrEmpty(PricingAction))
            {
                if (GetStreamFromFile(context))
                {
                    if (GetConfigurations())
                    {
                        if (GetExcelSheet())
                        {
                            if (ValidateSQHeader())
                            {                                
                                //Get all the branches
                                List<PricingIndexExcel> ListBranches = pricingReadExcel.GetValuesIndexExcel(PricingExcelStream, PricingSheetName, pricingConfig.ColumnBranch);
                                if (ValidateDualBranch(ListBranches))
                                {
                                    ErrorCounter = 0;
                                    //Get start and end rows index
                                    int startInfoRow = int.Parse(pricingConfig.PricingHeader) + 1;
                                    int endInfRow = ListBranches[ListBranches.Count - 1].Position;
                                    //Validate the information
                                    List<PricingExcelObj> ListPricingExcelObj = new List<PricingExcelObj>();
                                    for (int i = startInfoRow; i < endInfRow + 1; i++)
                                    {
                                        PricingExcelObj PEO = new PricingExcelObj();
                                        PEO.Prices = new List<SQFT>();
                                        ValidateBranch(i, ref PEO);
                                        ValidateAddINIFee(i, ref PEO);
                                        ValidateRNLMult(i, ref PEO);
                                        ValidateApps(i, ref PEO);
                                        ValidatePrices(i, ref PEO);
                                        ValidateExcessIncrement(i, ref PEO);
                                        ValidateExcessValuePerIncrement(i, ref PEO);
                                        ListPricingExcelObj.Add(PEO);
                                    }
                                    //Actions
                                    switch (PricingAction)
                                    {
                                        case "validate":
                                            if (ErrorCounter == 0)
                                            {
                                                if (ListPricingExcelObj.Count <= 250)
                                                {
                                                    PricingResults.IsSuccess = true;
                                                    PricingResults.NumberOfRows = ListPricingExcelObj.Count.ToString();
                                                    PricingResults.ExcelTabName = PricingSheetName;
                                                    PricingResults.ExcelFileName = GlobalFileName;
                                                }
                                                else
                                                {
                                                    PricingResults.IsSuccess = false;
                                                    AddError("There are more that 250 rows in the excel file");
                                                }
                                            }
                                            else
                                            {
                                                PricingResults.IsSuccess = false;
                                            }
                                            break;
                                        case "insert":
                                            int CounterUploadesPricingData = 0;
                                            int CounterRemovedPricingData = 0;

                                            if (ErrorCounter <= 0)
                                            {
                                                PricingResults.IsSuccess = true;

                                                SalesWebService SWS = new SalesWebService();
                                                List<string> ListProducts = ProductsId.Split(',').ToList();
                                                foreach (string prod in ListProducts)
                                                {
                                                    int intProductID = int.Parse(prod);
                                                    Product cProduct = SWS.GetProductByID(intProductID);
                                                    CounterRemovedPricingData += SWS.DeletePricingDataByProductID(intProductID);
                                                    List<PricingRecord> ListnewPrices = new List<PricingRecord>();
                                                    if (cProduct.IsOneTime)//OneTime = true
                                                    {
                                                        ListnewPrices.Clear();
                                                        foreach (PricingExcelObj cPEO in ListPricingExcelObj)
                                                        {
                                                            ListnewPrices.Clear();
                                                            foreach (SQFT cPrice in cPEO.Prices)
                                                            {
                                                                PricingRecord newPrice = new PricingRecord();
                                                                newPrice.ProductID = intProductID;
                                                                newPrice.Branch = cPEO.BranchNumber;
                                                                newPrice.INI_Amount = 0;
                                                                newPrice.Reg_Amount = 0;
                                                                newPrice.Renewal_Amount = 0;
                                                                newPrice.OneTime_Amount = decimal.Parse(cPrice.Value);
                                                                newPrice.SqFtFrom = cPrice.SqFtFrom;
                                                                newPrice.SqFtTo = cPrice.SqTfTo;
                                                                newPrice.IsMultiplier = false;
                                                                newPrice.IsBaseRecord = false;
                                                                newPrice.CreatedBy = SPContext.Current.Web.CurrentUser.Name;
                                                                newPrice.DateCreated = DateTime.Now;
                                                                newPrice.LastUpdatedBy = SPContext.Current.Web.CurrentUser.Name;
                                                                newPrice.DateLastUpdated = DateTime.Now;
                                                                newPrice.Brand = "TG";
                                                                newPrice.FlatCostPerSqFt = cPEO.ExcessIncrement;
                                                                newPrice.FlatCostPerSqFtPrice = cPEO.ExcessValuePerIncrement;
                                                                newPrice.SpcFlatCostPerSqFt = 0;
                                                                newPrice.SpcFlatCostPerSqFtPrice = 0;
                                                                ListnewPrices.Add(newPrice);
                                                            }

                                                            SalesWSResult resultws = SWS.SendPricingRecords(ListnewPrices, intProductID);
                                                            CounterUploadesPricingData += resultws.CounterUpdated;
                                                            if (!resultws.IsSuccess)
                                                            {
                                                                PricingResults.IsSuccess = false;
                                                                AddError(resultws.Message);
                                                            }

                                                            SalesWSResult BPResult = SWS.UpdateBranchProduct(cPEO.BranchNumber, intProductID, int.Parse(cPEO.NumberAppas));
                                                            if (!BPResult.IsSuccess)
                                                            {
                                                                PricingResults.IsSuccess = false;
                                                                AddError(BPResult.Message);
                                                            }
                                                        }
                                                        //send to record                                                                                                               
                                                    }
                                                    else
                                                    {
                                                        ListnewPrices.Clear();
                                                        foreach (PricingExcelObj cPEO in ListPricingExcelObj)
                                                        {
                                                            ListnewPrices.Clear();
                                                            foreach (SQFT cPrice in cPEO.Prices)
                                                            {
                                                                PricingRecord newPrice = new PricingRecord();
                                                                newPrice.ProductID = intProductID;
                                                                newPrice.Branch = cPEO.BranchNumber;
                                                                newPrice.INI_Amount = decimal.Parse(cPEO.AddINIFee);
                                                                newPrice.Reg_Amount = decimal.Parse(cPrice.Value);
                                                                newPrice.Renewal_Amount = decimal.Parse(cPrice.Value) * decimal.Parse(cPEO.RLNMult);
                                                                newPrice.OneTime_Amount = 0;
                                                                newPrice.SqFtFrom = cPrice.SqFtFrom;
                                                                newPrice.SqFtTo = cPrice.SqTfTo;
                                                                newPrice.IsMultiplier = false;
                                                                newPrice.IsBaseRecord = false;
                                                                newPrice.CreatedBy = SPContext.Current.Web.CurrentUser.Name;
                                                                newPrice.DateCreated = DateTime.Now;
                                                                newPrice.LastUpdatedBy = SPContext.Current.Web.CurrentUser.Name;
                                                                newPrice.DateLastUpdated = DateTime.Now;
                                                                newPrice.Brand = "TG";
                                                                newPrice.FlatCostPerSqFt = cPEO.ExcessIncrement;
                                                                newPrice.FlatCostPerSqFtPrice = cPEO.ExcessValuePerIncrement;
                                                                newPrice.SpcFlatCostPerSqFt = 0;
                                                                newPrice.SpcFlatCostPerSqFtPrice = 0;
                                                                ListnewPrices.Add(newPrice);
                                                            }

                                                            SalesWSResult resultws = SWS.SendPricingRecords(ListnewPrices, intProductID);
                                                            CounterUploadesPricingData += resultws.CounterUpdated;
                                                            if (!resultws.IsSuccess)
                                                            {
                                                                PricingResults.IsSuccess = false;
                                                                AddError(resultws.Message);
                                                            }

                                                            SalesWSResult BPResult = SWS.UpdateBranchProduct(cPEO.BranchNumber, intProductID, int.Parse(cPEO.NumberAppas));
                                                            if (!BPResult.IsSuccess)
                                                            {
                                                                PricingResults.IsSuccess = false;
                                                                AddError(BPResult.Message);
                                                            }
                                                        }
                                                        //send to record
                                                        
                                                    }
                                                }//End for eahc

                                                PricingResults.NumberOfRows = ListPricingExcelObj.Count.ToString();
                                                PricingResults.NumberofRemoved = CounterRemovedPricingData.ToString();
                                                PricingResults.NumberofRowsPricingData = CounterUploadesPricingData.ToString();
                                            }
                                            break;
                                    }
                                }
                                else//Fails validate branches dual
                                {
                                    PricingResults.IsSuccess = false;
                                }
                            }
                            else//Fails to validated the header prices
                            {
                                PricingResults.IsSuccess = false;
                            }
                        }
                        else//Fails read sheets in file
                        {
                            PricingResults.IsSuccess = false;
                        }
                    }
                    else//Fail to get cconfigurations
                    {
                        PricingResults.IsSuccess = false;
                    }
                }
                else//Fails get stream
                {
                    PricingResults.IsSuccess = false;                    
                }
            }
            else//Productid or action empty
            {
                PricingResults.IsSuccess = false;
                AddError("You need to select a action and products");                
            }
        }

        private bool GetConfigurations()
        {
            bool result = false;
            try
            {
                pricingConfig = new PricingConfigurations(SPContext.Current.Site.ID, SPContext.Current.Web.ID);
                if (pricingConfig.IsSuccess)
                {
                    result = true;
                }
                else
                {
                    result = false;
                    AddError(pricingConfig.Message);
                }
            }
            catch(Exception ex)
            {
                AddError(ex.Message);
                result = false;
            }
            return result;
        }

       

        /// <summary>
        /// Get the filestream from the Excel File
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns>True if the Stream is loaded correctly</returns>
        private bool GetStreamFromFile(HttpContext context)
        {
            try
            {
                if (context.Request.Files.Count > 0)
                {
                    string strExtension = Path.GetExtension(context.Request.Files[0].FileName).ToLower();
                    if (string.Equals(strExtension, ".xlsx"))
                    {
                        var FileUpload1 = context.Request.Files[0];
                        GlobalFileName = FileUpload1.FileName;
                        PricingExcelStream = FileUpload1.InputStream;
                        return true;
                    }
                    else
                    {
                        AddError("You can only import .xlsx files.");
                        return false;
                    }
                }
                else
                {
                    AddError("No selected files");
                    return false;
                }
            }
            catch (Exception ex)
            {
                AddError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get the name of the sheet name
        /// </summary>
        /// <returns>True/False</returns>
        private bool GetExcelSheet()
        {
            try
            {                                                               
                List<string> SheetNameList = pricingReadExcel.GetSheetsName(PricingExcelStream);                
                if (SheetNameList.Count > 0)
                {
                    PricingSheetName = SheetNameList[0];                    
                    return true;
                }
                else
                {
                    AddError("No sheets in the Excel File");
                    return false;
                }                
                
            }
            catch (Exception ex)
            {
                AddError(ex.Message);
                return false;
            }
        }               


        private void AnalizaDircetory(string DirectoryPath)
        {
            DirectoryInfo SCNTmpDirectory = new DirectoryInfo(DirectoryPath);
            if (!SCNTmpDirectory.Exists)
            {
                SCNTmpDirectory.Create();
            }              
        }

        private void InsertItemsInList(string URLWeb, DataTable dt)
        {            
            SPSite site =new SPSite(URLWeb);
            SPWeb web = site.OpenWeb();
            web.AllowUnsafeUpdates = true;
            SPList TramitesStoreList = web.Lists[""];//Constants.Lists.SCNStoreTramites];

            for (int r = 0; r < dt.Rows.Count; r++)
            {
                SPListItem TramiteItem = TramitesStoreList.Items.Add();
                TramiteItem["Title"] = dt.Rows[r][2];
                TramiteItem.Update();
            }
            TramitesStoreList.Update();
            web.AllowUnsafeUpdates = false;
        }

        //Validations

        private bool ValidateDualBranch(List<PricingIndexExcel> ListBranches)
        {
            bool result = true;
            List<string> tmpindex = new List<string>();
            foreach (PricingIndexExcel ind in ListBranches)
            {
                if (tmpindex.IndexOf(ind.Index) == -1)
                {
                    tmpindex.Add(ind.Index);
                    List<PricingIndexExcel> SameBranch = ListBranches.Where(l => l.Index == ind.Index).ToList();
                    if (SameBranch.Count > 1)
                    {
                        string positions = string.Empty;
                        foreach (PricingIndexExcel exc in SameBranch)
                        {
                            if (string.IsNullOrEmpty(positions))
                            {
                                positions = string.Format("{0}{1}", pricingConfig.ColumnBranch, exc.Position.ToString());
                            }
                            else
                            {
                                positions += positions = string.Format(" ,{0}{1}", pricingConfig.ColumnBranch, exc.Position.ToString());
                            }
                        }
                        AddError(string.Format("The Branch {0} is {1} times in the file - {2}", ind.Index, SameBranch.Count.ToString(), positions));
                        result = false;
                    }
                }
            }
            return result;
        }

        private void ValidateBranch(int rownumber, ref PricingExcelObj PEO)
        {            
            string CellLocation = string.Format("{0}{1}",pricingConfig.ColumnBranch, rownumber.ToString());
            string BranchValue = pricingReadExcel.GetIndividualCellValue(PricingExcelStream, PricingSheetName, CellLocation);
            if (!string.IsNullOrEmpty(BranchValue))
            {
                int converter;
                if (!int.TryParse(BranchValue, out converter))
                {                    
                    AddError(string.Format("The cell {0} is not valid", CellLocation));
                }
                else
                {
                    PEO.BranchNumber = converter.ToString();
                }
            }
            else
            {                
                AddError(string.Format("The cell {0} is empty", CellLocation));
            }
        }

        private void ValidateAddINIFee(int rownumber, ref PricingExcelObj PEO)
        {
            string CellLocation = string.Format("{0}{1}", pricingConfig.ColumnAddINIFee, rownumber.ToString());
            string AddINIFeeValue = pricingReadExcel.GetIndividualCellValue(PricingExcelStream, PricingSheetName, CellLocation);
            if (!string.IsNullOrEmpty(AddINIFeeValue))
            {
                int converter;
                if (!int.TryParse(AddINIFeeValue, out converter) || int.Parse(AddINIFeeValue) < 0)
                {                    
                    AddError(string.Format("The cell {0} is not valid", CellLocation));
                }
                else
                {
                    PEO.AddINIFee = AddINIFeeValue.ToString();
                }
            }
            else
            {                               
                AddError(string.Format("The cell {0} is empty", CellLocation));
            }
        }

        private void ValidateRNLMult(int rownumber, ref PricingExcelObj PEO)
        {
            string CellLocation = string.Format("{0}{1}", pricingConfig.ColumnRNLMult, rownumber.ToString());
            string RNLMultValue = pricingReadExcel.GetIndividualCellValue(PricingExcelStream, PricingSheetName, CellLocation);
            if (!string.IsNullOrEmpty(RNLMultValue))
            {
                int converter;
                if (!int.TryParse(RNLMultValue, out converter) || int.Parse(RNLMultValue) < 0)
                {                    
                    AddError(string.Format("The cell {0} is not valid", CellLocation));
                }
                else
                {
                    PEO.RLNMult = RNLMultValue.ToString();
                }
            }
            else
            {                
                AddError(string.Format("The cell {0} is empty", CellLocation));
            }
        }

        private void ValidateApps(int rownumber, ref PricingExcelObj PEO)
        {
            string CellLocation = string.Format("{0}{1}", pricingConfig.ColumnNofApps, rownumber.ToString());
            string AppsValue = pricingReadExcel.GetIndividualCellValue(PricingExcelStream, PricingSheetName, CellLocation);
            if (!string.IsNullOrEmpty(AppsValue))
            {
                double converter;
                if (!double.TryParse(AppsValue, out converter) || double.Parse(AppsValue) < 1)
                {                  
                    AddError(string.Format("The cell {0} is not valid", CellLocation));
                }
                else
                {
                    PEO.NumberAppas = AppsValue.ToString();
                }
            }
            else
            {               
                AddError(string.Format("The cell {0} is empty", CellLocation));
            }
        }

        private void ValidateExcessIncrement(int rownumber, ref PricingExcelObj PEO)
        {
            string CellLocation = string.Format("{0}{1}", pricingConfig.ColumnExcessIncrement, rownumber.ToString());
            string ExcessIncrementValue = pricingReadExcel.GetIndividualCellValue(PricingExcelStream, PricingSheetName, CellLocation);
            if (!string.IsNullOrEmpty(ExcessIncrementValue))
            {
                int converter;
                if (!int.TryParse(ExcessIncrementValue, out converter))
                {
                    AddError(string.Format("The cell {0} is not valid", CellLocation));
                }
                else
                {
                    PEO.ExcessIncrement = converter;
                }
            }
            else
            {
                PEO.ExcessIncrement = 0;
            }
        }

        private void ValidateExcessValuePerIncrement(int rownumber, ref PricingExcelObj PEO)
        {
            string CellLocation = string.Format("{0}{1}", pricingConfig.ColumnExcessValuePerIncrement, rownumber.ToString());
            string ExcessValuePerIncrementValue = pricingReadExcel.GetIndividualCellValue(PricingExcelStream, PricingSheetName, CellLocation);
            if (!string.IsNullOrEmpty(ExcessValuePerIncrementValue))
            {
                decimal converter;
                if (!decimal.TryParse(ExcessValuePerIncrementValue, out converter))
                {
                    AddError(string.Format("The cell {0} is not valid", CellLocation));
                }
                else
                {
                    PEO.ExcessValuePerIncrement = converter;
                }
            }
            else
            {
                PEO.ExcessValuePerIncrement = 0;
            }
        }

        private void ValidatePrices(int rownumber, ref PricingExcelObj PEO)
        {
            string tmpPrice = "-1";
            //Normal
            foreach (SQFT h in PricingFinalHeaders)
            {
                string CellLocation = string.Format("{0}{1}", h.Column, rownumber.ToString());
                string PriceValue = pricingReadExcel.GetIndividualCellValue(PricingExcelStream, PricingSheetName, CellLocation);
                
                if (!string.IsNullOrEmpty(PriceValue))
                {
                    double converter;
                    if (double.TryParse(PriceValue, out converter))
                    {
                        if (converter > 0)
                        {
                            double tmpconverter;
                            if (double.TryParse(tmpPrice, out tmpconverter))
                            {
                                if (converter < tmpconverter)
                                {                                    
                                    AddError(string.Format("The cell {0} must not less that the before value ", CellLocation));
                                }
                                else
                                {
                                    h.Value = converter.ToString();
                                    SQFT hh = h;
                                    PEO.Prices.Add(hh);
                                }
                            }
                        }
                        else
                        {                            
                            AddError(string.Format("The cell {0} is not allowed $0.00 or less", CellLocation));
                        }
                        tmpPrice = PriceValue;
                    }
                    else
                    {
                        tmpPrice = "0";                        
                        AddError(string.Format("The cell {0} is not valid", CellLocation));
                    }
                }
                else
                {
                    tmpPrice = "0";                    
                    AddError(string.Format("The cell {0} is empty", CellLocation));
                }
            }

            //Extra
            foreach (SQFT h in PricingExtraHeaders)
            {
                string CellLocation = string.Format("{0}{1}", h.Column, rownumber.ToString());
                string PriceValue = pricingReadExcel.GetIndividualCellValue(PricingExcelStream, PricingSheetName, CellLocation);

                if (!string.IsNullOrEmpty(PriceValue))
                {                                        
                    AddError(string.Format("Invalid data found in column '{0}'", CellLocation));
                }
            }
        }

        //Vlidate and create Header
        private bool ValidateSQHeader()
        {
            bool result = true;
            try
            {
                string startHeader = pricingConfig.ColumnHeaders;
                string HeaderRow = pricingConfig.PricingHeader;
                int spacecounter = 0;
                char tmpNextLeter = startHeader.ToCharArray()[0];

                string firstHeader = pricingReadExcel.GetIndividualCellValue(PricingExcelStream, PricingSheetName, string.Format("{0}{1}", startHeader, HeaderRow));

                int isnumber = 0;
                string sufixletter = string.Empty;
                int tmpSqFtFrom = 0;

                int temporalnumber = 0;

                if (!string.IsNullOrEmpty(firstHeader) && int.TryParse(firstHeader, out isnumber))
                {
                    temporalnumber = isnumber;
                    PricingFinalHeaders.Add(new SQFT
                    {
                        Column = tmpNextLeter.ToString(),
                        SqFtFrom = tmpSqFtFrom,
                        SqTfTo = isnumber
                    });
                    tmpSqFtFrom = isnumber + 1;

                    do
                    {
                        if (tmpNextLeter == 'Z')
                        {
                            if (string.IsNullOrEmpty(sufixletter))
                            {
                                sufixletter = "A";
                            }
                            else
                            {
                                char tmpsufixletter = (char)(((int)sufixletter.ToCharArray()[0]) + 1);
                                sufixletter = tmpsufixletter.ToString();
                            }

                            char nextLetter = 'A';
                            tmpNextLeter = nextLetter;
                            string addressCell = string.Format("{0}{1}{2}", sufixletter, nextLetter.ToString(), HeaderRow);
                            string HeaderVal = pricingReadExcel.GetIndividualCellValue(PricingExcelStream, PricingSheetName, addressCell);
                            if (!string.IsNullOrEmpty(HeaderVal))
                            {
                                //if(isnumber >
                                if (int.TryParse(HeaderVal, out isnumber))
                                {
                                    if (isnumber <= tmpSqFtFrom)
                                    {
                                        result = false;
                                        AddError(string.Format("The cell {0} is not increasing", addressCell));
                                    }
                                    PricingFinalHeaders.Add(new SQFT
                                    {
                                        Column = sufixletter + nextLetter.ToString(),
                                        SqFtFrom = tmpSqFtFrom,
                                        SqTfTo = isnumber
                                    });
                                    tmpSqFtFrom = isnumber + 1;
                                }
                                else
                                {
                                    result = false;
                                    AddError(string.Format("The cell {0} must be a number", addressCell));
                                    PricingFinalHeaders.Add(new SQFT
                                    {
                                        Column = sufixletter + nextLetter.ToString(),
                                        SqFtFrom = tmpSqFtFrom,
                                        SqTfTo = 0
                                    });
                                    tmpSqFtFrom = 0;
                                }
                            }
                            else
                            {
                                PricingExtraHeaders.Add(new SQFT
                                {
                                    Column = sufixletter + nextLetter.ToString(),
                                    SqFtFrom = 0,
                                    SqTfTo = 0
                                }); 
                                spacecounter++;
                            }
                        }
                        else
                        {
                            char nextLetter = (char)(((int)tmpNextLeter) + 1);
                            tmpNextLeter = nextLetter;
                            string addressCell = string.Format("{0}{1}{2}", sufixletter, nextLetter.ToString(), HeaderRow);
                            string HeaderVal = pricingReadExcel.GetIndividualCellValue(PricingExcelStream, PricingSheetName, addressCell);
                            if (!string.IsNullOrEmpty(HeaderVal))
                            {
                                if (int.TryParse(HeaderVal, out isnumber))
                                {
                                    if (isnumber <= tmpSqFtFrom)
                                    {
                                        result = false;
                                        AddError(string.Format("The cell {0} is not increasing", addressCell));
                                    }
                                    PricingFinalHeaders.Add(new SQFT
                                    {
                                        Column = sufixletter + nextLetter.ToString(),
                                        SqFtFrom = tmpSqFtFrom,
                                        SqTfTo = isnumber
                                    });
                                    tmpSqFtFrom = isnumber + 1;
                                }
                                else
                                {
                                    result = false;
                                    AddError(string.Format("The cell {0} must be a number", addressCell));
                                    PricingFinalHeaders.Add(new SQFT
                                    {
                                        Column = sufixletter + nextLetter.ToString(),
                                        SqFtFrom = tmpSqFtFrom,
                                        SqTfTo = 0
                                    });
                                    tmpSqFtFrom = 0;
                                }
                            }
                            else
                            {
                                PricingExtraHeaders.Add(new SQFT
                                {
                                    Column = sufixletter + nextLetter.ToString(),
                                    SqFtFrom = 0,
                                    SqTfTo = 0
                                }); 
                                spacecounter++;
                            }
                        }
                    } while (spacecounter <= 0);

                    //Extra
                    for (int e = 0; e < 4; e++)
                    {
                        if (tmpNextLeter == 'Z')
                        {
                            if (string.IsNullOrEmpty(sufixletter))
                            {
                                sufixletter = "A";
                            }
                            else
                            {
                                char tmpsufixletter = (char)(((int)sufixletter.ToCharArray()[0]) + 1);
                                sufixletter = tmpsufixletter.ToString();
                            }
                            char nextLetter = 'A';
                            tmpNextLeter = nextLetter;
                            string addressCell = string.Format("{0}{1}{2}", sufixletter, nextLetter.ToString(), HeaderRow);                                                        
                            PricingExtraHeaders.Add(new SQFT
                            {
                                Column = sufixletter + nextLetter.ToString(),
                                SqFtFrom = 0,
                                SqTfTo = 0
                            });                            
                        }
                        else
                        {
                            char nextLetter = (char)(((int)tmpNextLeter) + 1);
                            tmpNextLeter = nextLetter;
                            string addressCell = string.Format("{0}{1}{2}", sufixletter, nextLetter.ToString(), HeaderRow);
                            PricingExtraHeaders.Add(new SQFT
                            {
                                Column = sufixletter + nextLetter.ToString(),
                                SqFtFrom = 0,
                                SqTfTo = 0
                            });
                        }
                    }
                    //result = true;
                }
                else
                {
                    AddError("The first price header is null or not valid");
                    result = false;
                }
            }
            catch (Exception ex)
            {
                AddError(ex.Message);
                result = false;
            }
            return result;
        }

        private void AddError(string error)
        {
            ErrorCounter++;
            PricingLogErrors.Add(string.Format("<li>{0}</li>", error));
        }

    }
}
