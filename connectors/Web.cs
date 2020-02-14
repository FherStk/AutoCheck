using ExCSS;
using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Text;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator.Connectors{
    public class Web: Core.Connector{         
        
        public HtmlDocument HtmlDoc {get; private set;}
        public Stylesheet CssDoc {get; private set;}

        public Web(string studentFolder, string htmlFile, string cssFile=""){
            this.HtmlDoc = LoadHtmlFile(Directory.GetFiles(studentFolder, htmlFile, SearchOption.AllDirectories).FirstOrDefault());
            this.CssDoc = LoadCssFile(Directory.GetFiles(studentFolder, cssFile, SearchOption.AllDirectories).FirstOrDefault());
        } 

        private HtmlDocument LoadHtmlFile(string filePath){        
            if(string.IsNullOrEmpty(filePath)) return null;
            else{
                string sourceCode = File.ReadAllText(filePath);
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.Load(filePath);   
                return htmlDoc;       
            }                   
        }
        private Stylesheet LoadCssFile(string filePath){           
            if(string.IsNullOrEmpty(filePath)) return null;
            else{
                StylesheetParser parser = new StylesheetParser();       
                return parser.Parse(File.ReadAllText(filePath));
            }
        }
        public void ValidateHTML5AgainstW3C(){
            string html = string.Empty;
            string url = "https://validator.nu?out=xml";
            byte[] dataBytes = Encoding.UTF8.GetBytes(this.HtmlDoc.Text);

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
                    throw new Exception("Inavlid document."); //TODO: add the error description
            }                        
        }
        public void ValidateCSS3AgainstW3C(){
            string html = string.Empty;
            string url = "http://jigsaw.w3.org/css-validator/validator";
            string css = System.Web.HttpUtility.UrlEncode(this.CssDoc.ToCss().Replace("\r\n", ""));
            string parameters = string.Format("profile=css3&output=soap12&warning=0&text={0}", css);            
            byte[] dataBytes = System.Web.HttpUtility.UrlEncodeToBytes(parameters);

            //Documentation:    https://jigsaw.w3.org/css-validator/manual.html
            //                  https://jigsaw.w3.org/css-validator/api.html#requestformat            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("{0}?{1}", url, parameters));
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;            
    
            XmlDocument document = new XmlDocument();
            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using(Stream stream = response.GetResponseStream())            
            using(StreamReader reader = new StreamReader(stream))
            {
                string output = reader.ReadToEnd();                             
                document.LoadXml(output); 
            }
                        
            int errorCount = int.Parse(document.GetElementsByTagName("m:errorcount")[0].InnerText);
            if(errorCount > 0) throw new Exception("Inavlid document."); //TODO: add the error description            
        }  
        /// <summary>
        /// Count how many nodes of this kind are within the document.
        /// </summary>
        /// <param name="xpath">XPath expression</param>
        /// <returns></returns>
        public List<HtmlNode> SelectNodes(string xpath){
            return SelectNodes(this.HtmlDoc.DocumentNode, xpath);
        }
        /// <summary>
        /// Count how many nodes of this kind are within the document.
        /// </summary>
        /// <param name="xpath">XPath expression</param>
        /// <returns></returns>
        public List<HtmlNode> SelectNodes(HtmlNode root, string xpath){
            return root.SelectNodes(xpath).ToList();
        }
        /// <summary>
        /// Count how many nodes of this kind are within the document.
        /// </summary>
        /// <param name="xpath">XPath expression</param>
        /// <param name="root"></param>
        /// <returns></returns>
        public int CountNodes(string xpath){
            return CountNodes(this.HtmlDoc.DocumentNode, xpath);
        }
        /// <summary>
        /// Count how many nodes of this kind are within the document.
        /// </summary>
        /// <param name="xpath">XPath expression</param>
        /// <param name="root"></param>
        /// <returns></returns>
        public int CountNodes(HtmlNode root, string xpath){
            var nodes = root.SelectNodes(xpath);
            return (root == null ? 0 : nodes.Count());
        }
        /// <summary>
        /// Count how many nodes of this kind are siblings between them within the document.
        /// </summary>
        /// <param name="xpath">XPath expression</param>
        /// <param name="node"></param>
        /// <returns></returns>
        public int[] CountSiblings(string xpath){   
            return CountSiblings(this.HtmlDoc.DocumentNode, xpath);
        }
        /// <summary>
        /// Count how many nodes of this kind are siblings between them within the document.
        /// </summary>
        /// <param name="xpath">XPath expression</param>
        /// <param name="root"></param>
        /// <returns></returns>
        public int[] CountSiblings(HtmlNode root, string xpath){            
            List<int> total = new List<int>();
            int count = 0;
            HtmlNode lastParent = null;
            foreach(HtmlNode n in root.SelectNodes(xpath).OrderBy(x => x.ParentNode)){                                    
                if(n.ParentNode != lastParent && lastParent != null){
                    total.Add(count);
                    lastParent = n.ParentNode;
                    count = 0;
                }
                
                count++;
            }
            
            total.Add(count);
            return total.ToArray();            
        }
        /// <summary>
        /// The length of a node content, sum of all of them i there's more than one.
        /// </summary>
        /// <param name="xpath">XPath expression</param>
        /// <returns></returns>
        public int ContentLength(string xpath){
            return ContentLength(this.HtmlDoc.DocumentNode, xpath);            
        }
        /// <summary>
        /// The length of a node content, sum of all of them i there's more than one.
        /// </summary>
        /// <param name="xpath">XPath expression</param>
        /// <returns></returns>
        public int ContentLength(HtmlNode root, string xpath){
            var nodes = root.SelectNodes(xpath);
            return (root == null ? 0 : nodes.Sum(x => x.InnerText.Length));
        }
         /// <summary>
        /// Returns the label nodes related to the xpath resulting nodes.
        /// </summary>
        /// <param name="xpath">XPath expression</param>
        /// <returns></returns>
        public Dictionary<HtmlNode, HtmlNode[]> GetRelatedLabels(string xpath){                
            return GetRelatedLabels(this.HtmlDoc.DocumentNode, xpath);
        }
        /// <summary>
        /// Returns the label nodes related to the xpath resulting nodes.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="xpath">XPath expression</param>
        /// <returns></returns>
        public Dictionary<HtmlNode, HtmlNode[]> GetRelatedLabels(HtmlNode root, string xpath){            
            var results = new Dictionary<HtmlNode, HtmlNode[]>();
            
            foreach(HtmlNode node in root.SelectNodes(xpath)){
                string id = node.GetAttributeValue("id", "");
                if(string.IsNullOrEmpty(id)) results.Add(node, null);
                else{
                    HtmlNodeCollection labels = this.HtmlDoc.DocumentNode.SelectNodes("//label");
                    if(labels == null) results.Add(node, null);
                    else results.Add(node, labels.Where(x => x.GetAttributeValue("for", "").Equals(id)).ToArray());
                }
            }

            return results; 
        }
        public void CheckTableConsistence(string xpath){
            CheckTableConsistence(this.HtmlDoc.DocumentNode, xpath);
        }
        public void CheckTableConsistence(HtmlNode root, string xpath){
            foreach(HtmlNode node in root.SelectNodes(xpath)){  
                int row = 1;              
                int cols = CountNodes(node, "tr[1]/td");            

                foreach(HtmlNode tr in SelectNodes(node, "tr")){                    
                    int current = CountNodes(tr, "td");

                    if(current != cols){
                        //check the colspan
                        int colspan = 0;
                        foreach(HtmlNode td in SelectNodes(tr, "td")){
                            if(td.Attributes["colspan"] != null){
                                current -= 1;
                                colspan += int.Parse(td.Attributes["colspan"].Value);
                            } 
                        }

                        if(colspan+current != cols) throw new Exception(string.Format("Inconsistence detected on row {0}, amount of columns missmatch: expected->'{1}' found->'{2}'.", row, cols, colspan));
                    }

                    row ++;
                }
            }
        }
    }
}