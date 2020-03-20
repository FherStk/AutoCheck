
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
        /// <summary>
        /// Compares if the given company data matches with the current one stored in the database.
        /// </summary>
        /// <param name="companyID">The company ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfCompanyMatchesData(int companyID, Dictionary<string, object> expectedFields){    
            List<string> errors = new List<string>();                        

            if(!Output.Instance.Disabled)  Output.Instance.Write(string.Format("Getting the company data for ~ID={0}... ", companyID), ConsoleColor.Yellow);            

            Output.Instance.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(this.Connector.GetCompanyData(companyID), expectedFields));
            Output.Instance.UndoStatus();

            return errors;
        }  
        /// <summary>
        /// Compares if the given provider data matches with the current one stored in the database.
        /// </summary>
        /// <param name="providerID">The provider ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfProviderMatchesData(int providerID, Dictionary<string, object> expectedFields){    
            List<string> errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the provider data for ~ID={0}... ", providerID), ConsoleColor.Yellow);            
            
            Output.Instance.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(this.Connector.GetProviderData(providerID), expectedFields));
            Output.Instance.UndoStatus();

            return errors;
        }
        /// <summary>
        /// Compares if the given product data matches with the current one stored in the database.
        /// </summary>
        /// <param name="templateID">The product template ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfProductMatchesData(int templateID, Dictionary<string, object> expectedFields){    
            return CheckIfProductMatchesData(templateID, expectedFields, null);
        }
        /// <summary>
        /// Compares if the given product data matches with the current one stored in the database.
        /// </summary>
        /// <param name="templateID">The product template ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <param name="expectedAttributeValues">The expected attribute values (a single attribute is supported).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfProductMatchesData(int templateID, Dictionary<string, object> expectedFields, string[] expectedAttributeValues){    
            //TODO: expectedAttributeValues wont work when using more than one attribute, a Dictionary must be used... no more time to implement.
            List<string> errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the product data for ~ID={0}... ", templateID), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetProductTemplateData(templateID);                        
            errors.AddRange(this.CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckItemValues(dt, "variant", "value", expectedAttributeValues));

            Output.Instance.UndoStatus();
            return errors;
        } 
        /// <summary>
        /// Compares if the given purchase data matches with the current one stored in the database.
        /// </summary>
        /// <param name="purchaseID">The purchase ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <param name="expectedAttributeQty">The expected amount of purchased product for each attribute value [name, qty] (sizes, colors, etc.).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfPurchaseMatchesData(int purchaseID, Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty = null){    
            List<string> errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the purchase data for ~ID={0}... ", purchaseID), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetPurchaseData(purchaseID);                        
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.Instance.UndoStatus();
            
            return errors;
        } 
        /// <summary>
        /// Compares if the given order data matches with the current one stored in the database.
        /// </summary>
        /// <param name="orderCode">The order code that will be matched.</param>
        /// <param name="isReturn">If true, the order must be treated as a return.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <param name="expectedAttributeQty">The expected amount of purchased product for each attribute value [name, qty] (sizes, colors, etc.).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfStockMovementMatchesData(string orderCode, bool isReturn, Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty = null){
            List<string> errors = new List<string>();
            
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the stock movement data for the order ~{0}... ", orderCode), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetStockMovementData(orderCode, isReturn);
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.Instance.UndoStatus();                         
            return errors;                             
        }
        /// <summary>
        /// Compares if the given scrapped stock movement data matches with the current one stored in the database.
        /// </summary>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <param name="expectedAttributeQty">The expected amount of purchased product for each attribute value [name, qty] (sizes, colors, etc.).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfScrappedStockMatchesData(Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty = null){
            List<string> errors = new List<string>();
            
            if(!Output.Instance.Disabled) Output.Instance.Write("Getting the scrapped stock data... ");                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetScrappedStockData();
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.Instance.UndoStatus();                         
            return errors;                             
        }
        /// <summary>
        /// Compares if the given invoice data matches with the current one stored in the database.
        /// </summary>
        /// <param name="orderCode">The order code that will be matched.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfInvoiceMatchesData(string orderCode, Dictionary<string, object> expectedFields){
            List<string> errors = new List<string>();
            
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
            List<string> errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the POS sale data for ~ID={0}... ", posSaleID), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetPosSaleData(posSaleID);                        
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckAttributeQuantities(dt, expectedAttributeQty));

            Output.Instance.UndoStatus();
            
            return errors;
        } 
        /// <summary>
        /// Compares if the given sale data matches with the current one stored in the database.
        /// </summary>
        /// <param name="saleID">The sale ID that will be matched.</param>
        /// <param name="expectedFields">The expected data to match.</param>
        /// <param name="expectedAttributeQty">The expected amount of purchased product for each attribute value [name, qty] (sizes, colors, etc.).</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfSaleMatchesData(int saleID, Dictionary<string, object> expectedFields, Dictionary<string, int> expectedAttributeQty = null){    
            List<string> errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the backoffice sale data for ~ID={0}... ", saleID), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetSaleData(saleID);                        
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
            List<string> errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the user data for ~ID={0}... ", userID), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetUserData(userID);                        
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckItemValues(dt, "group", "group", expectedGroups));

            Output.Instance.UndoStatus();
            
            return errors;
        }
        private List<string> CheckItemValues(DataTable dt, string caption, string field, string[] values){
            List<string> errors = new List<string>();
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
        private List<string> CheckAttributeQuantities(DataTable dt, Dictionary<string, int> attributeQty){
            List<string> errors = new List<string>();
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
    }
}