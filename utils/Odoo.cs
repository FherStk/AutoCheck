
using System;
using System.Data;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Utils{       
    public partial class Odoo{  
        private Output Output {get; set;}
        private DataBase DB {get; set;}
        public int CompanyID  {get; private set;}
        public string CompanyName  {get; private set;}
        

        public Odoo(string companyName, string host, string database, string username, string password, Output output = null){
            this.Output = output;
            this.CompanyName = companyName;
            this.DB = new DataBase(host, database, username, password, output);
            
            try{
                this.CompanyID = GetCompanyID(this.CompanyName);
            }
            catch{
                this.CompanyID = -1;
            }

        }

        public List<string> CheckCompanyData(string companyName, bool forceDefaultCompany = true){    
            List<string> errors = new List<string>();

            if(forceDefaultCompany && this.CompanyID > 1){
                errors.Add("The default company is being used in order to store the business data.");
                this.CompanyID = 1;
            } 

            errors.AddRange(this.DB.CheckIfTableMatchesData("public", "res_company", new Dictionary<string, object>(){{"name", companyName}}, "id", this.CompanyID));
            if(!HasCompanyLogo(companyName)) errors.Add(string.Format("Unable to find any logo attached to the company '{0}'", companyName));            
            

            return errors;
        }  
        public bool HasCompanyLogo(string companyName){    
            object fileSize = this.DB.ExecuteScalar(string.Format("SELECT file_size FROM public.ir_attachment WHERE res_model='res.partner' AND res_field='image' AND res_name LIKE %{0}%", companyName));
            return (fileSize == null || fileSize == DBNull.Value ? false : true);
        }  
        public int GetCompanyID(string companyName){    
            return this.DB.GetID("public", "res_company", "id", "name", string.Format("%{0}%", companyName), '%');
        }  
    }
}