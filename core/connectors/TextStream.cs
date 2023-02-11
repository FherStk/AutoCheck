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
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace AutoCheck.Core.Connectors{       
    /// <summary>
    /// Allows data manipulation using regular expressions
    /// </summary>
    public class TextStream: Base{               
        
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>     
        public TextStream(){            
        }
               
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        }  

        /// <summary>
        /// Gets the matches found within the given text using a regular expression.
        /// </summary>
        /// <param name="content">The content where the regex will be applied to.</param>
        /// <param name="regex">The regular expression which will be used to search the content.</param>
        /// <returns>A set of matches.</returns>
        public string[] Find(string content, string regex){
            var found = new List<string>();
            foreach(Match match in Regex.Matches(content, regex)){
                found.Add(match.Value);
            }            

            return found.ToArray();
        }

        /// <summary>
        /// Gets the substring found within the given text using a regular expression (first match)
        /// </summary>
        /// <param name="content">The content where the regex will be applied to.</param>
        /// <param name="regex">The regular expression which will be used to search the content.</param>
        /// <returns>A set of matches.</returns>
        public string Substring(string content, string regex){
            var found = Find(content, regex);
            return (found.Length > 0 ? found[0] : string.Empty);
        }

        /// <summary>
        /// Replaces every match within the given content with the given replacement.
        /// </summary>
        /// <param name="content">The content where the regex will be applied to.</param>
        /// <param name="replacement">Every match will be replaced with this.</param>
        /// <param name="regex">The regular expression which will be used to search the content.</param>
        /// <returns>The replaced string.</returns>
        public string Replace(string content, string replacement, string regex){            
            foreach(var found in Find(content, regex)){
                content = content.Replace(found, replacement);
                break; //only one execution needed
            }

            return content;
        }

        /// <summary>
        /// Gets how many matches can be found within the given text using a regular expression.
        /// </summary>
        /// <param name="content">The content where the regex will be applied to.</param>
        /// <param name="regex">The regular expression which will be used to search the content.</param>
        /// <returns>The number of matches.</returns>
        public int Count(string content, string regex){
            return Find(content, regex).Length;
        }

        /// <summary>
        /// Returns the directory path (removes the file name)
        /// </summary>
        /// <param name="path">A path to a file</param>        
        /// <returns>A path to a directory</returns>
        public string DirectoryPath(string path){
            if(Path.HasExtension(path)) return Path.GetDirectoryName(path);
            else return path;
        }

        /// <summary>
        /// Returns the directory name (removes the path and also the filename)
        /// </summary>
        /// <param name="path">A path to a file or folder.</param>        
        /// <returns>A directory name.</returns>
        public string DirectoryName(string path){
            if(Path.HasExtension(path)) path = Path.GetDirectoryName(path);
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Returns the file name (removes the path)
        /// </summary>
        /// <param name="path">A path to a file.</param>        
        /// <returns>A file name.</returns>
        public string FileName(string path){
            return Path.GetFileName(path);            
        }
    }
}