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

using ToolBox.Bridge;
using ToolBox.Notification;

namespace AutoCheck.Connectors{    
    /// <summary>
    /// Allows in/out operations and/or data validations with a local computer.
    /// </summary>
    public class LocalShell: Core.Connector{
        private INotificationSystem NotificationSystem { get; set; }
        private IBridgeSystem BridgeSystem { get; set; }
        public ShellConfigurator Shell { get; private set; }
        
        public LocalShell(){
            //https://github.com/deinsoftware/toolbox#system
            this.NotificationSystem = ToolBox.Notification.NotificationSystem.Default;
            switch (ToolBox.Platform.OS.GetCurrent())
            {
                case "win":
                    this.BridgeSystem = ToolBox.Bridge.BridgeSystem.Bat;
                    break;
                case "mac":
                case "gnu":
                    this.BridgeSystem = ToolBox.Bridge.BridgeSystem.Bash;
                    break;
            }

            this.Shell = new ShellConfigurator(BridgeSystem, NotificationSystem);                                        
        }
        public (int code, string response) RunCommand(string command, string path = ""){
            Response r = this.Shell.Term(command, ToolBox.Bridge.Output.Hidden, path);
            return (r.code, (r.code > 0 ? r.stderr : r.stdout));
        } 
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        } 
    }
}