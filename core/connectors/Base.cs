/*
    Copyright © 2022 Fernando Porrino Serrano
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

namespace AutoCheck.Core.Connectors{ 
    /// <summary>
    /// Available option for comparing items
    /// </summary>
    public enum Operator{
        LOWER = '<',
        LOWEREQUALS = '≤',
        GREATER = '>',
        GREATEREQUALS = '≥',
        EQUALS = '=',
        NOTEQUALS = '!',
        LIKE = '%'
    }

    /// <summary>
    /// This class must be inherited in order to develop a custom connectors.
    /// This class is an abstraction layer between a checker (to a lesser extent, a script) in order to perform in/out operations and/or data validations.
    /// </summary>   
    public abstract class Base : IDisposable{               
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public abstract void Dispose();        

        /// <summary>
        /// Downloads a remote file into a temp folder, perfoms the action and removes the file.
        /// </summary>
        /// <param name="action">The action to run.</param>        
        protected void ProcessRemoteFile(Utils.OS remoteOS, string host, string username, string password, int port, string filePath, Action<string> action){
            var remote = new Shell(remoteOS, host, username, password, port);              

            if(string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if(!remote.ExistsFile(filePath)) throw new FileNotFoundException("filePath");

            filePath = remote.DownloadFile(filePath);                        
            action.Invoke(filePath);

            Utils.RunWithRetry<IOException>(new Action(() => {
                //Note: GC must be invoked in order to avoid an System.IO.IOException (file in use by another process).
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                File.Delete(filePath);
            }));                
        } 
    }   
}