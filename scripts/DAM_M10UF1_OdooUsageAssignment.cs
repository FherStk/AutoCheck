/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using AutoCheck.Core;

namespace AutoCheck.Scripts{
    public class DAM_M10UF1_OdooUsageAssignment: Core.ScriptDB<CopyDetectors.None>{                       
        public DAM_M10UF1_OdooUsageAssignment(string[] args): base(args){        
        }                

        public override void Run(){
            base.Run();            
            
            Output.Instance.Indent();      
            int companyID = 1;       
            var odoo = new Checkers.Odoo(companyID, this.Host, this.DataBase, this.Username, this.Password);
                        
            OpenQuestion("Question 1", "Company data", 1);                                     
                string companyName = string.Format("Samarretes Frikis {0}", this.Student); 
                EvalQuestion(odoo.CheckIfCompanyMatchesData(companyID, new Dictionary<string, object>(){
                    {"name", companyName}, 
                    {"logo", true}
                }));
            CloseQuestion();   
            
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
                    new Dictionary<string, string[]>(){{"Talla", new string[]{"S", "M", "L", "XL"}}}
                ));
            CloseQuestion();   
            
            OpenQuestion("Question 4", "Purchase order data", 1);                                         
                int purchaseID = odoo.Connector.GetLastPurchaseID();
                var purchaseQty = new Dictionary<string[], Dictionary<string, object>>(){
                    {new string[]{"S"}, new Dictionary<string, object>{{"product_qty", 15}}},
                    {new string[]{"M"}, new Dictionary<string, object>{{"product_qty", 30}}},
                    {new string[]{"L"}, new Dictionary<string, object>{{"product_qty", 50}}},
                    {new string[]{"XL"}, new Dictionary<string, object>{{"product_qty", 25}}}
                };

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
                var posQty = new Dictionary<string[], Dictionary<string, object>>(){
                    {new string[]{"L"}, new Dictionary<string, object>{{"product_qty", 1}}},
                };
                EvalQuestion(odoo.CheckIfPosSaleMatchesData(posSaleID, new Dictionary<string, object>(){
                    {"state", "done"}},
                    posQty
                ));
            CloseQuestion();       

            OpenQuestion("Question 8", "Backoffice sale data", 1);    
                int saleID = odoo.Connector.GetLastSaleID();
                var saleQty = new Dictionary<string[], Dictionary<string, object>>(){
                    {new string[]{"S"}, new Dictionary<string, object>{{"product_qty", 10}}},
                    {new string[]{"M"}, new Dictionary<string, object>{{"product_qty", 10}}},
                    {new string[]{"L"}, new Dictionary<string, object>{{"product_qty", 10}}},
                    {new string[]{"XL"}, new Dictionary<string, object>{{"product_qty", 10}}}
                };
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
                var stockQty = new Dictionary<string[], Dictionary<string, object>>(){
                    {new string[]{"S"}, new Dictionary<string, object>{{"product_qty", 5}}},
                    {new string[]{"M"}, new Dictionary<string, object>{{"product_qty", 5}}},
                    {new string[]{"L"}, new Dictionary<string, object>{{"product_qty", 5}}},
                    {new string[]{"XL"}, new Dictionary<string, object>{{"product_qty", 5}}}
                };                                      
                EvalQuestion(odoo.CheckIfStockMovementMatchesData(saleCode, true, new Dictionary<string, object>(){
                    {"state", "done"}}, 
                    stockQty
                ));
            CloseQuestion(); 

            OpenQuestion("Question 12", "Refund invoice data", 1);      
                string saleInvoiceCode = odoo.Connector.GetInvoiceCode(saleCode);                                   
                EvalQuestion(odoo.CheckIfInvoiceMatchesData(saleInvoiceCode, new Dictionary<string, object>(){
                    {"state", "paid"}
                }));
            CloseQuestion(); 

            OpenQuestion("Question 13", "Scrapped stock data", 1);      
                var scrappedQty = new Dictionary<string[], Dictionary<string, object>>(){
                    {new string[]{"XL"}, new Dictionary<string, object>{{"product_qty", 1}}}
                }; 
                EvalQuestion(odoo.CheckIfScrappedStockMatchesData(new Dictionary<string, object>(){
                    {"state", "done"}}, 
                    scrappedQty
                ));
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

            PrintScore();
            Output.Instance.UnIndent();
        }
    }
}