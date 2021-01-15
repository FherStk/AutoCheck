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
using System.Xml;
using System.Net;
using System.Xml.XPath;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using AutoCheck.Core.Exceptions;
using Wmhelp.XPath2;

namespace AutoCheck.Core.Connectors{       
    /// <summary>
    /// Allows in/out operations and/or data validations with Atom files.
    /// </summary>
    public class Atom: Rss{                        
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="folder">The folder containing the files.</param>
        /// <param name="file">Atom file name.</param>
        public Atom(string folder, string file): base (folder, file){                      
        }
        
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        }

        /// <summary>
        /// Validates the currently loaded Atom document against the W3C public API. 
        /// Throws an exception if the document is invalid.
        /// </summary>
        public void ValidateAtomAgainstW3C(){
            base.ValidateRssAgainstW3C();
        }
    }
}