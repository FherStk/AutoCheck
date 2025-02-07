/*
    Copyright © 2023 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/credits.md

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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using ExCSS;
using HtmlAgilityPack;
using AutoCheck.Core.Exceptions;

namespace AutoCheck.Core.Connectors{

    /// <summary>
    /// Allows in/out operations and/or data validations with CSS files.
    /// </summary>
    public class Css: Base{                
        /// <summary>
        /// The CSS document content.
        /// </summary>
        /// <value></value>
        public Stylesheet CssDoc {get; private set;}

        /// <summary>
        /// The original CSS file content (unparsed).
        /// </summary>
        /// <value></value>
        public string Raw {get; private set;}

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="filePath">CSS file path.</param>
        public Css(string filePath){            
            Parse(filePath);
        }

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="remoteOS"The remote host OS.</param>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="port">The remote machine's port where SSH is listening to.</param>
        /// <param name="filePath">CSS file path.</param>
        public Css(Utils.OS remoteOS, string host, string username, string password, int port, string filePath){  
            ProcessRemoteFile(remoteOS, host, username, password, port, filePath, new Action<string>((filePath) => {
                Parse(filePath); 
            }));   
        }

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="remoteOS"The remote host OS.</param>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="filePath">CSS file path.</param>
        public Css(Utils.OS remoteOS, string host, string username, string password, string filePath): this(remoteOS, host, username, password, 22, filePath){
        } 

        private void Parse(string filePath){
            if(string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if(!File.Exists(filePath)) throw new FileNotFoundException();
                        
            StylesheetParser parser = new StylesheetParser();    
            Raw = File.ReadAllText(filePath);
            CssDoc = parser.Parse(Raw);    
        }        
        
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        }
        
        /// <summary>
        /// Validates the currently loaded CSS document against the W3C public API. 
        /// Throws an exception if the document is invalid.
        /// </summary>
        public void ValidateCss3AgainstW3C(){
            //Documentation:    https://jigsaw.w3.org/css-validator/manual.html
            //                  https://jigsaw.w3.org/css-validator/api.html#requestformat        
            var httpClient = new HttpClient();
            var css = System.Web.HttpUtility.UrlEncode(Raw.Replace("\r\n", ""));
            var asyncGet = httpClient.GetAsync($"http://jigsaw.w3.org/css-validator/validator?profile=css3&output=soap12&warning=no&text={css}");
            asyncGet.Wait();

            asyncGet.Result.EnsureSuccessStatusCode();
            
            var asyncRead = asyncGet.Result.Content.ReadAsStringAsync();
            asyncRead.Wait();
            
            XmlDocument document = new XmlDocument();
            document.LoadXml(asyncRead.Result); 
            
            int errorCount = int.Parse(document.GetElementsByTagName("m:errorcount")[0].InnerText);
            if(errorCount > 0){
                //TODO: add the error list to the description
                //Loop through all the "m:error" nodes
                //  Display: "m:line" + "m:errortype" + "m:context" + "m_message"
                foreach(XmlNode error in document.GetElementsByTagName("m:message")){
                    //TODO: add the error list to the description
                    //Loop through all the "m:error" nodes
                    //  Display: "m:line" + "m:errortype" + "m:context" + "m_message"                    
                    var message = error.InnerText.Trim();

                    while(message.Contains('\n')){
                        int i = message.IndexOf('\n');
                        string temp = message.Substring(0, i);
                        message = $"{temp}\\${message.Substring(i+1).TrimStart()}";
                    }
                    
                    throw new DocumentInvalidException(message.Replace("\\$", System.Environment.NewLine));         
                }
            } 
        }          
        
        /// <summary>
        /// Determines if a property exists within the current CSS document.
        /// </summary>
        /// <param name="property">The CSS property name.</param>
        /// <param name="value">The CSS property value.</param>
        /// <returns>True if the property has been found</returns>
        public bool PropertyExists(string property, string value = null){ 
            foreach(StylesheetNode cssNode in CssDoc.Children){
                if(!NodeUsingProperty(cssNode, property, value)) continue;
                return true;
            }
                
            return false;
        }

        /// <summary>
        /// Determines if a set of properties exists within the current CSS document.
        /// </summary>
        /// <param name="properties">The CSS property names.</param>        
        /// <returns>Total amount of existing properties, plus the name of those properties</returns>
        public (int count, string[] exists) PropertyExists(string[] properties){ 
            return PropertyExists(properties.ToDictionary(x => x, y => string.Empty));
        }

        /// <summary>
        /// Determines if a set of properties exists within the current CSS document.
        /// </summary>
        /// <param name="properties">The CSS property names and its expected values.</param>        
        /// <returns>Total amount of existing properties, plus the name of those properties</returns>
        public (int count, string[] exists) PropertyExists(Dictionary<string, string> properties){ 
            List<string> found = new List<string>();
            foreach(string name in properties.Keys){
                var exists = PropertyExists(name, properties[name]);
                if(exists) found.Add(name);
            }

            return (found.Count, found.ToArray());
        }

        /// <summary>
        /// Determines if a current CSS document property is beeing within a given HTML document.
        /// </summary>
        /// </summary>
        /// <param name="htmlDoc">The HTML document that must be using the property.</param>
        /// <param name="property">The CSS property name.</param>
        /// <param name="value">The CSS property value.</param>
        /// <returns>True if the property is being used.</returns>
        public bool PropertyApplied(HtmlDocument htmlDoc, string property, string value = null){   
            //Not needed, redundand check          
            //if(!PropertyExists(property, value)) return false;
            //else{
                foreach(StylesheetNode cssNode in CssDoc.Children){
                    if(!NodeUsingProperty(cssNode, property, value)) continue;                    

                    //Checking if the given css style is being used. Important: only one selector is allowed when calling BuildXpathQuery, so comma split is needed
                    string[] selectors = GetCssSelectors(cssNode);
                    foreach(string s in selectors){
                        HtmlNodeCollection htmlNodes = htmlDoc.DocumentNode.SelectNodes(BuildXpathQuery(s));
                        if(htmlNodes != null && htmlNodes.Count > 0) return true;
                    }     
                }
            //}
            
            return false;
        }

        /// <summary>
        /// Determines if a set of CSS properties has been applied within the given HTML document.
        /// </summary>
        /// <param name="htmlDoc">The HTML document that must be using the property.</param>
        /// <param name="properties">The CSS property names.</param>        
        /// <returns>Total amount of existing properties, plus the name of those properties</returns>
        public (int count, string[] exists) PropertyApplied(HtmlDocument htmlDoc, string[] properties){ 
            return PropertyApplied(htmlDoc, properties.ToDictionary(x => x, y => string.Empty));
        }

        /// <summary>
        /// Determines if a set of CSS properties has been applied within the given HTML document.
        /// </summary>
        /// <param name="htmlDoc">The HTML document that must be using the property.</param>
        /// <param name="properties">The CSS property names and its expected values.</param>        
        /// <returns>Total amount of existing properties, plus the name of those properties</returns>
        public (int count, string[] exists) PropertyApplied(HtmlDocument htmlDoc, Dictionary<string, string> properties){ 
            List<string> found = new List<string>();
            foreach(string name in properties.Keys){
                var exists = PropertyApplied(htmlDoc, name, properties[name]);
                if(exists) found.Add(name);
            }

            return (found.Count, found.ToArray());
        }

        /// <summary>
        /// Determines if a current CSS document property is beeing within a given HTML document.
        /// </summary>
        /// </summary>
        /// <param name="htmlConn">The HTML connector containing the document to check.</param>
        /// <param name="property">The CSS property name.</param>
        /// <param name="value">The CSS property value.</param>
        /// <returns>True if the property is being used.</returns>
        public bool PropertyApplied(Connectors.Html htmlConn, string property, string value = null){   
            return PropertyApplied(htmlConn.HtmlDoc, property, value);
        }

        /// <summary>
        /// Determines if a set of CSS properties has been applied within the given HTML document.
        /// </summary>
        /// <param name="htmlConn">The HTML connector containing the document to check.</param>
        /// <param name="properties">The CSS property names.</param>        
        /// <returns>Total amount of existing properties, plus the name of those properties</returns>
        public (int count, string[] exists) PropertyApplied(Connectors.Html htmlConn, string[] properties){ 
            return PropertyApplied(htmlConn.HtmlDoc, properties.ToDictionary(x => x, y => string.Empty));
        }

        /// <summary>
        /// Determines if a set of CSS properties has been applied within the given HTML document.
        /// </summary>
        /// <param name="htmlConn">The HTML connector containing the document to check.</param>
        /// <param name="properties">The CSS property names and its expected values.</param>        
        /// <returns>Total amount of existing properties, plus the name of those properties</returns>
        public (int count, string[] exists) PropertyApplied(Connectors.Html htmlConn, Dictionary<string, string> properties){ 
            return PropertyApplied(htmlConn.HtmlDoc, properties);
        }

        private bool NodeUsingProperty(StylesheetNode node, string property, string value = null){
            List<string[]> definition = GetCssContent(node);
            foreach(string[] line in definition){
                //If looking for "margin", valid values are: margin and margin-x
                //If looking for "top", valid values are just top
                //So, the property must be alone or left-sided over the "-" symbol.
                bool found = false;
                if(property.Contains("-") || !line[0].Contains("-")) found = line[0].Equals(property);
                else if(line[0].Contains("-")) found = line[0].Split("-")[0].Equals(property);
                 
                if(found){
                    if(string.IsNullOrEmpty(value)) return true;
                    else if(line[1].Contains(value)) return true;
                }
            }

            return false;
        }
        
        private List<string[]> GetCssContent(StylesheetNode node){
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
        
        private string[] GetCssSelectors(StylesheetNode node){
            string css = node.ToCss();
            return css.Substring(0, css.IndexOf("{")).Trim().Split(',');
        }
        
        private string BuildXpathQuery(string cssSelector){
            //TODO: if a comma is found, build the correct query with ORs (check first if it's supported by HtmlAgilitypack)
            string xPathQuery = ".";
            string[] selectors = cssSelector.Trim().TrimStart(':').Replace(">", " > ").Split(' '); //important to force spaces between ">"

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