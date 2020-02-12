
using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Checkers{       
    public partial class Odoo : Postgres{  
        public new Connectors.Odoo Connector {get; private set;}
        public int CompanyID  {
            get{
                return this.Connector.CompanyID;
            }

            set{
                this.Connector.CompanyID = value;
            }
        }
        public string CompanyName  {
            get{
                return this.Connector.CompanyName;
            }

            set{
                this.Connector.CompanyName = value;
            }
        }
        
        public Odoo(string companyName, string host, string database, string username, string password, Core.Output output = null): base(host, database, username, password, output){         
            this.Connector = new Connectors.Odoo(companyName, host, database, username, password);            
        }
        public Odoo(int companyID, string host, string database, string username, string password, Core.Output output = null): base(host, database, username, password, output){         
            this.Connector = new Connectors.Odoo(companyID, host, database, username, password);            
        }

        public List<string> CheckIfCompanyMatchesData(int companyID, Dictionary<string, object> expectedFields){    
            List<string> errors = new List<string>();                        

            if(Output != null)  Output.Write(string.Format("Getting the company data for ~ID={0}... ", companyID), ConsoleColor.Yellow);            

            Output.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(this.Connector.GetCompanyData(companyID), expectedFields));
            Output.UndoStatus();

            return errors;
        }  
        public List<string> CheckIfProviderMatchesData(int providerID, Dictionary<string, object> expectedFields){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the provider data for ~ID={0}... ", providerID), ConsoleColor.Yellow);            
            
            Output.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(this.Connector.GetProviderData(providerID), expectedFields));
            Output.UndoStatus();

            return errors;
        }
        public List<string> CheckIfProductMatchesData(int templateID, Dictionary<string, object> expectedFields){    
            return CheckIfProductMatchesData(templateID, expectedFields, null);
        }
        public List<string> CheckIfProductMatchesData(int templateID, Dictionary<string, object> expectedFields, string[] expectedAttributeValues){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the product data for ~ID={0}... ", templateID), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetProductTemplateData(templateID);                        
            errors.AddRange(this.CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckItemValues(dt, "variant", "value", expectedAttributeValues));

            Output.UndoStatus();
            return errors;
        } 
        public List<string> CheckIfPurchaseMatchesData(int purchaseID, Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty = null){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the purchase data for ~ID={0}... ", purchaseID), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetPurchaseData(purchaseID);                        
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.UndoStatus();
            
            return errors;
        } 
        public List<string> CheckIfStockMovementMatchesData(string orderCode, bool isReturn, Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty = null){
            List<string> errors = new List<string>();
            
            if(Output != null) Output.Write(string.Format("Getting the stock movement data for the order ~{0}... ", orderCode), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetStockMovementData(orderCode, isReturn);
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.UndoStatus();                         
            return errors;                             
        }
        public List<string> CheckIfScrappedStockMatchesData(Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty = null){
            List<string> errors = new List<string>();
            
            if(Output != null) Output.Write("Getting the scrapped stock data... ");                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetScrappedStockData();
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.UndoStatus();                         
            return errors;                             
        }
        public List<string> CheckIfInvoiceMatchesData(string orderCode, Dictionary<string, object> expectedFields){
            List<string> errors = new List<string>();
            
            if(Output != null) Output.Write(string.Format("Getting the invoice data for the order ~{0}... ", orderCode), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetInvoiceData(orderCode);
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));           

            Output.UndoStatus();                                      
            return errors;          
        } 
        public List<string> CheckIfPosSaleMatchesData(int posSaleID, Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty = null){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the POS sale data for ~ID={0}... ", posSaleID), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetPosSaleData(posSaleID);                        
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.UndoStatus();
            
            return errors;
        } 
        public List<string> CheckIfSaleMatchesData(int saleID, Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty = null){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the backoffice sale data for ~ID={0}... ", saleID), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetSaleData(saleID);                        
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.UndoStatus();
            
            return errors;
        }
        public List<string> CheckIfUserMatchesData(int userID, Dictionary<string, object> expectedFields, string[] expectedGroups = null){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the user data for ~ID={0}... ", userID), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetUserData(userID);                        
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckItemValues(dt, "group", "group", expectedGroups));

            Output.UndoStatus();
            
            return errors;
        }
        private List<string> CheckItemValues(DataTable dt, string caption, string field, string[] values){
            List<string> errors = new List<string>();
            Dictionary<string, bool> found = values.ToDictionary(x => x, x => false);
            if(values != null){                
                foreach(DataRow dr in dt.Rows){
                    string key = dr[field].ToString().Trim();
                    if(values.Contains(key)) found[key] = true;
                    else errors.Add(String.Format("Unexpected {0} '{1} {2}' found.", caption, dr["value"].ToString(), dr["attribute"]));
                }

                foreach(string key in found.Keys){
                    if(!found[key]) errors.Add(String.Format("Unable to find the {0} '{1}'.", caption, key));
                }
            }

            return errors;
        }
        private List<string> CheckAttributeQuantities(DataTable dt, Dictionary<string, int> attributeQty){
            List<string> errors = new List<string>();
            Dictionary<string, bool> found = attributeQty.Keys.ToDictionary(x => x, x => false);

            if(attributeQty != null){                
                foreach(DataRow dr in dt.Rows){
                    string variant = dr["product_name"].ToString();
                    if(variant.Contains("(")){
                        variant = variant.Substring(variant.IndexOf("(")+1);
                        variant = variant.Substring(0, variant.IndexOf(")")).Replace(" ", "");
                    }                    

                    if(!attributeQty.ContainsKey(variant)) errors.Add(String.Format("Unexpected product '{0}' found.", dr["product_name"]));
                    else{
                        if(attributeQty[variant] != (int)(decimal)dr["product_qty"]) errors.Add(String.Format("Unexpected quantity found for the product '{0}': expected->'{1}' found->'{2}'.", dr["product_name"], attributeQty[variant],  dr["product_qty"]));                    
                        found[variant] = true;
                    } 
                }

                foreach(string key in found.Keys){
                    if(!found[key]) errors.Add(String.Format("Unable to find the quantity for the attribute value '{0}'.", key));
                }
            }

            return errors;
        }
        private string GetWhereForName(string name, string dbField){
            string company = name;
            company = company.Replace(this.Student, "").Trim();
            string[] student = this.Student.Split(" ");

            //TODO: check if student.length > 2
            return string.Format("{3} like '{0}%' AND {3} like '%{1}%' AND {3} like '%{2}%'", company, student[0], student[1], dbField);
        }       
    }
}