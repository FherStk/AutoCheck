/*
    Copyright © 2021 Fernando Porrino Serrano
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
using System.Xml;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using ExCSS;
using HtmlAgilityPack;
using AutoCheck.Core.Exceptions;

namespace AutoCheck.Core.Connectors{
    
    /// <summary>
    /// Allows in/out operations and/or data validations with web files (html, css, etc.).
    /// </summary>
    public class Html: Base{         
        /// <summary>
        /// The HTML document content.
        /// </summary>
        /// <value></value>
        public HtmlDocument HtmlDoc {get; private set;}
        
        /// <summary>
        /// The original HTML file content (unparsed).
        /// </summary>
        /// <value></value>
        public string Raw {get; private set;}
        
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="filePath">HTML file path.</param>
        public Html(string filePath){    
            if(string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if(!File.Exists(filePath)) throw new FileNotFoundException();
                        
            this.HtmlDoc = new HtmlDocument();
            this.HtmlDoc.Load(filePath);
            this.Raw = File.ReadAllText(filePath);
        }

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="remoteOS"The remote host OS.</param>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="port">The remote machine's port where SSH is listening to.</param>
        /// <param name="filePath">HTML file path.</param>
        public Html(Utils.OS remoteOS, string host, string username, string password, int port, string filePath){  
            var remote = new RemoteShell(remoteOS, host, username, password, port);
            
            if(string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if(!remote.ExistsFile(filePath)) throw new FileNotFoundException("filePath");                        
            
            filePath = remote.DownloadFile(filePath);

            this.HtmlDoc = new HtmlDocument();
            this.HtmlDoc.Load(filePath);
            this.Raw = File.ReadAllText(filePath);

            File.Delete(filePath);
        }

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="remoteOS"The remote host OS.</param>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="filePath">HTML file path.</param>
        public Html(Utils.OS remoteOS, string host, string username, string password, string filePath): this(remoteOS, host, username, password, 22, filePath){              
        }
        
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        } 
        
        /// <summary>
        /// Validates the currently loaded HTML document against the W3C public API. 
        /// Throws an exception if the document is invalid.
        /// </summary>
        public void ValidateHtml5AgainstW3C(){
            string html = string.Empty;
            string url = "https://validator.nu?out=xml";
            byte[] dataBytes = Encoding.UTF8.GetBytes(this.Raw);

            //Documentation:    https://validator.w3.org/docs/api.html
            //                  https://github.com/validator/validator/wiki/Service-%C2%BB-Input-%C2%BB-POST-body            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = dataBytes.Length;
            request.Method = "POST";
            request.ContentType = "text/html; charset=utf-8";
            request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.101 Safari/537.36";

            using(Stream requestBody = request.GetRequestStream())
                requestBody.Write(dataBytes, 0, dataBytes.Length);
        
            XmlDocument document = new XmlDocument();
            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using(Stream stream = response.GetResponseStream())             
            using(StreamReader reader = new StreamReader(stream)){
                string output = reader.ReadToEnd();                                
                document.LoadXml(output); 
            }
                        
            foreach(XmlNode msg in document.GetElementsByTagName("info")){               
                XmlAttribute type = msg.Attributes["type"];
                if(type != null && type.InnerText.Equals("error"))
                    throw new DocumentInvalidException();  //TODO: add the error list to the description
            }
            
            foreach(XmlNode msg in document.GetElementsByTagName("error")){
                //Workaround: works on manual validation but fails on API cal...
                if(msg.InnerText.StartsWith("Attribute allow not allowed on element iframe at this point.")) continue;
                
                //TODO: add the error list to the description                
                string node = "<ul>";                
                throw new DocumentInvalidException(msg.InnerText.Contains(node) ? msg.InnerText.Substring(0, msg.InnerText.LastIndexOf(node)) : msg.InnerText); //TODO: add the error list to the description
            }

        }        
        
        /// <summary>
        /// Requests for a set of nodes.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>A list of nodes.</returns>
        public List<HtmlNode> SelectNodes(string xpath){
            return SelectNodes(this.HtmlDoc.DocumentNode, xpath);
        }
        
        /// <summary>
        /// Requests for a set of nodes.
        /// </summary>
        /// <param name="root">Root node from where the XPath expression will be evaluated.</param>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>A list of nodes.</returns>
        public List<HtmlNode> SelectNodes(HtmlNode root, string xpath){
            if(root == null) return null;
            else return root.SelectNodes(xpath).ToList();
        }
        
        /// <summary>
        /// Count how many nodes of this kind are within the document.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>Amount of nodes.</returns>
        public int CountNodes(string xpath){
            return CountNodes(this.HtmlDoc.DocumentNode, xpath);
        }
        
        /// <summary>
        /// Count how many nodes of this kind are within the document, ideal for count groups of radio buttons or checkboxes.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <param name="root">Root node from where the XPath expression will be evaluated.</param>
        /// <returns>Amount of nodes.</returns>
        public int CountNodes(HtmlNode root, string xpath){
            if(root == null) return 0;
            else{
                var nodes = root.SelectNodes(xpath);
                return (nodes == null ? 0 : nodes.Count());
            }            
        }
        
        /// <summary>
        /// Get a set of siblings, grouped by father.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>A list of node groups.</returns>
        public HtmlNode[][] GroupSiblings(string xpath){   
            return GroupSiblings(this.HtmlDoc.DocumentNode, xpath);
        }
        
        /// <summary>
        /// Get a set of siblings, grouped by father.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <param name="root">Root node from where the XPath expression will be evaluated.</param>
        /// <returns>A list of node groups.</returns>
        public HtmlNode[][] GroupSiblings(HtmlNode root, string xpath){             
            var total = new List<HtmlNode[]>();                
            if(root != null){                
                foreach(var grp in root.SelectNodes(xpath).GroupBy(x => x.ParentNode)){
                    total.Add(grp.ToArray());
                }
            }

            return total.ToArray();  
        }

        /// <summary>
        /// Count how many nodes are siblings, grouped by father.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>Each group means how many nodes are siblings between each other.</returns>
        public int[] CountSiblings(string xpath){   
            return CountSiblings(this.HtmlDoc.DocumentNode, xpath);
        }
        
        /// <summary>
        /// Count how many nodes are siblings, grouped by father.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <param name="root">Root node from where the XPath expression will be evaluated.</param>
        /// <returns>Each group means how many nodes are siblings between each other.</returns>
        public int[] CountSiblings(HtmlNode root, string xpath){  
            return GroupSiblings(root, xpath).Select(x => x.Count()).ToArray();
        }
        
        /// <summary>
        /// The length of a node content, sum of all of them if there's more than one.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>Node content's length.</returns>
        public int ContentLength(string xpath){
            return ContentLength(this.HtmlDoc.DocumentNode, xpath);            
        }
        
        /// <summary>
        /// The length of a node content, sum of all of them i there's more than one.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <param name="root">Root node from where the XPath expression will be evaluated.</param>
        /// <returns>Node content's length.</returns>
        public int ContentLength(HtmlNode root, string xpath){
            if(root == null) return 0;
            else{
                var nodes = root.SelectNodes(xpath);
                return (root == null ? 0 : nodes.Sum(x => x.InnerText.Length));
            }            
        }
        
        /// <summary>
        /// Returns the label nodes related to the xpath resulting nodes.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>Dictonary with key-pair values, where the key is the main field node, and the value is a set of its related label nodes.</returns>
        public Dictionary<HtmlNode, HtmlNode[]> GetRelatedLabels(string xpath){                
            return GetRelatedLabels(this.HtmlDoc.DocumentNode, xpath);
        }
        
        /// <summary>
        /// Returns the label nodes related to the xpath resulting nodes.
        /// </summary>
        /// <param name="root">Root node from where the XPath expression will be evaluated.</param>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>Dictonary with key-pair values, where the key is the main field node, and the value is a set of its related label nodes.</returns>
        public Dictionary<HtmlNode, HtmlNode[]> GetRelatedLabels(HtmlNode root, string xpath){                    
            var results = new Dictionary<HtmlNode, HtmlNode[]>();
            if(root != null){            
                foreach(HtmlNode node in root.SelectNodes(xpath)){  //TODO: if none returns, throws a null object exception...
                    string id = node.GetAttributeValue("id", "");
                    if(string.IsNullOrEmpty(id)) results.Add(node, null);
                    else{
                        HtmlNodeCollection labels = this.HtmlDoc.DocumentNode.SelectNodes("//label");
                        if(labels == null) results.Add(node, null);
                        else results.Add(node, labels.Where(x => x.GetAttributeValue("for", "").Equals(id)).ToArray());
                    }
                }
            }

            return results; 
        }

        /// <summary>
        /// Returns the amount of label nodes related to the xpath resulting nodes.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>Dictonary with key-pair values, where the key is the main field node, and the value is a set of its related label nodes.</returns>
        public int CountRelatedLabels(string xpath){                
            return CountRelatedLabels(this.HtmlDoc.DocumentNode, xpath);
        }
        
        /// <summary>
        /// Returns the amount of label nodes related to the xpath resulting nodes.
        /// </summary>
        /// <param name="root">Root node from where the XPath expression will be evaluated.</param>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>Dictonary with key-pair values, where the key is the main field node, and the value is a set of its related label nodes.</returns>
        public int CountRelatedLabels(HtmlNode root, string xpath){                                
            return GetRelatedLabels(xpath).Select(x => x.Value.Count()).Sum(); 
        }
        
        /// <summary>
        /// Checks if a table's amount of columns is consistent within all its rows.
        /// Throws an exception if it's inconsistent.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        public void ValidateTable(string xpath){
            ValidateTable(this.HtmlDoc.DocumentNode, xpath);
        }
        
        /// <summary>
        /// Checks if a table's amount of columns is consistent within all its rows.
        /// Throws an exception if it's inconsistent.
        /// </summary>
        /// <param name="root">Root node from where the XPath expression will be evaluated.</param>
        /// <param name="xpath">XPath expression.</param>
        public void ValidateTable(HtmlNode root, string xpath){
            //TODO: rowspan!
            if(root == null) return;            
            foreach(HtmlNode node in root.SelectNodes(xpath)){  
                int row = 1;   

                //Starts with // to avoid theader and tbody nodes.
                int cols = CountColumns(node.SelectNodes(".//tr[1]").FirstOrDefault());

                //Starts with // to avoid theader and tbody nodes.              
                foreach(HtmlNode tr in node.SelectNodes(".//tr")){                                  
                    int current = CountColumns(tr);
                    if(current != cols)  throw new TableInconsistencyException(string.Format("Inconsistence detected on row {0}, amount of columns mismatch: expected->'{1}' found->'{2}'.", row, cols, current));
                    
                    row ++;
                }
            }
        }
        
        private int CountColumns(HtmlNode tr){
            int cols = 0;
            foreach(HtmlNode td in tr.SelectNodes("td | th")){
                if(td.Attributes["colspan"] != null) cols += int.Parse(td.Attributes["colspan"].Value);
                else cols += 1;
            }

            return cols;
        }                
    }
}