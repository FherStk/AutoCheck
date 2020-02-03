using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace AutomatedAssignmentValidator{
    class Utils{    
        public static string DataBaseToStudentName(string database){
            return database.Substring(database.IndexOf("_")+1).Replace("_", " ");
        }  
        public static string MoodleFolderToStudentName(string folder){            
            string studentFolder = Path.GetFileName(folder);
            
            //Moodle assignments download uses "_" in order to separate the student name from the assignment ID
            if(!studentFolder.Contains(" ")) return null;
            else return studentFolder.Substring(0, studentFolder.IndexOf("_"));            
        }
        public static string FolderNameToDataBase(string folder, string prefix = ""){
            string[] temp = Path.GetFileNameWithoutExtension(folder).Split("_"); 
            if(temp.Length < 5) throw new Exception("The given folder does not follow the needed naming convention.");
            else return RemoveDiacritics(string.Format("{0}_{1}", prefix, temp[0]).Replace(" ", "_")); 
        }         
        public static string RemoveDiacritics(string text) 
        {
            //Manual replacement step (due wrong format from source)
            text = text.Replace("Ã©", "é");

            //Source: https://stackoverflow.com/a/249126
            string norm = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in norm)
            {
                UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }   
    }
}