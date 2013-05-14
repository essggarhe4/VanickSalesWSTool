using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SalesWSServices.Data;

namespace SalesWSServices.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "SalesWS" in code, svc and config file together.    
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class SalesWS : ISalesWS
    {        
        public List<wsProducts> GetProducts()
        {
            List<wsProducts> products = new List<wsProducts>();

            SVMSalesWSDB SVM = new SVMSalesWSDB();

            foreach(Product p in SVM.Products)
            {
                products.Add(new wsProducts{
                    ProductID = p.ProductID,
                    Name = p.Name,
                    Description = p.Description
                });
            }

            return products;
        }

        public Product GetProductByID(int ID)
        {
            SVMSalesWSDB SVM = new SVMSalesWSDB();
            return SVM.Products.Single(p => p.ProductID == ID);
        }

        public SalesWSResult AddPricingRecord(List<PricingRecord> newPricingRecords, int productID)
        {
            SalesWSResult result = new SalesWSResult();
            result.CounterUpdated = 0;
            result.IsSuccess = false;
            result.Message = "No initilization";
            try
            {
                if (newPricingRecords.Count > 0)
                {
                    SVMSalesWSDB SVM = new SVMSalesWSDB();                    
                    foreach (PricingRecord PR in newPricingRecords)
                    {
                        PR.ProductID = productID;
                        SVM.PricingRecords.AddObject(PR);
                        result.CounterUpdated++;
                    }
                    SVM.SaveChanges();                    
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = "The values cannot be null or empty";
                }
                result.IsSuccess = true;
                result.Message = string.Empty;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
            }
            return result;
        }

        public SalesWSResult UpdateBranchProduct(string Branch, int ProductID, int newNumVisits)
        {
            SalesWSResult result = new SalesWSResult();
            result.IsSuccess = true;
            try
            {                
                SVMSalesWSDB SVM = new SVMSalesWSDB();
                List<Branch_Product> ListBranchProducts = SVM.Branch_Product.Where(b => b.ProductID == ProductID && b.BranchID == Branch).ToList();
                foreach (Branch_Product BP in ListBranchProducts)
                {
                    BP.NumVisits = newNumVisits;
                    SVM.SaveChanges();
                }
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
            }
            return result;
        }

        public int DeletePricingByProductID(int ProductID)
        {
            SVMSalesWSDB SVM = new SVMSalesWSDB();
            int countrProducts = SVM.PricingRecords.Count(p => p.ProductID == ProductID);
            SVM.PricingRecords.Where(p => p.ProductID == ProductID).ToList().ForEach(SVM.PricingRecords.DeleteObject);
            SVM.SaveChanges();
            return countrProducts;
        }
    }
}
