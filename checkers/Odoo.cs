
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
using System.Linq;
using System.Collections.Generic;
using AutoCheck.Core;

namespace AutoCheck.Checkers{    
    /// <summary>
    /// Allows data validations over an Odoo instance.
    /// </summary>
    public class Odoo : Postgres{  
#region "Attributes"
        /// <summary>
        /// The main connector, can be used to perform direct operations over the data source.
        /// </summary>
        /// <value></value>
        public new Connectors.Odoo Connector {get; private set;}
        
        /// <summary>
        /// The current company ID that will be used to acces and filter all the requested data.
        /// </summary>
        /// <value></value>
        public int CompanyID  {
            get{
                return this.Connector.CompanyID;
            }

            set{
                this.Connector.CompanyID = value;
            }
        }
        
        /// <summary>
        /// The current company name.
        /// </summary>
        /// <value></value>
        public string CompanyName  {
            get{
                return this.Connector.CompanyName;
            }

            set{
                this.Connector.CompanyName = value;
            }
        }
#endregion
#region "Constructor / Destructor"              
        /// <summary>
        /// Creates a new checker instance.
        /// </summary>
        /// <param name="companyID">The company ID which will be used to operate.</param>
        /// <param name="host">Host address in order to connect with the running PostgreSQL service, wich contains the Odoo database.</param>
        /// <param name="database">The Odoo database name.</param>
        /// <param name="username">The Odoo database username, which will be used to perform operations.</param>
        /// <param name="password">The Odoo database password, which will be used to perform operations.</param>
        public Odoo(string companyName, string host, string database, string username, string password): base(host, database, username, password){         
            this.Connector = new Connectors.Odoo(companyName, host, database, username, password);            
        }
        
        /// <summary>
        /// Creates a new checker instance.
        /// </summary>
        /// <param name="companyName">The company name which will be used to operate.</param>
        /// <param name="host">Host address in order to connect with the running PostgreSQL service, wich contains the Odoo database.</param>
        /// <param name="database">The Odoo database name.</param>
        /// <param name="username">The Odoo database username, which will be used to perform operations.</param>
        /// <param name="password">The Odoo database password, which will be used to perform operations.</param>
        /// <returns>A new instance.</returns>
        public Odoo(int companyID, string host, string database, string username, string password): base(host, database, username, password){         
            this.Connector = new Connectors.Odoo(companyID, host, database, username, password);            
        }
    
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
            base.Dispose();
            this.Connector.Dispose();            
        }
#endregion
#region "Company"        
        /// <summary>
        /// Compares if the given company data matches with the current one stored in the database.
        /// </summary>
        /// <param name="companyID">The company ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match, (all the 'res.company' table fields, logo).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfCompanyMatchesData(int companyID, Dictionary<string, object> expectedFields){    
            var errors = new List<string>();                        

            if(!Output.Instance.Disabled)  Output.Instance.Write(string.Format("Getting the company data for ~ID={0}... ", companyID), ConsoleColor.Yellow);            

            Output.Instance.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(this.Connector.GetCompanyData(companyID), expectedFields));
            Output.Instance.UndoStatus();

            return errors;
        }  
#endregion
#region "Providers"           
        /// <summary>
        /// Compares if the given provider data matches with the current one stored in the database.
        /// </summary>
        /// <param name="providerID">The provider ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match (all the 'res.partner' table fields, logo).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfProviderMatchesData(int providerID, Dictionary<string, object> expectedFields){    
            var errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the provider data for ~ID={0}... ", providerID), ConsoleColor.Yellow);            
            
            Output.Instance.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(this.Connector.GetProviderData(providerID), expectedFields));
            Output.Instance.UndoStatus();

            return errors;
        }
#endregion
#region "Products"               
        /// <summary>
        /// Compares if the given product data matches with the current one stored in the database.
        /// </summary>
        /// <param name="templateID">The product template ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <param name="expectedAttributes">The expected attribute as a collection of couples with [attribute, its attribute values].</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfProductMatchesData(int templateID, Dictionary<string, object> expectedFields, Dictionary<string, string[]> expectedAttributes = null){    
            //TODO: expectedAttributeValues wont work when using more than one attribute, a Dictionary must be used... no more time to implement.
            var errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the product data for ~ID={0}... ", templateID), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetProductTemplateData(templateID);                        
            errors.AddRange(this.CheckIfTableMatchesData(dt, expectedFields));

            //Only for variants
            if(expectedAttributes != null && expectedAttributes.Values.Count > 0){
                foreach(string attribute in expectedAttributes.Keys){
                    errors.AddRange(CheckAttributeValues(dt, attribute, expectedAttributes[attribute]));
                }
            }

            Output.Instance.UndoStatus();
            return errors;
        } 
