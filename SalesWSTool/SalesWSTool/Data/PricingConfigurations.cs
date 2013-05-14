using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace SalesWSTool.Data
{
    public class PricingConfigurations
    {
        public string ColumnBranch { set; get; }
        public string ColumnBranchName { set; get; }
        public string ColumnRegion { set; get; }
        public string ColumnDivision { set; get; }
        public string ColumnAddINIFee { set; get; }
        public string ColumnRNLMult { set; get; }
        public string ColumnExcessIncrement { set; get; }
        public string ColumnExcessValuePerIncrement { set; get; }
        public string ColumnNofApps { set; get; }
        public string ColumnHeaders { set; get; }

        public string PricingIndexEXCEL { get; set; }
        public string PricingHeader { get; set; }

        public string URLWebService { get; set; }

        public bool IsSuccess { get; set;}
        public string Message { get; set; }

        public PricingConfigurations(Guid site, Guid web)
        {
            GetDataConfiguration(site, web);
        }

        private void GetDataConfiguration(Guid gsite, Guid gweb)
        {
            this.IsSuccess = true;
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(gsite))
                    {
                        using (SPWeb web = site.OpenWeb(gweb))
                        {
                            //SPList ConfigurationList = web.Lists[Constants.Lists.Configuration];
                            if (web.Lists.TryGetList(Constants.Lists.Configuration) != null)
                            {

                                SPList ListConfiguration = web.Lists[Constants.Lists.Configuration];
                                SPListItemCollection ItemConfigurations = ListConfiguration.Items;
                                Dictionary<string, string> configurationKeys = new Dictionary<string, string>();

                                foreach (SPListItem configurationItem in ItemConfigurations)
                                {
                                    if (configurationItem.Fields.ContainsField(Constants.ConfigurationList.Key) &&
                                        configurationItem[Constants.ConfigurationList.Key] != null &&
                                        configurationItem.Fields.ContainsField(Constants.ConfigurationList.Value) &&
                                        configurationItem[Constants.ConfigurationList.Value] != null)
                                    {
                                        configurationKeys.Add(configurationItem[Constants.ConfigurationList.Key].ToString(),
                                                              configurationItem[Constants.ConfigurationList.Value].ToString()
                                                              );
                                    }
                                    else
                                    {
                                        this.Message = string.Format("There are some values empty in the configuration list");
                                        this.IsSuccess = false;
                                    }
                                }

                                this.ColumnBranch = configurationKeys[Constants.Fields.Configurations.Branch];
                                this.ColumnBranchName = configurationKeys[Constants.Fields.Configurations.BranchName];
                                this.ColumnRegion = configurationKeys[Constants.Fields.Configurations.Region];
                                this.ColumnDivision = configurationKeys[Constants.Fields.Configurations.Division];
                                this.ColumnAddINIFee = configurationKeys[Constants.Fields.Configurations.AddINIFee];
                                this.ColumnRNLMult = configurationKeys[Constants.Fields.Configurations.RNLMult];
                                this.ColumnExcessIncrement = configurationKeys[Constants.Fields.Configurations.ExcessIncrement];
                                this.ColumnExcessValuePerIncrement = configurationKeys[Constants.Fields.Configurations.ExcessValuePerIncrement];
                                this.ColumnNofApps = configurationKeys[Constants.Fields.Configurations.NofApps];
                                this.ColumnHeaders = configurationKeys[Constants.Fields.Configurations.ColumnHeaders];
                                this.PricingIndexEXCEL = configurationKeys[Constants.Fields.Configurations.IndexEXCEL];
                                this.PricingHeader = configurationKeys[Constants.Fields.Configurations.PricingHeader];
                                this.URLWebService = configurationKeys[Constants.Fields.Configurations.URLWebService];

                                //this.ColumnBranch = "A";
                                //this.ColumnBranchName = "B";
                                //this.ColumnRegion = "C";
                                //this.ColumnDivision = "D";
                                //this.ColumnAddINIFee = "E";
                                //this.ColumnRNLMult = "F";
                                //this.ColumnExcessIncrement = "G";
                                //this.ColumnExcessValuePerIncrement = "H";
                                //this.ColumnNofApps = "I";
                                //this.ColumnHeaders = "J";
                                //this.PricingIndexEXCEL = "A2";
                                //this.PricingHeader = "2";

                                //this.IsSuccess = true;
                            }
                            else
                            {
                                this.IsSuccess = false;
                                this.Message = "Ths Configuration List does not exist";
                            }
                        }
                    }
                });
            }
            catch(Exception ex)
            {
                this.IsSuccess = false;
                this.Message = ex.Message;
            }
        }

    }
}
