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

using System.IO;
using System.Linq;
using ToolBox.Bridge;
using ToolBox.Notification;

namespace AutoCheck.Core.Connectors{    
    /// <summary>
    /// Allows in/out operations and/or data validations with a local computer.
    /// </summary>
    public class LocalShell: Base{    
        
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
            this.BridgeSystem = (Utils.CurrentOS == Utils.OS.WIN ? ToolBox.Bridge.BridgeSystem.Bat : ToolBox.Bridge.BridgeSystem.Bash);            
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
        /// Returns the first folder's path found, using the given folder name or search pattern.
        /// </summary>
        /// <param name="path">Path where the folder will be searched into.</param>
        /// <param name="folder">The folder to search (searchpattern).</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>Folder's full path, NULL if does not exists.</returns>
        public virtual string GetFolder(string path, string folder, bool recursive = true){
            return GetFolders(path, folder, recursive).FirstOrDefault();
        }

        /// <summary>
        /// Returns a set of folder's path found, using the given folder name or search pattern.
        /// </summary>
        /// <param name="path">Path where the folders will be searched into.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>Folder's full path.</returns>
        public virtual string[] GetFolders(string path, bool recursive = true){
            return GetFolders(path, "*", recursive);
        }

        /// <summary>
        /// Returns a set of folder's path found, using the given folder name or search pattern.
        /// </summary>
        /// <param name="path">Path where the folders will be searched into.</param>
        /// <param name="searchpattern">The folder search pattern.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>Folder's full path.</returns>
        public virtual string[] GetFolders(string path, string searchpattern = "*", bool recursive = true){
            path = Utils.PathToCurrentOS(path);                         
            if(!Directory.Exists(path)) return null;
            
            return Directory.GetDirectories(path, searchpattern, (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
        }
        
        /// <summary>
        /// Returns the first file's path found, using the given file name or search pattern.
        /// </summary>
        /// <param name="path">Path where the file will be searched into.</param>
        /// <param name="file">The file to search (searchpattern).</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>File's full path, NULL if does not exists.</returns>
        public virtual string GetFile(string path, string file, bool recursive = true){
           return GetFiles(path, file, recursive).FirstOrDefault();
        }

        /// <summary>
        /// Returns a set of file's path found, using the given file name or search pattern.
        /// </summary>
        /// <param name="path">Path where the file will be searched into.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>File's full paths.</returns>
        public virtual string[] GetFiles(string path, bool recursive = true){
            return GetFiles(path, "*", recursive);
        }

        /// <summary>
        /// Returns a set of file's path found, using the given file name or search pattern.
        /// </summary>
        /// <param name="path">Path where the file will be searched into.</param>
        /// <param name="searchpattern">The folder search pattern.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>File's full paths.</returns>
        public virtual string[] GetFiles(string path, string searchpattern="*", bool recursive = true){
            path = Utils.PathToCurrentOS(path); 
            
            if(!Directory.Exists(path)) return null;            
            return Directory.GetFiles(path, searchpattern, (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
        }

        /// <summary>
        /// Returns how many folders has been found within the given path.
        /// </summary>
        /// <param name="path">Path where the folders will be searched into.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The amount of folders.</returns>
        public virtual int CountFolders(string path, bool recursive = true){
           return CountFolders(path, "*", recursive);
        }
        
        /// <summary>
        /// Returns how many folders has been found within the given path.
        /// </summary>
        /// <param name="path">Path where the folders will be searched into.</param>
        /// <param name="searchpattern">The folder search pattern.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The amount of folders.</returns>
        public virtual int CountFolders(string path, string searchpattern="*", bool recursive = true){
           return GetFolders(path, searchpattern, recursive).Count();
        }

        /// <summary>
        /// Returns how many files has been found within the given path.
        /// </summary>
        /// <param name="path">Path where the files will be searched into.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The amount of files.</returns>
        public virtual int CountFiles(string path, bool recursive = true){
             return CountFiles(path, "*", recursive);
        }
        
        /// <summary>
        /// Returns how many files has been found within the given path.
        /// </summary>
        /// <param name="path">Path where the files will be searched into.</param>
        /// <param name="searchpattern">The folder search pattern.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The amount of files.</returns>
        public virtual int CountFiles(string path, string searchpattern="*", bool recursive = true){
             return GetFiles(path, searchpattern, recursive).Count();
        }

        /// <summary>
        /// Determines if a folder exists.
        /// </summary>
        /// <param name="folder">The folder to get including its path.</param>
        public virtual bool ExistsFolder(string folder){
            folder = Utils.PathToCurrentOS(folder); 

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
            path = Utils.PathToCurrentOS(path); 
            return GetFolder(path, folder, recursive) != null;
        }

        /// <summary>
        /// Determines if a file exists.
        /// </summary>
        /// <param name="file">The file to get including its path.</param>
        public virtual bool ExistsFile(string file){
            file = Utils.PathToCurrentOS(file); 
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
            path = Utils.PathToCurrentOS(path); 
            return GetFile(path, file, recursive) != null;
        }
    }
}