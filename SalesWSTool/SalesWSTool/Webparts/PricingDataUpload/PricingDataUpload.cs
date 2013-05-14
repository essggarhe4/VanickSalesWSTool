using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace SalesWSTool.Webparts.PricingDataUpload
{
    [ToolboxItemAttribute(false)]
    public class PricingDataUpload : WebPart
    {
        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string _ascxPath = @"~/_CONTROLTEMPLATES/SalesWSTool.Webparts/PricingDataUpload/PricingDataUploadUserControl.ascx";

        protected override void CreateChildControls()
        {

            CssRegistration.Register("/_layouts/SalesWSTool/css/ui-lightness/jquery-ui.min.css");
            CssRegistration.Register("/_layouts/SalesWSTool/css/ui-lightness/jquery.ui.theme.css");

            CssRegistration.Register("/_layouts/SalesWSTool/css/PricingDataUpload.css");

            Page.ClientScript.RegisterStartupScript(GetType(), "JQuery",
                   "<SCRIPT language='javascript' src='/_layouts/SalesWSTool/js/jquery-1.9.1.js'></SCRIPT>", false);

            Page.ClientScript.RegisterStartupScript(GetType(), "JQueryUI",
                   "<SCRIPT language='javascript' src='/_layouts/SalesWSTool/js/jquery-ui.js'></SCRIPT>", false);

            Page.ClientScript.RegisterStartupScript(GetType(), "JQueryUploader",
                   "<SCRIPT language='javascript' src='/_layouts/SalesWSTool/js/jquery.ajax_upload.0.6.js'></SCRIPT>", false);            

            Page.ClientScript.RegisterStartupScript(GetType(), "PricingJS",
                   "<SCRIPT language='javascript' src='/_layouts/SalesWSTool/js/PricingDataUpload.js'></SCRIPT>", false);

            Control control = Page.LoadControl(_ascxPath);
            Controls.Add(control);
        }
    }
}
