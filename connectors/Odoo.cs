
/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Data;
using System.Globalization;

namespace AutoCheck.Connectors{       
    /// <summary>
    /// Allows in/out operations and/or data validations with an Odoo instance.
    /// </summary>
    public class Odoo : Postgres{  
        /// <summary>
        /// The current company ID that will be used to acces and filter all the requested data.
        /// </summary>
        /// <value></value>
        public int CompanyID  {get; set;}
        /// <summary>
        /// The current company name.
        /// </summary>
        /// <value></value>
        public string CompanyName  {get; set;}
        private CultureInfo CultureEN {
            get{
                return CultureInfo.CreateSpecificCulture("en-EN");
            }
        }        
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="companyID">The company ID which will be used to operate.</param>
        /// <param name="host">Host address in order to connect with the running PostgreSQL service, wich contains the Odoo database.</param>
        /// <param name="database">The Odoo database name.</param>
        /// <param name="username">The Odoo database username, which will be used to perform operations.</param>
        /// <param name="password">The Odoo database password, which will be used to perform operations.</param>
        public Odoo(int companyID, string host, string database, string username, string password): base(host, database, username, password){
            this.CompanyID = companyID;
                        
            try{
                this.CompanyName = GetCompanyData(companyID).Rows[0]["name"].ToString();
            }
            catch{
                throw new Exception(string.Format("Unable to find any company with the provided ID={0}", companyID));
            }

        }
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="companyName">The company name which will be used to operate.</param>
        /// <param name="host">Host address in order to connect with the running PostgreSQL service, wich contains the Odoo database.</param>
        /// <param name="database">The Odoo database name.</param>
        /// <param name="username">The Odoo database username, which will be used to perform operations.</param>
        /// <param name="password">The Odoo database password, which will be used to perform operations.</param>
        /// <returns>A new instance.</returns>
        public Odoo(string companyName, string host, string database, string username, string password): base(host, database, username, password){
            this.CompanyName = companyName;
            
            try{
                this.CompanyID = GetCompanyID(this.CompanyName);
            }
            catch{
                throw new Exception(string.Format("Unable to find any company with the provided name='{0}'", companyName));
            }

        }        
        /// <summary>
        /// Requests for the company ID.
        /// </summary>
        /// <param name="companyName">The company name wich will be used to request.</param>
        /// <param name="strict">When strict is on, the company name match will be exact.</param>
        /// <returns>The company ID.</returns>
        public int GetCompanyID(string companyName, bool strict = false){    
            if(strict) return GetID("public", "res_company", "id", "name", '=', companyName);
            else return GetID("public.res_company", "id", GetNonStrictWhere("name", companyName));
        }
        /// <summary>
        /// Requests for the company data.
        /// </summary>
        /// <param name="companyName">The company name wich will be used to request.</param>
        /// <returns>The company data.</returns>
        public DataTable GetCompanyData(string companyName){    
            return GetCompanyData(GetCompanyID(companyName));
        }   
        /// <summary>
        /// Requests for the company data.
        /// </summary>
        /// <param name="companyID">The company ID wich will be used to request.</param>
        /// <returns>The company data.</returns>      
        public DataTable GetCompanyData(int companyID){    
            return ExecuteQuery(string.Format(@"  
                SELECT com.*, (ata.file_size IS NOT NULL) AS logo 
                FROM public.res_company com
                    LEFT JOIN public.ir_attachment ata ON ata.res_id = com.id AND res_model = 'res.partner' AND res_field='image'
                WHERE com.parent_id IS NULL AND ata.company_id={0}
                ORDER BY com.id DESC", companyID)
            ).Tables[0];            
        } 
        /// <summary>
        /// Requests for the provider ID.
        /// </summary>
        /// <param name="providerName">The provider name wich will be used to request.</param>
        /// <param name="strict">When strict is on, the provider name match will be exact.</param>
        /// <returns>The provider ID.</returns>             
        public int GetProviderID(string providerName, bool strict = false){  
            ///TODO: is should use the same where as the main query!!!        
            string filter = string.Format("parent_id IS NULL AND supplier = TRUE AND company_id={0} ", this.CompanyID);
            if(strict) filter += string.Format("AND name = '{0}'", providerName);
            else  filter += string.Format("AND {0}", GetNonStrictWhere("name", providerName));
            
            return GetID("public.res_partner", "id", filter);
        }
        /// <summary>
        /// Requests for the provider data.
        /// </summary>
        /// <param name="providerName">The provider name wich will be used to request.</param>
        /// <returns>The provider data.</returns>
        public DataTable GetProviderData(string providerName){    
            return GetProviderData(GetProviderID(providerName));
        }
        /// <summary>
        /// Requests for the provider data.
        /// </summary>
        /// <param name="providerID">The provider ID wich will be used to request.</param>
        /// <returns>The provider data.</returns>
        public DataTable GetProviderData(int providerID){    
            return ExecuteQuery(string.Format(@"  
                SELECT pro.*, (ata.file_size IS NOT NULL) AS logo 
                FROM public.res_partner pro
                    LEFT JOIN public.ir_attachment ata ON ata.res_id = pro.id AND res_model = 'res.partner' AND res_field='image'
                WHERE pro.parent_id IS NULL AND pro.supplier = TRUE AND pro.company_id={0} AND pro.id={1}
                ORDER BY pro.id DESC", this.CompanyID, providerID)
            ).Tables[0];            
        } 
        /// <summary>
        /// Requests for the product template ID, so all the product data (including variants) can be retrieved.
        /// </summary>
        /// <param name="productName">The product name wich will be used to request.</param>
        /// <param name="strict">When strict is on, the product name match will be exact.</param>
        /// <returns>The product template ID.</returns>
        public int GetProductTemplateID(string productName, bool strict = false){    
            if(strict) return GetID("public", "product_template", "id", "name", '=', productName);
            else return GetID("public.product_template", "id", string.Format("company_id={0} AND {1}", this.CompanyID, GetNonStrictWhere("name", productName)));
        }  
        /// <summary>
        /// Requests for the product template data.
        /// </summary>
        /// <param name="productName">The product name wich will be used to request.</param>
        /// <returns>The product template data, including variants.</returns>    
        public DataTable GetProductTemplateData(string productName){    
            return GetProductTemplateData(GetProductTemplateID(productName));
        }   
        /// <summary>
        /// Requests for the product template data.
        /// </summary>
        /// <param name="templateID">The product template ID wich will be used to request.</param>
        /// <returns>The product template data, including variants.</returns>           
        public DataTable GetProductTemplateData(int templateID){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            return ExecuteQuery(string.Format(@"
                SELECT pro.id AS product_id, tpl.id AS template_id, tpl.name, tpl.type, tpl.list_price AS sell_price, sup.price AS purchase_price, sup.name AS supplier_id, ata.file_size, att.name AS attribute, val.name AS value 
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
        /// <summary>
        /// Requests for the last purchase ID (the higher ID).
        /// </summary>
        /// <returns>The last purchase ID.</returns>
        public int GetLastPurchaseID(){    
            return GetID("public.purchase_order", "id", string.Format("company_id={0}", this.CompanyID));
        }
        /// <summary>
        /// Requests for the purchase ID.
        /// </summary>
        /// <param name="purchaseCode">The purchase code wich will be used to request.</param>
        /// <returns>The purchase ID.</returns>
        public int GetPurchaseID(string purchaseCode){    
            return GetID("public.purchase_order", "id", string.Format("company_id={0} AND name='{1}'", this.CompanyID, purchaseCode));
        }
        /// <summary>
        /// Requests for the purchase code.
        /// </summary>
        /// <param name="purchaseID">The purchase ID wich will be used to request.</param>
        /// <returns>The purchase ID.</returns>
        public string GetPurchaseCode(int purchaseID){    
            return GetPurchaseData(purchaseID).Rows[0]["code"].ToString();
        } 
        /// <summary>
        /// Requests for the purchase data.
        /// </summary>
        /// <param name="purchaseCode">The purchase code wich will be used to request.</param>
        /// <returns>The purchase code.</returns>
        public DataTable GetPurchaseData(string purchaseCode){    
            return GetPurchaseData(GetPurchaseID(purchaseCode));
        }
        /// <summary>
        /// Requests for the purchase data.
        /// </summary>
        /// <param name="purchaseID">The purchase ID wich will be used to request.</param>
        /// <returns>The purchase data.</returns>
        public DataTable GetPurchaseData(int purchaseID){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            return ExecuteQuery(string.Format(@"
                SELECT h.id, h.name AS code, h.amount_total, l.name AS product_name, l.product_qty, l.price_unit AS product_price_unit, l.product_id
                FROM public.purchase_order h
                    LEFT JOIN public.purchase_order_line l ON l.order_id=h.id
                WHERE h.company_id={0} AND h.id={1}", this.CompanyID, purchaseID)
            ).Tables[0];            
        } 
        /// <summary>
        /// Requests for the stock movement data.
        /// </summary>
        /// <param name="orderCode">The order code (or number) wich will be used to request.</param>
        /// <param name="isReturn">If true, the stock movement is related with a return.</param>
        /// <returns>The stock movement data.</returns>
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
        /// <summary>
        /// Requests for all the information about scrapped stock data.
        /// </summary>
        /// <returns>The scrapped stock data.</returns>
        public DataTable GetScrappedStockData(){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            return ExecuteQuery(string.Format(@"
                SELECT m.id, m.product_qty, m.location_id, m.state, {0}
                FROM public.stock_move m
                    {1}
                WHERE m.company_id={2} AND m.scrapped=true", GetProductDataName(), GetProductDataJoin("m.product_id"), this.CompanyID)
            ).Tables[0];            
        }
        /// <summary>
        /// Requests for the invoice code.
        /// </summary>
        /// <param name="orderCode">The order code, related to the invoice, wich will be used to request.</param>
        /// <returns>The invoice data.</returns>
        public string GetInvoiceCode(string orderCode){    
            return GetInvoiceData(orderCode).Rows[0]["number"].ToString();
        } 
        /// <summary>
        /// Requests for the invoice data.
        /// </summary>
        /// <param name="orderCode">The order code, related to the invoice, wich will be used to request.</param>
        /// <returns>The invoice data.</returns>
        public DataTable GetInvoiceData(string orderCode){    
            string type = string.Empty;
            if(orderCode.Length > 0){            
                switch(orderCode.Substring(0, 2)){
                    case "PO":
                        type = "in_invoice";
                        break;

                    case "SO":
                        type = "out_invoice";                    
                        break;

                    default:
                        type = "out_refund";                                
                        break;
                }
            }

            return ExecuteQuery(string.Format(CultureEN, @"
                SELECT *
                FROM public.account_invoice
                WHERE company_id={0} AND origin='{1}' AND type='{2}'", CompanyID, orderCode, type)
            ).Tables[0];            
        } 
        /// <summary>
        /// Requests for the last (higher) Point Of Sale sale ID.
        /// </summary>
        /// <returns>The last POS sale ID.</returns>
        public int GetLastPosSaleID(){    
            return GetID("public.pos_order", "id", string.Format("company_id={0}", this.CompanyID));
        }
        /// <summary>
        /// Requests for the Point Of Sale sale ID.
        /// </summary>
        /// <param name="posSaleCode">The POS sale code wich will be used to request.</param>
        /// <returns>The POS sale ID.</returns>
        public int GetPosSaleID(string posSaleCode){    
            return GetID("public.pos_order", "id", string.Format("company_id={0} AND name='{1}'", this.CompanyID, posSaleCode));
        }
        /// <summary>
        /// Requests for the Point Of Sale sale code.
        /// </summary>
        /// <param name="posSaleID">The POS sale ID wich will be used to request.</param>
        /// <returns>The POS sale code.</returns>
        public string GetPosSaleCode(int posSaleID){    
            return GetPosSaleData(posSaleID).Rows[0]["code"].ToString();
        } 
        /// <summary>
        /// Requests for the Point Of Sale sale data.
        /// </summary>
        /// <param name="posSaleCode">The POS sale code wich will be used to request.</param>
        /// <returns>The POS sale data.</returns>
        public DataTable GetPosSaleData(string posSaleCode){    
            return GetPosSaleData(GetPurchaseID(posSaleCode));
        }
        /// <summary>
        /// Requests for the Point Of Sale sale data.
        /// </summary>
        /// <param name="posSaleID">The POS sale ID wich will be used to request.</param>
        /// <returns>The POS sale data.</returns>
        public DataTable GetPosSaleData(int posSaleID){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            return ExecuteQuery(string.Format(@"
                SELECT h.id, h.name AS code, h.state, l.product_id, l.qty as product_qty, {0} 
                FROM public.pos_order h
                    LEFT JOIN public.pos_order_line l ON l.order_id = h.id
                    {1}
                WHERE h.company_id={2} AND h.id={3}", GetProductDataName(), GetProductDataJoin("l.product_id"), this.CompanyID, posSaleID)
            ).Tables[0];            
        } 
        /// <summary>
        /// Requests for the last (higher) sale ID.
        /// </summary>
        /// <returns>The last sale ID.</returns>
        public int GetLastSaleID(){    
            return GetID("public.sale_order", "id", string.Format("company_id={0}", this.CompanyID));
        }
        /// <summary>
        /// Requests for the sale ID.
        /// </summary>
        /// <param name="saleCode">The sale code wich will be used to request.</param>
        /// <returns>The sale ID.</returns>
        public int GetSaleID(string saleCode){    
            return GetID("public.sale_order", "id", string.Format("company_id={0} AND name='{1}'", this.CompanyID, saleCode));
        }
        /// <summary>
        /// Requests for the sale code.
        /// </summary>
        /// <param name="saleID">The sale ID wich will be used to request.</param>
        /// <returns>The sale ID.</returns>
        public string GetSaleCode(int saleID){    
            return GetSaleData(saleID).Rows[0]["code"].ToString();
        }
        /// <summary>
        /// Requests for the sale data.
        /// </summary>
        /// <param name="saleCode">The sale code wich will be used to request.</param>
        /// <returns>The sale data.</returns> 
        public DataTable GetSaleData(string saleCode){    
            return GetSaleData(GetPurchaseID(saleCode));
        }
        /// <summary>
        /// Requests for the sale data.
        /// </summary>
        /// <param name="saleID">The sale ID wich will be used to request.</param>
        /// <returns>The sale data.</returns>
        public DataTable GetSaleData(int saleID){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            return ExecuteQuery(string.Format(@"
                SELECT h.id, h.name AS code, h.state, l.product_id, l.product_uom_qty as product_qty, {0} 
                FROM public.sale_order h
                    LEFT JOIN public.sale_order_line l ON l.order_id = h.id
                    {1}
                WHERE h.company_id={2} AND h.id={3}", GetProductDataName(), GetProductDataJoin("l.product_id"), this.CompanyID, saleID)
            ).Tables[0];            
        } 
        /// <summary>
        /// Requests for the user ID.
        /// </summary>
        /// <param name="userName">The user name wich will be used to request.</param>
        /// <param name="strict">When strict is on, the user name match will be exact.</param>
        /// <returns>The user ID.</returns>
        public int GetUserID(string userName, bool strict = false){    
            if(strict) return GetID("public", "res_users", "id", "login", '=', userName);
            else return GetID("public.res_users", "id", string.Format("company_id={0} AND {1}", this.CompanyID, GetNonStrictWhere("login", userName)));
        }
        /// <summary>
        /// Requests for the user name.
        /// </summary>
        /// <param name="userID">The user ID wich will be used to request.</param>
        /// <returns>The user ID.</returns>
        public string GetUserName(int userID){    
            return GetUserData(userID).Rows[0]["name"].ToString();
        } 
        /// <summary>
        /// Requests for the user data.
        /// </summary>
        /// <param name="userName">The user name wich will be used to request.</param>
        /// <returns>The user data.</returns>
        public DataTable GetUserData(string userName){    
            return GetUserData(GetUserID(userName));
        }
        /// <summary>
        /// Requests for the user data.
        /// </summary>
        /// <param name="userID">The user ID wich will be used to request.</param>
        /// <returns>The user data.</returns>
        public DataTable GetUserData(int userID){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            return ExecuteQuery(string.Format(@"
                SELECT u.id, u.login as name, u.active, g.name AS group
                FROM public.res_users u
                    LEFT JOIN public.res_groups_users_rel r ON r.uid = u.id
                    INNER JOIN public.res_groups g ON r.gid = g.id                    
                WHERE u.company_id={0} AND u.id={1}", this.CompanyID, userID)
            ).Tables[0];            
        }
        private string GetNonStrictWhere(string field, string value){
            //The idea is to avoid the usual errors when naming
            string[] items = value.Replace("_", " ").Replace("@", " ").Split(" ");            

            if(items.Length == 1) return string.Format("{0} LIKE '{1}'", field, items[0]);            
            else{
                string like = string.Format("{0} LIKE '{1}%'", field, items[0]);
                for(int i = 1; i<items.Length-2; i++)
                    like = string.Format("{0} AND {1} LIKE '%{2}%'", like, field, items[i]);

                return string.Format("{0} AND {1} LIKE '%{2}'", like, field, items[items.Length-1]);
            }            
        }
        private string GetProductDataJoin(string localProductIdField){
            return string.Format(@" 
                LEFT JOIN public.product_product pro ON pro.id = {0}
                LEFT JOIN public.product_template tpl ON tpl.id = pro.product_tmpl_id
                LEFT JOIN public.product_attribute_value_product_product_rel rel ON rel.product_product_id = pro.id
                LEFT JOIN public.product_attribute_value val ON val.id = rel.product_attribute_value_id", localProductIdField);
        }
        private string GetProductDataName(){
            return "CONCAT(tpl.name, ' (', val.name, ')') as product_name";
        }
    }
}