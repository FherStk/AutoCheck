using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AutoCheck.Web.Models;

namespace AutoCheck.Web.Controllers;

public class HomeController : Controller
{
    private class ScriptInfo{
        public string Name {get; set;}
        public string Path {get; set;}
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
