
using System;

namespace AutomatedAssignmentValidator.Connectors{       
    public partial class Odoo : Postgres{  
        public int CompanyID  {get; set;}
        public string CompanyName  {get; set;}
        
        public Odoo(string companyName, string host, string database, string username, string password): base(host, database, username, password){
            this.CompanyName = companyName;
            
            try{
                this.CompanyID = GetCompanyID(this.CompanyName);
            }
            catch{
                this.CompanyID = -1;
            }

        }

        //TODO: fulfill the methods avoiding SQL Queries (when possible) in order to avoid transcription errors.
        public bool HasCompanyLogo(string companyName){    
            object fileSize = ExecuteScalar(string.Format("SELECT file_size FROM public.ir_attachment WHERE res_model='res.partner' AND res_field='image' AND res_name LIKE %{0}%", companyName));
            return (fileSize == null || fileSize == DBNull.Value ? false : true);
        }  
        public int GetCompanyID(string companyName){    
            return GetID("public", "res_company", "id", "name", GetWhereForName(companyName, "com.name"));
        } 
        public int GetProviderID(string providerName){    
            return GetLastID(
                "public.res_partner pro LEFT JOIN public.ir_attachment ata ON ata.res_id = pro.id AND res_model = 'res.partner' AND res_field='image'",
                "pro.id", 
                string.Format("WHERE {0} AND pro.parent_id IS NULL AND pro.company_id={1}", GetWhereForName(providerName, "pro.name"), this.CompanyID)
            );
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