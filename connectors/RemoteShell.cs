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
using Renci.SshNet;

namespace AutoCheck.Connectors{    
    /// <summary>
    /// Allows in/out operations and/or data validations with a remote computer (like ssh, scp, etc.).
    /// </summary>
    public class RemoteShell: Core.Connector{      
        /// <summary>
        /// The remote host address.
        /// </summary>
        /// <value></value>
        public string Host {get; private set;}
        /// <summary>
        /// The remote host username used to login.
        /// </summary>
        /// <value></value>  
        public string Username {get; private set;}
        /// <summary>
        /// The remote host password used to login.
        /// </summary>
        /// <value></value>  
        public string Password {get; private set;}
        /// <summary>
        /// The remote host port, where SSH is listening to.
        /// </summary>
        /// <value></value>  
        public int Port {get; private set;}
        /// <summary>
        /// The SSH client used to send remote commands.
        /// </summary>
        /// <value></value>  
        public SshClient Shell {get; private set;}
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="port">The remote machine's port where SSH is listening to.</param>
        public RemoteShell(string host, string username, string password, int port = 22){
            this.Host = host;
            this.Username = username;
            this.Password = password;
            this.Port = port;
            this.Shell = new Renci.SshNet.SshClient(this.Host, this.Port, this.Username, this.Password);
        }  
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
            this.Shell.Dispose();
        }   
        /// <summary>
        /// Runs a remote shell command.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <returns>The return code and the complete response.</returns>        
        public (int code, string response) RunCommand(string command){
            SshCommand s = this.Shell.RunCommand(command);
            return (s.ExitStatus, (s.ExitStatus > 0 ? s.Error : s.Result));
        }         
        /// <summary>
        /// Returns a folder full path if exists.
        /// </summary>
        /// <param name="path">Path where the folder will be searched into.</param>
        /// <param name="folder">The folder to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>Folder's full path, NULL if does not exists.</returns>
        public string GetFolder(string path, string folder, bool recursive = true){
            //TODO: must be tested!
            string[] items = null;
            switch (ToolBox.Platform.OS.GetCurrent())
            {
                case "win":
                    var win = RunCommand(string.Format("dir \"{0}\" /AD /b /s", path));
                    items = win.response.Split("\r\n");                                       
                    break;

                case "mac":
                case "gnu":
                    var gnu = RunCommand(string.Format("find {0} {1} -name \"{2}\" -type d", path, (recursive ? "" : "-maxdepth 1"), folder));
                    items = gnu.response.Split("\r");
                    break;
            }

            foreach(string dir in items){
                string next = dir.Replace(path, "");
                if(!recursive && next.StartsWith(folder)) return dir;
                else if(recursive && next.Contains(folder)) return dir;
            } 

            return null;
        }
         /// <summary>
        /// Returns a file full path if exists.
        /// </summary>
        /// <param name="path">Path where the file will be searched into.</param>
        /// <param name="file">The file to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>Folder's full path, NULL if does not exists.</returns>
        public string GetFile(string path, string file, bool recursive = true){
            //TODO: must be tested!
            string[] items = null;
            switch (ToolBox.Platform.OS.GetCurrent())
            {
                case "win":
                    var win = RunCommand(string.Format("dir \"{0}\" /AD /b /s", path));
                    items = win.response.Split("\r\n");                                       
                    break;

                case "mac":
                case "gnu":
                    var gnu = RunCommand(string.Format("find {0} {1} -name \"{2}\" -type f", path, (recursive ? "" : "-maxdepth 1"), file));
                    items = gnu.response.Split("\r");
                    break;
            }

            foreach(string dir in items){
                string next = dir.Replace(path, "");
                if(!recursive && next.StartsWith(file)) return dir;
                else if(recursive && next.EndsWith(file)) return dir;
            } 

            return null;
        }
        /// <summary>
        /// Returns how many folders has been found within the given path.
        /// </summary>
        /// <param name="path">Path where the folders will be searched into.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The amount of folders.</returns>
        public int CountFolders(string path, bool recursive = true){
            //TODO: must be tested!
            switch (ToolBox.Platform.OS.GetCurrent())
            {
                case "win":
                    int count = 0;
                    var win = RunCommand(string.Format("dir \"{0}\" /AD /b /s", path));
                    foreach(string dir in win.response.Split("\r\n")){
                        if(!recursive && dir.StartsWith(path)) count++;
                        else if(recursive && dir.Contains(path)) count++;
                    }    
                    return count;                                  

                case "mac":
                case "gnu":
                    var gnu = RunCommand(string.Format("find {0} -name \"{1}\" -type d | wc - l", path, (recursive ? "" : "-maxdepth 1")));
                    return int.Parse(gnu.response);
            }

            return 0;
        }
        /// <summary>
        /// Returns how many files has been found within the given path.
        /// </summary>
        /// <param name="path">Path where the files will be searched into.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The amount of files.</returns>
        public int CountFiles(string path, bool recursive = true){
            //TODO: must be tested!
            switch (ToolBox.Platform.OS.GetCurrent())
            {
                case "win":
                    var win = RunCommand(string.Format("where {0} \"{1}\" *", (recursive ? "/r" : ""), path));
                    return win.response.Split("\r\n").Length;

                case "mac":
                case "gnu":
                    var gnu = RunCommand(string.Format("find {0} -name \"{1}\" -type f | wc - l", path, (recursive ? "" : "-maxdepth 1")));
                    return int.Parse(gnu.response);
            }

            return 0;
        }
    }
}