using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Web;
using SalesWSServices.Data;

namespace SalesWSServices.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISalesWS" in both code and config file together.
    [ServiceContract]
    public interface ISalesWS
    {       
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json)]
        List<wsProducts> GetProducts();

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json)]
        SalesWSResult AddPricingRecord(List<PricingRecord> newPricingRecords, int productsIDs);

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json)]
        SalesWSResult UpdateBranchProduct(string Branch, int ProductID, int newNumVisits);

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json)]
        Product GetProductByID(int ID);

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json)]
        int DeletePricingByProductID(int ProductID);

    }

    [DataContract]
    public class wsProducts
    {
        [DataMember]
        public int ProductID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }
    }

    [DataContract]
    public class SalesWSResult
    {
        [DataMember]
        public bool IsSuccess { set; get; }
        [DataMember]
        public string Message { set; get; }
        [DataMember]
        public int CounterUpdated { set; get; }
    }
}
