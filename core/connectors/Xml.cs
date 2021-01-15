/*
    Copyright © 2020 Fernando Porrino Serrano
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
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using AutoCheck.Core.Exceptions;
using Wmhelp.XPath2;

namespace AutoCheck.Core.Connectors{       
    /// <summary>
    /// Allows in/out operations and/or data validations with XML files.
    /// </summary>
    public class Xml: Base{     
        public enum XmlNodeType {
            ALL, 
            STRING,
            BOOLEAN,
            NUMERIC
        }

        private XmlDocument XmlDocNoDefaultNamespace {get; set;}   

        public string[] Comments {get; private set;}

        /// <summary>
        /// The XML document content.
        /// </summary>
        /// <value></value>
        public XmlDocument XmlDoc {get; private set;}   

        /// <summary>
        /// The original XML file content (unparsed).
        /// </summary>
        /// <value></value>
        public string Raw {get; private set;}    
        
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="folder">The folder containing the files.</param>
        /// <param name="file">XML file name.</param>
        /// <param name="validation">Validation type.</param>
        public Xml(string folder, string file, ValidationType validation = ValidationType.None){
            folder = Utils.PathToCurrentOS(folder);
            
            if(string.IsNullOrEmpty(folder)) throw new ArgumentNullException("path");
            if(string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");
            if(!Directory.Exists(folder)) throw new DirectoryNotFoundException();
                        
            var settings = new XmlReaderSettings { 
                IgnoreComments = false,
                ValidationType = validation,                 
                DtdProcessing = (validation == ValidationType.DTD ? DtdProcessing.Parse : DtdProcessing.Ignore),
                ValidationFlags = (validation == ValidationType.Schema ? (System.Xml.Schema.XmlSchemaValidationFlags.ProcessInlineSchema | System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation | System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints) : System.Xml.Schema.XmlSchemaValidationFlags.None),
                XmlResolver = new XmlUrlResolver()
            };            

            var messages = new StringBuilder();
            settings.ValidationEventHandler += (sender, args) => messages.AppendLine(args.Message);
            
            var coms = new List<string>();
            var filePath = Path.Combine(folder, file);            
            var reader = XmlReader.Create(filePath, settings);                         
            try{                                
                while (reader.Read()){
                    switch (reader.NodeType)
                    {                        
                        case System.Xml.XmlNodeType.Comment:
                            if(reader.HasValue) coms.Add(reader.Value);
                            break;
                    }                
                }
                
                if (messages.Length > 0) throw new DocumentInvalidException($"Unable to parse the XML file: {messages.ToString()}");    
                if(validation == ValidationType.Schema && reader.Settings.Schemas.Count == 0) throw new DocumentInvalidException("No XSD has been found within the document.");     
                                
                XmlDoc = new XmlDocument();
                XmlDoc.Load(filePath);      
                Comments = coms.ToArray();
                Raw = File.ReadAllText(filePath);
                
                //NOTE: When using a default namespace, some XPath queries fails within SelectNodes/CountNodes methods, so default namespace will be removed from the parsed document                
                var noDefaultNamespace = Raw;
                var start = noDefaultNamespace.IndexOf("xmlns=");
                if(start >= 0){
                    var left = noDefaultNamespace.Substring(0, start);
                    var right = noDefaultNamespace.Substring(start + 7);

                    right = right.Substring(right.IndexOf('"')+1);

                    if(left.EndsWith(" ") && right.StartsWith(">")) left = left.TrimEnd();
                    noDefaultNamespace = left + right;
                    
                    XmlDocNoDefaultNamespace = new XmlDocument();
                    XmlDocNoDefaultNamespace.LoadXml(noDefaultNamespace);                
                }                
            }
            catch(XmlException ex){
                throw new DocumentInvalidException("Unable to parse the XML file.", ex);     
            }            
        }
        
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        }

        /// <summary>
        /// Requests for a set of nodes.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>A list of nodes.</returns>
        public List<XPathNavigator> SelectNodes(string xpath, XmlNodeType type = XmlNodeType.ALL){            
            return  SelectNodes((XmlNode)XmlDoc, xpath, type);
        }            

        /// <summary>
        /// Requests for a set of nodes.
        /// </summary>
        /// <param name="root">Root node from where the XPath expression will be evaluated.</param>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>A list of nodes.</returns>
        public List<XPathNavigator> SelectNodes(XmlNode root, string xpath, XmlNodeType type = XmlNodeType.ALL){
            if(root == null) return null;
            else{                
                List<XPathNavigator> set = null;                
                var xDoc = new XPathDocument(new XmlNodeReader(root));
                var xNav = xDoc.CreateNavigator();                  
                
                try{    
                    //First try witj XPath 1.0 due compatibility issues with namespaces
                    set = xNav.Select(xpath).Cast<XPathNavigator>().ToList();                   
                }
                catch{
                    //If fails, XPath 2.0 will be used instead (it wont work properly to get namespaces being used...)
                    //NOTE: First ToList() needed in order to load the full document.
                    set = xNav.XPath2Select(xpath).ToList().Cast<XPathNavigator>().ToList();    
                }
                
                if(set.Count == 0 && XmlDocNoDefaultNamespace != null){
                    var doc = XmlDocNoDefaultNamespace;
                    XmlDocNoDefaultNamespace = null;    //this avoids infinite recursive calls
                    set = SelectNodes((XmlNode)doc, xpath, type);
                    XmlDocNoDefaultNamespace = doc;        

                    return set;            
                } 
                else{
                    if(type == XmlNodeType.ALL) return set;
                    else{
                        double d;
                        bool b;
                        var match = new List<XPathNavigator>();                        

                        foreach(var node in set){   
                            var content = node.SelectSingleNode("text()");
                            var value = (content == null ? (node.NodeType == XPathNodeType.Attribute ? node.Value : null) : content.Value);
                            
                            if(value != null){
                                var isNum = double.TryParse(value, out d);
                                var isBool = bool.TryParse(value, out b);

                                if(type == XmlNodeType.NUMERIC && isNum) match.Add(node);
                                else if(type == XmlNodeType.BOOLEAN && isBool) match.Add(node);
                                else if(type == XmlNodeType.STRING && !isNum && !isBool) match.Add(node);
                            }
                        }

                        return match;
                    }       
                }      
            } 
        }

        /// <summary>
        /// Requests for the amount of nodes.
        /// </summary>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>The amount of nodes.</returns>
        public int CountNodes(string xpath, XmlNodeType type = XmlNodeType.ALL){            
            return  CountNodes((XmlNode)XmlDoc, xpath, type);
        }
        
        /// <summary>
        /// Requests for the amount of nodes.
        /// </summary>
        /// <param name="root">Root node from where the XPath expression will be evaluated.</param>
        /// <param name="xpath">XPath expression.</param>
        /// <returns>The amount of nodes.</returns>
        public int CountNodes(XmlNode root, string xpath, XmlNodeType type = XmlNodeType.ALL){
            return SelectNodes(root, xpath, type).Count;
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public bool Equals(XmlDocument xmlDoc, bool ignoreSchema = true){
            return Equals(this.XmlDoc, xmlDoc, ignoreSchema);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public bool Equals(Xml xmlConn, bool ignoreSchema = true){
            return Equals(xmlConn.XmlDoc, ignoreSchema);            
        }

        private bool Equals(XmlNode left, XmlNode right, bool ignoreSchema, string xpath = "./*"){
            var leftChild = left.SelectNodes(xpath).Cast<XmlNode>().ToList();
            var rightChild = right.SelectNodes(xpath).Cast<XmlNode>().ToList();

            if(ignoreSchema){
                Func<XmlNode, bool> filter = (x => x.NodeType != System.Xml.XmlNodeType.Attribute || (x.NodeType == System.Xml.XmlNodeType.Attribute && !x.Name.StartsWith("xmlns")  && !x.Name.Contains(":")));
                leftChild = leftChild.Where(filter).ToList();
                rightChild = rightChild.Where(filter).ToList();
            }

            var tmp = leftChild.Count != rightChild.Count;
            if(leftChild.Count != rightChild.Count) return false;
            else{
                for(int i=0; i<leftChild.Count; i++){
                    //Attribute comparisson
                    var leftNode = leftChild[i];
                    var rightNode = rightChild[i];
                    var eq = Equals(leftNode, rightNode, ignoreSchema, "./@*");
                    if(!eq) return false;
                    
                    //Node name and value comparisson
                    var leftName = leftNode.Name;
                    var rightName = rightNode.Name;
                    if(ignoreSchema){
                        if(leftName.Contains(":")) leftName = leftName.Substring(leftName.IndexOf(":")+1);
                        if(rightName.Contains(":")) rightName = rightName.Substring(rightName.IndexOf(":")+1);
                    }

                    tmp = leftName.Equals(rightName);                
                    if(!leftName.Equals(rightName)) return false;

                    tmp = GetNodeContentValue(leftNode) != GetNodeContentValue(rightNode);
                    if(GetNodeContentValue(leftNode) != GetNodeContentValue(rightNode)) return false;

                    //Children comparisson
                    eq = Equals(leftNode, rightNode, ignoreSchema);
                    if(!eq) return false;
                }
            }

            return true;
        }

        private string GetNodeContentValue(XmlNode node){
            var content = node.SelectSingleNode("text()");
            return (content == null ? (node.NodeType == System.Xml.XmlNodeType.Attribute ? node.Value : null) : content.Value);
        }
    }
}