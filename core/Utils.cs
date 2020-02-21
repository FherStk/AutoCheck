/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace AutoCheck.Core{    
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
            string studentFolder = string.Empty;
            switch (ToolBox.Platform.OS.GetCurrent())
            {
                case "win":
                    studentFolder = Path.GetFileName(folder);
                    break;
                case "mac":
                case "gnu":
                    studentFolder = Path.GetDirectoryName(folder);
                    break;
            }

            if(!studentFolder.Contains("_")) throw new Exception("The current folder name does not follows the naming convetion 'prefix_STUDENT'.");
            else return studentFolder.Substring(0, studentFolder.IndexOf("_"));                                
        }            
    }
}