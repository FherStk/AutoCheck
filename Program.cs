using System;
using System.IO;

namespace AutomatedAssignmentValidator
{
    class Program
    {
       static void Main(string[] args)
        {
            Utils.BreakLine();
            Utils.Write("ASIX-DAM M04: ", ConsoleColor.Yellow);
            Utils.WriteLine("Web Assignment Validator for the HTML5 and CSS3 practical assignments.");
            Utils.Write("Version: ", ConsoleColor.Yellow);
            Utils.WriteLine("1.0.2.1");
            Utils.Write(String.Format("Copyright © {0}: ", DateTime.Now.Year), ConsoleColor.Yellow);            
            Utils.WriteLine("Fernando Porrino Serrano. Under the AGPL license (https://github.com/FherStk/ASIX-DAM-M04-WebAssignmentValidator/blob/master/LICENSE)");

            string path = null; 
            string folder = null; 
            string assig = null;           
            for(int i = 0; i < args.Length; i++){
                if(args[i].StartsWith("--") && args[i].Contains("=")){
                    string[] data = args[i].Split("=");
                    string param = data[0].Trim().Replace("\"", "").Substring(2);
                    string value = data[1].Trim().Replace("\"", "");

                    switch(param){
                        case "path":
                            path = value;
                            break;

                        case "folder":
                            folder = value;
                            break;
                        
                        case "assig":
                            assig = value;
                            break;
                    }                                        
                }                                
            }

            //param verification
            Utils.BreakLine();
            if(string.IsNullOrEmpty(assig)) Utils.WriteLine("   ERROR: A parameter 'assig' must be provided.", ConsoleColor.Red);                         
            if(string.IsNullOrEmpty(path) && string.IsNullOrEmpty(folder)) Utils.WriteLine("   ERROR: A parameter 'path' or 'folder' must be provided.", ConsoleColor.Red);                        
            
            if(!string.IsNullOrEmpty(assig) && (!string.IsNullOrEmpty(path) ||!string.IsNullOrEmpty(folder))){
                Utils.Write("Running test: ");          
                Utils.WriteLine(assig.ToUpper(), ConsoleColor.Cyan);            
                Utils.BreakLine();
                
                if(string.IsNullOrEmpty(path)) CheckFolder(folder, assig);
                else CheckPath(path, assig);
            }            
            
            Utils.BreakLine();
            Utils.WriteLine("Press any key to close.");
            Console.ReadKey();
        }   

        private static void CheckPath(string path, string assig)
        {                         
            foreach(string folder in Directory.EnumerateDirectories(path))
            { 
                string studentFolder = Path.GetFileName(folder);
                int i = studentFolder.IndexOf("_"); //Moodle assignments download uses "_" in order to separate the student name from the assignment ID
                string student = studentFolder.Substring(0, i);

                Utils.Write("Checking files for the student: ");
                Utils.WriteLine(student, ConsoleColor.DarkYellow);
                
                CheckFolder(folder, assig);

                Utils.WriteLine("Press any key to continue...");
                Utils.BreakLine();
                Console.ReadKey(); 
            }          
        }  
        private static void CheckFolder(string folder, string assig)
        {     
            if(string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) Utils.WriteLine(string.Format("   ERROR: Unable to find the folder '{0}'.", folder), ConsoleColor.Red);
            else{                                   
                switch(assig){
                    case "html5":
                        Html5Validator.ValidateIndex(folder);
                        Utils.BreakLine();
                        Html5Validator.ValidateContacte(folder);    
                        Utils.BreakLine();                    
                        break;

                    case "css3":
                        Css3Validator.ValidateIndex(folder);
                        Utils.BreakLine();
                        break;

                    default:
                        Utils.WriteLine(string.Format("   ERROR: No check method has been defined for the assig '{0}'.", assig), ConsoleColor.Red);
                        break;
                }                 
            }          
        }  
    }
}
