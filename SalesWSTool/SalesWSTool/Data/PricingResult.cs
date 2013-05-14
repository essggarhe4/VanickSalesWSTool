using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesWSTool.Data
{
    public class PricingResult
    {
        public bool IsSuccess { set; get; }        
        public string PricingAction { set; get; }

        //Validation
        public string NumberOfRows { set; get; }
        public string ExcelTabName { get; set; }
        public string ExcelFileName { get; set; }

        //Insert
        public string NumberofRemoved { get; set; }
        public string NumberofRowsPricingData { get; set; }

        //Error
        public string NumberofErrors { get; set; }
        public List<string> MessageErrors { get; set; }

    }
}
