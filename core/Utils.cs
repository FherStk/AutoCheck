using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace AutomatedAssignmentValidator.Core{    
    public partial class Utils{  
        /// <summary>
        /// Replaces the characters using diacritics with their equivalents without them (ñ->n; ü->u, etc.).
        /// </summary>
        /// <param name="text">The original string.</param>
        /// <returns>The replaced string.</returns>
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
        /// <summary>
        /// Given a folder name, returns a database name using the student's name, but only if it follows the naming convention 'prefix_STUDENT'.
        /// </summary>
        /// <param name="folder">The folder name name, it must follows the naming convention 'prefix_STUDENT'.</param>
        /// <param name="prefix">The database name prefix.</param>
        /// <returns>A database name like 'prefix_STUDENT'</returns>
        public static string FolderNameToDataBase(string folder, string prefix = "database"){
            return Core.Utils.RemoveDiacritics(string.Format("{0}_{1}", prefix, FolderNameToStudentName(folder).Replace(" ", "_"))); 
        }
        /// <summary>
        /// Extracts the student's name from de database's name, but only if it follows the naming convention 'prefix_STUDENT'.
        /// </summary>
        /// <param name="database">The database name, it must follows the naming convention 'prefix_STUDENT'.</param>
        /// <returns>The student's name.</returns>
        public static string DataBaseNameToStudentName(string database){
            if(!database.Contains("_")) throw new Exception("The current database name does not follows the naming convetion 'prefix_STUDENT'.");
            return database.Substring(database.IndexOf("_") + 1).Replace("_", " ");
        } 
        /// <summary>
        /// Given a folder name, returns the student's name, but only if it follows the naming convention 'prefix_STUDENT'.
        /// </summary>
        /// <param name="folder">The folder name name, it must follows the naming convention 'prefix_STUDENT'.</param>
        /// <returns>The student's name.</returns>
        public static string FolderNameToStudentName(string folder){            
            string studentFolder = Path.GetFileName(folder);
            if(!folder.Contains("_")) throw new Exception("The current folder name does not follows the naming convetion 'prefix_STUDENT'.");
            return studentFolder.Substring(0, studentFolder.IndexOf("_"));                                
        }            
    }
}