using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.ServiceModel;
using System.Net;
using SalesWSTool.ServiceSalesWS;
using System.Text;
using SalesWSTool.Data;
using Microsoft.SharePoint;

namespace SalesWSTool.Webparts.PricingDataUpload
{
    public partial class PricingDataUploadUserControl : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected override void OnPreRender(EventArgs e)
        {
            GetProducts();
            base.OnPreRender(e);
        }

        private void GetProducts()
        {
            try
            {
                PricingConfigurations pricingConfig = new PricingConfigurations(SPContext.Current.Site.ID, SPContext.Current.Web.ID);
                if (pricingConfig.IsSuccess)
                {
                    BasicHttpBinding binding = new BasicHttpBinding() { MaxReceivedMessageSize = 1048576 }; //1048576 = 1mb
                    binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                    binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                    EndpointAddress endpoint = new EndpointAddress(pricingConfig.URLWebService);//http://localhost:51372/Services/SalesWS.svc

                    ServiceSalesWS.SalesWSClient proxy = new ServiceSalesWS.SalesWSClient(binding, endpoint);

                    //proxy.ClientCredentials.Windows.ClientCredential = CredentialCache.DefaultNetworkCredentials;
                    //proxy.ClientCredentials.Windows.ClientCredential.Domain = this.Domain;
                    //proxy.ClientCredentials.Windows.ClientCredential.UserName = this.UserName;
                    //proxy.ClientCredentials.Windows.ClientCredential.Password = this.Password;

                    proxy.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

                    wsProducts[] Products = proxy.GetProducts();

                    StringBuilder ProductsLi = new StringBuilder();

                    for (int i = 0; i < Products.Length; i++)
                    {
                        ProductsLi.Append(string.Format("<li class='ui-widget-content' data-id='{0}'>{1}</li>", Products[i].ProductID.ToString(), Products[i].Name));
                    }

                    OLProductData.Text = ProductsLi.ToString();
                }
                else
                {
                    OLProductData.Text = "Error: " + pricingConfig.Message;
                }                
            }
            catch (Exception ex)
            {
                OLProductData.Text = "Error: " + ex.Message;
            }
        }

        protected void btn_getproducts_Click(object sender, EventArgs e)
        {
            //GetProducts();
        }
    }
}
