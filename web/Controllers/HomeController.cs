﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AutoCheck.Core;
using AutoCheck.Core.Connectors;
using AutoCheck.Web.Models;
using YamlDotNet.RepresentationModel;
using System.Text.Json.Serialization;

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

        public YamlStream InjectTarget(string target){                      
            var root = (YamlMappingNode)YAML.Documents[0].RootNode;                        
            
            ForEachChild(root, new Action<string, YamlNode>((name, node) => { 
                switch(name){                        
                    case "single":                           
                    case "batch":                        
                        ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                            switch(name){                        
                                case "local":      
                                    ForEachChild(node, new Action<string, YamlNode>((name, node) => { 
                                        switch(name){                        
                                            case "folder":                       
                                            case "path":                            
                                                var scalar = (YamlScalarNode)node;
                                                scalar.Value = target;       
                                            break;             
                                        }
                                    }));                     
                                    break;

                                case "remote":   
                                    throw new NotImplementedException();                     
                            }
                        }));
                        break;
                }
            }));

            return this.YAML;
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

    public IActionResult Run(string script, string mode, string target)
    {
        //Still not available for remote scripts        
        //TODO: file / path within a script will be ignored and will be injected from web into the script (multi file/path)
        //TODO: enable remote

            
        var ws = new WebScript(script);
        var yaml = ws.InjectTarget(target);
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
        var result = shell.RunCommand("git remote update");
        if(result.code != 0) throw new Exception(result.response);            

        result = shell.RunCommand((Core.Utils.CurrentOS == Utils.OS.WIN ? "set LC_ALL=C.UTF-8 & git status -uno" : "LC_ALL=C git status -uno"));
        if(result.code != 0) throw new Exception(result.response);            
        
        return Json(!result.response.Contains("Your branch is up to date with 'origin/master'"));
    }

    public IActionResult PerformUpdate()
    {
        var shell = new Shell();
        var result = shell.RunCommand("git fetch --all");
        if(result.code != 0) throw new Exception(result.response);            

        result = shell.RunCommand("git reset --hard origin/master");
        if(result.code != 0) throw new Exception(result.response);     

        result = shell.RunCommand("git pull");
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