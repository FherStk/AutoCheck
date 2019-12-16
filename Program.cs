using System;
using System.IO;
using System.Linq;

namespace AutomatedAssignmentValidator
{
    class Program
    {
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
            Utils.BreakLine();
            Utils.Write("Automated Assignment Validator: ", ConsoleColor.Yellow);                        
            Utils.WriteLine("v1.2.3.1");
            Utils.Write(String.Format("Copyright © {0}: ", DateTime.Now.Year), ConsoleColor.Yellow);            
            Utils.WriteLine("Fernando Porrino Serrano. Under the AGPL license (https://github.com/FherStk/ASIX-DAM-M04-WebAssignmentValidator/blob/master/LICENSE)");
            
            LoadArguments(args);
            RunWithArguments();
            
            Utils.BreakLine();
            Utils.WriteLine("Press any key to close.");
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
        private static void RunWithArguments(){
            Utils.BreakLine();
            if(_ASSIG == AssignType.UNDEFINED) Utils.WriteLine("   ERROR: A parameter 'assig' must be provided with an accepted value (see README.md).", ConsoleColor.Red);
            else{
                Utils.Write("Running test: ");          
                Utils.WriteLine(_ASSIG.ToString(), ConsoleColor.Cyan);            
                Utils.BreakLine();
                
                if(string.IsNullOrEmpty(_PATH)) CheckFolder();
                else CheckPath();            
            }                
        }
        private static void CheckPath()
        { 
            if(!Directory.Exists(_PATH)) Utils.WriteLine(string.Format("   ERROR: The provided path '{0}' does not exist.", _PATH), ConsoleColor.Red);         
            else{
                switch(_ASSIG){
                    case AssignType.HTML5:
                    case AssignType.CSS3:
                        //MOODLE assignment batch download directory composition
                        foreach(string f in Directory.EnumerateDirectories(_PATH))
                        {   
                            //TODO: self-extract the zip into a folder with the same name                                                           
                            Utils.Write("Checking files for the student: ");
                            Utils.WriteLine(Utils.MoodleFolderToStudentName(f), ConsoleColor.DarkYellow);

                            _FOLDER = f;                
                            CheckFolder();

                            Utils.WriteLine("Press any key to continue...");
                            Utils.BreakLine();
                            Console.ReadKey(); 
                        }                         
                        break;
                    
                    case AssignType.ODOO:
                    case AssignType.PERMISSIONS:
                        //A folder containing all the SQL files, named as "x_NAME_SURNAME".
                        foreach(string f in Directory.EnumerateDirectories(_PATH))
                        {
                            //TODO: self-extract the zip into a folder with the same name                            
                            _FOLDER = f;
                            _DATABASE = string.Empty;   //no database can be selected when using 'path' mode
                            CheckFolder();

                            Utils.WriteLine("Press any key to continue...");
                            Utils.BreakLine();
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
                    if(string.IsNullOrEmpty(_FOLDER)) Utils.WriteLine("   ERROR: The parameter 'folder' or 'path' must be provided when using 'assig=html5'.", ConsoleColor.Red);
                    if(!Directory.Exists(_FOLDER)) Utils.WriteLine(string.Format("   ERROR: Unable to find the provided folder '{0}'.", _FOLDER), ConsoleColor.Red);
                    else Html5Validator.ValidateAssignment(_FOLDER);                                        
                    break;

                case AssignType.CSS3:
                    if(string.IsNullOrEmpty(_FOLDER)) Utils.WriteLine("   ERROR: The parameter 'folder' or 'path' must be provided when using 'assig=html5'.", ConsoleColor.Red);
                    if(!Directory.Exists(_FOLDER)) Utils.WriteLine(string.Format("   ERROR: Unable to find the provided folder '{0}'.", _FOLDER), ConsoleColor.Red);
                    else{
                        Css3Validator.ValidateIndex(_FOLDER);
                        Utils.BreakLine();
                    }                    
                    break;

                case AssignType.ODOO:       
                case AssignType.PERMISSIONS:                      
                    try{
                        bool exist = false;
                        if(string.IsNullOrEmpty(_DATABASE)){
                            _DATABASE = Utils.FolderNameToDataBase(_FOLDER, (_ASSIG == AssignType.ODOO ? "odoo" : "empresa"));
                            string sql = Directory.GetFiles(_FOLDER, "*.sql", SearchOption.AllDirectories).FirstOrDefault();
                            
                            if(string.IsNullOrEmpty(sql)) Utils.WriteLine(string.Format("   ERROR: The current folder '{0}' does not contains any sql file.", _FOLDER), ConsoleColor.Red);
                            else if(string.IsNullOrEmpty(_SERVER)) Utils.WriteLine("   ERROR: The parameter 'server' must be provided when using --assig=odoo.", ConsoleColor.Red);                                
                            else{
                                exist = Utils.DataBaseExists(_SERVER, _DATABASE);
                                if(!exist) exist = Utils.CreateDataBase(_SERVER, _DATABASE, sql);
                                if(!exist) Utils.WriteLine(string.Format("   ERROR: Unable to create the database '{0}' on server '{1}'.", _DATABASE, _SERVER), ConsoleColor.Red);
                            }

                            if(!exist) break;
                        }                          

                        if(!exist) exist = Utils.DataBaseExists(_SERVER, _DATABASE);
                        if(!exist) Utils.WriteLine(string.Format("   ERROR: Unable to create the database '{0}' on server '{1}'.", _DATABASE, _SERVER), ConsoleColor.Red);
                        else {
                            if(_ASSIG == AssignType.ODOO) OdooValidator.ValidateDataBase(_SERVER, _DATABASE);
                            else if(_ASSIG == AssignType.PERMISSIONS) PermissionsValidator.ValidateDataBase(_SERVER, _DATABASE);
                        }
                                                                                  
                    }
                    catch(Exception e){
                        Utils.WriteLine(string.Format("   ERROR: {0}", e.Message), ConsoleColor.Red);
                    }                    
                    break;

                default:
                    Utils.WriteLine(string.Format("   ERROR: No check method has been defined for the assig '{0}'.", _ASSIG), ConsoleColor.Red);
                    break;
            }                 
                    
        }  
    }
}
