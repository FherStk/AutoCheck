
using System;
using System.Data;
using System.Globalization;

namespace AutomatedAssignmentValidator.Connectors{       
    public partial class Odoo : Postgres{  
        public int CompanyID  {get; set;}
        public string CompanyName  {get; set;}
        private CultureInfo CultureEN {
            get{
                return CultureInfo.CreateSpecificCulture("en-EN");
            }
        }        
        
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
                SELECT com.*, (ata.file_size IS NOT NULL) AS logo 
                FROM public.res_company com
                    LEFT JOIN public.ir_attachment ata ON ata.res_id = com.id AND res_model = 'res.partner' AND res_field='image'
                WHERE com.parent_id IS NULL AND ata.company_id={0}
                ORDER BY com.id DESC", companyID)
            ).Tables[0];            
        }                 
        public int GetProviderID(string providerName){    
             return GetID("public.res_partner", "id", string.Format("company_id={0} AND {1}", this.CompanyID, GetWhereForName(providerName, "name")));           
        }
        public DataTable GetProviderData(string providerName){    
            return GetProviderData(GetProviderID(providerName));
        }
        public DataTable GetProviderData(int providerID){    
            return ExecuteQuery(string.Format(@"  
                SELECT pro.*, (ata.file_size IS NOT NULL) AS logo 
                FROM public.res_partner pro
                    LEFT JOIN public.ir_attachment ata ON ata.res_id = pro.id AND res_model = 'res.partner' AND res_field='image'
                WHERE pro.parent_id IS NULL AND pro.company_id={0} AND pro.id={1}
                ORDER BY pro.id DESC", this.CompanyID, providerID)
            ).Tables[0];            
        } 
        /// <summary>
        /// Returns the template ID, so all the product data (including variants) can be retrieved.
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public int GetProductTemplateID(string productName){    
            return GetID("public.product_template", "id", string.Format("company_id={0} AND {1}", this.CompanyID, GetWhereForName(productName, "name")));
        }      
        public DataTable GetProductTemplateData(string productName){    
            return GetProductTemplateData(GetProductTemplateID(productName));
        }          
        public DataTable GetProductTemplateData(int templateID){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            return ExecuteQuery(string.Format(@"
                SELECT pro.id AS product_id, tpl.id AS template_id, tpl.name, tpl.type, tpl.list_price AS sell_price, sup.price AS purchase_price, sup.name AS supplier_ID, ata.file_size, att.name AS attribute, val.name AS value 
                FROM public.product_product pro
                    LEFT JOIN public.product_template tpl ON tpl.id = pro.product_tmpl_id
                    LEFT JOIN public.ir_attachment ata ON ata.res_id=tpl.id AND ata.res_model='product.template' AND ata.res_id = tpl.id  AND ata.res_field='image'
                    LEFT JOIN public.product_attribute_value_product_product_rel rel ON rel.product_product_id = pro.id
                    LEFT JOIN public.product_attribute_value val ON val.id = rel.product_attribute_value_id
                    LEFT JOIN public.product_attribute att ON att.id = val.attribute_id
                    LEFT JOIN public.product_supplierinfo sup ON sup.product_id = pro.id
                WHERE tpl.company_id={0} AND tpl.id={1}", this.CompanyID, templateID)
            ).Tables[0];            
        }
        public int GetLastPurchaseID(){    
            return GetID("public.purchase_order", "id", string.Format("company_id={0}", this.CompanyID));
        }
        public int GetPurchaseID(string purchaseCode){    
            return GetID("public.purchase_order", "id", string.Format("company_id={0} AND name='{1}'", this.CompanyID, purchaseCode));
        }
        public string GetPurchaseCode(int purchaseID){    
            return GetPurchaseData(purchaseID).Rows[0]["code"].ToString();
        } 
        public DataTable GetPurchaseData(string purchaseCode){    
            return GetPurchaseData(GetPurchaseID(purchaseCode));
        }
        public DataTable GetPurchaseData(int purchaseID){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            return ExecuteQuery(string.Format(@"
                SELECT h.id, h.name AS code, h.amount_total, l.name AS product_name, l.product_qty, l.price_unit AS product_price_unit, l.product_id
                FROM public.purchase_order h
                    LEFT JOIN public.purchase_order_line l ON l.order_id=h.id
                WHERE h.company_id={0} AND h.id={1}", this.CompanyID, purchaseID)
            ).Tables[0];            
        } 
        public DataTable GetStockMovementData(string orderCode, bool isReturn){    
            bool input = orderCode.StartsWith("PO");
            if(isReturn) input = !input; 

            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            return ExecuteQuery(string.Format(CultureEN, @"
                SELECT id, name as product_name, product_qty, location_id, state
                FROM public.stock_move
                WHERE company_id={0} AND origin='{1}' AND reference LIKE '%/{2}/%'", this.CompanyID, orderCode, (input ? "IN" : "OUT"))
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