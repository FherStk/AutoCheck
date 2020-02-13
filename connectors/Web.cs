using ExCSS;
using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Text;
using System.Linq;
using HtmlAgilityPack;

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
        public int CountNodes(string node){
            return this.HtmlDoc.DocumentNode.SelectNodes("//h1").Count();            
        }
    }
}