
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

namespace AutoCheck.Checkers{     
    /// <summary>
    /// Allows data validations over a local shell (running local commands).
    /// </summary>  
    public class RemoteShell : LocalShell{  
        //NOTE: The RemoteShell must perform the same opperations as LocalShell, but using the Remote connector.

        /// <summary>
        /// The main connector, can be used to perform direct operations over the data source.
        /// </summary>
        /// <value></value>    
        public new Connectors.RemoteShell Connector {get; private set;}      

        /// <summary>
        /// Creates a new checker instance.
        /// </summary>
        /// <param name="host">Host address where the command will be run.</param>
        /// <param name="username">The remote machine's username which one will be used to login.</param>
        /// <param name="password">The remote machine's password which one will be used to login.</param>
        /// <param name="port">The remote machine's port where SSH is listening to.</param>
        public RemoteShell(string host, string username, string password, int port = 22){
            this.Connector = new Connectors.RemoteShell(host, username, password, port);            
        }              
    }    
}