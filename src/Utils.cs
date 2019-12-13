using System;
using Npgsql;
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
        public static void PrintScore(int score){
            PrintScore(score, 0);
        }
        public static void PrintScore(int success, int errors){
            float score = ((float)success / (float)(success + errors))*10;
            
            Utils.BreakLine(); 
            Utils.Write("   TOTAL SCORE: ", ConsoleColor.Cyan);
            Utils.Write(Math.Round(score, 2).ToString(), (score < 5 ? ConsoleColor.Red : ConsoleColor.Green));
            Utils.BreakLine();
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
        public static bool CreateDataBase(string server, string database, string sqlDump)
        {
            Write("Creating database for the student ");
            Write(database.Substring(database.IndexOf("_")+1).Replace("_", " "), ConsoleColor.DarkYellow);
            Write(": ");

            string defaultWinPath = "C:\\Program Files\\PostgreSQL\\10\\bin";   
            string cmdPassword = "PGPASSWORD=postgres";
            string cmdCreate = string.Format("createdb -h {0} -U postgres -T template0 {1}", server, database);
            string cmdRestore = string.Format("psql -h {0} -U postgres {1} < {2}", server, database, sqlDump);            
            Response resp = null;
            List<string> errors = new List<string>();

            switch (OS.GetCurrent())
            {
                  //TODO: this must be correctly configured as a path wehn a terminal session begins
                  //Once path is ok on windows and unix the almost same code will be used.
                  case "win":                  
                    resp = Shell.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdCreate), Output.Hidden, defaultWinPath);
                    if(resp.code > 0) errors.Add(resp.stderr.Replace("\n", ""));

                    resp = Shell.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdRestore), Output.Hidden, defaultWinPath);
                    if(resp.code > 0) errors.Add(resp.stderr.Replace("\n", ""));
                    
                    break;

                case "mac":                
                case "gnu":
                    resp = Shell.Term(string.Format("{0} {1}", cmdPassword, cmdCreate));
                    if(resp.code > 0) errors.Add(resp.stderr.Replace("\n", ""));

                    resp = Shell.Term(string.Format("{0} {1}", cmdPassword, cmdRestore));
                    if(resp.code > 0) errors.Add(resp.stderr.Replace("\n", ""));
                    break;
            }   

            PrintResults(errors);
            return (errors.Count == 0);
        }
        public static bool DataBaseExists(string server, string database)
        {
            Write("Checking if a database exists for the student ");
            Write(database.Substring(database.IndexOf("_")+1).Replace("_", " "), ConsoleColor.DarkYellow);
            Write(": ");
            
            bool exist = true;
            List<string> errors = new List<string>();            
            using (NpgsqlConnection conn = new NpgsqlConnection(string.Format("Server={0};User Id={1};Password={2};Database={3};", server, "postgres", "postgres", database))){
                try{
                    conn.Open();                    
                }               
                catch(Exception e){                    
                    if(e.Message.Contains(string.Format("database \"{0}\" does not exist", database))) exist = false;                       
                    else throw e;
                } 
            }

            PrintResults(errors);
            return (exist);
        }
    }
}