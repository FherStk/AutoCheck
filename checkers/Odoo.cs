
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

        public List<string> CheckIfCompanyMatchesData(Dictionary<string, object> expectedFields, int companyID){    
            List<string> errors = new List<string>();                        

            if(Output != null)  Output.Write(string.Format("Getting the company data for ~ID={0}... ", companyID), ConsoleColor.Yellow);            

            Output.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(expectedFields, this.Connector.GetCompanyData(companyID)));
            Output.UndoStatus();

            return errors;
        }  
        public List<string> CheckIfProviderMatchesData(Dictionary<string, object> expectedFields, int providerID){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the provider data for ~ID={0}... ", providerID), ConsoleColor.Yellow);            
            
            Output.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(expectedFields, this.Connector.GetProviderData(providerID)));
            Output.UndoStatus();

            return errors;
        }
        public List<string> CheckIfProductMatchesData(Dictionary<string, object> expectedFields, int templateID){    
            return CheckIfProductMatchesData(expectedFields, null, templateID);
        }
        public List<string> CheckIfProductMatchesData(Dictionary<string, object> expectedFields, string[] expectedAttributeValues, int templateID){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the product data for ~ID={0}... ", templateID), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetProductTemplateData(templateID);                        
            errors.AddRange(this.CheckIfTableMatchesData(expectedFields, dt));

            Dictionary<string, bool> found = expectedAttributeValues.ToDictionary(x => x, x => false);
            if(expectedAttributeValues != null){
                if(!expectedFields.ContainsKey("attribute")) throw new Exception("The filed 'attribute' must be provided when using the attributeValues parameter.");
                
                foreach(DataRow dr in dt.Rows){
                    string key = dr["value"].ToString().Trim();
                    if(expectedAttributeValues.Contains(key)) found[key] = true;
                    else errors.Add(String.Format("Unexpected variant value '{0}' found for the variant attribute '{1}'.", dr["value"].ToString(), dr["attribute"]));
                }

                foreach(string key in found.Keys){
                    if(!found[key]) errors.Add(String.Format("Unable to find the variant '{0} {1}'.", expectedFields["attribute"], key));
                }
            }

            Output.UndoStatus();
            return errors;
        } 
        public List<string> CheckIfPurchaseMatchesData(Dictionary<string, object> expectedFields, int purchaseID, Dictionary<string, int> attributeQty = null){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the purchase data for ~ID={0}... ", purchaseID), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetPurchaseData(purchaseID);                        
            errors.AddRange(CheckIfTableMatchesData(expectedFields, dt));
            errors.AddRange(CheckAttributeQuantities(dt, attributeQty));

            Output.UndoStatus();
            
            return errors;
        } 
        public List<string> CheckIfStockMovementMatchesData(Dictionary<string, object> expectedFields, string orderCode, bool isReturn, Dictionary<string, int> attributeQty = null){
            List<string> errors = new List<string>();
            
            if(Output != null) Output.Write(string.Format("Getting the stock movement data for the order ~{0}... ", orderCode), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetStockMovementData(orderCode, isReturn);                        
            errors.AddRange(CheckIfTableMatchesData(expectedFields, dt));
            errors.AddRange(CheckAttributeQuantities(dt, attributeQty));

            Output.UndoStatus();                         
            return errors;                             
        }
        public List<string> CheckIfInvoiceMatchesData(Dictionary<string, object> expectedFields, string orderCode){
            List<string> errors = new List<string>();
            
            if(Output != null) Output.Write(string.Format("Getting the invoice data for the order ~{0}... ", orderCode), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetInvoiceData(orderCode);
            errors.AddRange(CheckIfTableMatchesData(expectedFields, dt));           

            Output.UndoStatus();                                      
            return errors;          
        } 
        public List<string> CheckIfPosSaleMatchesData(Dictionary<string, object> expectedFields, int posSaleID, Dictionary<string, int> attributeQty = null){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the POS sale data for ~ID={0}... ", posSaleID), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetPosSaleData(posSaleID);                        
            errors.AddRange(CheckIfTableMatchesData(expectedFields, dt));
            errors.AddRange(CheckAttributeQuantities(dt, attributeQty));

            Output.UndoStatus();
            
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

                    if(!attributeQty.ContainsKey(variant)) errors.Add(String.Format("Unexpected product '{0}' found.", dr["name"]));
                    else{
                        if(attributeQty[variant] != (int)(decimal)dr["product_qty"]) errors.Add(String.Format("Unexpected quantity found for the product '{0}': expected->'{1}' found->'{2}'.", dr["name"], attributeQty[variant],  dr["qtyField"]));                    
                        found[variant] = true;
                    } 
                }

                foreach(string key in found.Keys){
                    if(!found[key]) errors.Add(String.Format("Unable to find the quantity for the attribute value '{0}'.", key));
                }
            }

            return errors;
        }
        private string GetWhereForName(string expectedValue, string dbField){
            string company = expectedValue;
            company = company.Replace(this.Student, "").Trim();
            string[] student = this.Student.Split(" ");

            //TODO: check if student.length > 2
            return string.Format("{3} like '{0}%' AND {3} like '%{1}%' AND {3} like '%{2}%'", company, student[0], student[1], dbField);
        }       
    }
}