using ExCSS;
using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Text;
using System.Linq;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    public abstract class ValidatorBaseHtml5 : ValidatorBase{
        public string StudentFolder {get; private set;}
        public HtmlDocument HtmlDoc {get; private set;}
        public Stylesheet CssDoc {get; private set;}
        protected ValidatorBaseHtml5(string studentFolder): base(){
            this.StudentFolder = studentFolder;
        } 
        public new void Dispose()
        {                                   
            base.Dispose();
        }   
        protected bool LoadHtml5Document(string fileName){
            Terminal.WriteLine(string.Format("Validating the file ~{0}:", fileName), ConsoleColor.DarkBlue);
            Terminal.Indent();

            OpenTest("Loading the file... ");            
            HtmlDocument htmlDoc = LoadHtmlFile(fileName);        
            if(htmlDoc == null) CloseTest("Unable to read the HTML file.", 0);
            else{
                CloseTest(string.Empty, 0);

                OpenTest("Validating against the W3C official validation tool... ");
                if(W3CSchemaValidationForHtml5(htmlDoc)) CloseTest(string.Empty, 0);
                else CloseTest("Unable to validate.", 0);
            }
            
            Terminal.UnIndent();
            this.HtmlDoc = htmlDoc;
            return htmlDoc != null;
        }
        private HtmlDocument LoadHtmlFile(string fileName){        
            string filePath = Directory.GetFiles(this.StudentFolder, fileName, SearchOption.AllDirectories).FirstOrDefault();            
            if(string.IsNullOrEmpty(filePath)) return null;
            else{
                string sourceCode = File.ReadAllText(filePath);
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.Load(filePath);   
                return htmlDoc;       
            }                   
        }
        protected bool LoadCss3Document(string fileName){
            Terminal.WriteLine(string.Format("Validating the file ~{0}:", fileName), ConsoleColor.DarkBlue);
            Terminal.Indent();
            
            OpenTest("Loading the file... ");                
            Stylesheet stylesheet = null;        
            string cssDoc = LoadCssFile(fileName);                      

            //one point if the CSS name is OK
            if(!string.IsNullOrEmpty(cssDoc)) CloseTest(string.Empty); 
            else{                                
                cssDoc = LoadCssFile("*.css");
                
                if(string.IsNullOrEmpty(cssDoc)) CloseTest("Unable to read the CSS file.");
                else CloseTest("Incorrect file name"); 
            }

            if(!string.IsNullOrEmpty(cssDoc)){
                OpenTest("Validating against the W3C official validation tool... ");

                if(!W3CSchemaValidationForCss3(cssDoc)) CloseTest("Unable to validate.", 0);
                else{
                    CloseTest(string.Empty, 0); 

                    OpenTest("Parsing the CSS file... ");
                    StylesheetParser parser = new StylesheetParser();       
                    stylesheet = parser.Parse(cssDoc);

                    if(stylesheet == null) CloseTest("Unable to parse the CSS file.", 0);
                    else CloseTest(string.Empty, 0);
                }
            }

            Terminal.UnIndent();        
            this.CssDoc = stylesheet;
            return stylesheet != null;
        }
        private string LoadCssFile(string fileName){           
            string filePath = Directory.GetFiles(this.StudentFolder, fileName, SearchOption.AllDirectories).FirstOrDefault();            
            if(string.IsNullOrEmpty(filePath)) return null;
            else return File.ReadAllText(filePath);
        }
        private bool W3CSchemaValidationForHtml5(HtmlDocument htmlDoc){
            string html = string.Empty;
            string url = "https://validator.nu?out=xml";
            byte[] dataBytes = Encoding.UTF8.GetBytes(htmlDoc.Text);

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
                    return false;
            }            

            //TODO: send the errors list
            return true;
        }
        private bool W3CSchemaValidationForCss3(string cssDoc){
            string html = string.Empty;
            string url = "http://jigsaw.w3.org/css-validator/validator";

            //WARNING: some properties are not validating properly throug the API like border-radius.
            string css = System.Web.HttpUtility.UrlEncode(cssDoc.Replace("\r\n", ""));//.Replace("border-radius", "border-width"));
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
            return errorCount == 0;            
        }
    }
}