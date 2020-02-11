
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

        public List<string> CheckIfCompanyMatchesData(Dictionary<string, object> fields, int companyID){    
            List<string> errors = new List<string>();                        

            if(Output != null)  Output.Write(string.Format("Getting the company data for ~ID={0}... ", companyID), ConsoleColor.Yellow);            

            Output.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(fields, this.Connector.GetCompanyData(companyID)));
            Output.UndoStatus();

            return errors;
        }  
        public List<string> CheckIfProviderMatchesData(Dictionary<string, object> fields, int providerID){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the provider data for ~ID={0}... ", providerID), ConsoleColor.Yellow);            
            
            Output.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(fields, this.Connector.GetProviderData(providerID)));
            Output.UndoStatus();

            return errors;
        }
        public List<string> CheckIfProductMatchesData(Dictionary<string, object> fields, int templateID, string[] attributeValues = null){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the product data for ~ID={0}... ", templateID), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetProductTemplateData(templateID);                        
            errors.AddRange(this.CheckIfTableMatchesData(fields, dt));

            if(attributeValues != null){
                foreach(DataRow dr in dt.Rows){
                    if(attributeValues.Contains(dr["value"].ToString().Trim()))
                        errors.Add(String.Format("Unexpected variant value '{0}' found for the variant attribute '{1}' on product '{2}'.", dr["value"].ToString(), dr["attribute"], dr["name"]));                    
                }
            }

            Output.UndoStatus();
            return errors;
        } 
        public List<string> CheckIfPurchaseMatchesData(Dictionary<string, object> fields, int purchaseID, Dictionary<string, object> attributeQty = null){    
            List<string> errors = new List<string>();            
                        
            if(Output != null) Output.Write(string.Format("Getting the purchase data for ~ID={0}... ", purchaseID), ConsoleColor.Yellow);                        
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetPurchaseData(purchaseID);                        
            errors.AddRange(this.CheckIfTableMatchesData(fields, dt));

            if(attributeQty != null){
                foreach(DataRow dr in dt.Rows){
                    bool sale = true;   //TODO: refactor for sale/purchase/refund
                    string qtyField = string.Format("product_{0}", sale ? "uom_qty" : "qty");
                    string variant = dr["name"].ToString();
                    if(variant.Contains("(")){
                        variant = variant.Substring(variant.IndexOf("(")+1);
                        variant = variant.Substring(0, variant.IndexOf(")")).Replace(" ", "");
                    }                    

                    if(!attributeQty.ContainsKey(variant)) errors.Add(String.Format("Unexpected product and/or variant attribute '{0}' on purchase '{1}'.", dr["name"], purchaseID));
                    else if(attributeQty[variant] != dr["qtyField"]) errors.Add(String.Format("Unexpected quantity found for the product '{0}' on purchase '{1}': expected->'{2}' found->'{3}'.", dr["name"], purchaseID, attributeQty[variant],  dr["qtyField"]));                    
                }
            }

            Output.UndoStatus();
            
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