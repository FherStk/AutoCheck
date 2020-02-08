
using System;
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

            if(Output != null) Output.Write(string.Format("Getting the company data for ~{0}... ", companyName), ConsoleColor.Yellow);
            if(forceDefaultCompany && this.CompanyID > 1){
                errors.Add("The default company is being used in order to store the business data.");
                this.CompanyID = 1;
            } 

            Output.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(new Dictionary<string, object>(){{"name", companyName}}, "public", "res_company", "id", this.CompanyID));
            if(!this.Connector.HasCompanyLogo(companyName)) errors.Add(string.Format("Unable to find any logo attached to the company '{0}'", companyName));            
            Output.UndoStatus();

            return errors;
        }  
        public List<string> CheckIfProviderMatchesData(int providerID, string providerName, bool forceDefaultCompany = true){    
            List<string> errors = new List<string>();

            if(Output != null) Output.Write(string.Format("Getting the provider data for ~{0}... ", providerName), ConsoleColor.Yellow);
            
            Output.Disable();   //no output for native database checker wanted.
            /*errors.AddRange(this.CheckIfSelectMatchesData(new Dictionary<string, object>(){{"name", providerName}}, string.Format(@"
                SELECT pro.id, pro.name, pro.is_company, ata.file_size FROM public.res_partner pro
                LEFT JOIN public.ir_attachment ata ON ata.res_id = pro.id AND res_model = 'res.partner' AND res_field='image'
                WHERE {0} AND pro.parent_id IS NULL AND pro.company_id={1}
                ORDER BY pro.id DESC", GetWhereForName(providerName, "pro.name"), this.CompanyID)));

            
            providerID = (int)dr["id"] ;                                                                
            if(dr["file_size"] == System.DBNull.Value) errors.Add(String.Format("Unable to find any picture attached to the provider named '{0}'.", data.providerName));
            if(((bool)dr["is_company"]) == false) errors.Add(String.Format("The provider named '{0}' has not been set up as a company.", data.providerName));
            if(!dr["name"].ToString().Equals(data.providerName)) errors.Add(string.Format("Incorrect provider name: expected->'{0}'; found->'{1}'", data.providerName, dr["name"].ToString()));
            */
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