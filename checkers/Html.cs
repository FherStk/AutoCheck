
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
using System.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;
using AutoCheck.Core;

namespace AutoCheck.Checkers{     
    /// <summary>
    /// Allows data validations over a WEB set of files.
    /// </summary>  
    public class Html : Checker{  
        /// <summary>
        /// The main connector, can be used to perform direct operations over the data source.
        /// </summary>
        /// <value></value>    
        public Connectors.Html Connector {get; private set;}       
        /// <summary>
        /// Creates a new checker instance.
        /// </summary>
        /// <param name="studentFolder">The folder containing the web files.</param>
        /// <param name="file">HTML file name.</param>
        public Html(string studentFolder, string file){
            this.Connector = new Connectors.Html(studentFolder, file);            
        }
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
            this.Connector.Dispose();
        }         
        /// <summary>
        /// Checks if the amount of nodes results of the XPath query execution, is lower, higher or equals than the expected.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <param name="expected">The expected amount.</param>
        /// <param name="op">The comparation operator to use.</param>
        /// <param name="siblings">The count will be done within siblings elements, for example: //ul/li will count only the 'li' elements within the parent 'ul' in order to check.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfNodesMatchesAmount(string xpath, int expected, Connector.Operator op = AutoCheck.Core.Connector.Operator.EQUALS, bool siblings = false){
            List<string> errors = new List<string>();

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the node amount for ~{0}... ", xpath), ConsoleColor.Yellow);   
                int count = 0;
                
                if(!siblings) count = this.Connector.CountNodes(xpath);
                else count = this.Connector.CountSiblings(xpath).Max();                    
                errors.AddRange(CompareItems("Amount of nodes missmatch:", expected, count, op));                               
            }
            catch(Exception e){
                errors.Add(e.Message);
            }
        
            return errors;
        }                     
        /// <summary>
        /// Checks if the content length of nodes results of the XPath query execution, is lower, higher or equals than the expected.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <param name="expected">The content length expected.</param>
        /// <param name="op">Comparison operator to be used.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfNodesContentMatchesAmount(string xpath, int expected, Connector.Operator op = AutoCheck.Core.Connector.Operator.EQUALS){
            List<string> errors = new List<string>();

            try{
                if(!Output.Instance.Disabled)  Output.Instance.Write(string.Format("Checking the content length for ~{0}... ", xpath), ConsoleColor.Yellow);   
                errors.AddRange(CompareItems("Node's content length missmatch:", expected, this.Connector.ContentLength(xpath), op));                
            }
            catch(Exception e){
                errors.Add(e.Message);
            }
        
            return errors;
        }        
        /// <summary>
        /// Checks if the nodes results of the XPath query execution, have any label nodes related, checking if the total amount is lower, higher or equals than the expected.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <param name="max"></param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfNodesRelatedLabelsMatchesAmount(string xpath, int expected, Connector.Operator op = AutoCheck.Core.Connector.Operator.EQUALS){
            List<string> errors = new List<string>();
        
            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the related labels for ~{0}... ", xpath), ConsoleColor.Yellow);   

                var related = this.Connector.GetRelatedLabels(xpath);
                foreach(HtmlNode key in related.Keys){
                    HtmlNode[] labels = related[key];
                    if(labels == null || labels.Length == 0) errors.Add("There are no labels in the document for the current field.");
                    else errors.AddRange(CompareItems("Amount of labels missmatch:", expected, labels.Length, op));
                }             
            }
            catch(Exception e){
               errors.Add(e.Message);
            }

            return errors;
        }                                             
        /// <summary>
        /// Checks if a table's amount of columns is consistent within all its rows.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfTablesIsConsistent(string xpath){
            List<string> errors = new List<string>();
            
            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the table consistence (all the rows has the same amount of columns) for ~{0}... ", xpath), ConsoleColor.Yellow);   
                this.Connector.ValidateTable(xpath);
            }
            catch(Exception e){
                errors.Add(e.Message);
            }  

            return errors;
        }          
        private List<string> CompareItems(string caption, int expected, int current, Connector.Operator op){
            //TODO: must be reusable by other checkers
            List<string> errors = new List<string>();
            string info = string.Format("expected->'{0}' found->'{1}'.", expected, current);

            switch(op){
                case AutoCheck.Core.Connector.Operator.EQUALS:
                    if(current != expected) errors.Add(string.Format("{0} {1}.", caption, info));
                    break;

                case AutoCheck.Core.Connector.Operator.MAX:
                    if(current > expected) errors.Add(string.Format("{0} maximum {1}.", caption, info));
                    break;

                case AutoCheck.Core.Connector.Operator.MIN:
                    if(current < expected) errors.Add(string.Format("{0} minimum {1}.", caption, info));
                    break;
            }

            return errors;
        }
    }    
}