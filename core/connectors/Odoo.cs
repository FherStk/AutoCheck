
/*
    Copyright © 2022 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/credits.md

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

namespace AutoCheck.Core.Connectors{       
    /// <summary>
    /// Allows in/out operations and/or data validations with an Odoo instance.
    /// </summary>
    public class Odoo : Postgres{  
#region "Attributes"
        private int _companyID = 0;
        private string _companyName = null;

        /// <summary>
        /// The current company ID that will be used to acces and filter all the requested data.
        /// </summary>
        /// <value></value>
        public int CompanyID  {
            set {
                _companyID = value;
            }
            get {
                if(_companyID > 0) return _companyID;
                else{
                    try{
                        _companyID = GetCompanyID(this.CompanyName);
                        return _companyID;
                    }
                    catch{
                        throw new Exception(string.Format("Unable to find any company with the provided name='{0}'", this.CompanyName));
                    } 
                }
            } 
        }

        /// <summary>
        /// The current company name.
        /// </summary>
        /// <value></value>
        public string CompanyName  {
            set {
                _companyName = value;
            }
            get {
                if(!string.IsNullOrEmpty(_companyName)) return _companyName;
                else{
                    try{
                        _companyName = GetCompanyData(this.CompanyID).Rows[0]["name"].ToString();
                        return _companyName;
                    }
                    catch{
                        throw new Exception(string.Format("Unable to find any company with the provided ID={0}", this.CompanyID));
                    } 
                }
            }            
        }
        
        /// <summary>
        /// The culture info to use within the database.
        /// </summary>
        /// <value></value>
        private CultureInfo CultureEN {
            get{
                return CultureInfo.CreateSpecificCulture("en-EN");
            }
        }        
#endregion
#region "Constructor / Destructor"        
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="companyID">The company ID which will be used to operate.</param>
        /// <param name="host">Host address in order to connect with the running PostgreSQL service, wich contains the Odoo database.</param>
        /// <param name="database">The Odoo database name.</param>
        /// <param name="username">The Odoo database username, which will be used to perform operations.</param>
        /// <param name="password">The Odoo database password, which will be used to perform operations.</param>
        public Odoo(int companyID, string host, string database, string username, string password=null, string binPath = "C:\\Program Files\\PostgreSQL\\10\\bin"): base(host, database, username, password, binPath){
            if(companyID < 1) throw new ArgumentOutOfRangeException("companyID", companyID, "Must be an number greater than 0.");
            this.CompanyID = companyID;
            //NOTE: companyName cannot be loaded because the database could not exist yet (this connectors inherits the createDatabase method from postgres)        
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
        public Odoo(string companyName, string host, string database, string username, string password=null, string binPath = "C:\\Program Files\\PostgreSQL\\10\\bin"): base(host, database, username, password, binPath){
            if(string.IsNullOrEmpty(companyName)) throw new ArgumentNullException("companyName");
            this.CompanyName = companyName;   
            //NOTE: companyID cannot be loaded because the database could not exist yet (this connectors inherits the createDatabase method from postgres)        
        }        
#endregion
#region "Company"
        /// <summary>
        /// Requests for the company ID.
        /// </summary>
        /// <param name="companyName">The company name wich will be used to request.</param>
        /// <param name="strict">When strict is on, the company name match will be exact.</param>
        /// <returns>The company ID.</returns>
        public int GetCompanyID(string companyName, bool strict = false){ 
            if(string.IsNullOrEmpty(companyName)) throw new ArgumentNullException(companyName);

            string filter = (strict ? string.Format("com.name='{0}'", companyName) :  GetNonStrictWhere("com.name", companyName));               
            var com = GetCompanyData(filter, "com.name");
            return (com.Rows.Count == 0 ? 0 : (int)com.Rows[0]["id"]);
        }
        
        /// <summary>
        /// Requests for the company data.
        /// </summary>
        /// <param name="companyName">The company name wich will be used to request.</param>
        /// <returns>The company data (all the 'res.company' table fields, logo).</returns>
        public DataTable GetCompanyData(string companyName){    
            if(string.IsNullOrEmpty(companyName)) throw new ArgumentNullException(companyName);
            return GetCompanyData(string.Format("com.name='{0}'", companyName), "com.name");
        }  

        /// <summary>
        /// Requests for the company data.
        /// </summary>
        /// <param name="companyID">The company ID wich will be used to request.</param>
        /// <returns>The company data (all the 'res.company' table fields, logo).</returns>      
        public DataTable GetCompanyData(int companyID){  
            if(companyID < 1) throw new ArgumentOutOfRangeException("companyID", companyID, "Must be an number greater than 0.");
            return GetCompanyData(string.Format("ata.company_id={0}", companyID), "com.id");
        }
        
        private DataTable GetCompanyData(string filter, string order){     
            if(string.IsNullOrEmpty(filter)) filter = "1=1";
            if(string.IsNullOrEmpty(order)) order = "1";

            return ExecuteQuery(string.Format(@"  
                SELECT com.*, (ata.file_size IS NOT NULL) AS logo 
                FROM public.res_company com
                    LEFT JOIN public.ir_attachment ata ON ata.res_id = com.id AND res_model = 'res.partner' AND res_field='image'
                WHERE com.parent_id IS NULL AND {0}
                ORDER BY {1} DESC", filter, order)
            ).Tables[0];
        } 
#endregion
#region "Providers"        
        /// <summary>
        /// Requests for the provider ID.
        /// </summary>
        /// <param name="providerName">The provider name wich will be used to request.</param>
        /// <param name="strict">When strict is on, the provider name match will be exact.</param>
        /// <returns>The provider ID.</returns>             
        public int GetProviderID(string providerName, bool strict = false){                
            if(string.IsNullOrEmpty(providerName)) throw new ArgumentNullException(providerName);

            string filter = (strict ? string.Format("pro.name='{0}'", providerName) :  GetNonStrictWhere("pro.name", providerName));               
            var com = GetProviderData(filter, "pro.name");
            return (com.Rows.Count == 0 ? 0 : (int)com.Rows[0]["id"]);
        }
        
        /// <summary>
        /// Requests for the provider data.
        /// </summary>
        /// <param name="providerName">The provider name wich will be used to request.</param>
        /// <returns>The provider data (all the 'res.partner' table fields, logo).</returns>
        public DataTable GetProviderData(string providerName){    
            if(string.IsNullOrEmpty(providerName)) throw new ArgumentNullException(providerName);
            return GetProviderData(string.Format("pro.name='{0}'", providerName), "pro.name");
        }
        
        /// <summary>
        /// Requests for the provider data.
        /// </summary>
        /// <param name="providerID">The provider ID wich will be used to request.</param>
        /// <returns>The provider data (all the 'res.partner' table fields, logo).</returns>
        public DataTable GetProviderData(int providerID){    
            if(providerID < 1) throw new ArgumentOutOfRangeException("providerID", providerID, "Must be an number greater than 0.");
            return GetProviderData(string.Format("pro.id={0}", providerID), "pro.id");
        } 

        private DataTable GetProviderData(string filter, string order){  
            if(string.IsNullOrEmpty(filter)) filter = "1=1";        
            if(string.IsNullOrEmpty(order)) order = "1";

            return ExecuteQuery(string.Format(@"  
               SELECT pro.*, (ata.file_size IS NOT NULL) AS logo 
                FROM public.res_partner pro
                    LEFT JOIN public.ir_attachment ata ON ata.res_id = pro.id AND res_model = 'res.partner' AND res_field='image'
                WHERE pro.parent_id IS NULL AND pro.supplier = TRUE AND pro.company_id={0} AND {1}
                ORDER BY {2} DESC", this.CompanyID, filter, order)
            ).Tables[0];
        } 
#endregion
#region "Products"
        /// <summary>
        /// Requests for the product template ID.
        /// </summary>
        /// <param name="productName">The product name wich will be used to request.</param>
        /// <param name="strict">When strict is on, the product name match will be exact.</param>
        /// <returns>The product template ID.</returns>
        public int GetProductTemplateID(string productName, bool strict = false){    
            if(string.IsNullOrEmpty(productName)) throw new ArgumentNullException(productName);

            string filter = (strict ? string.Format("tpl.name='{0}'", productName) :  GetNonStrictWhere("tpl.name", productName));               
            var com = GetProductTemplateData(filter, "tpl.name");
            return (com.Rows.Count == 0 ? 0 : (int)com.Rows[0]["template_id"]);
        }  
        
        /// <summary>
        /// Requests for the product template data, so all the product data (including variants) can be retrieved.
        /// </summary>
        /// <param name="productName">The product name wich will be used to request.</param>
        /// <returns>The product template data, including variants (product_id, template_id, name, type, sell_price, purchase_price, supplier_id, file_size, attribute, value).</returns>           
        public DataTable GetProductTemplateData(string productName){   
             if(string.IsNullOrEmpty(productName)) throw new ArgumentNullException(productName); 
             return GetProductTemplateData(string.Format("tpl.name='{0}'", productName), "tpl.name");  
        }   
        
        /// <summary>
        /// Requests for the product template data, so all the product data (including variants) can be retrieved.
        /// </summary>
        /// <param name="templateID">The product template ID wich will be used to request.</param>
        /// <returns>The product template data, including variants (product_id, template_id, name, type, sell_price, purchase_price, supplier_id, file_size, attribute, value).</returns>           
        public DataTable GetProductTemplateData(int templateID){    
            if(templateID < 1) throw new ArgumentOutOfRangeException("templateID", templateID, "Must be an number greater than 0.");
            return GetProductTemplateData(string.Format("tpl.id={0}", templateID), "tpl.id");                   
        }
       
        private DataTable GetProductTemplateData(string filter, string order){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed    
            if(string.IsNullOrEmpty(filter)) filter = "1=1";     
            if(string.IsNullOrEmpty(order)) order = "1";

            return ExecuteQuery(string.Format(@"
                SELECT pro.id AS product_id, tpl.id AS template_id, tpl.name, tpl.type, tpl.list_price AS sell_price, sup.price AS purchase_price, sup.name AS supplier_id, ata.file_size, att.name AS attribute, val.name AS value 
                FROM public.product_product pro
                    LEFT JOIN public.product_template tpl ON tpl.id = pro.product_tmpl_id
                    LEFT JOIN public.ir_attachment ata ON ata.res_id=tpl.id AND ata.res_model='product.template' AND ata.res_id = tpl.id  AND ata.res_field='image'
                    LEFT JOIN public.product_attribute_value_product_product_rel rel ON rel.product_product_id = pro.id
                    LEFT JOIN public.product_attribute_value val ON val.id = rel.product_attribute_value_id
                    LEFT JOIN public.product_attribute att ON att.id = val.attribute_id
                    LEFT JOIN public.product_supplierinfo sup ON sup.product_id = pro.id
                WHERE tpl.company_id={0} AND {1} ORDER BY {2} DESC", this.CompanyID, filter, order)
            ).Tables[0];            
        }
#endregion
#region "Purchases"
        /// <summary>
        /// Requests for the last purchase ID (the higher ID).
        /// </summary>
        /// <returns>The last purchase ID.</returns>
        public int GetLastPurchaseID(){   
            var result = GetPurchaseData(null, "h.id"); 
            return (result.Rows.Count == 0 ? 0 : (int)result.Rows[0]["id"]);     
        }
        
        /// <summary>
        /// Requests for the purchase ID.
        /// </summary>
        /// <param name="purchaseCode">The purchase code wich will be used to request.</param>
        /// <returns>The purchase ID.</returns>
        public int GetPurchaseID(string purchaseCode){  
            var result = GetPurchaseData(purchaseCode);
            return (result.Rows.Count == 0 ? 0 : (int)result.Rows[0]["id"]);
        }
        
        /// <summary>
        /// Requests for the purchase code.
        /// </summary>
        /// <param name="purchaseID">The purchase ID wich will be used to request.</param>
        /// <returns>The purchase ID.</returns>
        public string GetPurchaseCode(int purchaseID){ 
            var result = GetPurchaseData(purchaseID);
            return (result.Rows.Count == 0 ? null : result.Rows[0]["code"].ToString());
        } 
        
        /// <summary>
        /// Requests for the purchase data, header and lines
        /// </summary>
        /// <param name="purchaseCode">The purchase code wich will be used to request.</param>
        /// <returns>The purchase data (id, code, amount_total, product_name, product_qty, product_price_unit, product_id)</returns>
        public DataTable GetPurchaseData(string purchaseCode){    
            if(string.IsNullOrEmpty(purchaseCode)) throw new ArgumentNullException(purchaseCode); 
            return GetPurchaseData(string.Format("h.name='{0}'", purchaseCode), "h.name");  
        }

        /// <summary>
        /// Requests for the purchase data, header and lines
        /// </summary>
        /// <param name="purchaseID">The purchase ID wich will be used to request.</param>
        /// <returns>The purchase data (id, code, amount_total, product_name, product_qty, product_price_unit, product_id)</returns>
        public DataTable GetPurchaseData(int purchaseID){  
            if(purchaseID < 1) throw new ArgumentOutOfRangeException("purchaseID", purchaseID, "Must be an number greater than 0.");   
            return GetPurchaseData(string.Format("h.id={0}", purchaseID), "h.id");         
        } 
       
        private DataTable GetPurchaseData(string filter, string order){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            if(string.IsNullOrEmpty(filter)) filter = "1=1";
            if(string.IsNullOrEmpty(order)) order = "1";

            return ExecuteQuery(string.Format(@"
                SELECT h.id, h.name AS code, h.amount_total, l.name AS product_name, l.product_qty, l.price_unit AS product_price_unit, l.product_id
                FROM public.purchase_order h
                    LEFT JOIN public.purchase_order_line l ON l.order_id=h.id
                WHERE h.company_id={0} AND {1} ORDER BY {2} DESC", this.CompanyID, filter, order)
            ).Tables[0];            
        }
#endregion
#region "Sales"   
        /// <summary>
        /// Requests for the last (higher) sale ID.
        /// </summary>
        /// <returns>The last sale ID.</returns>
        public int GetLastSaleID(){    
            var result = GetSaleData(null, "h.id"); 
            return (result.Rows.Count == 0 ? 0 : (int)result.Rows[0]["id"]);   
        }
        
        /// <summary>
        /// Requests for the sale ID.
        /// </summary>
        /// <param name="saleCode">The sale code wich will be used to request.</param>
        /// <returns>The sale ID.</returns>
        public int GetSaleID(string saleCode){    
            var result = GetSaleData(saleCode);
            return (result.Rows.Count == 0 ? 0 : (int)result.Rows[0]["id"]);
        }
        
        /// <summary>
        /// Requests for the sale code.
        /// </summary>
        /// <param name="saleID">The sale ID wich will be used to request.</param>
        /// <returns>The sale code.</returns>
        public string GetSaleCode(int saleID){    
            var result = GetSaleData(saleID);
            return (result.Rows.Count == 0 ? null : result.Rows[0]["code"].ToString());
        } 
        
        /// <summary>
        /// Requests for the sale data, header and lines.
        /// </summary>
        /// <param name="saleCode">The sale code wich will be used to request.</param>
        /// <returns>The sale data (id, code, amount_total, product_name, product_qty, product_price_unit, product_id).</returns>
        public DataTable GetSaleData(string saleCode){  
            if(string.IsNullOrEmpty(saleCode)) throw new ArgumentNullException(saleCode);              
            return GetSaleData(string.Format("h.name='{0}'", saleCode), "h.name");
        }
        
        /// <summary>
        /// Requests for the sale data, header and lines.
        /// </summary>
        /// <param name="saleID">The sale ID wich will be used to request.</param>
        /// <returns>The sale data (id, code, amount_total, product_name, product_qty, product_price_unit, product_id).</returns>
        public DataTable GetSaleData(int saleID){    
            if(saleID < 1) throw new ArgumentOutOfRangeException("saleID", saleID, "Must be an number greater than 0.");   
            return GetSaleData(string.Format("h.id={0}", saleID), "h.id");    
        } 
        
        private DataTable GetSaleData(string filter, string order){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            if(string.IsNullOrEmpty(filter)) filter = "1=1";
            if(string.IsNullOrEmpty(order)) order = "1";
          
            return ExecuteQuery(string.Format(@"
                SELECT h.id, h.name AS code, h.amount_total, l.name AS product_name, l.product_uom_qty as product_qty, l.price_unit AS product_price_unit, l.product_id
                FROM public.sale_order h
                    LEFT JOIN public.sale_order_line l ON l.order_id=h.id
                WHERE h.company_id={0} AND {1} ORDER BY {2} DESC", this.CompanyID, filter, order)
            ).Tables[0];  
        }
#endregion 
#region "POS"
        /// <summary>
        /// Requests for the last (higher) Point Of Sale sale ID.
        /// </summary>
        /// <returns>The last POS sale ID (id, code, amount_total, product_name, product_qty, product_price_unit, product_id).</returns>
        public int GetLastPosSaleID(){    
            var result = GetPosSaleData(null, "h.id"); 
            return (result.Rows.Count == 0 ? 0 : (int)result.Rows[0]["id"]);   
        }
        
        /// <summary>
        /// Requests for the Point Of Sale sale ID.
        /// </summary>
        /// <param name="posSaleCode">The POS sale code wich will be used to request.</param>
        /// <returns>The POS sale ID (id, code, amount_total, product_name, product_qty, product_price_unit, product_id).</returns>
        public int GetPosSaleID(string posSaleCode){    
            var result = GetPosSaleData(posSaleCode);
            return (result.Rows.Count == 0 ? 0 : (int)result.Rows[0]["id"]);
        }
        
        /// <summary>
        /// Requests for the Point Of Sale sale code.
        /// </summary>
        /// <param name="posSaleID">The POS sale ID wich will be used to request.</param>
        /// <returns>The POS sale code.</returns>
        public string GetPosSaleCode(int posSaleID){    
            var result = GetPosSaleData(posSaleID);
            return (result.Rows.Count == 0 ? null : result.Rows[0]["code"].ToString());
        } 
        
        /// <summary>
        /// Requests for the Point Of Sale sale data, header and lines.
        /// </summary>
        /// <param name="saleCode">The POS sale code wich will be used to request.</param>
        /// <returns>The POS sale data.</returns>
        public DataTable GetPosSaleData(string posSaleCode){  
            if(string.IsNullOrEmpty(posSaleCode)) throw new ArgumentNullException(posSaleCode);              
            return GetPosSaleData(string.Format("h.name='{0}'", posSaleCode), "h.name");
        }
        
        /// <summary>
        /// Requests for the Point Of Sale sale data, header and lines.
        /// </summary>
        /// <param name="saleID">The POS sale ID wich will be used to request.</param>
        /// <returns>The POS sale data.</returns>
        public DataTable GetPosSaleData(int posSaleID){    
            if(posSaleID < 1) throw new ArgumentOutOfRangeException("posSaleID", posSaleID, "Must be an number greater than 0.");   
            return GetPosSaleData(string.Format("h.id={0}", posSaleID), "h.id");    
        } 
        
        private DataTable GetPosSaleData(string filter, string order){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            if(string.IsNullOrEmpty(filter)) filter = "1=1";
            if(string.IsNullOrEmpty(order)) order = "1";

            return ExecuteQuery(string.Format(@"
                SELECT h.id, h.name AS code, h.state, l.product_id, l.qty as product_qty, l.price_unit AS product_price_unit, {0} 
                FROM public.pos_order h
                    LEFT JOIN public.pos_order_line l ON l.order_id = h.id
                    {1}
                WHERE h.company_id={2} AND {3} ORDER BY {4} DESC", GetProductDataName(), GetProductDataJoin("l.product_id"), this.CompanyID, filter, order)
            ).Tables[0];            
        }
#endregion  
#region "Stock"
        /// <summary>
        /// Requests for the stock movement data, only headers.
        /// </summary>
        /// <param name="orderCode">The order code (or number) wich will be used to request.</param>
        /// <param name="isReturn">If true, the stock movement is related with a return.</param>
        /// <returns>The stock movement data (id, product_id, product_name, product_qty, location_id, state).</returns>
        public DataTable GetStockMovementData(string orderCode, bool isReturn){    
            if(string.IsNullOrEmpty(orderCode)) throw new ArgumentNullException(orderCode); 

            bool input = orderCode.StartsWith("PO");
            if(isReturn) input = !input; 

            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            return ExecuteQuery(string.Format(CultureEN, @"
                SELECT id, product_id, name as product_name, product_qty, location_id, state
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
#endregion
#region "Invoices"
        /// <summary>
        /// Requests for the invoice code.
        /// </summary>
        /// <param name="orderCode">The order code, related to the invoice, wich will be used to request.</param>
        /// <returns>The invoice data.</returns>
        public int GetInvoiceID(string orderCode){   
            var result = GetInvoiceData(orderCode);
            return (result.Rows.Count == 0 ? 0 : (int)result.Rows[0]["id"]);
        }

        /// <summary>
        /// Requests for the invoice code.
        /// </summary>
        /// <param name="orderCode">The order code, related to the invoice, wich will be used to request.</param>
        /// <returns>The invoice data.</returns>
        public string GetInvoiceCode(string orderCode){   
            var result = GetInvoiceData(orderCode);
            return (result.Rows.Count == 0 ? null : result.Rows[0]["number"].ToString());
        } 

        /// <summary>
        /// Requests for the purchase data, header and lines
        /// </summary>
        /// <param name="invoiceID">The invoiceID ID wich will be used to request.</param>
        /// <returns>The purchase data.</returns>
        public DataTable GetInvoiceData(int invoiceID){  
            if(invoiceID < 1) throw new ArgumentOutOfRangeException("invoiceID", invoiceID, "Must be an number greater than 0.");   
            return GetInvoiceData(string.Format("id={0}", invoiceID), "id");         
        }

        /// <summary>
        /// Requests for the invoice data, headers only
        /// </summary>
        /// <param name="orderCode">The order code, related to the invoice, wich will be used to request.</param>
        /// <returns>The invoice data.</returns>
        public DataTable GetInvoiceData(string orderCode){    
            if(string.IsNullOrEmpty(orderCode)) throw new ArgumentNullException(orderCode);             

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

            return GetInvoiceData(string.Format("origin='{0}' AND type='{1}'", orderCode, type), "origin");  
        } 
       
        private DataTable GetInvoiceData(string filter, string order){  
            if(string.IsNullOrEmpty(filter)) filter = "1=1";
            if(string.IsNullOrEmpty(order)) order = "1";

            return ExecuteQuery(string.Format(@"
                SELECT *
                FROM public.account_invoice                    
                WHERE company_id={0} AND {1} ORDER BY {2} DESC", this.CompanyID, filter, order)
            ).Tables[0]; 
        }
#endregion     
#region "Users"             
        /// <summary>
        /// Requests for the user ID.
        /// </summary>
        /// <param name="userName">The user name wich will be used to request.</param>
        /// <param name="strict">When strict is on, the user name match will be exact.</param>
        /// <returns>The user ID.</returns>
        public int GetUserID(string userName, bool strict = false){   
            if(string.IsNullOrEmpty(userName)) throw new ArgumentNullException(userName);

            string filter = (strict ? string.Format("u.login='{0}'", userName) :  GetNonStrictWhere("u.login", userName));               
            var com = GetUserData(filter, "u.login");
            return (com.Rows.Count == 0 ? 0 : (int)com.Rows[0]["id"]);
        }

        /// <summary>
        /// Requests for the user name.
        /// </summary>
        /// <param name="userID">The user ID wich will be used to request.</param>
        /// <returns>The user ID.</returns>
        public string GetUserName(int userID){    
            var result = GetUserData(userID);
            return (result.Rows.Count == 0 ? null : result.Rows[0]["name"].ToString());
        }

        /// <summary>
        /// Requests for the user data, including the groups and permissions
        /// </summary>
        /// <param name="userName">The user name wich will be used to request.</param>
        /// <returns>The user data (id, name, active, group).</returns>
        public DataTable GetUserData(string userName){    
            if(string.IsNullOrEmpty(userName)) throw new ArgumentNullException(userName);              
            return GetUserData(string.Format("u.login='{0}'", userName), "u.login");
        }

        /// <summary>
        /// Requests for the user data, including the groups and permissions
        /// </summary>
        /// <param name="userID">The user ID wich will be used to request.</param>
        /// <returns>The user data (id, name, active, group).</returns>
        public DataTable GetUserData(int userID){    
            if(userID < 1) throw new ArgumentOutOfRangeException("saleID", userID, "Must be an number greater than 0.");   
            return GetUserData(string.Format("u.id={0}", userID), "u.id");               
        }

        private DataTable GetUserData(string filter, string order){    
            //Note: aliases are needed, so no '*' is loaded... modify the query if new fields are needed
            if(string.IsNullOrEmpty(filter)) filter = "1=1";
            if(string.IsNullOrEmpty(order)) order = "1";

            return ExecuteQuery(string.Format(@"
                SELECT u.id, u.login as name, u.active, g.name AS group
                FROM public.res_users u
                    LEFT JOIN public.res_groups_users_rel r ON r.uid = u.id
                    INNER JOIN public.res_groups g ON r.gid = g.id                    
                WHERE u.company_id={0} AND {1} ORDER BY {2} DESC", this.CompanyID, filter, order)
            ).Tables[0];            
        }
#endregion
#region "Auxiliar methods"
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
        
        private string GetProductDataJoin(string productIdField){
            //Must be used always in addition to GetProductDataName()
            //Returns the product name as used in some tables (like stock one): [REFERENCE] PRODUCT (VARIANTS)
            return string.Format(@" 
                LEFT JOIN public.product_product pro ON pro.id = {0}
                LEFT JOIN public.product_template tpl ON tpl.id = pro.product_tmpl_id
                LEFT JOIN (
			        SELECT pro.id, string_agg(val.name, ', ') AS attributes
                    FROM public.product_product pro 
                        LEFT JOIN public.product_template tpl ON tpl.id = pro.product_tmpl_id
                        LEFT JOIN public.product_attribute_value_product_product_rel rel ON rel.product_product_id = pro.id
                        LEFT JOIN public.product_attribute_value val ON val.id = rel.product_attribute_value_id
                    GROUP BY 1
                ) t ON t.id = pro.id", productIdField);
        }
        
        private string GetProductDataName(){  
            //Must be used always in addition to GetProductDataJoin()
            //Returns the product name as used in some tables (like stock one): [REFERENCE] PRODUCT (VARIANTS)
            //TODO: add translations, it's not desplayed as has been stored into a database
            return @"(CASE 
                        WHEN pro.default_code IS NOT NULL AND t.attributes IS NOT NULL 
                            THEN CONCAT('[', pro.default_code, '] ', tpl.name, ' (', t.attributes, ')')
                        WHEN pro.default_code IS NULL AND t.attributes IS NOT NULL 
                            THEN CONCAT(tpl.name, ' (', t.attributes, ')')
                        WHEN pro.default_code IS NOT NULL AND t.attributes IS NULL 
                            THEN CONCAT('[', pro.default_code, '] ', tpl.name)
                        ELSE
                            tpl.name
                    END) AS product_name";
        }
    }
#endregion
}