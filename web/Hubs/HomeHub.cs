using Microsoft.AspNetCore.SignalR;
using AutoCheck.Web.Models;
using AutoCheck.Core;

namespace AutoCheck.Web.Hubs
{
    public class HomeHub : Hub
    {    
        public async Task Run(string script, Dictionary<string, string> target, Dictionary<string, string> vars)
        {        
            //TODO: multi target mode (local and remote) so those can be added from the web and injected into the script                
            var ws = new WebScript(script);
            var yaml = ws.InjectTarget(target, vars);
            AutoCheck.Core.Script? result = null;
            Output output;

            try{
                result = new AutoCheck.Core.Script(yaml);        
                output = result.Output;
            }
            catch(Exception ex){                 
                output = new Output();            
                output.WriteLine($"ERROR: {ex.Message}", AutoCheck.Core.Output.Style.ERROR);   
                
                while(ex.InnerException != null){
                    ex = ex.InnerException;
                    output.WriteLine($"{AutoCheck.Core.Output.SingleIndent}---> {ex.Message}", AutoCheck.Core.Output.Style.ERROR);   
                }
            }               

            //return Content(output.ToJson(), "application/json");
            //await Clients.Caller.SendAsync("ReceiveMessage", user, message);
        }
        
    }
}