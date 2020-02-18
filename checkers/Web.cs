
using System;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;
using AutomatedAssignmentValidator.Core;

namespace AutomatedAssignmentValidator.Checkers{     
    /// <summary>
    /// Allows data validations over a WEB set of files.
    /// </summary>  
    public class Web : Checker{  
        /// <summary>
        /// The main connector, can be used to perform direct operations over the data source.
        /// </summary>
        /// <value></value>    
        public Connectors.Web Connector {get; private set;}
        /// <summary>
        /// Comparator operator (=, <, >)
        /// </summary>
        public enum Operator{
            MIN,
            MAX,
            EQUALS
        }  
        /// <summary>
        /// Creates a new checker instance.
        /// </summary>
        /// <param name="studentFolder">The folder containing the web files.</param>
        /// <param name="htmlFile">HTML file name.</param>
        /// <param name="cssFile">CSS file name.</param>     
        public Web(string studentFolder, string htmlFile, string cssFile=""){
            this.Connector = new Connectors.Web(studentFolder, htmlFile, cssFile);            
        }         
        /// <summary>
        /// Checks if the amount of nodes results of the XPath query execution, is lower, higher or equals than the expected.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <param name="expected">The expected amount.</param>
        /// <param name="siblings">The count will be done within siblings elements, for example: //ul/li will count only the 'li' elements within the parent 'ul' in order to check.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfNodesMatchesAmount(string xpath, int expected, Operator op = Operator.EQUALS, bool siblings = false){
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
        public List<string> CheckIfNodesContentMatchesAmount(string xpath, int expected, Operator op = Operator.EQUALS){
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
        public List<string> CheckIfNodesRelatedLabelsMatchesAmount(string xpath, int expected, Operator op = Operator.EQUALS){
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
                this.Connector.CheckTableConsistence(xpath);
            }
            catch(Exception e){
                errors.Add(e.Message);
            }  

            return errors;
        } 
        /// <summary>
        /// Given a CSS property, checks if its has been applied within the HTML document.
        /// </summary>
        /// <param name="property">The CSS property name.</param>
        /// <param name="value">The CSS property value.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfCssPropertyHasBeenApplied(string property, string value = null){  
            List<string> errors = new List<string>();

            try{
                if(!Output.Instance.Disabled){
                    if(string.IsNullOrEmpty(value)) Output.Instance.Write(string.Format("Checking the '{0}' CSS property... ", property));
                    else Output.Instance.Write(string.Format("Checking the '{0}:{1}' CSS property... ", property, value));
                }
                
                this.Connector.CheckIfCssPropertyHasBeenApplied(property, value);  //exception if not applied             
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        }  
        /// <summary>
        ///  Given a set of CSS properties, checks how many of them has been applied within the HTML document, and if the total amount is lower, higher or equals than the expected.
        /// </summary>
        /// <param name="properties">A set of CSS property names.</param>
        /// <param name="expected">Expected applied amount.</param>
        /// <param name="op">Comparison operator to be used.</param>
        /// <returns>The list of errors found (the list will be empty it there's no errors).</returns>
        public List<string> CheckIfCssPropertiesAppliedMatchesAmount(string[] properties, int expected, Operator op = Operator.EQUALS){  
             List<string> errors = new List<string>();
             
             try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the '({0})' CSS properties... ", string.Join(" | ", properties)));
                
                Output.Instance.Disable();
                int applied = 0;
                foreach(string prop in properties){
                    //this.Connector.CheckIfCssPropertyApplied can be also called, but might be better to use CheckIfCssPropertyApplied in order to unify behaviours
                    if(CheckIfCssPropertyHasBeenApplied(prop).Count == 0) applied++;                   
                }

                Output.Instance.UndoStatus();
                errors.AddRange(CompareItems("Applied CSS properties missmatch:", expected, applied, op));                
            }
            catch(Exception e){
                errors.Add(e.Message);
            }            

            return errors;
        } 
        private List<string> CompareItems(string caption, int expected, int current, Operator op){
            List<string> errors = new List<string>();
            string info = string.Format("expected->'{0}' found->'{1}'.", expected, current);

            switch(op){
                case Operator.EQUALS:
                    if(current != expected) errors.Add(string.Format("{0} {1}.", caption, info));
                    break;

                case Operator.MAX:
                    if(current > expected) errors.Add(string.Format("{0} maximum {1}.", caption, info));
                    break;

                case Operator.MIN:
                    if(current < expected) errors.Add(string.Format("{0} minimum {1}.", caption, info));
                    break;
            }

            return errors;
        }
    }    
}