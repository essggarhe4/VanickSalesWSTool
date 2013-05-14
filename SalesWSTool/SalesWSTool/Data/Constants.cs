using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SalesWSTool.Data
{
    public class Constants
    {
        public class Lists
        {
            public const string Configuration = "Configurations";
        }

        public class Fields
        {
            public class Configurations
            {
                public const string Branch = "Branch";
                public const string BranchName = "BranchName";
                public const string Region = "Region";
                public const string Division = "Division";
                public const string AddINIFee = "AddINIFee";
                public const string RNLMult = "RNLMult";
                public const string ExcessIncrement = "ExcessIncrement";
                public const string ExcessValuePerIncrement = "ExcessValuePerIncrement";
                public const string NofApps = "NofApps";
                public const string ColumnHeaders = "ColumnHeaders";
                public const string IndexEXCEL = "IndexEXCEL";
                public const string PricingHeader = "PricingHeader";
                public const string URLWebService = "URLWebService";
            }
        }

        public class ConfigurationList
        {
            public const string Key = "Title";
            public const string Value = "Column Value";
        }
    }
}
