/*
    Copyright © 2020 Fernando Porrino Serrano
    Third party software licenses can be found at /docs/credits/thirdparties.md

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
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;
using AutoCheck.Exceptions;


namespace AutoCheck.Core{
    //TODO: This will be the new Script (without V2)
    public class ScriptV2{
        public string Name {
            get{
                return Vars["script_name"].ToString();
            }
        }

        public string Folder {
            get{
                return Vars["current_folder"].ToString();
            }
        }

        public Dictionary<string, object> Vars {get; private set;}

#region Constructor    
        /// <summary>
        /// Creates a new script instance using the given script file.
        /// </summary>
        /// <param name="path">Path to the script file (yaml).</param>
        public ScriptV2(string path){
            if(!File.Exists(path)) throw new FileNotFoundException(path);
            
            Vars = new Dictionary<string, object>();
            ParseScript(path);
        }
#endregion
#region Parsing   
        private void ParseScript(string path){            
            var yaml = new YamlStream();
            yaml.Load(new StringReader(File.ReadAllText(path)));
            
            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

            Vars.Add("script_name", (mapping.Children["name"] != null ? mapping.Children["name"].ToString() : Regex.Replace(Path.GetFileNameWithoutExtension(path).Replace("_", " "), "[A-Z]", " $0")));
            Vars.Add("current_folder", (mapping.Children["folder"] != null ? mapping.Children["folder"].ToString() : AppContext.BaseDirectory));
            
            ParseVars((YamlMappingNode)mapping.Children[new YamlScalarNode("vars")]);
            ParsePre((YamlMappingNode)mapping.Children[new YamlScalarNode("pre")]);
            
            //Validation
            var expected = new string[]{"name", "folder", "vars", "pre", "post", "body"};
            foreach (var entry in mapping.Children)
            {                
                var current = entry.Key.ToString().ToLower();
                if(!expected.Contains(current)) throw new DocumentInvalidException($"Unexpected value '{current}' found.");              
            }
        }
        
        private void ParseVars(YamlMappingNode root){
            foreach (var item in root.Children){
                var name = item.Key.ToString();
                var value = item.Value.ToString();

                var reserved = new string[]{"script_name", "current_folder", "now"};
                if(reserved.Contains(name)) throw new DocumentInvalidException($"The variable name {name} is reserved and cannot be declared.");

                foreach(Match match in Regex.Matches(value, "{(.*?)}")){
                    var replace = match.Value.TrimStart('{').TrimEnd('}');
                    
                    if(replace.StartsWith("#") || replace.StartsWith("$")){                        
                        //Check if the regex is valid and/or also the referred var exists.
                        var regex = string.Empty;
                        if(replace.StartsWith("#")){
                            regex = replace.Substring(1, replace.LastIndexOf("$")-1);
                            replace = replace.Substring(replace.LastIndexOf("$"));
                        }

                        replace = replace.TrimStart('$');
                        if(replace.Equals("NOW")) replace = DateTime.Now.ToString();
                        else if(!Vars.ContainsKey(replace.ToLower())) throw new InvalidDataException($"Unable to apply a regular expression over the undefined variable {replace} as requested within the variable '{name}'.");                            

                        if(string.IsNullOrEmpty(regex)) replace = Vars[replace.ToLower()].ToString();
                        else {
                            try{
                                replace = Regex.Match(replace, regex).Value;
                            }
                            catch{
                                throw new InvalidDataException($"Invalid regular expression defined inside the variable '{name}'.");
                            }
                        }
                    }
                    
                    value = value.Replace(match.Value, replace);
                }
                
                if(Vars.ContainsKey(name)) throw new InvalidDataException($"Repeated variables defined with name '{name}'.");
                else Vars.Add(name, value);
            }
        }
        
        private void ParsePre(YamlMappingNode root){
            //Loop through because the order matters
            foreach (var item in root.Children){
                var name = item.Key.ToString();
                var children = (YamlMappingNode)root.Children[new YamlScalarNode(name)];

                switch(name){
                    case "extract":
                        Extract(children[new YamlScalarNode("file")].ToString(), bool.Parse(children[new YamlScalarNode("remove")].ToString()),  bool.Parse(children[new YamlScalarNode("recursive")].ToString()));                        
                        break;

                    case "restore_db":
                        break;

                    case "upload_gdrive":
                        break;

                    default:
                        throw new DocumentInvalidException($"Unexpected value '{name}' found.");
                }

            }
        }
#endregion
#region ZIP        
        private void Extract(string file, bool remove, bool recursive){
            Output.Instance.WriteLine("Extracting files: ");
            Output.Instance.Indent();
           
            try{
                string[] files = Directory.GetFiles(Folder, file, (recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));                    
                if(files.Length == 0) Output.Instance.WriteLine("Done!");                    
                else{
                    foreach(string zip in files){
                        try{
                            Output.Instance.Write($"Extracting the file ~{zip}... ", ConsoleColor.DarkYellow);
                            Utils.ExtractFile(zip);
                            Output.Instance.WriteResponse();
                        }
                        catch(Exception e){
                            Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));                           
                            continue;
                        }

                        if(remove){                        
                            try{
                                Output.Instance.Write($"Removing the file ~{zip}... ", ConsoleColor.DarkYellow);
                                File.Delete(zip);
                                Output.Instance.WriteResponse();
                                Output.Instance.BreakLine();
                            }
                            catch(Exception e){
                                Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));
                                continue;
                            }  
                        }
                    }                                                                  
                }                    
            }
            catch (Exception e){
                Output.Instance.WriteResponse(string.Format("ERROR {0}", e.Message));
            }
            finally{    
                Output.Instance.UnIndent();
                if(!remove) Output.Instance.BreakLine();
            }            
        }
#endregion
    }
}