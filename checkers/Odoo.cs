
using System;
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

        public List<string> CheckIfCompanyMatchesData(Dictionary<string, object> fields, bool mustHaveLogo = true, bool forceDefaultCompany = true){    
            List<string> errors = new List<string>();            
            if(fields["name"] == null || string.IsNullOrEmpty(fields["name"].ToString())) throw new Exception("Unable to find any company with no name.");                                        

            if(Output != null)  Output.Write(string.Format("Getting the company data for ~{0}... ", fields["name"]), ConsoleColor.Yellow);
            if(forceDefaultCompany && this.CompanyID > 1){
                errors.Add("The default company is being used in order to store the business data.");
                this.CompanyID = 1;
            } 

            Output.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfSelectMatchesData(fields, string.Format(@"  
                    SELECT com.id, com.name, (ata.file_size IS NOT NULL) AS logo FROM public.res_company com
                    LEFT JOIN public.ir_attachment ata ON ata.res_id = com.id AND res_model = 'res.partner' AND res_field='image'
                    WHERE com.parent_id IS NULL AND com.company_id={0}
                    ORDER BY pro.id DESC", this.CompanyID))
            );          
            Output.UndoStatus();

            return errors;
        }  
        public List<string> CheckIfProviderMatchesData(Dictionary<string, object> fields, bool mustHaveLogo = true){    
            List<string> errors = new List<string>();
            if(fields["name"] == null || string.IsNullOrEmpty(fields["name"].ToString())) throw new Exception("Unable to find any company with no name.");                                        
            
            if(Output != null)  Output.Write(string.Format("Getting the company data for ~{0}... ", fields["name"]), ConsoleColor.Yellow);            
            Output.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfSelectMatchesData(fields, string.Format(@"  
                    SELECT pro.id, pro.name, pro.is_company, (ata.file_size IS NOT NULL) AS logo FROM public.res_partner pro
                    LEFT JOIN public.ir_attachment ata ON ata.res_id = pro.id AND res_model = 'res.partner' AND res_field='image'
                    WHERE {0} AND pro.parent_id IS NULL AND pro.company_id={1}
                    ORDER BY pro.id DESC", GetWhereForName(fields["name"].ToString(), "pro.name"), this.CompanyID))
            );
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