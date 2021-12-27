using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AutoCheck.Web.Models;
using YamlDotNet.RepresentationModel;

namespace AutoCheck.Web.Controllers;

public class HomeController : Controller
{
    private class ScriptInfo{
        public string Name {get; set;}
        public string Path {get; set;}
    }

    private class WebScript : AutoCheck.Core.Script{
        //LoadYamlFile is protected, because make it public could be danegerous; also, the Script constructors executes the scripts, so inheriting will do the trick.

        private string path;
        public WebScript(string path): base(){
            this.path = path;
        }
        
        // public string[] GetTargetInfo(){
        //     var info = new List<string>();
        //     var local = new List<Local>();  
        //     var remote = new List<Remote>(); 

        //     var root = (YamlMappingNode)this.LoadYamlFile(this.path).Documents[0].RootNode;
        //     if(root.Children.ContainsKey("single") || root.Children.ContainsKey("batch")){
        //         ForEachChild(root, new Action<string, YamlMappingNode>((name, node) => { 
        //             switch(name){                        
        //                 case "local":                        
        //                     local.Add(ParseLocal(node, name, string.Empty));                            
        //                     break;

        //                 case "remote":                        
        //                     remote.Add(ParseRemote(node, name, string.Empty));
        //                     break;
        //             }
        //         }));
        //     }             

        //     //TODO: handle remote scripts
        //     if(remote.Count > 0) throw new NotImplementedException("Remote script execution has not been implemented yet, use the console client app instead.");

        //     foreach(var l in local){
                
        //     }
          
        //     return info.ToArray();
        // }
    }    

    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult GetScripts(string type)
    {
        var items = new List<ScriptInfo>();
        try{            
            foreach(var item in Directory.GetFiles(Path.Combine(AutoCheck.Core.Utils.ScriptsFolder, "targets", type.ToLower())).OrderBy(x => x)){
                items.Add(new ScriptInfo{
                    Name = Path.GetFileName(item), 
                    Path = item
                });
            }
        }
        catch{

        }
                
        return Json(items);
    }

    // public IActionResult GetExecutionInputs(string script)
    // {
    //     //Still not available for remote scripts        
    //     //TODO: file / path within a script will be ignored and will be injected from web into the script (multi file/path)
    //     //TODO: enable remote

    //     //1. Get local YAML data
    //     //2. Generate file / path inputs

    //     // var ws = new WebScript();
    //     // ws.Load


    //     // return Json(items);
    // }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
