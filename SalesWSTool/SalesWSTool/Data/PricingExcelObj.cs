using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesWSTool.Data
{
    public class PricingExcelObj
    {
        public string BranchNumber { get; set; }
        public string AddINIFee { get; set; }
        public string RLNMult { get; set;}
        public string NumberAppas { get; set; }
        public int ExcessIncrement { get; set; }
        public decimal ExcessValuePerIncrement { get; set; }
        public List<SQFT> Prices { get; set; }
    }
}
