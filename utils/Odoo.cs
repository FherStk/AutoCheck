
using System;
using System.Data;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Utils{       
    public partial class Odoo{  
        private Output Output {get; set;}
        private DataBase DB {get; set;}

        public Odoo(string host, string database, string username, string password, Output output = null){
            this.Output = output;
            this.DB = new DataBase(host, database, username, password, output);
        }

        public List<string> CheckCompanyData(string companyName){    
            return this.DB.CheckIfTableMatchesData("public", "res_company", new Dictionary<string, object>(){{"name", companyName}}, "name", companyName);
        }   
    }
}