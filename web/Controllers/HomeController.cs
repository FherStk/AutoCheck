using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AutoCheck.Core;
using AutoCheck.Core.Connectors;
using AutoCheck.Web.Models;
using YamlDotNet.RepresentationModel;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;


namespace AutoCheck.Web.Controllers;

public class HomeController : Controller
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

    private class WebScript : AutoCheck.Core.Script{
        //LoadYamlFile is protected, because make it public could be danegerous; also, the Script constructors executes the scripts, so inheriting will do the trick.
        
        private YamlStream YAML {get; set;}

        public WebScript(string path): base(){
            this.YAML = this.LoadYamlFile(path);  
        }

        public YamlStream InjectTarget(Dictionary<string, string> target, Dictionary<string, string> vars){                      
            var root = (YamlMappingNode)YAML.Documents[0].RootNode;                                    

            ForEachChild(root, new Action<string, YamlNode>((name, node) => { 
                switch(name){                        
                    case "single":                           
                    case "batch":                        
                        ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                            switch(name){                        
                                case "local":      
                                    ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                                        //folder; path
                                        var scalar = (YamlScalarNode)node;
                                        scalar.Value = target[name];                                        
                                    }));                     
                                    break;

                                case "remote":   
                                   ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                                        //os; host; user; password; vars                                        
                                        switch(name){
                                            case "os":
                                                var os = Enum.Parse(typeof(AutoCheck.Core.Utils.OS), target[name]);
                                                ((YamlScalarNode)node).Value = os.ToString();
                                                break;
                                            
                                            case "vars":
                                                ForEachChild(node, new Action<string, YamlNode>((name, node) => {
                                                    ((YamlScalarNode)node).Value = vars[name];
                                                })); 
                                                break;
                                            
                                            default:
                                                ((YamlScalarNode)node).Value = target[name];
                                                break;
                                        }                                        
                                    }));                     
                                    break;
                            }
                        }));
                        break;
                }
            }));

            return this.YAML;
        } 

        public Dictionary<string, object> GetTargetData(){
            var data = new Dictionary<string, object>();            
            var root = (YamlMappingNode)YAML.Documents[0].RootNode;                        
            
            ForEachChild(root, new Action<string, YamlNode>((name, node) => { 
                switch(name){                        
                    case "single":                           
                    case "batch":                        
                        ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                            switch(name){                        
                                case "local":      
                                    ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                                        //folder; path
                                        data.Add(name, string.Empty);
                                    }));                     
                                    break;

                                case "remote":   
                                   ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                                        //os; host; user; password; vars
                                        if(name != "vars") data.Add(name, string.Empty);
                                        else{
                                            var vars = new Dictionary<string, object>();
                                            data.Add(name, vars);

                                            ForEachChild(node, new Action<string, YamlNode>((name, node) => {
                                                vars.Add(name, string.Empty);
                                            }));      
                                        }
                                    }));                     
                                    break;
                            }
                        }));
                        break;
                }
            }));

            return data;
        }     
    }    

    private readonly ILogger<HomeController> _logger;

    private readonly IHostApplicationLifetime _applicationLifetime;

    public HomeController(IHostApplicationLifetime applicationLifetime, ILogger<HomeController> logger)
    {
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }    

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult GetScripts(ScriptMode mode)
    {
        var items = GetScripts(ScriptSource.Default, mode);
        items.AddRange(GetScripts(ScriptSource.Custom, mode));        
                    
        return Json(items);
    }

    public IActionResult GetTargetData(string script)
    {
        var ws = new WebScript(script);                    
        return Json(ws.GetTargetData());
    }

    public IActionResult Run(string script, Dictionary<string, string> target, Dictionary<string, string> vars)
    {        
        //TODO: multi target mode (local and remote) so those can be added from the web and injected into the script
            
        var ws = new WebScript(script);
        var yaml = ws.InjectTarget(target, vars);
        AutoCheck.Core.Script? result = null;
        Output output;

        try{
            //TODO: write log in async mode
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

        return Content(output.ToJson(), "application/json");
    }

    public IActionResult CheckForUpdate()
    {
        var shell = new Shell();
        var result = shell.Run("git remote update");
        if(result.code != 0) throw new Exception(result.response);            

        result = shell.Run((Core.Utils.CurrentOS == Utils.OS.WIN ? "set LC_ALL=C.UTF-8 & git status -uno" : "LC_ALL=C git status -uno"));
        if(result.code != 0) throw new Exception(result.response);            
        
        return Json(!result.response.Contains("Your branch is up to date with 'origin/master'"));
    }

    public IActionResult PerformUpdate()
    {
        var shell = new Shell();
        var result = shell.Run("git fetch --all");
        if(result.code != 0) throw new Exception(result.response);            

        result = shell.Run("git reset --hard origin/master");
        if(result.code != 0) throw new Exception(result.response);     

        result = shell.Run("git pull");
        if(result.code != 0) throw new Exception(result.response);   
        
        var runScript = Path.Combine(Utils.AppFolder, (Core.Utils.CurrentOS == Utils.OS.WIN ? "run.bat" : "run.sh"));
        var restartScript = Path.Combine(Utils.AppFolder, "Utils", (Core.Utils.CurrentOS == Utils.OS.WIN ? "restart.bat" : "restart.sh"));                        
        
        //On Ubuntu, the sh files needs execution permissions
        SetExecPermissions(runScript);
        SetExecPermissions(restartScript);

        var restart = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = false,
                UseShellExecute = false,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = restartScript
            }
        }; 
        
        _applicationLifetime.ApplicationStopped.Register(() => {
            restart.Start(); 
        });        

        ShutDown();
        return Json(true);
    }

    public IActionResult ShutDown()
    {
        _applicationLifetime.StopApplication();
        return Json(true);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static void SetExecPermissions(string file){
            if(Core.Utils.CurrentOS == Utils.OS.GNU){
            //On Ubuntu, the sh files needs execution permissions
            var chmod = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = false,
                    UseShellExecute = false,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = "/bin/bash",
                    Arguments = $"-c \"chmod +x {file}\""
                }
            }; 

            chmod.Start();
            chmod.WaitForExit();
        }
    }
    private List<ScriptInfo> GetScripts(ScriptSource source, ScriptMode mode){
        var items = new List<ScriptInfo>();
        foreach(var item in Directory.GetFiles(Path.Combine(AutoCheck.Core.Utils.ScriptsFolder, (source == ScriptSource.Default ? "targets" : "custom"), mode.ToString().ToLower()), "*.yaml").OrderBy(x => x)){
            items.Add(new ScriptInfo(source, Path.GetFileName(item), item));            
        }

        return items;
    }
}
