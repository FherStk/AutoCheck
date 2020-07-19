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
using System.Linq;
using ToolBox.Bridge;
using ToolBox.Notification;
using AutoCheck.Core;

namespace AutoCheck.Connectors{    
    /// <summary>
    /// Allows in/out operations and/or data validations with a local computer.
    /// </summary>
    public class LocalShell: Core.Connector{    
        
        private INotificationSystem NotificationSystem { get; set; }
        
        private IBridgeSystem BridgeSystem { get; set; }
       
        /// <summary>
        /// The shell client used to send local commands.
        /// </summary>
        /// <value></value>          
        public ShellConfigurator Shell { get; private set; }      
         
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        public LocalShell(){
            //https://github.com/deinsoftware/toolbox#system
            this.NotificationSystem = ToolBox.Notification.NotificationSystem.Default;
            this.BridgeSystem = (this.CurrentOS == OS.WIN ? ToolBox.Bridge.BridgeSystem.Bat : ToolBox.Bridge.BridgeSystem.Bash);            
            this.Shell = new ShellConfigurator(BridgeSystem, NotificationSystem);                                        
        }
        
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        }
        
        /// <summary>
        /// Runs a local shell command.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <param name="path">The binary path where the command executable is located.</param>
        /// <returns>The return code (0 = OK) and the complete response.</returns>
        public virtual (int code, string response) RunCommand(string command, string path = ""){
            Response r = this.Shell.Term(command, ToolBox.Bridge.Output.Hidden, path);
            return (r.code, (r.code > 0 ? r.stderr : r.stdout));
        }        
        
        /// <summary>
        /// Returns a folder full path if exists.
        /// </summary>
        /// <param name="path">Path where the folder will be searched into.</param>
        /// <param name="folder">The folder to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>Folder's full path, NULL if does not exists.</returns>
        public virtual string GetFolder(string path, string folder, bool recursive = true){
            if(!Directory.Exists(path)) return null;
            
            string[] found = Directory.GetDirectories(path, folder, (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            return (found.Length > 0 ? found.FirstOrDefault() : null);
        }
        
        /// <summary>
        /// Returns a file full path if exists.
        /// </summary>
        /// <param name="path">Path where the file will be searched into.</param>
        /// <param name="file">The file to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>Folder's full path, NULL if does not exists.</returns>
        public virtual string GetFile(string path, string file, bool recursive = true){
            if(!Directory.Exists(path)) return null;
            
            string[] found = Directory.GetFiles(path, file, (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            return (found.Length > 0 ? found.FirstOrDefault() : null);
        }
        
        /// <summary>
        /// Returns how many folders has been found within the given path.
        /// </summary>
        /// <param name="path">Path where the folders will be searched into.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The amount of folders.</returns>
        public virtual int CountFolders(string path, bool recursive = true){
            if(!Directory.Exists(path)) return 0;            
            return Directory.GetDirectories(path, "*", (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)).Count();
        }
        
        /// <summary>
        /// Returns how many files has been found within the given path.
        /// </summary>
        /// <param name="path">Path where the files will be searched into.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The amount of files.</returns>
        public virtual int CountFiles(string path, bool recursive = true){
            if(!Directory.Exists(path)) return 0;
            return Directory.GetFiles(path, "*", (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)).Count();            
        }

        /// <summary>
        /// Determines if a folder exists.
        /// </summary>
        /// <param name="folder">The folder to get including its path.</param>
        public virtual bool ExistsFolder(string folder){
            folder = folder.TrimEnd('\\');
            return ExistsFolder(Path.GetDirectoryName(folder), Path.GetFileName(folder));
        }

        /// <summary>
        /// Determines if a folder exists.
        /// </summary>
        /// <param name="path">Path where the folder will be searched into.</param>
        /// <param name="folder">The folder to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>If the folder exists or not.</returns>
        public virtual bool ExistsFolder(string path, string folder, bool recursive = false){
            return GetFolder(path, folder, recursive) != null;
        }

        /// <summary>
        /// Determines if a file exists.
        /// </summary>
        /// <param name="file">The file to get including its path.</param>
        public virtual bool ExistsFile(string file){
            return ExistsFile(Path.GetDirectoryName(file), Path.GetFileName(file));
        }

        /// <summary>
        /// Determines if a file exists.
        /// </summary>
        /// <param name="path">Path where the file will be searched into.</param>
        /// <param name="file">The file to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>If the file exists or not.</returns>
        public virtual bool ExistsFile(string path, string file, bool recursive = false){
            return GetFile(path, file, recursive) != null;
        }
    }
}