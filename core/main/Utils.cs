/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/credits.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace AutoCheck.Core{    
    public static class Utils{  
        //An Operative System family.
        public enum OS{
            GNU,
            MAC,
            WIN
        }
        
#region Properties        
        /// <summary>
        /// Returns the current app root folder
        /// </summary>
        /// <returns>A folder's path.</returns>
        public static string AppFolder{
            get{
                var exec = ExecutionFolder.Substring(0, ExecutionFolder.IndexOf("bin"));
                return Path.TrimEndingDirectorySeparator(exec);
            }
        }     

        /// <summary>
        /// Returns the current app execution folder
        /// </summary>
        /// <returns>A folder's path.</returns>
        public static string ExecutionFolder{
            get{
                return Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);
            }
        }    

        /// <summary>
        /// Returns the current app config folder
        /// </summary>
        /// <returns>A folder's path.</returns>
        public static string ConfigFolder{
            get{
                return Path.Combine(Path.GetDirectoryName(AppFolder), "core", "config");
            }
        }

        /// <summary>
        /// Returns the current OS host type (Windows; Mac; GNU/Linux)
        /// </summary>
        /// <value></value>
        public static OS CurrentOS {
            get {
                return (OS)Enum.Parse(typeof(OS), ToolBox.Platform.OS.GetCurrent(), true);               
            }
        }
#endregion
#region Extensions
        /// <summary>
        /// Converts the string to its camelcase convention.
        /// </summary>
        /// <param name="text">The original input</param>
        /// <returns></returns>
        public static string ToCamelCase(this string text){
            return Regex.Replace(text, @"([A-Z])([A-Z]+|[a-z0-9_]+)($|[A-Z]\w*)", m => {
                return m.Groups[1].Value.ToLower() + m.Groups[2].Value.ToLower() + m.Groups[3].Value;
            });
        }

        /// <summary>
        /// Replaces the characters using diacritics with their equivalents without them (ñ->n; ü->u, etc.).
        /// </summary>
        /// <param name="text">The original string.</param>
        /// <returns>The replaced string.</returns>
        public static string RemoveDiacritics(this string text) 
        {
            //Manual replacement step (due wrong format from source)
            text = text.Replace("Ã©", "é");

            //Source: https://stackoverflow.com/a/249126
            var norm = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in norm)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }        
#endregion
#region Methods       
        /// <summary>
        /// Returns a path that uses the directory separators of the current OS.
        /// </summary>
        /// <returns>A path.</returns>
        public static string PathToCurrentOS(string path){
            if(string.IsNullOrEmpty(path)) return path;            
            else if(path.Contains('\\')) return path.Replace('\\', Path.DirectorySeparatorChar);
            else if(path.Contains('/')) return path.Replace('/', Path.DirectorySeparatorChar);
            else return path;
        }

        /// <summary>
        /// Returns a path that uses the directory separators of the remote OS.
        /// </summary>
        /// <returns>A path.</returns>
        public static string PathToRemoteOS(string path, OS remoteOS){
            if(string.IsNullOrEmpty(path)) return path;            
            else if(remoteOS == OS.WIN && path.Contains('/')) return path.Replace('/', '\\');
            else if(remoteOS != OS.WIN && path.Contains('\\')) return path.Replace('\\', '/');
            else return path;
        }         

        /// <summary>
        /// Returns the requested app config file
        /// </summary>
        /// <returns>A file's path.</returns>
        public static string ConfigFile(string file){
            return Path.Combine(ConfigFolder, file);
        }         
        
        /// <summary>
        /// Given a folder name, returns a database name using the student's name, but only if it follows the naming convention 'prefix_STUDENT'.
        /// </summary>
        /// <param name="folder">The folder name name, it must follows the naming convention 'prefix_STUDENT'.</param>
        /// <param name="prefix">The database name prefix.</param>
        /// <returns>A database name like 'prefix_STUDENT'</returns>
        public static string FolderNameToDataBase(string folder, string prefix = "database"){
            return ($"{prefix}_{FolderNameToStudentName(folder).Replace(" ", "_")}").RemoveDiacritics();
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

        /// <summary>
        /// Extracts a zip file into the given folder.
        /// </summary>
        /// <param name="path">ZIP file's path.</param>
        /// <param name="output">Destination folder for the extracted files.</param>
        /// <param name="password">ZIP file's password.</param>
        public static void ExtractFile(string path, string output = null, string password = null) {
            if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if(string.IsNullOrEmpty(output)) output = Path.GetDirectoryName(path);
            if(!Directory.Exists(output)) throw new DirectoryNotFoundException();
            if(!File.Exists(path)) throw new FileNotFoundException();
            
            //Encoding must be manually setup in order to avoid errors during decompression
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ZipStrings.CodePage = System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage;

            //source:https://github.com/icsharpcode/SharpZipLib/wiki/Unpack-a-Zip-with-full-control-over-the-operation
            using(Stream fsInput = File.OpenRead(path)){ 
                using(ZipFile zf = new ZipFile(fsInput)){
                    
                    if (!string.IsNullOrEmpty(password)) {
                        // AES encrypted entries are handled automatically
                        zf.Password = password;
                    }

                    foreach (ZipEntry zipEntry in zf) {
                        if (!zipEntry.IsFile) {
                            // Ignore directories
                            continue;
                        }
                        
                        //zipEntry.IsUnicodeText <- is false with error

                        string entryFileName = zipEntry.Name;
                        // to remove the folder from the entry:
                        //entryFileName = Path.GetFileName(entryFileName);
                        // Optionally match entrynames against a selection list here
                        // to skip as desired.
                        // The unpacked length is available in the zipEntry.Size property.

                        // Manipulate the output filename here as desired.
                        var fullZipToPath = Path.Combine(output, entryFileName);
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
#endregion
    }
}