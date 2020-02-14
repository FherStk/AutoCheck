using ToolBox.Bridge;
using ToolBox.Notification;

namespace AutomatedAssignmentValidator.Connectors{    
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