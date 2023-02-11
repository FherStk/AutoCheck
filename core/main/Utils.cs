/*
    Copyright © 2023 Fernando Porrino Serrano
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
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Text.RegularExpressions;

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
        /// Returns the current app utils folder
        /// </summary>
        /// <returns>A folder's path.</returns>
        public static string UtilsFolder{
            get{
                return Path.Combine(Path.GetDirectoryName(AppFolder), "core", "utils");
            }
        }

        /// <summary>
        /// Returns the current app scripts folder
        /// </summary>
        /// <returns>A folder's path.</returns>
        public static string ScriptsFolder{
            get{
                return Path.Combine(Path.GetDirectoryName(AppFolder), "scripts");
            }
        }

        /// <summary>
        /// Returns the current app config folder
        /// </summary>
        /// <returns>A folder's path.</returns>
        public static string TempFolder{
            get{
                return Path.Combine(ExecutionFolder, "temp");
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
        /// Returns the product version for the given assembly, which has been setup within the .csproj file.
        /// </summary>
        /// <param name="assembly">The assembly to use.</param>
         /// <returns>The product version.</returns>
        public static string GetProductVersion(Assembly assembly){
            return ((AssemblyInformationalVersionAttribute)assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault()).InformationalVersion;
        }

        /// <summary>
        /// Runs the given action and retries it only for the given exception type (use Exception for generic behaviour).
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="max">Max retries.</param>
        /// <param name="wait">Retry time will be exponential as step*wait.</param>      
        public static void RunWithRetry<T>(Action action, int max=10, int wait=250) where T: Exception{
             RunWithRetry<string, T>(() => {
                action.Invoke();
                return "";
            });
        } 

        /// <summary>
        /// Runs the given action and retries it only for the given exception type (use Exception for generic behaviour).
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="max">Max retries.</param>
        /// <param name="wait">Retry time will be exponential as step*wait.</param>
        public static R RunWithRetry<R, T>(Func<R> function, int max=10, int wait=250) where T: Exception where R: class{
            T exception = null;

            for(int i = 0; i < max; i++){
                try{
                    return function.Invoke();                    
                }
                catch (T ex){
                    exception = ex;
                    System.Threading.Thread.Sleep(i*wait);
                }
            }

            if(exception != null) throw exception;
            return null;
        }    
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
#endregion
    }
}