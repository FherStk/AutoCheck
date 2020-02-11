
using System;
using System.Data;

namespace AutomatedAssignmentValidator.Connectors{       
    public partial class Odoo : Postgres{  
        public int CompanyID  {get; set;}
        public string CompanyName  {get; set;}
        
        public Odoo(int companyID, string host, string database, string username, string password): base(host, database, username, password){
            this.CompanyID = companyID;
                        
            try{
                this.CompanyName = GetCompanyData(companyID).Rows[0]["name"].ToString();
            }
            catch{
                throw new Exception(string.Format("Unable to find any company with the provided ID={0}", companyID));
            }

        }
        public Odoo(string companyName, string host, string database, string username, string password): base(host, database, username, password){
            this.CompanyName = companyName;
            
            try{
                this.CompanyID = GetCompanyID(this.CompanyName);
            }
            catch{
                throw new Exception(string.Format("Unable to find any company with the provided name='{0}'", companyName));
            }

        }

        //TODO: fulfill the methods avoiding SQL Queries (when possible) in order to avoid transcription errors.
        public int GetCompanyID(string companyName){    
            return GetID("public.res_company", "id", GetWhereForName(companyName, "name"));
        }
        public DataTable GetCompanyData(string companyName){    
            return GetCompanyData(GetCompanyID(companyName));
        }         
        public DataTable GetCompanyData(int companyID){    
            return ExecuteQuery(string.Format(@"  
                SELECT com.id, com.name, (ata.file_size IS NOT NULL) AS logo FROM public.res_company com
                LEFT JOIN public.ir_attachment ata ON ata.res_id = com.id AND res_model = 'res.partner' AND res_field='image'
                WHERE com.parent_id IS NULL AND ata.company_id={0}
                ORDER BY com.id DESC", companyID)
            ).Tables[0];            
        }                 
        public int GetProviderID(string providerName){    
             return GetID("public.res_partner", "id", GetWhereForName(providerName, "name"));           
        }
        public DataTable GetProviderData(string providerName){    
            return GetProviderData(GetProviderID(providerName));
        }
        public DataTable GetProviderData(int providerID){    
            return ExecuteQuery(string.Format(@"  
                SELECT pro.id, pro.name, pro.is_company, (ata.file_size IS NOT NULL) AS logo FROM public.res_partner pro
                LEFT JOIN public.ir_attachment ata ON ata.res_id = pro.id AND res_model = 'res.partner' AND res_field='image'
                WHERE pro.parent_id IS NULL AND pro.company_id={0} AND pro.id={1}
                ORDER BY pro.id DESC",this.CompanyID, providerID)
            ).Tables[0];            
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