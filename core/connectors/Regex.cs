/*
    Copyright © 2021 Fernando Porrino Serrano
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
    public class Regex: Base{               
        
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>     
        public Regex(){            
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
            foreach(Match match in System.Text.RegularExpressions.Regex.Matches(content, regex)){
                found.Add(match.Value);
            }            

            return found.ToArray();
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
    }
}