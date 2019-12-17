using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HtmlAgilityPack;       
using ExCSS;

namespace AutomatedAssignmentValidator{
    class Css3Validator{
        private static int success;
        private static int errors;
        public static void ValidateAssignment(string studentFolder)
        {            
            ClearResults();

            string fileName = "index.html";            
            Utils.Write("   Validating the file: ");
            Utils.WriteLine(fileName, ConsoleColor.DarkBlue);

            Utils.Write("      Loading the file...");
            HtmlDocument htmlDoc = Utils.LoadHtmlDocument(studentFolder, fileName);        
            if(htmlDoc != null) Utils.PrintResults();
            else{
                Utils.PrintResults(new List<string>(){"Unable to read the HTML file."});
                Utils.PrintScore(0);
                return;
            }

            Utils.Write("      Validating against the W3C official validation tool... ");
            if(Utils.W3CSchemaValidationForHtml5(htmlDoc)) Utils.PrintResults();
            else{
                Utils.PrintResults(new List<string>());
                Utils.PrintScore(0);
                return;
            }
            
            Utils.Write("      Validating inline CSS... ");
            ProcessResults(CheckInlineCss(htmlDoc));    

            Utils.Write("      Validating the divs... ");
            ProcessResults(CheckDivs(htmlDoc));
                       
            Utils.Write("      Validating the video... ");
            ProcessResults(CheckVideo(htmlDoc));                                                
           
            Utils.BreakLine();
            fileName = "index.css";    
            Utils.Write("   Validating the file: ");
            Utils.WriteLine(fileName, ConsoleColor.DarkBlue);   
             
            Utils.Write("      Loading the file...");
            string css = Utils.LoadCssDocument(studentFolder, fileName);      
            if(!string.IsNullOrEmpty(css)) Utils.PrintResults();
            else{
                ProcessResults(new List<string>(){"Unable to read the CSS file."});
                
                Utils.Write("      Loading another file: ");
                css = Utils.LoadCssDocument(studentFolder, "*.css");
                if(!string.IsNullOrEmpty(css)) Utils.PrintResults();
                else {
                    ProcessResults(new List<string>(){"Unable to read the CSS file."});
                    Utils.PrintScore(0);
                    return;
                }  
            }

            Utils.Write("      Validating against the W3C official validation tool... ");
            if(Utils.W3CSchemaValidationForCss3(css)) Utils.PrintResults();
            else{
                Utils.PrintResults(new List<string>());
                Utils.PrintScore(0);
                return;
            }

            StylesheetParser parser = new StylesheetParser();       
            Stylesheet stylesheet = parser.Parse(css);
            if(stylesheet == null){
                Utils.PrintResults(new List<string>(){"Unable to parse the CSS file."});
                Utils.PrintScore(0);
                return;
            }

            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "font"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "border"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "text"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "color"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "background"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "float", "left"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "float", "right"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "position", "absolute"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "position", "relative"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "clear"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "width"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "height"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "margin"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "padding"));
            ProcessResults(CheckCssProperty(htmlDoc, stylesheet, "list"));

            //Just one needed
            Utils.Write("      Validating 'top / right / left / bottom' style... ");
            List<string> top = CheckCssProperty(htmlDoc, stylesheet, "top", null, false);
            List<string> right = CheckCssProperty(htmlDoc, stylesheet, "right", null, false);
            List<string> left = CheckCssProperty(htmlDoc, stylesheet, "left", null, false);
            List<string> bottom = CheckCssProperty(htmlDoc, stylesheet, "bottom", null, false);
            if(top.Count == 0 || right.Count == 0 || left.Count == 0 || bottom.Count == 0) ProcessResults(new List<string>());
            else ProcessResults(top.Concat(right).Concat(left).Concat(bottom).ToList());

