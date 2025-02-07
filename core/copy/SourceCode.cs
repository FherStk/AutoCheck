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
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using AutoCheck.Core.Connectors;
using MathNet.Numerics.Statistics;

namespace AutoCheck.Core.CopyDetectors{
    /// <summary>
    /// Copy detector for plain text files.
    /// </summary>
    public class SourceCode: PlainText{   
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="sensibility">The copy detection sensibility, lower values increases the probability of false positives.</param>
        /// <param name="mode">The comparisson mode.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>  
        public SourceCode(float threshold, int sensibility, DetectionMode mode, string filePattern = "*.java"): base(threshold, (sensibility == -1 ? 7 : sensibility), mode, filePattern){ 
            //JPlag uses 7 as the default sensibility value (-t 7)   
        }        

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="sensibility">The copy detection sensibility, lower values increases the probability of false positives.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        /// <returns></returns>
        public SourceCode(float threshold, int sensibility, string filePattern = "*.java"): this(threshold, sensibility, DetectionMode.DEFAULT, filePattern){           
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="mode">The comparisson mode.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        /// <returns></returns>
        public SourceCode(float threshold, DetectionMode mode, string filePattern = "*.java"): this(threshold, 7, mode, filePattern){    
            //JPlag uses 7 as the default sensibility value (-t 7)          
        }

        /// <summary>
        /// Creates a new instance, setting up its properties in order to allow copy detection with the lowest possible false-positive probability.
        /// </summary>
        /// <param name="threshold">Matches above this value will be computed as potential copies.</param>
        /// <param name="filePattern">Only the files mathing this pattern will be compared.</param>
        /// <returns></returns>
        public SourceCode(float threshold, string filePattern = "*.java"): this(threshold, 7, DetectionMode.DEFAULT, filePattern){ 
            //JPlag uses 7 as the default sensibility value (-t 7)             
        } 
       
        /// <summary>
        /// Compares all the files between each other
        /// </summary>
        public override void Compare(){  
            //Compare will be invoked even for a single file
            if(Files.Count < 2) return;
            
            //JPlag uses one single path
            var path = GetMinimalPath(Files);
            var shell = new Connectors.Shell();
            var output = Path.Combine(Utils.TempFolder, $@"{Guid.NewGuid()}");
            if(!Directory.Exists(output)) Directory.CreateDirectory(output);      
            
            try{
                //Setting up execution
                
                var filter = String.Empty;
                var names = Files.DistinctBy(x => x.FileName);
                if(names.Count() == 1) filter = $"-p {names.FirstOrDefault().FileName}";
                
                var lang = Path.GetExtension(FilePattern).TrimStart('.');
                var report = Path.Combine(output, "report");
                var result = shell.Run($"java -jar jplag-4.2.0-jar-with-dependencies.jar {filter} -n -1 -t {Sensibility} -r \"{report}\" -l {lang} \"{path}\"", Utils.UtilsFolder);                
                
                //Parsing result (JPlag creates JSON files with the output data)
                var folders = new Dictionary<string, int>();

                //temp directory to match the JPlag directory name with the original index (directory path)
                foreach(var key in Index.Keys){
                    folders.Add(Path.GetFileName(key).Trim(), Index[key]);
                }

                //collecting matches
                var accum = new List<double>();
                Matches = new float[Files.Count(), Files.Count()];

                //JPlag v4 generates a ZIP file with the results
                using(Compressed conn = new Compressed($"{report}.zip"))
                    conn.Extract(output);
                
                foreach(var jsonPath in Directory.GetFiles(output, "*.json")){
                    var jsonName = Path.GetFileName(jsonPath);
                    if(jsonName == "overview.json") continue;

                    var json = JObject.Parse(System.IO.File.ReadAllText(jsonPath));
                    try{                        
                        var left = folders[json["id1"].ToString()];
                        var right = folders[json["id2"].ToString()];
                        var match = (float)json["similarity"];

                        accum.Add(match);
                        Matches[left, right] = match;
                        Matches[right, left] = match;                                              
                    }                    
                    catch(KeyNotFoundException){
                        //Could happen if the file has not been loaded (but the folder comes from JPlag with match as 0%)
                        continue;
                    }
                }

                //1-1 matches
                for(int i=0; i<Matches.GetLength(0); i++){
                    Matches[i, i] = 1;
                } 

                //Computing the median if needed
                if(Mode == DetectionMode.AUTO) ComputeAutoModeProperties(accum);
            }
            finally{
                Directory.Delete(output, true);
            }
        }                 
        
        private string GetMinimalPath(List<File> paths){
            var left = paths.FirstOrDefault().FolderPath;
            foreach(var right in paths.Skip(1)){
                left = GetMinimalPath(left, right.FolderPath);
            }

            return left;
        }

        private string GetMinimalPath(string left, string right){
            var absolute = left.StartsWith("/");
            var leftPath = new List<string>();            
            while(left.Length > 1){
                leftPath.Add(Path.GetFileName(left));
                left = Path.GetDirectoryName(left);
            }

            var rightPath = new List<string>();
            while(right.Length > 1){
                rightPath.Add(Path.GetFileName(right));
                right = Path.GetDirectoryName(right);
            }

            leftPath.Reverse();
            rightPath.Reverse();

            var minPath = string.Empty;
            for(var i=0; i<leftPath.Count; i++){
                if(leftPath[i].Equals(rightPath[i])) minPath = Path.Combine(minPath, leftPath[i]);
                else break;
            }

            if(absolute && !minPath.StartsWith("/")) minPath = "/" + minPath;
            return minPath;

        }
    }
}