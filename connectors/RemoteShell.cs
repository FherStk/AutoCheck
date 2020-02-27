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
using Renci.SshNet;

namespace AutoCheck.Connectors{    
    /// <summary>
    /// Allows in/out operations and/or data validations with a remote computer (like ssh, scp, etc.).
    /// </summary>
    public class RemoteShell: Core.Connector{      
        public string Host {get; private set;}
        public string Username {get; private set;}
        public string Password {get; private set;}
        public int Port {get; private set;}
        public SshClient Shell {get; private set;}
        public RemoteShell(string host, string username, string password, int port = 22){
            this.Host = host;
            this.Username = username;
            this.Password = password;
            this.Port = port;
            this.Shell = new Renci.SshNet.SshClient(this.Host, this.Port, this.Username, this.Password);
        }            
        public (int code, string response) RunCommand(string command){
            SshCommand s = this.Shell.RunCommand(command);
            return (s.ExitStatus, (s.ExitStatus > 0 ? s.Error : s.Result));
        } 
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
            this.Shell.Dispose();
        } 
    }
}