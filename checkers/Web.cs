
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
        /// <param name="within">When within is on, the count will be done within the hierarchy, for example: //ul/li will count only the 'li' elements within the parent 'ul' in order to check.</param>
        /// <returns></returns>
        public List<string> CheckIfNodesMatchesAmount(string xpath, int expected, Operator op = Operator.EQUALS, bool within = false){
            List<string> errors = new List<string>();

            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking the node amount for ~{0}... ", xpath), ConsoleColor.Yellow);   
                int count = 0;
                
                if(!within) count = this.Connector.CountNodes(xpath);
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
        private int CountNodesSharingAttribute(IEnumerable<HtmlNode> list, string attribute){
            return list.GroupBy(x => x.Attributes[attribute] != null).Where(x => x.Key == true).SelectMany(x => x.ToList()).Count();
        }
        public List<string> CheckIfNodesAttributeMatchesAmount(string xpath, string attribute, int expected, Operator op = Operator.EQUALS, bool within = false){
            List<string> errors = new List<string>();
                        
            try{
                if(!Output.Instance.Disabled) Output.Instance.Write(string.Format("Checking mandatory attribute '{0}' value for ~{1}... ", attribute, xpath), ConsoleColor.Yellow);                                   
                
                List<int> countGrp = new List<int>();

                //TODO: Test this carefully
                if(!within) countGrp.Add(CountNodesSharingAttribute(this.Connector.SelectNodes(xpath), attribute));

                //if(!within) countGrp.Add(this.Connector.SelectNodes(xpath).GroupBy(x => x.Attributes[attribute] != null).Where(x => x.Key == true).SelectMany(x => x.ToList()).Count());
                else if(attribute.Equals("checked")){
                    //Checks or radios, same name
                    foreach(var grp in Connector.SelectNodes(xpath).GroupBy(x => x.Attributes["name"].Value)){
                        //Counting amount of equals attributes within items with the same name
                        //countGrp.Add(grp.GroupBy(x => x.Attributes[attribute] != null).Where(x => x.Key == true).Count());
                        countGrp.Add(CountNodesSharingAttribute(grp, attribute));
                    }
                }
                else{
                    foreach(var grp in Connector.SelectNodes(xpath).GroupBy(x => x.ParentNode)){
                        //Counting amount of equals attributes within items with the same parent
                        //countGrp.Add(grp.GroupBy(x => x.Attributes[attribute] != null).Where(x => x.Key == true).Count());
                        countGrp.Add(CountNodesSharingAttribute(grp, attribute));
                    }
                }

                foreach(int count in countGrp){
                    switch(op){
                        case Operator.EQUALS:
                            if(count != expected) errors.Add(string.Format("Amount of '{0}' attribute missmatch: expected->'{1}' found->'{2}'.", attribute, expected, count));
                            break;

                        case Operator.MAX:
                            if(count > expected) errors.Add(string.Format("Amount of '{0}' attribute missmatch: maximum expected->'{1}' found->'{2}'.", attribute, expected, count));
                            break;

                        case Operator.MIN:
                            if(count < expected) errors.Add(string.Format("Amount of '{0}' attribute missmatch: minimum expected->'{1}' found->'{2}'.", attribute, expected, count));
                            break;
                    }
                }
            }
            catch(Exception e){
                errors.Add(e.Message);
            }                  
            return errors;                             
        }        
        public List<string> CheckIfNodesContentMatchesAmount(string xpath, int expected, Operator op = Operator.EQUALS){
            //TODO: allow an enum comparator parameter, so =, <, > can be checked
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
            //TODO: allow an enum comparator parameter, so =, <, > can be checked
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
        public List<string> CheckIfNodesAttributeMatchesData(string xpath, string attribute,  string[] values){
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
          
    }    
}