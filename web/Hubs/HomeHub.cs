using Microsoft.AspNetCore.SignalR;
using AutoCheck.Core.Events;
using AutoCheck.Web.Models;
using AutoCheck.Core;

namespace AutoCheck.Web.Hubs
{
    public class HomeHub : Hub
    {
        private IClientProxy? Caller;    
        public void Run(string script, Dictionary<string, string> target, Dictionary<string, string> vars)
        {        
            //TODO: multi target mode (local and remote) so those can be added from the web and injected into the script                
            var ws = new WebScript(script);
            var yaml = ws.InjectTarget(target, vars);

            try{
                Caller = Clients.Caller;
                var result = new AutoCheck.Core.Script(yaml, OnLogUpdateEventHandler);    
                Clients.Caller.SendAsync("ReceiveLog", null, false, true);    
            }
            catch(Exception ex){                 
                var output = new Output();            
                output.WriteLine($"ERROR: {ex.Message}", AutoCheck.Core.Output.Style.ERROR);   
                
                while(ex.InnerException != null){
                    ex = ex.InnerException;
                    output.WriteLine($"{AutoCheck.Core.Output.SingleIndent}---> {ex.Message}", AutoCheck.Core.Output.Style.ERROR);   
                }

                Clients.Caller.SendAsync("ReceiveLog", output.ToJson(), false, true);
            }             
        }

        private void OnLogUpdateEventHandler(object? sender, LogUpdateEventArgs e){             
            if(sender == null) return;
            Output output = (Output)sender;

            Clients.Caller.SendAsync("ReceiveLog", (e.Log == null ? null : e.Log.ToJson()), e.EndOfScript, false);
        }           
    }
}