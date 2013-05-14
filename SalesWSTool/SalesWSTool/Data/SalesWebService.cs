using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SalesWSTool.ServiceSalesWS;

namespace SalesWSTool.Data
{
    public class SalesWebService
    {
        BasicHttpBinding binding;
        EndpointAddress endpoint;
        ServiceSalesWS.SalesWSClient proxy;

        public SalesWebService()
        {
            //GetDataConfiguration(site, web);
            binding = new BasicHttpBinding() { MaxReceivedMessageSize = 1048576 }; //1048576 = 1mb
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
            EndpointAddress endpoint = new EndpointAddress("http://localhost:51372/Services/SalesWS.svc");//http://devsp.kimind.pro:81/_layouts/Lisi/Notifications/LisiNotifications.svc");                

            proxy = new ServiceSalesWS.SalesWSClient(binding, endpoint);

            //proxy.ClientCredentials.Windows.ClientCredential = CredentialCache.DefaultNetworkCredentials;
            //proxy.ClientCredentials.Windows.ClientCredential.Domain = this.Domain;
            //proxy.ClientCredentials.Windows.ClientCredential.UserName = this.UserName;
            //proxy.ClientCredentials.Windows.ClientCredential.Password = this.Password;

            proxy.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation;

                      
        }

        public List<wsProducts> GetProducts()
        {
            wsProducts[] Products = proxy.GetProducts();
            return Products.ToList();
        }

        public Product GetProductByID(int ID)
        {
            return proxy.GetProductByID(ID);
        }

        public int DeletePricingDataByProductID(int ProductID)
        {
            return proxy.DeletePricingByProductID(ProductID);
        }

        public SalesWSResult SendPricingRecords(List<PricingRecord> newPrices, int ProductID)
        {
            return proxy.AddPricingRecord(newPrices.ToArray(), ProductID);
        }

        public SalesWSResult UpdateBranchProduct(string BranchID, int ProductID, int newNumVisits)
        {
            return proxy.UpdateBranchProduct(BranchID, ProductID, newNumVisits);
        }
    }
}
