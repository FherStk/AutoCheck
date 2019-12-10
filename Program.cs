using System;
using System.IO;

namespace AutomatedAssignmentValidator
{
    class Program
    {
        private static string _PATH = null; 
        private static string _FOLDER = null; 
        private static string _ASSIG = null; 
        private static string _SERVER = null; 

        static void Main(string[] args)
        {
            Utils.BreakLine();
            Utils.Write("Automated Assignment Validator: ", ConsoleColor.Yellow);                        
            Utils.WriteLine("v1.1.0.0");
            Utils.Write(String.Format("Copyright © {0}: ", DateTime.Now.Year), ConsoleColor.Yellow);            
            Utils.WriteLine("Fernando Porrino Serrano. Under the AGPL license (https://github.com/FherStk/ASIX-DAM-M04-WebAssignmentValidator/blob/master/LICENSE)");
            
            for(int i = 0; i < args.Length; i++){
                if(args[i].StartsWith("--") && args[i].Contains("=")){
                    string[] data = args[i].Split("=");
                    string param = data[0].Trim().Replace("\"", "").Substring(2);
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
                    }                                        
                }                                
            }

            //param verification
            Utils.BreakLine();
            if(string.IsNullOrEmpty(_ASSIG)) Utils.WriteLine("   ERROR: A parameter 'assig' must be provided.", ConsoleColor.Red);
            else{
                Utils.Write("Running test: ");          
                Utils.WriteLine(_ASSIG.ToUpper(), ConsoleColor.Cyan);            
                Utils.BreakLine();
                
                if(string.IsNullOrEmpty(_PATH)) CheckFolder();
                else CheckPath();            
            }                                
            
            Utils.BreakLine();
            Utils.WriteLine("Press any key to close.");
            Console.ReadKey();
        }   

        private static void CheckPath()
        {               
            if(!Directory.Exists(_PATH)) Utils.WriteLine(string.Format("   ERROR: The provided path '{0}' does not exist.", _PATH), ConsoleColor.Red);         
            else{
                foreach(string f in Directory.EnumerateDirectories(_PATH))
                {                 
                    string student = Utils.MoodleFolderToStudentName(f);

                    Utils.Write("Checking files for the student: ");
                    Utils.WriteLine(student, ConsoleColor.DarkYellow);

                    _FOLDER = f;                
                    CheckFolder();

                    Utils.WriteLine("Press any key to continue...");
                    Utils.BreakLine();
                    Console.ReadKey(); 
                } 
            }                     
        }  
        private static void CheckFolder()
        {     
            if(string.IsNullOrEmpty(_FOLDER) || !Directory.Exists(_FOLDER)) Utils.WriteLine(string.Format("   ERROR: Unable to find the provided folder '{0}'.", _FOLDER), ConsoleColor.Red);
            else{                                   
                switch(_ASSIG){
                    case "html5":
                        Html5Validator.ValidateIndex(_FOLDER);
                        Utils.BreakLine();
                        Html5Validator.ValidateContacte(_FOLDER);    
                        Utils.BreakLine();                    
                        break;

                    case "css3":
                        Css3Validator.ValidateIndex(_FOLDER);
                        Utils.BreakLine();
                        break;

                    case "odoo":
                        if(string.IsNullOrEmpty(_SERVER)) Utils.WriteLine("   ERROR: The parameter 'server' must be provided when using --assig=odoo.", ConsoleColor.Red);
                        else OdooValidator.ValidateDataBase(Utils.MoodleFolderToStudentName(_FOLDER), _SERVER);
                        break;

                    default:
                        Utils.WriteLine(string.Format("   ERROR: No check method has been defined for the assig '{0}'.", _ASSIG), ConsoleColor.Red);
                        break;
                }                 
            }          
        }  
    }
}