using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using SalesWSTool.Data;

namespace SalesWSTool.Features.Lists
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("b9bc768f-653f-418d-87eb-f566ff2af4e0")]
    public class ListsEventReceiver : SPFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {

            // Get a reference to the site we activated the feature on
            SPWeb thisWeb = (SPWeb)properties.Feature.Parent;

            // Create a new list and get a reference to it
            Guid thisListId = thisWeb.Lists.Add(Constants.Lists.Configuration, "List to store the configuration Values", SPListTemplateType.GenericList);
            SPList thisList = thisWeb.Lists[thisListId];

            // Add some custom fields to the list 
            thisList.Fields.Add(Constants.ConfigurationList.Value, SPFieldType.Text, true);
            thisList.Update();

            // Adjust the default view to show our custom fields 
            SPView thisView = thisList.DefaultView;
            //thisView.ViewFields.DeleteAll(); 
            thisView.ViewFields.Add(Constants.ConfigurationList.Value);
            thisView.Update();

            // Add a few sample list items 
            SPListItem thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.Branch;
            thisListItem[Constants.ConfigurationList.Value] = "A";
            thisListItem.Update();

            thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.BranchName;
            thisListItem[Constants.ConfigurationList.Value] = "B";
            thisListItem.Update();

            thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.Region;
            thisListItem[Constants.ConfigurationList.Value] = "C";
            thisListItem.Update();

            thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.Division;
            thisListItem[Constants.ConfigurationList.Value] = "D";
            thisListItem.Update();

            thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.AddINIFee;
            thisListItem[Constants.ConfigurationList.Value] = "E";
            thisListItem.Update();

            thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.RNLMult;
            thisListItem[Constants.ConfigurationList.Value] = "F";
            thisListItem.Update();

            thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.ExcessIncrement;
            thisListItem[Constants.ConfigurationList.Value] = "G";
            thisListItem.Update();

            thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.ExcessValuePerIncrement;
            thisListItem[Constants.ConfigurationList.Value] = "H";
            thisListItem.Update();

            thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.NofApps;
            thisListItem[Constants.ConfigurationList.Value] = "I";
            thisListItem.Update();

            thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.ColumnHeaders;
            thisListItem[Constants.ConfigurationList.Value] = "J";
            thisListItem.Update();

            thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.IndexEXCEL;
            thisListItem[Constants.ConfigurationList.Value] = "A2";
            thisListItem.Update();

            thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.PricingHeader;
            thisListItem[Constants.ConfigurationList.Value] = "2";
            thisListItem.Update();

            thisListItem = thisList.Items.Add();
            thisListItem[Constants.ConfigurationList.Key] = Constants.Fields.Configurations.URLWebService;
            thisListItem[Constants.ConfigurationList.Value] = "http://localhost:51372/Services/SalesWS.svc";
            thisListItem.Update();
        }


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            // Get a reference to the site we activated the feature on 
            SPWeb thisWeb = (SPWeb)properties.Feature.Parent;

            // Remove the custom list 
            if (thisWeb.Lists.TryGetList(Constants.Lists.Configuration) != null)
            {
                thisWeb.Lists[Constants.Lists.Configuration].Delete();
                thisWeb.Update();
            }
        }


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}