#endregion
#region "Purchases"        
        /// <summary>
        /// Compares if the purchase data stored in the database contains the given data (within its header and lines).
        /// </summary>
        /// <param name="purchaseID">The purchase ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match (id, code, product_id, product_name, product_qty, product_price_unit, amount_total).</param>
        /// <param name="ignoreVariants">The variants or attribute values will be ignored, so it will be removed from the product name when comparing (meaning that all the variations over a product will be treated as the same).</param>
        /// <param name="ignoreInternalReference">The internal reference will be ignored, so it will be removed from the product name when comparing.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfPurchaseMatchesData(int purchaseID, Dictionary<string, object> expectedFields, bool ignoreVariants = true, bool ignoreInternalReference = true){                           
            return CheckIfPurchaseMatchesData(purchaseID, expectedFields, null, ignoreVariants, ignoreInternalReference);
        } 
       
        /// <summary>
        /// Compares if the purchase data stored in the database contains the given data (within its header and lines).
        /// </summary>
        /// <param name="purchaseID">The purchase ID that will be matched.</param>
        /// <param name="expectedCommonFields">The expected order's common data (without using product variants) to match (id, code, product_name, amount_total).</param>
        /// <param name="expectedAttributeFields">The expected order's attribute-related data to match as [comma separated list of used attribute values (exact match), [order line's field, order line's expected value]]; valid order line's fields are (product_id, product_qty, product_price_unit).</param>
        /// <param name="ignoreInternalReference">The internal reference will be ignored, so it will be removed from the product name when comparing.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfPurchaseMatchesData(int purchaseID, Dictionary<string, object> expectedCommonFields, Dictionary<string[], Dictionary<string, object>> expectedAttributeFields, bool ignoreInternalReference = true){                
            return CheckIfPurchaseMatchesData(purchaseID, expectedCommonFields, expectedAttributeFields, false, true);
        }               

        /// <summary>
        /// Compares if the purchase data stored in the database contains the given data.
        /// </summary>
        /// <param name="purchaseID">The purchase ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match (id, code, amount_total, product_name, product_qty, product_price_unit, product_id).</param>
        /// <param name="expectedAttributeQty">The expected amount of purchased product for each couple of [attribute value, qty] (sizes, colors, etc.).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        [Obsolete("CheckIfPurchaseMatchesData has been deprecated. Use other overloads instead")]
        public List<string> CheckIfPurchaseMatchesData(int purchaseID, Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty){    
            //TODO: strict option, not for includes but for exact match (expectedFields would be per line as an array).
            var errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the purchase data for ~ID={0}... ", purchaseID), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetPurchaseData(purchaseID);             
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));

            //Only for variants
            if(expectedAttributeQty != null && expectedAttributeQty.Values.Count > 0)
                errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.Instance.UndoStatus();
            
            return errors;
        } 
        
        private List<string> CheckIfPurchaseMatchesData(int purchaseID, Dictionary<string, object> expectedCommonFields, Dictionary<string[], Dictionary<string, object>> expectedAttributeFields, bool ignoreVariants, bool ignoreInternalReference){                           
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the purchase data for ~ID={0}... ", purchaseID), ConsoleColor.Yellow);                        
            
            Output.Instance.Disable();   //no output for native database checker wanted.                        
            var errors = CheckIfDataTableMatchesData(this.Connector.GetPurchaseData(purchaseID), expectedCommonFields, expectedAttributeFields, ignoreVariants, ignoreInternalReference);            
            Output.Instance.UndoStatus();            

            return errors;
        }
