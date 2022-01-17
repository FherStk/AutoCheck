/*
    Copyright © 2022 Fernando Porrino Serrano
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
using System.Collections.Generic;
using AutoCheck.Core.Exceptions;

namespace AutoCheck.Core.CopyDetectors{
    /// <summary>
    /// Copy detector for plain text files.
    /// </summary>
    public class SourceCode: Base{
        private List<string> Files {get; set;}
        

        /// <summary>
        /// The amount of items loaded into the copy detector.
        /// </summary>
        /// <value></value>
        public override int Count {
            get {
                return 0;
            }
        }  

        /// <summary>
        /// Creates a new instance, setting up its properties in order to allow copy detection with the lowest possible false-positive probability.
        /// Internally uses JPlag which supports: java, python3, cpp, csharp, char, text, scheme.
        /// </summary>     
        public SourceCode(float threshold, string filePattern = "*.java"): base(threshold, filePattern){    
            Files = new List<string>();             
        } 

        /// <summary>
        /// Loads the given file into the local collection, in order to compare it when Compare() is called.
        /// </summary>
        /// <param name="folder">Path where the files will be looked for.</param>                       
        /// <param name="file">File that will be loaded into the copy detector.</param>
        public override void Load(string folder, string file){    
            Files.Add(file);          
        }

        /// <summary>
        /// Compares all the files between each other
        /// </summary>
        public override void Compare(){  
            var shell = new Connectors.Shell();
            var output = Path.Combine(Utils.TempFolder, DateTime.Now.ToString("yyyyMMddhhhhMMssffff"));
            if(!Directory.Exists(output)) Directory.CreateDirectory(output);

            //TODO: load all files. The JPlag tool uses a set of paths and filenames. Maybe can be done without copying.

            //var result = shell.RunCommand($"java -jar jplag-3.0.0-jar-with-dependencies.jar -r \"{output}\" -p \"{Path.GetExtension(FilePattern).TrimStart('.')}\" \"{RootPath}\"", Utils.UtilsFolder);

            //TODO: read the file "mayches_avg.csv"            
            Directory.Delete(output, true);
        } 

        /// <summary>
        /// Checks if a potential copy has been detected.
        /// The Compare() method should be called firts.
        /// </summary>
        /// <param name="source">The source item asked for.</param>
        /// <param name="threshold">The threshold value, a higher one will be considered as copy.</param>
        /// <returns>True of copy has been detected.</returns>
        public override bool CopyDetected(string path){
            // if(string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            // if(!Index.ContainsKey(path)) throw new ArgumentInvalidException("The given 'path' has not been used within the current copy detector instance.");

            // int i = Index[path];   
            // for(int j=0; j < Files.Count(); j++){
            //     if(i != j){
            //         if(Matches[i,j] >= Threshold) return true;     
            //     }                        
            // }            
           
            return false;
        }

        

        /// <summary>
        /// Disposes the current copy detector instance and releases its internal objects.
        /// </summary>
        public override void Dispose(){            
        }   

        /// <summary>
        /// Returns a printable details list, containing information about the comparissons (student, source and % of match).
        /// </summary>
        /// <param name="path">Path where the files has been loaded.</param>
        /// <returns>Left file followed by all the right files compared with its matching score.</returns>
        public override (string Folder, string File, (string Folder, string File, float Match)[] matches) GetDetails(string path){
            // int i = Index[path];   
            var matches = new List<(string, string, float)>();            
            // for(int j=0; j < Files.Count(); j++){                
            //     if(i != j) matches.Add((Files[j].Folder, Files[j].Path, Matches[i,j]));                     
            // }            
           
            // return (Files[i].Folder, Files[i].Path, matches.ToArray());
            return ("", "", matches.ToArray());
        }  
    }
}