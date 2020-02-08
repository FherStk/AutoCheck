
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Checkers{       
    public partial class Odoo : DataBase{  
        private Output Output {get; set;}
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
        
        public Odoo(string companyName, string host, string database, string username, string password, Output output = null): base(host, database, username, password, output){         
            this.Connector = new Connectors.Odoo(companyName, host, database, username, password);            
        }

        public List<string> CheckIfCompanyMatchesData(string companyName, bool forceDefaultCompany = true){    
            List<string> errors = new List<string>();

            if(forceDefaultCompany && this.CompanyID > 1){
                errors.Add("The default company is being used in order to store the business data.");
                this.CompanyID = 1;
            } 

            errors.AddRange(this.CheckIfTableMatchesData(new Dictionary<string, object>(){{"name", companyName}}, "public", "res_company", "id", this.CompanyID));
            if(!this.Connector.HasCompanyLogo(companyName)) errors.Add(string.Format("Unable to find any logo attached to the company '{0}'", companyName));            
            
            return errors;
        }          
    }
}