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

        private string Path {get; set;}

        public WebScript(string path): base(){
            this.Path = path;
        }

        public void InjectTarget(string target){
            var yaml = this.LoadYamlFile(Path);            
            var root = (YamlMappingNode)yaml.Documents[0].RootNode;


            if(root.Children.ContainsKey("single") || root.Children.ContainsKey("batch")){
                ForEachChild(root, new Action<string, YamlNode>((name, node) => { 
                    switch(name){                        
                        case "local":   
                            ForEachChild(root, new Action<string, YamlNode>((name, node) => { 
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
            }   

            var file = yaml.ToString();

        }     
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

    public IActionResult GetScripts(string mode)
    {
        var items = new List<ScriptInfo>();
        try{            
            foreach(var item in Directory.GetFiles(Path.Combine(AutoCheck.Core.Utils.ScriptsFolder, "targets", mode.ToLower())).OrderBy(x => x)){
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

    public IActionResult Run(string script, string mode, string target)
    {
        //Still not available for remote scripts        
        //TODO: file / path within a script will be ignored and will be injected from web into the script (multi file/path)
        //TODO: enable remote

        
        //2. Generate file / path inputs
        
        //1. Getting the local YAML data
        var ws = new WebScript(script);
        ws.InjectTarget(target);

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
}
