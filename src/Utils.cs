using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Text;
using System.Linq;
using HtmlAgilityPack;
using ToolBox.Bridge;
using ToolBox.Platform;
using ToolBox.Notification;
using System.Collections.Generic;

namespace AutomatedAssignmentValidator{
    class Utils{
        private static INotificationSystem _notificationSystem { get; set; }
        private static IBridgeSystem _bridgeSystem { get; set; }
        private static ShellConfigurator _shell { get; set; }
        public static ShellConfigurator Shell { 
            get{ 
                if(_shell == null){
                    //https://github.com/deinsoftware/toolbox#system
                    //This is used in order to launch terminal commands on diferent OS systems (Windows + Linux + Mac)
                    _notificationSystem = NotificationSystem.Default;
                    switch (OS.GetCurrent())
                    {
                        case "win":
                            _bridgeSystem = BridgeSystem.Bat;
                            break;
                        case "mac":
                        case "gnu":
                            _bridgeSystem = BridgeSystem.Bash;
                            break;
                    }
                    _shell = new ShellConfigurator(_bridgeSystem, _notificationSystem);                    
                }
                
                return _shell;
            }
        }

        public static void PrintResults(List<string> errors){
            string prefix = "\n\t-";
            if(errors.Count == 0) WriteLine("OK", ConsoleColor.DarkGreen);
            else{
                if(errors.Where(x => x.Length > 0).Count() == 0) WriteLine("ERROR", ConsoleColor.Red);
                else WriteLine(string.Format("ERROR: {0}{1}", prefix, string.Join(prefix, errors)), ConsoleColor.Red);
            }
        }
        public static HtmlDocument LoadHtmlDocument(string studentFolder, string fileName){
            Write("      Loading the file...");

            string filePath = Directory.GetFiles(studentFolder, fileName, SearchOption.AllDirectories).FirstOrDefault();            
            if(string.IsNullOrEmpty(filePath)){
                WriteLine("ERROR", ConsoleColor.Red);
                return null;
            }
            WriteLine("OK", ConsoleColor.DarkGreen);            

            Write("      Validating against the W3C official validation tool... ");
            if(!W3CSchemaValidationForHtml5(filePath)){
                WriteLine("ERROR", ConsoleColor.Red);
                return null;
            }
            WriteLine("OK", ConsoleColor.DarkGreen);
           
            string sourceCode = File.ReadAllText(filePath);
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(filePath);   
            return htmlDoc;            
        }
        public static string LoadCssDocument(string studentFolder, string fileName){
            Write("      Loading the file...");

            string filePath = Directory.GetFiles(studentFolder, fileName, SearchOption.AllDirectories).FirstOrDefault();            
            if(string.IsNullOrEmpty(filePath)){
                WriteLine("ERROR", ConsoleColor.Red);
                return null;
            }
            WriteLine("OK", ConsoleColor.DarkGreen);

            Write("      Validating against the W3C official validation tool... ");
            if(!W3CSchemaValidationForCss3(filePath)){
                WriteLine("ERROR", ConsoleColor.Red);
                return null;
            }
            WriteLine("OK", ConsoleColor.DarkGreen);
           
            string sourceCode = File.ReadAllText(filePath);            
            return sourceCode;
        }
        private static bool W3CSchemaValidationForHtml5(string filePath){
            string html = string.Empty;
            string url = "https://validator.nu?out=xml";
            byte[] dataBytes = Encoding.UTF8.GetBytes(File.ReadAllText(filePath));

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

            return true;
        }
        private static bool W3CSchemaValidationForCss3(string filePath){
            string html = string.Empty;
            string url = "http://jigsaw.w3.org/css-validator/validator";

            //WARNING: some properties are not validating properly throug the API like border-radius.
            string css = System.Web.HttpUtility.UrlEncode(File.ReadAllText(filePath).Replace("\r\n", "").Replace("border-radius", "border-width"));
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
                        
            int errorCount =  int.Parse(document.GetElementsByTagName("m:errorcount")[0].InnerText);
            return errorCount == 0;            
        }
        public static void BreakLine(int lines = 1){
            for(int i=0; i < lines; i++)
                WriteLine("");
        } 
        public static void Write(string text, ConsoleColor color = ConsoleColor.Gray){
            WriteColor(false, text, color);
        }  
        public static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray){
            WriteColor(true, text, color);
        }
        public static void WriteColor(bool newLine, string text, ConsoleColor color){
            Console.ResetColor();   
            Console.ForegroundColor = color;     
            if(newLine) Console.WriteLine(text);
            else Console.Write(text);
            Console.ResetColor();   
        }
        public static string MoodleFolderToStudentName(string folder){
            string studentFolder = Path.GetFileName(folder);
            int i = studentFolder.IndexOf("_"); //Moodle assignments download uses "_" in order to separate the student name from the assignment ID
            return studentFolder.Substring(0, i);
        }      
    }
}