using System.Collections.Generic;
using AutoCheck.Core;

namespace AutoCheck.Scripts{
    public class DAM_M10UF2_OdooCsvAssignment: Core.ScriptDB<CopyDetectors.None>{                       
        public DAM_M10UF2_OdooCsvAssignment(string[] args): base(args){        
        }

        protected override void DefaultArguments(){
            //This assignement has only 5 points
            this.MaxScore = 5f;
        }                

        public override void Run(){
            base.Run();            
            
            Output.Instance.Indent();      
                        
            OpenQuestion("Question 1", "CSV data");                                                     
                OpenQuestion("Question 1.1", "The file has been created", 0.5f);             
                    Output.Instance.Write("Checking the CSV file... ");
                    var csv = new Checkers.Csv(this.Path, "*.csv"); //Exception if wont parse                    
                    EvalQuestion();
                CloseQuestion();   

                OpenQuestion("Question 1.2", "The file has been modified", 1);             
                    EvalQuestion(csv.CheckIfRegistriesMatchesAmount(1));
                CloseQuestion();  

                OpenQuestion("Question 1.3", "The file has the correct data", 1);             
                    EvalQuestion(csv.CheckIfRegistriesMatchesData(1, new Dictionary<string, object>(){
                    {"name", this.Student}, 
                    {"email", "@elpuig.xeill.net"}, 
                    {"active", true}, 
                    {"customer", false}, 
                    {"supplier", true}, 
                    {"employee", false}, 
                    {"is_company", true}
                }));
                CloseQuestion();   
            CloseQuestion();   

            OpenQuestion("Question 2", "Odoo's database data");
                Checkers.Odoo odoo = new Checkers.Odoo(1, this.Host, this.DataBase, this.Username, this.Password);
                OpenQuestion("Question 2.1", "Some data loaded", 1.5f);
                    EvalQuestion(csv.CheckIfRegistriesMatchesAmount(1));
                CloseQuestion();   

                OpenQuestion("Question 1.2", "The file has been modified", 1);             
                    EvalQuestion(csv.CheckIfRegistriesMatchesAmount(1));
                CloseQuestion();  

                OpenQuestion("Question 1.3", "The file has the correct data", 1);             
                    EvalQuestion(csv.CheckIfRegistriesMatchesData(1, new Dictionary<string, object>(){
                    {"name", this.Student}, 
                    {"email", "@elpuig.xeill.net"}, 
                    {"active", true}, 
                    {"customer", false}, 
                    {"supplier", true}, 
                    {"employee", false}, 
                    {"is_company", true}
                }));
                CloseQuestion();   
            CloseQuestion();  
            

            /*
            int companyID = 1;       
            Checkers.Odoo odoo = new Checkers.Odoo(companyID, this.Host, this.DataBase, "postgres", "postgres");
            OpenQuestion("Question 2", "Provider data", 1);                                                 
                string providerName = string.Format("Bueno Bonito y Barato {0}", this.Student); 
                int providerID = odoo.Connector.GetProviderID(providerName);
                EvalQuestion(odoo.CheckIfProviderMatchesData(providerID, new Dictionary<string, object>(){
                    {"name", providerName}, 
                    {"is_company", true}, 
                    {"logo", true}
                }));
            CloseQuestion();  
            
            OpenQuestion("Question 3", "Product data", 1);                                                 
                string productName = string.Format("Samarreta Friki {0}", this.Student);
                int templateID = odoo.Connector.GetProductTemplateID(productName);
                EvalQuestion(odoo.CheckIfProductMatchesData(templateID, new Dictionary<string, object>(){
                    {"name", productName}, 
                    {"type", "product"}, 
                    {"attribute", "Talla"}, 
                    {"supplier_id", providerID}, 
                    {"purchase_price", 9.99m}, 
                    {"sell_price", 19.99m}},                     
                    new string[]{"S", "M", "L", "XL"}
                ));
            CloseQuestion();   
            
            OpenQuestion("Question 4", "Purchase order data", 1);                                         
                int purchaseID = odoo.Connector.GetLastPurchaseID();
                var purchaseQty = new Dictionary<string, int>(){{"S", 15}, {"M", 30}, {"L", 50}, {"XL", 25}};
                EvalQuestion(odoo.CheckIfPurchaseMatchesData(purchaseID, new Dictionary<string, object>(){
                    {"amount_total", 1450.56m}}, 
                    purchaseQty
                ));
            CloseQuestion(); 

            OpenQuestion("Question 5", "Input cargo movement", 1);                                         
                string purchaseCode = odoo.Connector.GetPurchaseCode(purchaseID);
                EvalQuestion(odoo.CheckIfStockMovementMatchesData(purchaseCode, false, new Dictionary<string, object>(){
                    {"state", "done"}},
                    purchaseQty
                ));
            CloseQuestion();  

            OpenQuestion("Question 6", "Purchase invoice data", 1);                                         
                EvalQuestion(odoo.CheckIfInvoiceMatchesData(purchaseCode, new Dictionary<string, object>(){
                    {"state", "paid"}
                }));
            CloseQuestion(); 

            OpenQuestion("Question 7", "Point Of Sale data", 1);    
                int posSaleID = odoo.Connector.GetLastPosSaleID();
                EvalQuestion(odoo.CheckIfPosSaleMatchesData(posSaleID, new Dictionary<string, object>(){
                    {"state", "done"}}, new Dictionary<string, int>(){
                    {"L", 1}
                }));
            CloseQuestion();       

            OpenQuestion("Question 8", "Backoffice sale data", 1);    
                int saleID = odoo.Connector.GetLastSaleID();
                var saleQty = new Dictionary<string, int>(){{"S", 10}, {"M", 10}, {"L", 10}, {"XL", 10}};
                EvalQuestion(odoo.CheckIfSaleMatchesData(saleID, new Dictionary<string, object>(){
                    {"state", "sale"}}, 
                    saleQty
                ));
            CloseQuestion();  

            OpenQuestion("Question 9", "Output cargo movement", 1);                                         
                string saleCode = odoo.Connector.GetSaleCode(saleID);
                EvalQuestion(odoo.CheckIfStockMovementMatchesData(saleCode, false, new Dictionary<string, object>(){
                    {"state", "done"}},
                    saleQty
                ));
            CloseQuestion(); 

            OpenQuestion("Question 10", "Sale invoice data", 1);                                         
                EvalQuestion(odoo.CheckIfInvoiceMatchesData(saleCode, new Dictionary<string, object>(){
                    {"state", "paid"}
                }));
            CloseQuestion(); 

            OpenQuestion("Question 11", "Return cargo movement", 1);                                         
                EvalQuestion(odoo.CheckIfStockMovementMatchesData(saleCode, true, new Dictionary<string, object>(){
                    {"state", "done"}}, new Dictionary<string, int>(){
                    {"S", 5}, 
                    {"M", 5}, 
                    {"L", 5}, 
                    {"XL", 5}
                }));
            CloseQuestion(); 

            OpenQuestion("Question 12", "Refund invoice data", 1);      
                string saleInvoiceCode = odoo.Connector.GetInvoiceCode(saleCode);                                   
                EvalQuestion(odoo.CheckIfInvoiceMatchesData(saleInvoiceCode, new Dictionary<string, object>(){
                    {"state", "paid"}
                }));
            CloseQuestion(); 

            OpenQuestion("Question 13", "Scrapped stock data", 1);      
                EvalQuestion(odoo.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(){
                    {"state", "done"}}, new Dictionary<string, int>(){                   
                    {"XL", 1}
                }));
            CloseQuestion(); 

            OpenQuestion("Question 14", "User data", 1);  
                int userID = odoo.Connector.GetUserID(string.Format("{0}@elpuig.xeill.net", this.Student.ToLower().Replace(" ", "_")));     
                EvalQuestion(odoo.CheckIfUserMatchesData(userID, new Dictionary<string, object>(){
                    {"active", true}}, new string[]{
                    "Technical Features", 
                    "Contact Creation", 
                    "Sales Pricelists", 
                    "Manage Pricelist Items", 
                    "Manage Product Variants", 
                    "Tax display B2B", 
                    "User"
                }));
            CloseQuestion();      
            */
            
            PrintScore();
            Output.Instance.UnIndent();
        }
    }
}