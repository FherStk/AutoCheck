using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoCheck.Web
{    
    public class Startup
    {
        private class JsonRootNode{
            public string rootFolderId {get; set;}
            public Dictionary<string, Dictionary<string, object>> fileMap {get; set;}
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigureAvailableYamlScripts();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureAvailableYamlScripts(){                 
            //scripts root folder    
            var scriptsFolder = Core.Utils.ScriptsFolder;
            JsonRootNode rootNode = new JsonRootNode();
            rootNode.rootFolderId = System.Math.Abs(scriptsFolder.GetHashCode()).ToString();
            rootNode.fileMap = new Dictionary<string, Dictionary<string, object>>();

            ConfigureAvailableYamlScriptsRec(null, scriptsFolder, rootNode.fileMap);          

            string json = JsonSerializer.Serialize(rootNode);
            File.WriteAllText(@"ClientApp\src\components\chonky\files.production.json", json);
        }

        private void ConfigureAvailableYamlScriptsRec(string parentID, string currentPath, Dictionary<string, Dictionary<string, object>> fileMap){
            //current folder
            var info = new Dictionary<string, object>();
            info.Add("id",  System.Math.Abs(currentPath.GetHashCode()).ToString());
            info.Add("name", Path.GetFileName(currentPath));
            info.Add("isDir", true);
            info.Add("parentId", parentID);
        
            var children = Directory.GetDirectories(currentPath).Select(x => System.Math.Abs(x.GetHashCode()).ToString()).ToList();
            children.AddRange(Directory.GetFiles(currentPath, "*.yaml").Select(x => System.Math.Abs(x.GetHashCode()).ToString()).ToList());

            info.Add("childrenIds", children.ToArray());
            info.Add("childrenCount", children.Count); 

            fileMap.Add(info["id"].ToString(), info);

            if(children.Count > 0) {
                //child folders (recursive)
                foreach(var folder in Directory.GetDirectories(currentPath)){
                    ConfigureAvailableYamlScriptsRec(info["id"].ToString(), folder, fileMap);
                }

                //child files
                foreach(var file in Directory.GetFiles(currentPath, "*.yaml")){
                    info = new Dictionary<string, object>();
                    info.Add("id", System.Math.Abs(file.GetHashCode()).ToString());
                    info.Add("name", Path.GetFileName(file));
                    info.Add("isDir", false);
                    info.Add("parentId", parentID);    

                    fileMap.Add(info["id"].ToString(), info);  
                }
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllersWithViews();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
