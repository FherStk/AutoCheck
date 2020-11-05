/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

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
using System.Xml;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using AutoCheck.Core.Exceptions;

namespace AutoCheck.Core.Connectors{       
    /// <summary>
    /// Allows in/out operations and/or data validations with plaint text files.
    /// </summary>
    public class PlainText: Base{         
        private List<string> Comments;
        
        /// <summary>
        /// The plain text document content.
        /// </summary>
        /// <value></value>
        public string plainTextDoc {get; private set;}       
        
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="path">The folder containing the files.</param>
        /// <param name="file">CSV file name.</param>
        /// <param name="fieldDelimiter">Field delimiter char.</param>
        /// <param name="textDelimiter">Text delimiter char.</param>
        public PlainText(string path, string file){
            if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if(string.IsNullOrEmpty(file)) throw new ArgumentNullException("file");
            if(!Directory.Exists(path)) throw new DirectoryNotFoundException();
                        
            plainTextDoc = File.ReadAllText(Path.Combine(path, file));         
        }
        
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        }  

        /// <summary>
        /// Gets how many matches can be found within the document content using a regular expression.
        /// </summary>
        /// <param name="regex">The regular expression which will be used to search the content.</param>
        /// <returns>A set of matches.</returns>
        public string[] Find(string regex){
            var found = new List<string>();
            foreach(Match match in Regex.Matches(plainTextDoc, regex)){
                found.Add(match.Value);
            }            

            return found.ToArray();
        }

        /// <summary>
        /// Counts how many matches can be found within the document content using a regular expression.
        /// </summary>
        /// <param name="regex">The regular expression which will be used to search the content.</param>
        /// <returns>The amount of matches.</returns>
        public int Count(string regex){
           return Find(regex).Length;
        }        
    }
}