using System;
using System.IO;

namespace AutomatedAssignmentValidator.Utils{
    //TODO: this methods will be spread along different Utils classes
    public partial class Moodle{            
        public static string FolderToStudentName(string folder){            
            string studentFolder = Path.GetFileName(folder);
                        
            try{
                //Moodle assignments download uses "_" in order to separate the student name from the assignment ID
                return studentFolder.Substring(0, studentFolder.IndexOf("_"));            
            }
            catch{
                return "UNKNOWN";
            }            
        }
        public static string FolderToDataBase(string folder, string prefix = "database"){
            string[] temp = Path.GetFileNameWithoutExtension(folder).Split("_"); 
            if(temp.Length < 5) throw new Exception("The given folder does not follow the needed naming convention.");
            else return String.RemoveDiacritics(string.Format("{0}_{1}", prefix, temp[0]).Replace(" ", "_")); 
        }         
        
         
    }
}