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
using System.Linq;
using AutoCheck.Core.Exceptions;
using Renci.SshNet;

namespace AutoCheck.Core.Connectors{    
    /// <summary>
    /// Allows in/out operations and/or data validations with a remote computer (like ssh, scp, etc.).
    /// </summary>
    public class RemoteShell : LocalShell{      
        
        /// <summary>
        /// The remote host OS.
        /// </summary>
        /// <value></value>
        public Utils.OS RemoteOS {get; private set;}

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
        public new SshClient Shell {get; private set;}

        /// <summary>
        /// The SCP client used to intecract with the remote file system.
        /// </summary>
        /// <value></value>  
        private ScpClient FileSystem;
        
        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="remoteOS"The remote host OS.</param>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="port">The remote machine's port where SSH is listening to.</param>
        public RemoteShell(Utils.OS remoteOS, string host, string username, string password, int port = 22): base(){
            if(string.IsNullOrEmpty(host)) throw new ArgumentNullException("host");
            if(string.IsNullOrEmpty(username)) throw new ArgumentNullException("username");
            if(string.IsNullOrEmpty(password)) throw new ArgumentNullException("password");

            this.RemoteOS = remoteOS;
            this.Host = host;
            this.Username = username;
            this.Password = password;
            this.Port = port;
            this.Shell = new Renci.SshNet.SshClient(this.Host, this.Port, this.Username, this.Password);          
            this.FileSystem = new Renci.SshNet.ScpClient(this.Host, this.Port, this.Username, this.Password);          
        }  
        
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
            this.Shell.Dispose();
        }  

        /// <summary>
        /// Test the connection to the remote host, so an exception will be thrown if any problem occurs.
        /// </summary>
        public void TestConnection(){
            try{
                this.Shell.Connect();
                this.Shell.Disconnect();
            }
            catch(Exception ex){
                throw new ConnectionInvalidException("Invalid connection data to the remote host has been provided, check the inner exception for further details.", ex);
            } 
        } 

        /// <summary>
        /// Runs a remote shell command.
        /// </summary>
        /// <param name="command">The command to run.</param>
        /// <returns>The return code and the complete response.</returns>        
        public override (int code, string response) RunCommand(string command, string path = ""){
            this.Shell.Connect();
            SshCommand s = this.Shell.RunCommand(command);
            this.Shell.Disconnect();

            //return (s.ExitStatus, (s.ExitStatus > 0 ? s.Error : s.Result)); //find command returns 1 when permission denied
            return (s.ExitStatus, (string.IsNullOrEmpty(s.Error) ? s.Result : s.Error));
        }                     

        /// <summary>
         /// Returns a set of folder's path found, using the given folder name or search pattern.
        /// </summary>
        /// <param name="path">Path where the folders will be searched into.</param>
        /// <param name="searchpattern">The folder search pattern.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>Folder's full path.</returns>
        public override string[] GetFolders(string path, string searchpattern = "*", bool recursive = true){
            return GetFileOrFolder(path, searchpattern, recursive, true).Items;
        }
          
        /// <summary>
        /// Returns a set of file's path found, using the given file name or search pattern.
        /// </summary>
        /// <param name="path">Path where the file will be searched into.</param>
        /// <param name="searchpattern">The folder search pattern.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>File's full paths.</returns>
        public override string[] GetFiles(string path, string searchpattern = "*", bool recursive = true){
            return GetFileOrFolder(path, searchpattern, recursive, false).Items;
        }       

        /// <summary>
        /// Returns how many folders has been found within the given path.
        /// </summary>
        /// <param name="path">Path where the folders will be searched into.</param>
        /// <param name="searchpattern">The folder search pattern.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The amount of folders.</returns>
        public override int CountFolders(string path, string searchpattern = "*", bool recursive = true){
            return GetFolders(path, searchpattern, recursive).Count();
        }
        
        /// <summary>
        /// Returns how many files has been found within the given path.
        /// </summary>
        /// <param name="path">Path where the files will be searched into.</param>
         /// <param name="searchpattern">The folder search pattern.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>The amount of files.</returns>
        public override int CountFiles(string path, string searchpattern = "*", bool recursive = true){
            return GetFiles(path, searchpattern, recursive).Count();
        }

        /// <summary>
        /// Determines if a folder exists.
        /// </summary>
        /// <param name="folder">The folder to get including its path.</param>
        public override bool ExistsFolder(string folder){
            folder = Utils.PathToRemoteOS(folder, RemoteOS).TrimEnd(RemoteOS == Utils.OS.WIN ? '\\' : '/');
            return ExistsFolder(Path.GetDirectoryName(folder), Path.GetFileName(folder));
        }

        /// <summary>
        /// Determines if a folder exists.
        /// </summary>
        /// <param name="path">Path where the folder will be searched into.</param>
        /// <param name="folder">The folder to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>If the folder exists or not.</returns>
        public override bool ExistsFolder(string path, string folder, bool recursive = false){
            path = Utils.PathToRemoteOS(path, RemoteOS);
            return GetFolder(path, folder, recursive) != null;
        }

        /// <summary>
        /// Determines if a file exists.
        /// </summary>
        /// <param name="file">The file to get including its path.</param>
        public override bool ExistsFile(string file){
            file = Utils.PathToRemoteOS(file, RemoteOS);
            return ExistsFile(Path.GetDirectoryName(file), Path.GetFileName(file));
        }

        /// <summary>
        /// Downloads the remote file into the local system.
        /// </summary>
        /// <param name="file">The file to get including its path.</param>
        /// <returns>The local file path once downloaded.</returns>
        public string DownloadFile(string file){
            if(!ExistsFile(file)) throw new FileNotFoundException();

            var remotePath = Utils.PathToRemoteOS(file, RemoteOS);            
            var localPath = Path.Combine("tmp", Path.GetFileName(remotePath));                        

            FileSystem.Connect();
            FileSystem.Download(remotePath, new FileInfo(localPath));
            FileSystem.Disconnect();

            return localPath;      
        }

        /// <summary>
        /// Determines if a file exists.
        /// </summary>
        /// <param name="path">Path where the file will be searched into.</param>
        /// <param name="file">The file to search.</param>
        /// <param name="recursive">Recursive deep search.</param>
        /// <returns>If the file exists or not.</returns>
        public override bool ExistsFile(string path, string file, bool recursive = false){
            path = Utils.PathToRemoteOS(path, RemoteOS);
            return GetFile(path, file, recursive) != null;
        }

        private (string Path, string[] Items) GetFileOrFolder(string path, string item, bool recursive, bool folder){
            path = Utils.PathToRemoteOS(path, RemoteOS);
            
            string[] items = null;
            switch (this.RemoteOS)
            {
                case Utils.OS.WIN:
                    //TODO: must be tested!
                    var win = RunCommand($"dir \"{path}\" /AD /b /s");
                    items = win.response.Split("\r\n");                                       
                    break;

                case Utils.OS.MAC:
                case Utils.OS.GNU:
                    var gnu = RunCommand($"find '{path}' -mindepth 1 {(recursive ? "" : "-maxdepth 1")} -name '{item}' -type {(folder ? 'd' : 'f')} 2>&-");
                    items = gnu.response.Split("\n").Where(x => !string.IsNullOrEmpty(x)).ToArray();
                    break;
            }

            foreach(string dir in items){
                string next = dir.Replace(path, "").Trim('/');
                if(!recursive && next.StartsWith(item)) return (dir, items);
                else if(recursive && ((folder && next.Contains(item)) || (!folder && next.EndsWith(item.TrimStart('*'))))) return (dir, items);
            } 

            return (null, items);
        }
    }
}