#endregion  
#region "Sales"        
        /// <summary>
        /// Compares if the given sale data matches with the current one stored in the database.
        /// </summary>
        /// <param name="saleID">The sale ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match (id, code, state, amount_total, product_id, prduct_name, product_price_unit, product_qty).</param>
        /// <param name="ignoreVariants">The variants or attribute values will be ignored, so it will be removed from the product name when comparing (meaning that all the variations over a product will be treated as the same).</param>
        /// <param name="ignoreInternalReference">The internal reference will be ignored, so it will be removed from the product name when comparing.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfSaleMatchesData(int saleID, Dictionary<string, object> expectedFields, bool ignoreVariants = true, bool ignoreInternalReference = true){                           
            return CheckIfSaleMatchesData(saleID, expectedFields, null, ignoreVariants, ignoreInternalReference);
        } 
       
        /// <summary>
        /// Compares if the given sale data matches with the current one stored in the database.
        /// </summary>
        /// <param name="saleID">The sale ID that will be matched.</param>
        /// <param name="expectedCommonFields">The expected order's common data (without using product variants) to match (id, code, state, amount_total, product_id, prduct_name, product_price_unit, product_qty).</param>
        /// <param name="expectedAttributeFields">The expected order's attribute-related data to match as [comma separated list of used attribute values (exact match), [order line's field, order line's expected value]]; valid order line's fields are (product_id, product_qty, product_price_unit).</param>
        /// <param name="ignoreInternalReference">The internal reference will be ignored, so it will be removed from the product name when comparing.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfSaleMatchesData(int saleID, Dictionary<string, object> expectedCommonFields, Dictionary<string[], Dictionary<string, object>> expectedAttributeFields, bool ignoreInternalReference = true){                
            return CheckIfSaleMatchesData(saleID, expectedCommonFields, expectedAttributeFields, false, true);
        }               

        /// <summary>
        /// Compares if the given sale data matches with the current one stored in the database.
        /// </summary>
        /// <param name="saleID">The sale ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <param name="expectedAttributeQty">The expected amount of purchased product for each attribute value [name, qty] (sizes, colors, etc.).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        [Obsolete("CheckIfPurchaseMatchesData has been deprecated. Use other overloads instead")]
        public List<string> CheckIfSaleMatchesData(int saleID, Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty){    
            var errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the backoffice sale data for ~ID={0}... ", saleID), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetSaleData(saleID);                        
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.Instance.UndoStatus();
            
            return errors;
        }
        
        private List<string> CheckIfSaleMatchesData(int saleID, Dictionary<string, object> expectedCommonFields, Dictionary<string[], Dictionary<string, object>> expectedAttributeFields, bool ignoreVariants, bool ignoreInternalReference){                           
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the purchase data for ~ID={0}... ", saleID), ConsoleColor.Yellow);                        
            
            Output.Instance.Disable();   //no output for native database checker wanted.                        
            var errors = CheckIfDataTableMatchesData(this.Connector.GetSaleData(saleID), expectedCommonFields, expectedAttributeFields, ignoreVariants, ignoreInternalReference);            
            Output.Instance.UndoStatus();            

            return errors;
        }
