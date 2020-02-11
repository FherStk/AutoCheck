
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
        public List<string> CheckIfProductMatchesData(Dictionary<string, object> fields, int idTemplate, string[] attributeValues = null){    
            List<string> errors = new List<string>();            
            
            if(!fields.ContainsKey("id") && !fields.ContainsKey("name")) throw new Exception("At least a product ID and/or name must be provided in order to check if data matches.");            
            if(Output != null){
                if(fields.ContainsKey("name")) Output.Write(string.Format("Getting the product data for ~{0}... ", fields["name"]), ConsoleColor.Yellow);            
                else Output.Write(string.Format("Getting the product data for ~ID={0}... ", fields["id"]), ConsoleColor.Yellow);            
            }
            
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = null;
            if(fields.ContainsKey("id")) dt = this.Connector.GetProductTemplateData((int)fields["id"]);
            else dt = this.Connector.GetProductTemplateData(fields["name"].ToString());
            
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
        public List<string> CheckIfPurchaseMatchesData(Dictionary<string, object> fields, string[] attributeValues = null){    
            List<string> errors = new List<string>();            
            
            /*if(!fields.ContainsKey("id") && !fields.ContainsKey("name")) throw new Exception("At least a product ID and/or name must be provided in order to check if data matches.");            
            if(Output != null){
                if(fields.ContainsKey("name")) Output.Write(string.Format("Getting the product data for ~{0}... ", fields["name"]), ConsoleColor.Yellow);            
                else Output.Write(string.Format("Getting the product data for ~ID={0}... ", fields["id"]), ConsoleColor.Yellow);            
            }
            
            Output.Disable();   //no output for native database checker wanted.

            DataTable dt = null;
            if(fields.ContainsKey("id")) dt = this.Connector.GetProductData((int)fields["id"]);
            else dt = this.Connector.GetProductData(fields["name"].ToString());
            
            errors.AddRange(this.CheckIfTableMatchesData(fields, dt));
            if(attributeValues != null){
                foreach(DataRow dr in dt.Rows){
                    if(attributeValues.Contains(dr["value"].ToString().Trim()))
                        errors.Add(String.Format("Unexpected variant value '{0}' found for the variant attribute '{1}' on product '{2}'.", dr["value"].ToString(), dr["attribute"], dr["name"]));                    
                }
            }

            Output.UndoStatus();
            */
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