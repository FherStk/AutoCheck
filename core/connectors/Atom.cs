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
        /// <param name="filePath">Atom file path.</param>
        public Atom(string filePath): base (filePath){                      
        }

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="remoteOS"The remote host OS.</param>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="port">The remote machine's port where SSH is listening to.</param>
        /// <param name="filePath">Atom file path.</param>
        public Atom(Utils.OS remoteOS, string host, string username, string password, int port, string filePath): base (remoteOS, host, username, password, port, filePath){                      
        }

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="remoteOS"The remote host OS.</param>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="filePath">Atom file path.</param>
        public Atom(Utils.OS remoteOS, string host, string username, string password, string filePath): base (remoteOS, host, username, password, 22, filePath){                      
            //This method can be avoided if the overload containing the 'port' argument moves it to the end and makes it optional, but then the signature becomes inconsistent compared with the remoteShell constructor...
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