#endregion 
#region "Stock"      
        /// <summary>
        /// Compares if the given order data matches with the current one stored in the database.
        /// </summary>
        /// <param name="orderCode">The order code that will be matched.</param>
        /// <param name="isReturn">If true, the order must be treated as a return.</param>
        /// <param name="expectedFields">The expected data to match (id, product_id, product_name, product_qty, location_id, state).</param>
        /// <param name="expectedAttributeQty">The expected amount of purchased product for each attribute value [name, qty] (sizes, colors, etc.).</param>
        /// <param name="ignoreVariants">The variants or attribute values will be ignored, so it will be removed from the product name when comparing (meaning that all the variations over a product will be treated as the same).</param>
        /// <param name="ignoreInternalReference">The internal reference will be ignored, so it will be removed from the product name when comparing.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfStockMovementMatchesData(string orderCode, bool isReturn, Dictionary<string, object> expectedFields, bool ignoreVariants = true, bool ignoreInternalReference = true){
            return CheckIfStockMovementMatchesData(orderCode, isReturn, expectedFields, null, ignoreVariants, ignoreInternalReference);                        
        }

        /// <summary>
        /// Compares if the given order data matches with the current one stored in the database.
        /// </summary>
        /// <param name="orderCode">The order code that will be matched.</param>
        /// <param name="isReturn">If true, the order must be treated as a return.</param>
        /// <param name="expectedCommonFields">The expected order's common data (without using product variants) to match (id, product_id, product_name, product_qty, location_id, state).</param>
        /// <param name="expectedAttributeFields">The expected order's attribute-related data to match as [comma separated list of used attribute values (exact match), [order line's field, order line's expected value]]; valid order line's fields are (product_id, product_qty, product_price_unit).</param>
        /// <param name="ignoreInternalReference">The internal reference will be ignored, so it will be removed from the product name when comparing.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfStockMovementMatchesData(string orderCode, bool isReturn,  Dictionary<string, object> expectedCommonFields, Dictionary<string[], Dictionary<string, object>> expectedAttributeFields, bool ignoreInternalReference = true){
            return CheckIfStockMovementMatchesData(orderCode, isReturn, expectedCommonFields, expectedAttributeFields, false, true);                            
        }

        /// <summary>
        /// Compares if the given order data matches with the current one stored in the database.
        /// </summary>
        /// <param name="orderCode">The order code that will be matched.</param>
        /// <param name="isReturn">If true, the order must be treated as a return.</param>
        /// <param name="expectedFields">The expected data to match (id, name as product_name, product_qty, location_id, state).</param>
        /// <param name="expectedAttributeQty">The expected amount of purchased product for each attribute value [name, qty] (sizes, colors, etc.).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        [Obsolete("CheckIfStockMovementMatchesData has been deprecated. Use other overloads instead")]
        public List<string> CheckIfStockMovementMatchesData(string orderCode, bool isReturn, Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty){
            var errors = new List<string>();
            
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the stock movement data for the order ~{0}... ", orderCode), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetStockMovementData(orderCode, isReturn);
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.Instance.UndoStatus();                         
            return errors;                             
        }

        private List<string> CheckIfStockMovementMatchesData(string orderCode, bool isReturn, Dictionary<string, object> expectedCommonFields, Dictionary<string[], Dictionary<string, object>> expectedAttributeFields, bool ignoreVariants, bool ignoreInternalReference){                
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the stock movement data for the order ~{0}... ", orderCode), ConsoleColor.Yellow);                        
            
            Output.Instance.Disable();   //no output for native database checker wanted.                        
            var errors = CheckIfDataTableMatchesData(this.Connector.GetStockMovementData(orderCode, isReturn), expectedCommonFields, expectedAttributeFields, ignoreVariants, ignoreInternalReference);            
            Output.Instance.UndoStatus();            

            return errors;        
        }

        /// <summary>
        /// Compares if the given scrapped stock movement data matches with the current one stored in the database.
        /// </summary>
        /// <param name="expectedFields">The expected data to match (id, product_id, product_name, product_qty, location_id, state).</param>
        /// <param name="expectedAttributeQty">The expected amount of purchased product for each attribute value [name, qty] (sizes, colors, etc.).</param>
        /// <param name="ignoreVariants">The variants or attribute values will be ignored, so it will be removed from the product name when comparing (meaning that all the variations over a product will be treated as the same).</param>
        /// <param name="ignoreInternalReference">The internal reference will be ignored, so it will be removed from the product name when comparing.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfScrappedStockMatchesData(Dictionary<string, object> expectedFields, bool ignoreVariants = true, bool ignoreInternalReference = true){
            return CheckIfScrappedStockMatchesData(expectedFields, null, ignoreVariants, ignoreInternalReference);                        
        }

        /// <summary>
        /// Compares if the given scrapped stock movement data matches with the current one stored in the database.
        /// </summary>
        /// <param name="expectedCommonFields">The expected order's common data (without using product variants) to match (id, product_id, product_name, product_qty, location_id, state).</param>
        /// <param name="expectedAttributeFields">The expected order's attribute-related data to match as [comma separated list of used attribute values (exact match), [order line's field, order line's expected value]]; valid order line's fields are (product_id, product_qty, product_price_unit).</param>
        /// <param name="ignoreInternalReference">The internal reference will be ignored, so it will be removed from the product name when comparing.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfScrappedStockMatchesData(Dictionary<string, object> expectedCommonFields, Dictionary<string[], Dictionary<string, object>> expectedAttributeFields, bool ignoreInternalReference = true){
            return CheckIfScrappedStockMatchesData(expectedCommonFields, expectedAttributeFields, false, true);                            
        }

        private List<string> CheckIfScrappedStockMatchesData(Dictionary<string, object> expectedCommonFields, Dictionary<string[], Dictionary<string, object>> expectedAttributeFields, bool ignoreVariants, bool ignoreInternalReference){                
            if(!Output.Instance.Disabled) Output.Instance.Write("Getting the scrapped stock data... ");
            
            Output.Instance.Disable();   //no output for native database checker wanted.                        
            var errors = CheckIfDataTableMatchesData(this.Connector.GetScrappedStockData(), expectedCommonFields, expectedAttributeFields, ignoreVariants, ignoreInternalReference);            
            Output.Instance.UndoStatus();            

            return errors;        
        }

        /// <summary>
        /// Compares if the given scrapped stock movement data matches with the current one stored in the database.
        /// </summary>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <param name="expectedAttributeQty">The expected amount of purchased product for each attribute value [name, qty] (sizes, colors, etc.).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        [Obsolete("CheckIfStockMovementMatchesData has been deprecated. Use other overloads instead")]
        public List<string> CheckIfScrappedStockMatchesData(Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty = null){
            var errors = new List<string>();
            
            if(!Output.Instance.Disabled) Output.Instance.Write("Getting the scrapped stock data... ");                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetScrappedStockData();
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.Instance.UndoStatus();                         
            return errors;                             
        }
