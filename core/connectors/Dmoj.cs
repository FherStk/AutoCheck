/*
    Copyright © 2023 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/credits.md

    This file is part of AutoCheck.

    AutoCheck is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    AutoCheck is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with AutoCheck.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
using System.Text.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using ExCSS;
using HtmlAgilityPack;
using AutoCheck.Core.Exceptions;

//NOTA: en función del directorio de batch y del file puesto en el script, se invoca a este método con el folder y el file que se ha encontrado por cada iteración.
//      en este caso eso no es necesario, porque se trata de cargar desde un servicio. 
//      habria que definir el destino del batch como un "service" que habría que inventárselo. En ese service habría que definir lo necesario para la conexión, en este caso al DMOJ.
//      se podria lanzar excepción si se intenta hacer cualquier cosa dentro de un "service" que no sea el copy_detector
//      en un futuro podría interesar conectarse a un servicio remoto, descargar todo lo descargable de allí en local y empezar a lanzar un batch (como Moodle por ejemplo).

//ALT:  como alternativa, se puede usar el INIT para descargar todas las entregas del DMOJ de un concurso.
//      luego, basta con pasarle el anticopia de código fuente a esas carpetas generadas con el modo batch tradicional.
//      esta opción parece más sencilla y factible.

namespace AutoCheck.Core.Connectors{

    /// <summary>
    /// Allows in/out operations and/or data validations with the DMOJ service.
    /// </summary>
    public class Dmoj: Base{                
        /// <summary>
        /// DMOJ instance's host address.
        /// </summary>
        /// <value></value>
        public string Host {get; private set;}


        private string ApiToken{
            get{
                string fileName = "dmoj_token.txt";
                string filePath = Utils.ConfigFile(fileName);

                if(!File.Exists(filePath)) throw new ConfigFileMissingException($"Unable to load the DMOJ's API token from the config file '{filePath}'");
                return File.ReadAllText(filePath);
            }
        }

        // /// <summary>
        // /// The original CSS file content (unparsed).
        // /// </summary>
        // /// <value></value>
        // public string Raw {get; private set;}

        /// <summary>
        /// Creates a new connector instance.
        /// </summary>
        /// <param name="host">DMOJ's host address.</param>
        public Dmoj(string host){
            if(string.IsNullOrEmpty(host)) throw new ArgumentNullException("The 'host' cannot be null or empty");
            this.Host = host;            
        }   
        
        /// <summary>
        /// Disposes the object releasing its unmanaged properties.
        /// </summary>
        public override void Dispose(){
        }
        
        /// <summary>
        /// Downloads all contest submissions, generating a a folder for each participant.
        /// </summary>
        /// <param name="contestCode">The contest code to download.</param>
        /// <param name="outputPath">The contest code to download.</param>
        public void DownloadContestSubmissions(string contestCode, string outputPath = ""){
            if(string.IsNullOrEmpty(contestCode)) throw new ArgumentNullException("The 'contestCode' cannot be null or empty");
            if(string.IsNullOrEmpty(outputPath)) outputPath = Path.Combine(Utils.TempFolder, "DMOJ", contestCode);

            //Documentation:    https://docs.dmoj.ca/#/site/api
            var httpClient = new HttpClient();    
            httpClient.DefaultRequestHeaders.Authorization  = new AuthenticationHeaderValue("Bearer", ApiToken);
            
            var contest = DmojApiCall(httpClient, $"https://{Host}/api/v2/contest/{contestCode}");            
            var problems = contest["data"]["object"]["problems"];

            int i=0;
            string[] problemCodes = new string[problems.Count()];
            foreach(var problem in problems){
                problemCodes[i++] = problem["code"].ToString();
            }

            if(string.IsNullOrEmpty(outputPath)) outputPath = Utils.TempFolder;
            var rankings = contest["data"]["object"]["rankings"];
            foreach(var ranking in rankings){
                var user = ranking["user"].ToString();
                var userPath = Path.Combine(outputPath, user);

                if(Directory.Exists(userPath)) Directory.Delete(userPath, true);
                Directory.CreateDirectory(userPath);

                i=0;
                foreach(var submit in ranking["solutions"]){
                    if(submit.HasValues){                       
                        var submissions = DmojApiCall(httpClient, $"https://{Host}/api/v2/submissions?user={user}&problem={problemCodes[i]}");
                        var submitAC = submissions["data"]["objects"].Where(x => x["result"].ToString().Equals("AC")).FirstOrDefault();

                        if(submitAC != null){
                            var submitID = submitAC["id"].ToString();
                            
                            var sourceCode = DmojSrcCall(httpClient, $"https://{Host}/src/{submitID}/raw");        
                            
                            var problemFile = Path.Combine(userPath, $"{problemCodes[i]}.java");
                            File.WriteAllText(problemFile, sourceCode);
                        }
                    }

                    i++;
                }
            }
        }

        private JObject DmojApiCall(HttpClient httpClient, string uri){              
            return JObject.Parse(DmojSrcCall(httpClient, uri));
        }

        private string DmojSrcCall(HttpClient httpClient, string uri){  
            var asyncGet = httpClient.GetAsync(uri);
            asyncGet.Wait();
            asyncGet.Result.EnsureSuccessStatusCode();
            
            var asyncRead = asyncGet.Result.Content.ReadAsStringAsync();
            asyncRead.Wait();
            
            return asyncRead.Result;
        }                   
    }
}