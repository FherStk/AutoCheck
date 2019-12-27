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
            UNDEFINED

        }  

        static void Main(string[] args)
        {
            Terminal.BreakLine();
            Terminal.Write("Automated Assignment Validator: ", ConsoleColor.Yellow);                        
            Terminal.WriteLine("v1.4.0.0");
            Terminal.Write(String.Format("Copyright © {0}: ", DateTime.Now.Year), ConsoleColor.Yellow);            
            Terminal.WriteLine("Fernando Porrino Serrano. Under the AGPL license (https://github.com/FherStk/ASIX-DAM-M04-WebAssignmentValidator/blob/master/LICENSE)");
            
            LoadArguments(args);
            
            Terminal.BreakLine();            
            if(_ASSIG == AssignType.UNDEFINED) Terminal.WriteError("A parameter 'assig' must be provided with an accepted value (see README.md).");
            else{
                Terminal.WriteCaption(string.Format("Running test ~{0}: ", _ASSIG.ToString()), ConsoleColor.Cyan);
                Terminal.BreakLine();
                
                if(string.IsNullOrEmpty(_PATH)) CheckFolder();
                else CheckPath();            
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
        private static void CheckPath()
        { 
            if(!Directory.Exists(_PATH)) Terminal.WriteLine(string.Format("The provided path '{0}' does not exist.", _PATH), ConsoleColor.Red);   
            else{
                switch(_ASSIG){
                    case AssignType.HTML5:
                    case AssignType.CSS3:
                        //MOODLE assignment batch download directory composition
                        foreach(string f in Directory.EnumerateDirectories(_PATH))
                        {
                            try{
                                string student = MoodleFolderToStudentName(f);
                                if(string.IsNullOrEmpty(student)){
                                    Terminal.WriteCaption(string.Format("Skipping folder ~{0}: ", Path.GetFileNameWithoutExtension(f)), ConsoleColor.DarkYellow);                                    
                                    continue;
                                }
                                Terminal.WriteCaption(string.Format("Checking files for the student ~{0}: ", student), ConsoleColor.DarkYellow);                                                                    

                                string zip = Directory.GetFiles(f, "*.zip", SearchOption.AllDirectories).FirstOrDefault();    
                                if(!string.IsNullOrEmpty(zip)){
                                    Terminal.Write("   Unzipping the files: ");
                                    try{
                                        ExtractZipFile(zip);
                                        Terminal.WriteOK();
                                    }
                                    catch(Exception e){
                                        Terminal.WriteError(string.Format("ERROR {0}", e.Message));
                                        continue;
                                    }
                                    
                                    Terminal.Write("   Removing the zip file: ");
                                    try{
                                        File.Delete(zip);
                                        Terminal.WriteOK();
                                    }
                                    catch(Exception e){
                                       Terminal.WriteError(string.Format("ERROR {0}", e.Message));
                                        //the process can continue
                                    }
                                    finally{
                                        Terminal.BreakLine();
                                    }                                              
                                }    

                                _FOLDER = f; 
                                CheckFolder();
                            }
                            catch{

                            }
                            finally{
                                Terminal.WriteLine("Press any key to continue...");
                                Terminal.BreakLine();
                                Console.ReadKey(); 
                            }
                        }                         
                        break;
                    
                    case AssignType.ODOO:
                    case AssignType.PERMISSIONS:
                        //A folder containing all the SQL files, named as "x_NAME_SURNAME".
                        //TODO: it will be easier if the files are delivered through a regular assignment instead of the quiz one.
                        //after that, a merge with CSS3 and HTML5 will be possible (so some code will be simplified)
                        foreach(string f in Directory.EnumerateDirectories(_PATH))
                        {
                            //TODO: self-extract the zip into a folder with the same name                            
                            _FOLDER = f;
                            _DATABASE = string.Empty;   //no database can be selected when using 'path' mode
                            CheckFolder();

                            Terminal.WriteLine("Press any key to continue...");
                            Terminal.BreakLine();
                            Console.ReadKey(); 
                        }
                        break;                   
                }
            }                            
        }  
        private static void CheckFolder()
        {                             
            switch(_ASSIG){
                case AssignType.HTML5:
                    if(string.IsNullOrEmpty(_FOLDER)) Terminal.WriteError("The parameter 'folder' or 'path' must be provided when using 'assig=html5'.");
                    if(!Directory.Exists(_FOLDER)) Terminal.WriteError(string.Format("Unable to find the provided folder '{0}'.", _FOLDER));
                    else{
                        Html5Validator v = new Html5Validator(_FOLDER);
                        v.Validate();  
                    } 
                    break;

                case AssignType.CSS3:
                    if(string.IsNullOrEmpty(_FOLDER)) Terminal.WriteError("The parameter 'folder' or 'path' must be provided when using 'assig=html5'.");
                    if(!Directory.Exists(_FOLDER))Terminal.WriteError(string.Format("Unable to find the provided folder '{0}'.", _FOLDER));
                    else{
                        Css3Validator v = new Css3Validator(_FOLDER);
                        v.Validate();  
                    } 
                    break;

                case AssignType.ODOO:       
                case AssignType.PERMISSIONS:                      
                    try{
                        bool exist = false;
                        if(string.IsNullOrEmpty(_DATABASE)){
                            _DATABASE = FolderNameToDataBase(_FOLDER, (_ASSIG == AssignType.ODOO ? "odoo" : "empresa"));
                            string sql = Directory.GetFiles(_FOLDER, "*.sql", SearchOption.AllDirectories).FirstOrDefault();
                            
                            if(string.IsNullOrEmpty(sql)) Terminal.WriteError(string.Format("The current folder '{0}' does not contains any sql file.", _FOLDER));
                            else if(string.IsNullOrEmpty(_SERVER)) Terminal.WriteError("The parameter 'server' must be provided when using --assig=odoo.");
                            else{
                                exist = DataBaseExists(_SERVER, _DATABASE);
                                if(!exist) exist = CreateDataBase(_SERVER, _DATABASE, sql);
                                if(!exist) Terminal.WriteError(new List<string>(){string.Format("Unable to create the database '{0}' on server '{1}'.", _DATABASE, _SERVER)});
                            }

                            if(!exist) break;
                        }                          

                        if(!exist) exist = DataBaseExists(_SERVER, _DATABASE);
                        if(!exist) Terminal.WriteError(new List<string>(){string.Format("Unable to create the database '{0}' on server '{1}'.", _DATABASE, _SERVER)});
                        else {
                            if(_ASSIG == AssignType.ODOO){
                                OdooValidator v = new OdooValidator(_SERVER, _DATABASE);
                                v.Validate();
                            } 
                            else if(_ASSIG == AssignType.PERMISSIONS) {
                                PermissionsValidator v = new PermissionsValidator(_SERVER, _DATABASE);
                                v.Validate();
                            } 
                        }
                                                                                  
                    }
                    catch(Exception e){
                        Terminal.WriteError(string.Format("EXCEPTION: {0}", e.Message));
                    }                    
                    break;

                default:
                    Terminal.WriteError(string.Format("No check method has been defined for the assig '{0}'.", _ASSIG));
                    break;
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
            Terminal.WriteCaption(string.Format("Checking if a database exists for the student ~{0}:", database.IndexOf("_")+1).Replace("_", " "), ConsoleColor.DarkYellow);                
            
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

            Terminal.WriteError(errors);
            return (exist);
        }   
        private static bool CreateDataBase(string server, string database, string sqlDump)
        {
            Terminal.WriteCaption(string.Format("Creating database for the student ~{0}:", database.Substring(database.IndexOf("_")+1).Replace("_", " ")), ConsoleColor.DarkYellow);            

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

            Terminal.WriteError(errors);
            return (errors.Count == 0);
        }                                 
        private static string MoodleFolderToStudentName(string folder){            
            string studentFolder = Path.GetFileName(folder);
            
            //Moodle assignments download uses "_" in order to separate the student name from the assignment ID
            if(!studentFolder.Contains(" ")) return null;
            else return studentFolder.Substring(0, studentFolder.IndexOf("_"));            
        }  
        private static string FolderNameToDataBase(string folder, string prefix = null){
            string[] temp = Path.GetFileNameWithoutExtension(folder).Split("_"); 
            if(temp.Length < 3) throw new Exception("The given folder does not follow the needed naming convention.");
            else return string.Format("{0}_{1}_{2}", (prefix == null ? temp[0] : prefix), temp[1], temp[2]); 
        }    
    }
}
