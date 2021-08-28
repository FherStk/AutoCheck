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
            //Sample:
            /*
                {
                    "rootFolderId": "9514a3d74d57",
                    "fileMap": {                   
                        "ed918037b975": {
                            "id": "ed918037b975",
                            "name": "custom",
                            "isDir": true,
                            "childrenIds": [                
                                "0729af954fe6",
                                "a1361e98e01d",
                                "12dd195bb146"                
                            ],
                            "childrenCount": 3,
                            "parentId": "9514a3d74d57"
                        },
                        ...    
                        "a1361e98e01d": {
                            "id": "a1361e98e01d",
                            "name": "views_single.yaml",
                            "parentId": "ed918037b975"
                        },
                        "12dd195bb146": {
                            "id": "12dd195bb146",
                            "name": "web_sindication_single.yaml",
                            "parentId": "ed918037b975"
                        },  
                        ...              
                    }
                }
            */       

            //scripts root folder    
            JsonRootNode root = new JsonRootNode();
            root.rootFolderId = Core.Utils.ScriptsFolder.GetHashCode().ToString();
            root.fileMap = new Dictionary<string, Dictionary<string, object>>();

            //scripts child folders
            foreach(var folder in Directory.GetDirectories(Core.Utils.ScriptsFolder)){
                var info = new Dictionary<string, object>();
                info.Add("id", folder.GetHashCode().ToString());
                info.Add("name", Path.GetFileName(folder));
                info.Add("isDir", true);
                info.Add("parentId", root.rootFolderId);

                var children = Directory.GetDirectories(folder).Select(x => Path.GetFileName(x).GetHashCode().ToString()).ToList();
                children.AddRange(Directory.GetFiles(folder).Select(x => Path.GetFileName(x).GetHashCode().ToString()).ToList());

                info.Add("childrenIds", children.ToArray());
                info.Add("childrenCount", children.Count); 

                root.fileMap.Add(info["id"].ToString(), info);           
            }

            //scripts child files
            foreach(var file in Directory.GetFiles(Core.Utils.ScriptsFolder, "*.yaml")){
                var info = new Dictionary<string, object>();
                info.Add("id", file.GetHashCode().ToString());
                info.Add("name", Path.GetFileName(file));
                info.Add("isDir", false);
                info.Add("parentId", root.rootFolderId);    

                root.fileMap.Add(info["id"].ToString(), info);  
            }

            //TODO: make it recursive

            string json = JsonSerializer.Serialize(root);
            File.WriteAllText(@"ClientApp\src\components\chonky\files.production.json", json);
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
