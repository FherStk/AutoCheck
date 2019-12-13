using System;
using System.IO;
using System.Linq;

namespace AutomatedAssignmentValidator
{
    class Program
    {
        private static string _PATH = null; 
        private static string _FOLDER = null; 
        private static string _ASSIG = null; 
        private static string _SERVER = null; 
        private static string _DATABASE = null;    

        static void Main(string[] args)
        {
            Utils.BreakLine();
            Utils.Write("Automated Assignment Validator: ", ConsoleColor.Yellow);                        
            Utils.WriteLine("v1.2.1.0");
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
                            _ASSIG = value;
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
            if(string.IsNullOrEmpty(_ASSIG)) Utils.WriteLine("   ERROR: A parameter 'assig' must be provided.", ConsoleColor.Red);
            else{
                Utils.Write("Running test: ");          
                Utils.WriteLine(_ASSIG.ToUpper(), ConsoleColor.Cyan);            
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
                    case "html5":
                    case "css3":
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
                    
                    case "odoo":
                        //A folder containing all the SQL files, named as "x_NAME_SURNAME".
                        foreach(string f in Directory.EnumerateDirectories(_PATH))
                        {
                            //TODO: self-extract the zip into a folder with the same name                                             
                            string[] temp = Path.GetFileNameWithoutExtension(f).Split("_");                                          
                            string sql = Directory.GetFiles(f, "dump.sql", SearchOption.AllDirectories).FirstOrDefault();

                            if(string.IsNullOrEmpty(sql) || temp.Length < 3) Utils.WriteLine(string.Format("   ERROR: The current folder '{0}' does not contains valid files or folders.", f), ConsoleColor.Red);
                            else{
                                _DATABASE = string.Format("{0}_{1}_{2}", temp[0], temp[1], temp[2]);                                                    
                                if(Utils.CreateDataBase(_SERVER, _DATABASE, sql)){
                                    //Only called if the databse could be created
                                    CheckFolder();
                                }                                                                                        
                            }                            

                            Utils.WriteLine("Press any key to continue...");
                            Utils.BreakLine();
                            Console.ReadKey(); 
                        }
                        break;

                    case "permissions":
                        //A folder containing all the SQL files, named as "x_NAME_SURNAME".
                        foreach(string f in Directory.EnumerateFiles(_PATH))
                        {      
                            //TODO: self-extract the zip into a folder with the same name                                                                                    
                            _DATABASE = Path.GetFileNameWithoutExtension(f);
                            if(Utils.CreateDataBase(_SERVER, _DATABASE, f)){
                                //Only called if the databse could be created
                                CheckFolder();
                            }                                                                                        

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
                case "html5":
                    if(string.IsNullOrEmpty(_FOLDER) || !Directory.Exists(_FOLDER)) Utils.WriteLine(string.Format("   ERROR: Unable to find the provided folder '{0}'.", _FOLDER), ConsoleColor.Red);
                    else{
                        Html5Validator.ValidateIndex(_FOLDER);
                        Utils.BreakLine();
                        Html5Validator.ValidateContacte(_FOLDER);    
                        Utils.BreakLine();                    
                    }                    
                    break;

                case "css3":
                    if(string.IsNullOrEmpty(_FOLDER) || !Directory.Exists(_FOLDER)) Utils.WriteLine(string.Format("   ERROR: Unable to find the provided folder '{0}'.", _FOLDER), ConsoleColor.Red);
                    else{
                        Css3Validator.ValidateIndex(_FOLDER);
                        Utils.BreakLine();
                    }                    
                    break;

                case "odoo":
                    if(string.IsNullOrEmpty(_SERVER)) Utils.WriteLine("   ERROR: The parameter 'server' must be provided when using --assig=odoo.", ConsoleColor.Red);
                    else if(string.IsNullOrEmpty(_DATABASE)) Utils.WriteLine("   ERROR: The parameter 'database' must be provided when using --assig=odoo.", ConsoleColor.Red);
                    else OdooValidator.ValidateDataBase(_SERVER, _DATABASE);
                    break;

                case "permissions":                   
                    if(string.IsNullOrEmpty(_SERVER)) Utils.WriteLine("   ERROR: The parameter 'server' must be provided when using --assig=permissions.", ConsoleColor.Red);
                    else if(string.IsNullOrEmpty(_DATABASE)) Utils.WriteLine("   ERROR: The parameter 'database' must be provided when using --assig=permissions.", ConsoleColor.Red);
                    else PermissionsValidator.ValidateDataBase(_SERVER, _DATABASE);
                    break;

                default:
                    Utils.WriteLine(string.Format("   ERROR: No check method has been defined for the assig '{0}'.", _ASSIG), ConsoleColor.Red);
                    break;
            }                 
                    
        }  
    }
}