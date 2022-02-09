using Microsoft.AspNetCore.SignalR;
using System.Text.Json.Serialization;

namespace AutoCheck.Web.Hubs
{
    public class HomeHub : Hub
    {
        private enum ScriptSource{
            Custom,
            Default
        }

        public enum ScriptMode{
            Single,
            Batch
        }
        
        private class ScriptInfo{        
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public ScriptSource Source {get; set;}
            public string Name {get; set;}
            public string Path {get; set;}

            public ScriptInfo(ScriptSource source, string name, string path){
                Source = source;
                Name = name;
                Path = path;
            }
        }
        
        public async Task SendMessage(string user, string message)
        {            
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task GetScripts(ScriptMode mode)
        {
            var items = GetScripts(ScriptSource.Default, mode);
            items.AddRange(GetScripts(ScriptSource.Custom, mode)); 

            await Clients.Caller.SendAsync("ReceiveScripts", items);       
        }

        private List<ScriptInfo> GetScripts(ScriptSource source, ScriptMode mode){
            var items = new List<ScriptInfo>();
            foreach(var item in Directory.GetFiles(Path.Combine(AutoCheck.Core.Utils.ScriptsFolder, (source == ScriptSource.Default ? "targets" : "custom"), mode.ToString().ToLower()), "*.yaml").OrderBy(x => x)){
                items.Add(new ScriptInfo(source, Path.GetFileName(item), item));            
            }

            return items;
        }
    }
}