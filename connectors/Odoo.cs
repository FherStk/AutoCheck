
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
        public int GetCompanyID(string companyName){    
            return GetID("public.res_company", "id", GetWhereForName(companyName, "name"));
        }
        public DataTable GetCompanyData(string companyName){    
            return GetCompanyData(GetCompanyID(companyName));
        }         
        public DataTable GetCompanyData(int companyID){    
            return ExecuteQuery(string.Format(@"  
                SELECT com.id, com.name, (ata.file_size IS NOT NULL) AS logo 
                FROM public.res_company com
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
                SELECT pro.id, pro.name, pro.is_company, (ata.file_size IS NOT NULL) AS logo 
                FROM public.res_partner pro
                    LEFT JOIN public.ir_attachment ata ON ata.res_id = pro.id AND res_model = 'res.partner' AND res_field='image'
                WHERE pro.parent_id IS NULL AND pro.id={0}
                ORDER BY pro.id DESC", providerID)
            ).Tables[0];            
        } 
        /// <summary>
        /// Returns the template ID, so all the product data (including variants) can be retrieved.
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public int GetProductID(string productName){    
            return GetID("public.product_template", "id", GetWhereForName(productName, "name"));
        }      
        public DataTable GetProductData(string productName){    
            return GetProductData(GetProductID(productName));
        }          
        public DataTable GetProductData(int templateID){    
            return ExecuteQuery(string.Format(@"
                SELECT pro.id AS product_id, tpl.id AS template_id, tpl.name, tpl.type, tpl.list_price AS sell_price, sup.price AS purchase_price, sup.name AS supplier_ID, ata.file_size, att.name AS attribute, val.name AS value 
                FROM public.product_product pro
                    LEFT JOIN public.product_template tpl ON tpl.id = pro.product_tmpl_id
                    LEFT JOIN public.ir_attachment ata ON ata.res_id=tpl.id AND ata.res_model='product.template' AND ata.res_id = tpl.id  AND ata.res_field='image'
                    LEFT JOIN public.product_attribute_value_product_product_rel rel ON rel.product_product_id = pro.id
                    LEFT JOIN public.product_attribute_value val ON val.id = rel.product_attribute_value_id
                    LEFT JOIN public.product_attribute att ON att.id = val.attribute_id
                    LEFT JOIN public.product_supplierinfo sup ON sup.product_tmpl_id = tpl.id
                WHERE tpl.id={0}", templateID)
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