
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

        //TODO: some methods can be removed? check if methods with "attribute" parameter can be replaced by other existing methods (and XPath query using the attribute filter)

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

                switch(op){
                    case Operator.EQUALS:
                        if(count != expected) errors.Add(string.Format("Amount of '{0}' nodes missmatch: expected->'{1}' found->'{2}'.", xpath, expected, count));
                        break;

                    case Operator.MAX:
                        if(count > expected) errors.Add(string.Format("Amount of '{0}' nodes missmatch: maximum expected->'{1}' found->'{2}'.", xpath, expected, count));
                        break;

                    case Operator.MIN:
                        if(count < expected) errors.Add(string.Format("Amount of '{0}' nodes missmatch: minimum expected->'{1}' found->'{2}'.", xpath, expected, count));
                        break;
                }                
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
                int length = this.Connector.ContentLength(xpath);

                switch(op){
                    case Operator.EQUALS:
                        if(length != expected) errors.Add(string.Format("Length of '{0}' conent missmatch: expected->'{1}' found->'{2}'.", xpath, expected, length));
                        break;

                    case Operator.MAX:
                        if(length > expected) errors.Add(string.Format("Length of '{0}' conent missmatch: maximum expected->'{1}' found->'{2}'.", xpath, expected, length));
                        break;

                    case Operator.MIN:
                        if(length < expected) errors.Add(string.Format("Length of '{0}' conent missmatch: minimum expected->'{1}' found->'{2}'.", xpath, expected, length));
                        break;
                }
                
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
                    if(labels == null || labels.Length == 0) errors.Add(string.Format("There are no labels in the document for the current {0} field.", key.Name));
                    else{
                        switch(op){
                            case Operator.EQUALS:
                                if(labels.Length != expected) errors.Add(string.Format("Amount of '{0}' nodes missmatch: expected->'{1}' found->'{2}'.", xpath, expected, labels.Length));
                                break;

                            case Operator.MAX:
                                if(labels.Length > expected) errors.Add(string.Format("Amount of '{0}' nodes missmatch: maximum expected->'{1}' found->'{2}'.", xpath, expected, labels.Length));
                                break;

                            case Operator.MIN:
                                if(labels.Length < expected) errors.Add(string.Format("Amount of '{0}' nodes missmatch: minimum expected->'{1}' found->'{2}'.", xpath, expected, labels.Length));
                                break;
                        }
                    } 
                }             
            }
            catch(Exception e){
               errors.Add(e.Message);
            }

            return errors;
        }             
        public List<string> CheckIfNodesSharesAttributeData(string xpath, string attribute){
            List<string> errors = new List<string>();

            //TODO: this sould accept a "group by" attribute, for example, in order to check if only one of a group of checkboxes is the checked one (now will check through all the document)
            //      sorry, no more time to spend here... 

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the shared attribute '{0}' value for ~{1}... ", attribute, xpath), ConsoleColor.Yellow);   
                if(this.Connector.SelectNodes(xpath).GroupBy(x => x.GetAttributeValue(attribute, "")).Count() > 1) errors.Add(string.Format("The nodes are not sharing the same {0}.", attribute));
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
        private int CountNodesSharingAttribute(IEnumerable<HtmlNode> list, string attribute){
            return list.GroupBy(x => x.Attributes[attribute] != null).Where(x => x.Key == true).SelectMany(x => x.ToList()).Count();
        }
    }    
}