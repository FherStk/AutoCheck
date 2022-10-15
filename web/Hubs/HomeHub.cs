using AutoCheck.Core;
using AutoCheck.Core.Events;

using AutoCheck.Web.Models;

using Microsoft.AspNetCore.SignalR;

namespace AutoCheck.Web.Hubs
{
    public class HomeHub : Hub
    {
        public void Run(string script, Dictionary<string, string> target, Dictionary<string, string> vars)
        {        
            //TODO: multi target mode (local and remote) so those can be added from the web and injected into the script                
            var s = new Script(script, OnLogUpdateEventHandler, false);
            s.OverrideTarget(target, vars);

            try{
                s.Start();
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
            Clients.Caller.SendAsync("ReceiveLog", (e.Log == null ? null : e.Log.ToJson()), e.EndOfScript, false);
        }           
    }
}