#endregion
        /// <summary>
        /// Compares if the given invoice data matches with the current one stored in the database.
        /// </summary>
        /// <param name="orderCode">The order code that will be matched.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfInvoiceMatchesData(string orderCode, Dictionary<string, object> expectedFields){
            var errors = new List<string>();
            
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the invoice data for the order ~{0}... ", orderCode), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetInvoiceData(orderCode);
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));           

            Output.Instance.UndoStatus();                                      
            return errors;          
        } 
        /// <summary>
        /// Compares if the given Point Of Sale sale data matches with the current one stored in the database.
        /// </summary>
        /// <param name="posSaleID">The POS sale ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <param name="expectedAttributeQty">The expected amount of purchased product for each attribute value [name, qty] (sizes, colors, etc.).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfPosSaleMatchesData(int posSaleID, Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty = null){    
            var errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the POS sale data for ~ID={0}... ", posSaleID), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetPosSaleData(posSaleID);                        
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.Instance.UndoStatus();
            
            return errors;
        } 
        
        /// <summary>
        /// Compares if the given user data matches with the current one stored in the database.
        /// </summary>
        /// <param name="userID">The user ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <param name="expectedGroups">The expected groups where the user should belongs.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfUserMatchesData(int userID, Dictionary<string, object> expectedFields, string[] expectedGroups = null){    
            var errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the user data for ~ID={0}... ", userID), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetUserData(userID);                        
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckItemValues(dt, "group", "group", expectedGroups));

            Output.Instance.UndoStatus();
            
            return errors;
        }
        private List<string> CheckItemValues(DataTable dt, string caption, string field, string[] values){            
            //All the values must be present within field
            //  1 single field -> Multiple possible values
            var errors = new List<string>();
            Dictionary<string, bool> found = values.ToDictionary(x => x, x => false);
            if(values != null){                
                foreach(DataRow dr in dt.Rows){
                    string key = dr[field].ToString().Trim();
                    if(values.Contains(key)) found[key] = true;
                    else errors.Add(String.Format("Unexpected {0} '{1} {2}' found.", caption, dr["value"].ToString(), dr["attribute"]));
                }

                foreach(string key in found.Keys){
                    if(!found[key]) errors.Add(String.Format("Unable to find the {0} '{1}'.", caption, key));
                }
            }

            return errors;
        }
        private List<string> CheckAttributeValues(DataTable dt, string attribute, string[] values){            
            //All the values must be present within the attribute
            //  2 fields (attribute name + attribute value) -> 1 single attribute name -> Multiple attribute values to check
            var errors = new List<string>();
            Dictionary<string, bool> found = values.ToDictionary(x => x, x => false);
            if(values != null){                
                foreach(var dr in dt.Select(string.Format("attribute='{0}'", attribute))){
                    string value = dr["value"].ToString().Trim();
                    if(values.Contains(value)) found[value] = true;
                    else errors.Add(String.Format("Unexpected attribute '{0} {1}' found.", attribute, value));
                }

                foreach(string key in found.Keys){
                    if(!found[key]) errors.Add(String.Format("Unable to find the attribute '{0} {1}'.", attribute, key));
                }
            }

            return errors;
        }
        private List<string> CheckAttributeQuantities(DataTable dt, Dictionary<string, int> attributeQty){
            //Checks the amount of variants within a purchase or sale
            var errors = new List<string>();
            Dictionary<string, bool> found = attributeQty.Keys.ToDictionary(x => x, x => false);

            if(attributeQty != null){                
                foreach(DataRow dr in dt.Rows){
                    string variant = dr["product_name"].ToString();
                    if(variant.Contains("(")){
                        variant = variant.Substring(variant.IndexOf("(")+1);
                        variant = variant.Substring(0, variant.IndexOf(")")).Replace(" ", "");
                    }                    

                    if(!attributeQty.ContainsKey(variant)) errors.Add(String.Format("Unexpected product '{0}' found.", dr["product_name"]));
                    else{
                        if(attributeQty[variant] != (int)(decimal)dr["product_qty"]) errors.Add(String.Format("Unexpected quantity found for the product '{0}': expected->'{1}' found->'{2}'.", dr["product_name"], attributeQty[variant],  dr["product_qty"]));                    
                        found[variant] = true;
                    } 
                }

                foreach(string key in found.Keys){
                    if(!found[key]) errors.Add(String.Format("Unable to find the quantity for the attribute value '{0}'.", key));
                }
            }

            return errors;
        }
        private string GetWhereForName(string name, string dbField){
            string company = name;
            company = company.Replace(this.Student, "").Trim();
            string[] student = this.Student.Split(" ");

            return string.Format("{3} like '{0}%' AND {3} like '%{1}%' AND {3} like '%{2}%'", company, student[0], student[1], dbField);
        }               
        
 #region "Auxiliar methods"       
        private List<string> CheckIfDataTableMatchesData(DataTable dt, Dictionary<string, object> expectedCommonFields, Dictionary<string[], Dictionary<string, object>> expectedAttributeFields, bool ignoreVariants, bool ignoreInternalReference){                
            if(expectedCommonFields == null || expectedCommonFields.Keys.Count == 0) throw new ArgumentNullException("expectedCommonFields");
            
            var name = string.Empty;
            var errors = new List<string>();                                         
            var explicitVariants = (expectedAttributeFields != null && expectedAttributeFields.Keys.Count > 0);

            if(expectedCommonFields.ContainsKey("product_name")){
                foreach(DataRow dr in dt.Rows){
                    //removing extra info (only included on sales)
                    name = dr["product_name"].ToString();
                    if(name.Contains("\n")) name = name.Substring(0, name.IndexOf("\n"));

                    if(ignoreInternalReference && name.StartsWith("[")){
                        //removing the internal reference from the product name
                        name = name.Substring(name.IndexOf("]")+1).Trim();
                    }

                    if(ignoreVariants && name.EndsWith(")")){
                        //removing the variant combination from the product name if variants are not used
                        name = name.Substring(0, name.LastIndexOf("(")).Trim();
                    }

                    dr["product_name"] = name;
                }
            }

            //Checking values with no variants or implicit ones (name + variant)                     
            name = (expectedCommonFields.ContainsKey("product_name") ? expectedCommonFields["product_name"].ToString() : null);
            if(!ignoreVariants && explicitVariants) expectedCommonFields.Remove("product_name");
            if(expectedCommonFields.Keys.Count > 0) errors.AddRange(CheckIfTableMatchesData(dt, expectedCommonFields));
            if(!ignoreVariants && explicitVariants && !string.IsNullOrEmpty(name)) expectedCommonFields.Add("product_name", name);

            //Checking values with explicit variants (name with no variant + expectedCommonFields)
            if(explicitVariants){
                if(!expectedCommonFields.ContainsKey("product_name") || string.IsNullOrEmpty(expectedCommonFields["product_name"].ToString())) throw new Exception("The 'product_name' field's value must be provided when looking for attribute matching (product variants).");
                foreach(var variant in expectedAttributeFields.Keys){
                    var expected = expectedAttributeFields[variant];
                    expected.Add("product_name", string.Format("{0} ({1})", expectedCommonFields["product_name"], string.Join(", ", variant)));
                    errors.AddRange(CheckIfTableMatchesData(dt, expected));
                }
            }
 
            return errors;
        }
#endregion
    }
}