            Utils.PrintScore(success, errors);
        } 
        private static void ClearResults(){
            success = 0;
            errors = 0;
        }
        private static void ProcessResults(List<string> list){
            if(list.Count == 0) success++;
            else errors++;
            
            Utils.PrintResults(list);
        } 
         private static List<string> CheckDivs(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();

            try{
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//div");
                if(nodes == null || nodes.Count < 1) errors.Add("Does not contains any div.");                            
            }
            catch(Exception e){
                errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }
        
            return errors;
        }
        private static List<string> CheckVideo(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();

            try{
                bool video = CheckVideo(htmlDoc, "video");
                bool iframe = CheckVideo(htmlDoc, "iframe", "src", "youtube.com");
                bool @object = CheckVideo(htmlDoc, "object", "data", "youtube.com");

                if(!video && !iframe && !@object) errors.Add(string.Format("Unable to find any video pointing to some of the following streaming services: {0}.", "youtube.com"));
            }
            catch(Exception e){
                errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }                       

            return errors;
        } 
        private static bool CheckVideo(HtmlDocument htmlDoc, string node, string attribute = null, string url = null){
            //TODO: url must be a list of valid values
            HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes(string.Format("//{0}", node));
            if(nodes != null && nodes.Count > 0){
                if(string.IsNullOrEmpty(attribute)) return true;
                foreach(HtmlNode n in nodes){
                    string value = n.GetAttributeValue(attribute, "");
                    if(!string.IsNullOrEmpty(value)){
                        if(string.IsNullOrEmpty(url)) return true;
                        else if(value.Contains(url)) return true;
                    }                    
                }
            }  

            return false;
        }

        private static List<string> CheckInlineCss(HtmlDocument htmlDoc){
            List<string> errors = new List<string>();

            try{
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes("//style");           
                if(nodes != null && nodes.Count > 0) errors.Add("CSS definition found using the Style tag.");
                
                foreach(HtmlNode node in htmlDoc.DocumentNode.DescendantsAndSelf()){
                    if(!string.IsNullOrEmpty(node.GetAttributeValue("style", ""))){
                        errors.Add("CSS definition found using inline declarations with style attributes.");
                        break;
                    }
                }                
            }
            catch(Exception e){
                errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }
            
            return errors;
        }

        private static List<string> CheckCssProperty(HtmlDocument htmlDoc, Stylesheet stylesheet, string property, string value = null, bool verbose = true){  
            List<string> errors = new List<string>();

            try{
                if(verbose && string.IsNullOrEmpty(value)) Utils.Write(string.Format("      Validating '{0}' style... ", property));
                else if(verbose) Utils.Write(string.Format("      Validating '{0}:{1}' style... ", property, value));

                bool found = false;
                bool applied = false;
                foreach(StylesheetNode cssNode in stylesheet.Children){
                    if(!CssNodeUsingProperty(cssNode, property, value)) continue;
                    found = true;

                    //Checking if the given css style is being used. Important: only one selector is allowed when calling BuildXpathQuery, so comma split is needed
                    string[] selectors = GetCssSelectors(cssNode);
                    foreach(string s in selectors){
                        HtmlNodeCollection htmlNodes = htmlDoc.DocumentNode.SelectNodes(BuildXpathQuery(s));
                        if(htmlNodes != null && htmlNodes.Count > 0){
                            applied = true;
                            break;
                        }                     
                    }     

                    if(applied) break; 
                }
                    
                if(!found) errors.Add("Unable to find the style within the CSS file.");
                else if(!applied) errors.Add("The style has been found applied the CSS but it's not being applied into the HTML document.");
               
            }
            catch(Exception e){
                errors.Add(string.Format("EXCEPTION: {0}", e.Message));
            }            

            return errors;
        }       
        private static string[] GetCssSelectors(StylesheetNode node){
            string css = node.ToCss();
            return css.Substring(0, css.IndexOf("{")).Trim().Split(',');
        }        
        private static List<string[]> GetCssContent(StylesheetNode node){
            List<string[]> lines = new List<string[]>();
            string css = node.ToCss();

            css = css.Substring(css.IndexOf("{")+1);            
            foreach(string line in css.Split(";")){
                if(line.Contains(":")){
                    string[] item = line.Replace(" ", "").Split(":");
                    if(item[1].Contains("}")) item[1]=item[1].Replace("}", "");
                    if(item[1].Length > 0) lines.Add(item);
                }                
            }

            return lines;
        }
        private static bool CssNodeUsingProperty(StylesheetNode node, string property, string value = null){
            List<string[]> definition = GetCssContent(node);
            foreach(string[] line in definition){
                //If looking for "margin", valid values are: margin and margin-x
                //If looking for "top", valid values are just top
                //So, the property must be alone or left-sided over the "-" symbol.
                if(line[0].Contains(property) && (!line[0].Contains("-") || line[0].Split("-")[0] == property)){                                        
                    if(value == null) return true;
                    else if(line[1].Contains(value)) return true;
                }
            }

            return false;
        }
        private static string BuildXpathQuery(string cssSelector){
            //TODO: if a comma is found, build the correct query with ORs (check first if it's supported by HtmlAgilitypack)
            string xPathQuery = ".";
            string[] selectors = cssSelector.Trim().Replace(">", " > ").Split(' '); //important to force spaces between ">"

            bool children = false;
            for(int i = 0; i < selectors.Length; i++){
                //ignoring modifiers like ":hover"
                if(selectors[i].Contains(":")) selectors[i] = selectors[i].Substring(0, selectors[i].IndexOf(":"));
                
                if(selectors[i].Substring(1).Contains(".")){
                    //Recursive case: combined selectors like "p.bold" (wont work with multi-class selectors)
                    int idx = selectors[i].Substring(1).IndexOf(".")+1;
                    string left = BuildXpathQuery(selectors[i].Substring(0, idx));
                    string right = BuildXpathQuery(selectors[i].Substring(idx));

                    left = left.Substring(children ? 2 : 3);
                    if(left.StartsWith("*")) xPathQuery = right + left.Substring(1);
                    else xPathQuery = right.Replace("*", left);                        
                }
                else{
                    //Base case
                    if(selectors[i].StartsWith("#") || selectors[i].StartsWith(".")){                    
                        xPathQuery += string.Format("{0}*[@{1}='{2}']", (children ? "/" : "//"), (selectors[i].StartsWith("#") ? "id" : "class"), selectors[i].Substring(1));
                        children = false; 
                    }                
                    else if(selectors[i].StartsWith(">")){
                        children = true;
                    }
                    else if(!string.IsNullOrEmpty(selectors[i].Trim())){
                        xPathQuery += string.Format("{0}{1}", (children ? "/" : "//"),  selectors[i].Trim());
                        children = false;
                    }
                }
            }

            return xPathQuery;
        }
    }
}