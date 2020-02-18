
using System;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;
using AutomatedAssignmentValidator.Core;

namespace AutomatedAssignmentValidator.Checkers{       
    public class Web : Checker{  
        public Connectors.Web Connector {get; private set;}
        public enum Operator{
            MIN,
            MAX,
            EQUALS
        }        
        public Web(string studentFolder, string htmlFile, string cssFile=""){
            this.Connector = new Connectors.Web(studentFolder, htmlFile, cssFile);            
        }         
        /// <summary>
        /// Checks if the amount of nodes results of the XPath query execution, is higher or equals than the expected.
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="expected"></param>
        /// <param name="siblings">The count will be done within siblings elements, for example: //ul/li will count only the 'li' elements within the parent 'ul' in order to check.</param>
        /// <returns></returns>
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
        /// For each node resulting from the XPath query, the maximum amount of 'label' nodes are expected to be related with.
        /// </summary>
        /// <param name="xpath"></param>
        /// <param name="max"></param>
        /// <returns></returns>
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
        public List<string> CheckIfCssPropertiesApplied(string[] properties, int expected, Operator op = Operator.EQUALS){  
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