
using System;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;
using AutomatedAssignmentValidator.Core;

namespace AutomatedAssignmentValidator.Checkers{       
    public class Web : Checker{  
        public Connectors.Web Connector {get; private set;}

        public Web(string studentFolder, string htmlFile, string cssFile=""){
            this.Connector = new Connectors.Web(studentFolder, htmlFile, cssFile);            
        }         
        /// <summary>
        /// Checks if the amount of nodes results of the XPath query execution, is higher or equals than the expected.
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="expected"></param>
        /// <param name="within">When within is on, the count will be done within the hierarchy, for example: //ul/li will count only the 'li' elements within the parent 'ul' in order to check.</param>
        /// <returns></returns>
        public List<string> CheckIfNodeAmountMatchesMinimum(string xpath, int expected, bool within = false){
            List<string> errors = new List<string>();

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the node amount for ~{0}... ", xpath), ConsoleColor.Yellow);   
                int count = 0;
                
                if(!within) count = this.Connector.CountNodes(xpath);
                else count = this.Connector.CountSiblings(xpath).Max();                    
                    
                if(count < expected) errors.Add(string.Format("Amount of '{0}' nodes missmatch: minimum expected->'{1}' found->'{2}'.", xpath, expected, count));
            }
            catch(Exception e){
                errors.Add(e.Message);
            }
        
            return errors;
        }
        public List<string> CheckIfNodeContentMatchesMinimum(string xpath, int expected){
            List<string> errors = new List<string>();

            try{
                if(!Output.Instance.Disabled)  Output.Instance.Write(string.Format("Checking the content length for ~{0}... ", xpath), ConsoleColor.Yellow);   
                int length = this.Connector.ContentLength(xpath);
                if(length < expected) 
                    errors.Add(string.Format("Length of '{0}' conent missmatch: minimum expected->'{1}' found->'{2}'.", xpath, expected, length));
            }
            catch(Exception e){
                errors.Add(e.Message);
            }
        
            return errors;
        }
        public List<string> CheckIfNodeAttributeMatchesData(string xpath, string attribute,  string[] values){
            List<string> errors = new List<string>();
            Dictionary<string, bool> found = values.ToDictionary(x => x, x => false);

            try{
                if(!Output.Instance.Disabled)  Output.Instance.Write(string.Format("Checking the attribute '{0}' value for ~{1}... ", attribute, xpath), ConsoleColor.Yellow);   
                if(values != null){                
                    foreach(HtmlNode n in this.Connector.SelectNodes(xpath)){
                        string key = n.Attributes[attribute].Value.Trim();
                        if(values.Contains(key)) found[key] = true;
                        else errors.Add(String.Format("Unexpected {0} value found: {1}.", attribute, key));
                    }

                    foreach(string key in found.Keys){
                        if(!found[key]) errors.Add(String.Format("Unable to find the {0} value '{1}'.", attribute, key));
                    }
                }                
            }
            catch(Exception e){
                errors.Add(e.Message);
            }                  
            return errors;                             
        }
        /*
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
        
        public Odoo(string companyName, string host, string database, string username, string password): base(host, database, username, password){         
            this.Connector = new Connectors.Odoo(companyName, host, database, username, password);            
        }
        public Odoo(int companyID, string host, string database, string username, string password): base(host, database, username, password){         
            this.Connector = new Connectors.Odoo(companyID, host, database, username, password);            
        }

        public List<string> CheckIfCompanyMatchesData(int companyID, Dictionary<string, object> expectedFields){    
            List<string> errors = new List<string>();                        

            if(!Output.Instance.Disabled)  Output.Instance.Write(string.Format("Getting the company data for ~ID={0}... ", companyID), ConsoleColor.Yellow);            

            Output.Instance.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(this.Connector.GetCompanyData(companyID), expectedFields));
            Output.Instance.UndoStatus();

            return errors;
        }  
        public List<string> CheckIfProviderMatchesData(int providerID, Dictionary<string, object> expectedFields){    
            List<string> errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the provider data for ~ID={0}... ", providerID), ConsoleColor.Yellow);            
            
            Output.Instance.Disable();   //no output for native database checker wanted.
            errors.AddRange(this.CheckIfTableMatchesData(this.Connector.GetProviderData(providerID), expectedFields));
            Output.Instance.UndoStatus();

            return errors;
        }
        public List<string> CheckIfProductMatchesData(int templateID, Dictionary<string, object> expectedFields){    
            return CheckIfProductMatchesData(templateID, expectedFields, null);
        }
        public List<string> CheckIfProductMatchesData(int templateID, Dictionary<string, object> expectedFields, string[] expectedAttributeValues){    
            List<string> errors = new List<string>();            
                        
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the product data for ~ID={0}... ", templateID), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetProductTemplateData(templateID);                        
            errors.AddRange(this.CheckIfTableMatchesData(dt, expectedFields));
            errors.AddRange(CheckItemValues(dt, "variant", "value", expectedAttributeValues));

            Output.Instance.UndoStatus();
            return errors;
        } 
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
        public List<string> CheckIfInvoiceMatchesData(string orderCode, Dictionary<string, object> expectedFields){
            List<string> errors = new List<string>();
            
            if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Getting the invoice data for the order ~{0}... ", orderCode), ConsoleColor.Yellow);                        
            Output.Instance.Disable();   //no output for native database checker wanted.

            DataTable dt = this.Connector.GetInvoiceData(orderCode);
            errors.AddRange(CheckIfTableMatchesData(dt, expectedFields));           

            Output.Instance.UndoStatus();                                      
            return errors;          
        } 
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

            //TODO: check if student.length > 2
            return string.Format("{3} like '{0}%' AND {3} like '%{1}%' AND {3} like '%{2}%'", company, student[0], student[1], dbField);
        }       
        */
    }    
}