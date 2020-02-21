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
    public class Shell: Core.Connector{
        private static INotificationSystem _notificationSystem { get; set; }
        private static IBridgeSystem _bridgeSystem { get; set; }
        private static ShellConfigurator _shell { get; set; }
        public static ShellConfigurator Instance { 
            get{ 
                if(_shell == null){
                    //https://github.com/deinsoftware/toolbox#system
                    //This is used in order to launch terminal commands on diferent OS systems (Windows + Linux + Mac)
                    _notificationSystem = NotificationSystem.Default;
                    switch (ToolBox.Platform.OS.GetCurrent())
                    {
                        case "win":
                            _bridgeSystem = BridgeSystem.Bat;
                            break;
                        case "mac":
                        case "gnu":
                            _bridgeSystem = BridgeSystem.Bash;
                            break;
                    }
                    _shell = new ShellConfigurator(_bridgeSystem, _notificationSystem);                    
                }
                
                return _shell;
            }
        }  
    }
}