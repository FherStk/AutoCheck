
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
    public class Html : Checker<Connectors.Html>{  
        /// <summary>
        /// The main connector, can be used to perform direct operations over the data source.
        /// </summary>
        /// <value></value>    
        public override Connectors.Html Connector {get; protected set;}       
        
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
        /// <param name="op">The comparation operator to use, so current 'OP' expected.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfNodesMatchesAmount(string xpath, int expected, Operator op = AutoCheck.Core.Operator.EQUALS){
            var errors = new List<string>();

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the node amount for ~{0}... ", xpath), ConsoleColor.Yellow);   
                
                int count = this.Connector.CountNodes(xpath);
                errors.AddRange(CompareItems("Amount of nodes mismatch:", count, op, expected));                               
            }
            catch(Exception e){
                errors.Add(e.Message);
            }
        
            return errors;
        }

        /// <summary>
        /// Checks if the amount of sibling nodes is lower, higher or equals than the expected, for example: //ul/li will count only the 'li' elements within the parent 'ul' in order to check.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <param name="expected">The expected amount.</param>
        /// <param name="op">The comparation operator to use, so current 'OP' expected.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfSiblingsMatchesAmount(string xpath, int expected, Operator op = AutoCheck.Core.Operator.EQUALS){
            return CheckIfSiblingsMatchesAmount(xpath, new int[]{expected}, op);
        } 

        /// <summary>
        /// Checks if the amount of sibling nodes is lower, higher or equals than the expected, for example: //ul/li will count only the 'li' elements within the parent 'ul' in order to check.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <param name="expected">The expected amount (grouped by father).</param>
        /// <param name="op">The comparation operator to use, so current 'OP' expected.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfSiblingsMatchesAmount(string xpath, int[] expected, Operator op = AutoCheck.Core.Operator.EQUALS){
            var errors = new List<string>();

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the node amount for ~{0}... ", xpath), ConsoleColor.Yellow);   
                
                int[] count = this.Connector.CountSiblings(xpath);                                    
                errors.AddRange(CompareItems("Amount of siblings mismatch:", count, op, expected));
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
        public List<string> CheckIfNodesContentMatchesAmount(string xpath, int expected, Operator op = AutoCheck.Core.Operator.EQUALS){
            var errors = new List<string>();

            try{
                if(!Output.Instance.Disabled)  Output.Instance.Write(string.Format("Checking the content length for ~{0}... ", xpath), ConsoleColor.Yellow);   
                errors.AddRange(CompareItems("Node's content length mismatch:", this.Connector.ContentLength(xpath), op, expected));                
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
        /// <param name="expected">The expected amount of related labels for the given query (single node).</param>
        /// <param name="op">The comparation operator to use, so current 'OP' expected.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfNodesRelatedLabelsMatchesAmount(string xpath, int expected, Operator op = AutoCheck.Core.Operator.EQUALS){
            return CheckIfNodesRelatedLabelsMatchesAmount(xpath, new int[]{expected}, op);
        }

        /// <summary>
        /// Checks if the nodes results of the XPath query execution, have any label nodes related, checking if the total amount is lower, higher or equals than the expected.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <param name="expected">The expected amount of related labels for the given query (each field can be related with multiple labels).</param>
        /// <param name="op">The comparation operator to use, so current 'OP' expected.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfNodesRelatedLabelsMatchesAmount(string xpath, int[] expected, Operator op = AutoCheck.Core.Operator.EQUALS){
            var errors = new List<string>();
        
            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the related labels for ~{0}... ", xpath), ConsoleColor.Yellow);   

                var related = this.Connector.GetRelatedLabels(xpath).Select(x => x.Value.Count()).ToArray();
                if(related.Length == 0) errors.Add("There are no labels in the document for the current field.");
                else errors.AddRange(CompareItems("Amount of labels mismatch:",related, op, expected));           
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
            var errors = new List<string>();
            
            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the table consistence (all the rows has the same amount of columns) for ~{0}... ", xpath), ConsoleColor.Yellow);   
                this.Connector.ValidateTable(xpath);
            }
            catch(Exception e){
                errors.Add(e.Message);
            }  

            return errors;
        }        
    }    
}