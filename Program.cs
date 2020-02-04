using Npgsql;
using System;
using System.IO;
using System.Linq;
using ToolBox.Bridge;
using ToolBox.Platform;
using ToolBox.Notification;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace AutomatedAssignmentValidator
{
    class Program
    {
        private static INotificationSystem _notificationSystem { get; set; }
        private static IBridgeSystem _bridgeSystem { get; set; }
        private static ShellConfigurator _shell { get; set; }
        private static ShellConfigurator Shell { 
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
        private static string _PATH = null; 
        private static string _FOLDER = null; 
        private static AssignType _ASSIG = AssignType.UNDEFINED; 
        private static string _SERVER = null; 
        private static string _DATABASE = null;  
        private enum AssignType{
            CSS3,
            HTML5,
            ODOO,
            PERMISSIONS,
            VIEWS,
            VIEWSEXTENDED,
            UNDEFINED,
            SQLLOG

        }  

        static void Main(string[] args)
        {
            Terminal.BreakLine();
            Terminal.Write("Automated Assignment Validator: ", ConsoleColor.Yellow);                        
            Terminal.WriteLine("v2.0.0.0");
            Terminal.Write(String.Format("Copyright © {0}: ", DateTime.Now.Year), ConsoleColor.Yellow);            
            Terminal.WriteLine("Fernando Porrino Serrano.");
            Terminal.Write(String.Format("Under the AGPL license: ", DateTime.Now.Year), ConsoleColor.Yellow);            
            Terminal.WriteLine("https://github.com/FherStk/ASIX-DAM-M04-WebAssignmentValidator/blob/master/LICENSE");
            
            //TODO: list of chanes and ideas
            /*
                Each new validator will be a script, which will call pre-defined actions.
                Each current validator script actions will be spliten into self-contained tests, so all validators will be able tu use those.
                Each validator must declare and code a Student method and a Group method: 
                    Student: contains what to test for a single student.
                    Group:   contains how to loop through each group of students
                             on certain cases, a call to Student is not needed (sql log comparator) but in all the other cases it is...
                Terminal will be renamed to Output, and it will prompt to the terminal but also collect all the data into a returnable item.                
                Terminal class will now be used to exec commands on local or remote computers, it will be used by the scripts.
                Each current ValidatorBase methods will be stored into a helper class (Core.Helpers.Html, Core.Helpers.DataBase)                
                For the moment, each validator (or script) will be coded in C#, but maybe a YAML file could be used too...
                A new download from / upload to Moodle class will be created, but maybe on v3...
            */

            LoadArguments(args);
            
            Terminal.BreakLine();            
            if(_ASSIG == AssignType.UNDEFINED) Terminal.WriteResponse("A parameter 'assig' must be provided with an accepted value (see README.md).");
            else{
                Terminal.WriteLine(string.Format("Running test ~{0}: ", _ASSIG.ToString()), ConsoleColor.Cyan);    
                Terminal.Indent();

                if(string.IsNullOrEmpty(_PATH)) CheckFolder();
                else CheckPath(); 
                
                Terminal.UnIndent();           
            }           
            
            Terminal.BreakLine();
            Terminal.WriteLine("Press any key to close.");
            Console.ReadKey();
        }   
        private static void LoadArguments(string[] args){
            for(int i = 0; i < args.Length; i++){
                if(args[i].StartsWith("--") && args[i].Contains("=")){
                    string[] data = args[i].Split("=");
                    string param = data[0].ToLower().Trim().Replace("\"", "").Substring(2);
                    string value = data[1].Trim().Replace("\"", "");
                    
                    switch(param){
                        case "path":
                            _PATH = value;
                            break;

                        case "folder":
                            _FOLDER = value;
                            break;
                        
                        case "assig":
                            try{
                                _ASSIG = (AssignType)Enum.Parse(typeof(AssignType), value, true);    
                            }                            
                            catch{
                                _ASSIG = AssignType.UNDEFINED;
                            }
                                                        
                            break;

                        case "server":
                            _SERVER = value;                            
                            break;

                        case "database":
                            _DATABASE = value;
                            break;
                    }                                        
                }                                
            }
        }            
        //TODO: CheckPath and CheckFolder are only used within the main program, but could be usefull to be called from the outside as a library.../
        //IDEA: Use events like "pre-path", "post-path", "pre-folder"... So some code could be moved within its Validator or ValidatorBase?
        private static void CheckPath()
        { 
            if(!Directory.Exists(_PATH)) Terminal.WriteLine(string.Format("The provided path '{0}' does not exist.", _PATH), ConsoleColor.Red);   
            else{
               
                //The path must contain a set of folders following the Moddle's assignement batch download convention.
                //TODO: test this for the ViewsValidator.
                foreach(string f in Directory.EnumerateDirectories(_PATH))
                {
                    _FOLDER = f; 
                    _DATABASE = string.Empty;   //no database can be selected when using 'path' mode

                    try{                                                                        
                        UnZip(f);    
                        Terminal.Indent();
                        CheckFolder();
                        Terminal.UnIndent();
                    }
                    catch (Exception e){
                        Terminal.WriteResponse(string.Format("ERROR {0}", e.Message));
                        Terminal.UnIndent();
                    }
                    finally{                        
                        if(_ASSIG != AssignType.SQLLOG){
                            Terminal.WriteLine("Press any key to continue...");                                                
                            Console.ReadKey(); 
                        } 
                        
                        Terminal.BreakLine();
                    }
                } 

                if(_ASSIG == AssignType.SQLLOG){
                    //SPECIAL CASE:
                    //All the data has been collecyed, so the result can be computed and printed now.
                    SqlLogValidator sqlLogVal = SqlLogValidator.Instance;
                    Terminal.Write("Comparing the SQL log files between each other... ");
                    try{
                        sqlLogVal.Compare();
                        Terminal.WriteResponse();
                    }
                    catch (Exception ex){
                        Terminal.WriteResponse(ex.Message);
                    }  

                    try{
                        Terminal.WriteLine("Printing the results: ");
                        Terminal.Indent();
                        sqlLogVal.Print();         
                        sqlLogVal.ClearResults();
                    }
                    catch (Exception ex){
                        Terminal.WriteResponse(ex.Message);
                    }                    
                    finally{
                        Terminal.UnIndent();
                    }
                }                        
                       
            }                            
        }  
        private static void CheckFolder()
        { 
            ValidatorBase val = null;

            //TODO: It could be interesting to move database creation to the ValidatorBaseDataBase into (for example) the constructor
            switch(_ASSIG){
                case AssignType.SQLLOG:   
                case AssignType.HTML5:
                case AssignType.CSS3:
                    if(string.IsNullOrEmpty(_FOLDER)) Terminal.WriteResponse(string.Format("The parameter 'folder' or 'path' must be provided when using 'assig={0}'.", _ASSIG.ToString().ToLower()));
                    if(!Directory.Exists(_FOLDER)) Terminal.WriteResponse(string.Format("Unable to find the provided folder '{0}'.", _FOLDER));
                    else{
                        switch(_ASSIG){
                            case AssignType.HTML5:
                                val = new Html5Validator(_FOLDER);
                                break;
                            
                            case AssignType.CSS3:
                                val = new Css3Validator(_FOLDER);
                                break;

                            case AssignType.SQLLOG:
                                //NOTE: this validator follows the singleton pattern in order to accumulate the student's logs.
                                val = SqlLogValidator.Instance;
                                ((SqlLogValidator)val).StudentFolder = _FOLDER;

                                break;
                        }                                         
                    }                     
                    break;           

                case AssignType.ODOO:       
                case AssignType.PERMISSIONS:   
                case AssignType.VIEWS:                   
                    try{
                        bool exist = false;
                        if(string.IsNullOrEmpty(_DATABASE)){
                            _DATABASE = Core.Utils.FolderNameToDataBase(_FOLDER, (_ASSIG == AssignType.ODOO ? "odoo" : "empresa"));
                            string sqlDump = Directory.GetFiles(_FOLDER, "*.sql", SearchOption.AllDirectories).FirstOrDefault();
                            
                            if(string.IsNullOrEmpty(sqlDump)) Terminal.WriteResponse(string.Format("The current folder '{0}' does not contains any sql file.", _FOLDER));
                            else if(string.IsNullOrEmpty(_SERVER)) Terminal.WriteResponse("The parameter 'server' must be provided when using --assig=odoo.");
                            else{
                                exist = DataBaseExists(_SERVER, _DATABASE);
                                if(!exist) exist = CreateDataBase(_SERVER, _DATABASE, sqlDump);
                                if(!exist) Terminal.WriteResponse(string.Format("Unable to create the database '{0}' on server '{1}'.", _DATABASE, _SERVER));
                            }

                            if(!exist) break;
                        }                          

                        if(!exist) exist = DataBaseExists(_SERVER, _DATABASE);
                        if(!exist) Terminal.WriteResponse(string.Format("Unable to create the database '{0}' on server '{1}'.", _DATABASE, _SERVER));
                        else {

                            switch(_ASSIG){
                                case AssignType.ODOO:
                                    val = new OdooValidator(_SERVER, _DATABASE);
                                    break;

                                case AssignType.PERMISSIONS:
                                    val = new PermissionsValidator(_SERVER, _DATABASE);
                                    break;

                                case AssignType.VIEWS:
                                    val = new ViewsValidator(_SERVER, _DATABASE);
                                    break;
                            }                                  
                        }                                                                                  
                    }
                    catch(Exception e){
                        Terminal.WriteResponse(string.Format("EXCEPTION: {0}", e.Message));
                    }                    
                    break;

                default:
                    Terminal.WriteResponse(string.Format("No check method has been defined for the assig '{0}'.", _ASSIG));
                    break;
            } 

            if(val != null){
                using(val){                    
                    val.Validate(); 
                }   
            }                                     
        }
        private static void UnZip(string folder){
            string student = Core.Utils.MoodleFolderToStudentName(folder);
            if(string.IsNullOrEmpty(student)){
                Terminal.WriteLine(string.Format("Skipping folder ~{0}: ", Path.GetFileNameWithoutExtension(folder)), ConsoleColor.DarkYellow);                                    
                return;
            }

            Terminal.WriteLine(string.Format("Checking files for the student ~{0}: ", student), ConsoleColor.DarkYellow);
            string zip = Directory.GetFiles(folder, "*.zip", SearchOption.AllDirectories).FirstOrDefault();    

            if(!string.IsNullOrEmpty(zip)){
                Terminal.Write("Unzipping the files: ");

                try{
                    ExtractZipFile(zip);
                    Terminal.WriteResponse();
                }
                catch(Exception e){
                    Terminal.WriteResponse(string.Format("ERROR {0}", e.Message));
                    return;
                }
                
                Terminal.Write("Removing the zip file: ");
                try{
                    File.Delete(zip);
                    Terminal.WriteResponse();
                }
                catch(Exception e){
                    Terminal.WriteResponse(string.Format("ERROR {0}", e.Message));
                    //the process can continue
                }
                finally{
                    Terminal.BreakLine();
                }                                              
            }
        }
        private static void ExtractZipFile(string zipPath, string password = null){
            ExtractZipFile(zipPath, Path.GetDirectoryName(zipPath), null);
        } 
        private static void ExtractZipFile(string zipPath, string outFolder, string password = null) {
            //source:https://github.com/icsharpcode/SharpZipLib/wiki/Unpack-a-Zip-with-full-control-over-the-operation
            using(Stream fsInput = File.OpenRead(zipPath)){ 
                using(ZipFile zf = new ZipFile(fsInput)){
                    
                    if (!String.IsNullOrEmpty(password)) {
                        // AES encrypted entries are handled automatically
                        zf.Password = password;
                    }

                    foreach (ZipEntry zipEntry in zf) {
                        if (!zipEntry.IsFile) {
                            // Ignore directories
                            continue;
                        }

                        String entryFileName = zipEntry.Name;
                        // to remove the folder from the entry:
                        //entryFileName = Path.GetFileName(entryFileName);
                        // Optionally match entrynames against a selection list here
                        // to skip as desired.
                        // The unpacked length is available in the zipEntry.Size property.

                        // Manipulate the output filename here as desired.
                        var fullZipToPath = Path.Combine(outFolder, entryFileName);
                        var directoryName = Path.GetDirectoryName(fullZipToPath);
                        if (directoryName.Length > 0) {
                            Directory.CreateDirectory(directoryName);
                        }

                        // 4K is optimum
                        var buffer = new byte[4096];

                        // Unzip file in buffered chunks. This is just as fast as unpacking
                        // to a buffer the full size of the file, but does not waste memory.
                        // The "using" will close the stream even if an exception occurs.
                        using(var zipStream = zf.GetInputStream(zipEntry))
                        using (Stream fsOutput = File.Create(fullZipToPath)) {
                            StreamUtils.Copy(zipStream, fsOutput , buffer);
                        }
                    }
                }
            }
        }
        private static bool DataBaseExists(string server, string database)
        {
            Terminal.Write(string.Format("Checking if a database exists for the student ~{0}... ", Core.Utils.DataBaseToStudentName(database)), ConsoleColor.DarkYellow);                
            
            bool exist = true;            
            using (NpgsqlConnection conn = new NpgsqlConnection(string.Format("Server={0};User Id={1};Password={2};Database={3};", server, "postgres", "postgres", database))){
                try{
                    conn.Open();                    
                }               
                catch(Exception e){                    
                    if(e.Message.Contains(string.Format("database \"{0}\" does not exist", database))) exist = false;                       
                    else throw e;
                } 
            }

            Terminal.WriteResponse();
            return (exist);
        }   
        private static bool CreateDataBase(string server, string database, string sqlDump)
        {
            Terminal.Write(string.Format("Creating database for the student ~{0}... ", Core.Utils.DataBaseToStudentName(database)), ConsoleColor.DarkYellow);            

            string defaultWinPath = "C:\\Program Files\\PostgreSQL\\10\\bin";   
            string cmdPassword = "PGPASSWORD=postgres";
            string cmdCreate = string.Format("createdb -h {0} -U postgres -T template0 {1}", server, database);
            string cmdRestore = string.Format("psql -h {0} -U postgres {1} < \"{2}\"", server, database, sqlDump);            
            Response resp = null;
            List<string> errors = new List<string>();

            switch (OS.GetCurrent())
            {
                //TODO: this must be correctly configured as a path wehn a terminal session begins
                //Once path is ok on windows and unix the almost same code will be used.
                case "win":                  
                    resp = Shell.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdCreate), ToolBox.Bridge.Output.Hidden, defaultWinPath);
                    if(resp.code > 0) errors.Add(resp.stderr.Replace("\n", ""));

                    resp = Shell.Term(string.Format("SET \"{0}\" && {1}", cmdPassword, cmdRestore), ToolBox.Bridge.Output.Hidden, defaultWinPath);
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

            Terminal.WriteResponse(errors);
            return (errors.Count == 0);
        }                                                 
    }
}
